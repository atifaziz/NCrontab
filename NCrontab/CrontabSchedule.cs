#region License and Terms
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
// Portions Copyright (c) 2001 The OpenSymphony Group. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace NCrontab
{
    using NCrontab.Utils;
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    #endregion

    /// <summary>
    /// Represents a schedule initialized from the crontab expression.
    /// </summary>

    // ReSharper disable once PartialTypeWithSinglePart

    public sealed partial class CrontabSchedule
    {
        readonly CrontabField? _seconds;
        readonly CrontabField _minutes;
        readonly CrontabField _hours;
        readonly CrontabField _days;
        readonly CrontabField _months;
        readonly CrontabField _daysOfWeek;

        static readonly CrontabField SecondZero = CrontabField.Seconds("0");

        // ReSharper disable once PartialTypeWithSinglePart

#pragma warning disable CA1034 // Nested types should not be visible (by design)
        public sealed partial class ParseOptions
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public bool IncludingSeconds { get; set; }
        }

        //
        // Crontab expression format:
        //
        // * * * * *
        // - - - - -
        // | | | | |
        // | | | | +----- day of week (0 - 6) (Sunday=0)
        // | | | +------- month (1 - 12)
        // | | +--------- day of month (1 - 31)
        // | +----------- hour (0 - 23)
        // +------------- min (0 - 59)
        //
        // Star (*) in the value field above means all legal values as in
        // braces for that column. The value column can have a * or a list
        // of elements separated by commas. An element is either a number in
        // the ranges shown above or two numbers in the range separated by a
        // hyphen (meaning an inclusive range).
        //
        // Source: http://www.adminschoice.com/docs/crontab.htm
        //

        // Six-part expression format:
        //
        // * * * * * *
        // - - - - - -
        // | | | | | |
        // | | | | | +--- day of week (0 - 6) (Sunday=0)
        // | | | | +----- month (1 - 12)
        // | | | +------- day of month (1 - 31)
        // | | +--------- hour (0 - 23)
        // | +----------- min (0 - 59)
        // +------------- sec (0 - 59)
        //
        // The six-part expression behaves similarly to the traditional
        // crontab format except that it can denotate more precise schedules
        // that use a seconds component.
        //

        public static CrontabSchedule Parse(string expression) => Parse(expression, null);

        public static CrontabSchedule Parse(string expression, ParseOptions? options) =>
            TryParse(expression, options, v => v, e => throw e());

        public static CrontabSchedule? TryParse(string expression) => TryParse(expression, null);

        public static CrontabSchedule? TryParse(string expression, ParseOptions? options) =>
            TryParse(expression ?? string.Empty, options, v => v, _ => (CrontabSchedule?)null);

        public static T TryParse<T>(string expression,
                                    Func<CrontabSchedule, T> valueSelector,
                                    Func<ExceptionProvider, T> errorSelector) =>
            TryParse(expression ?? string.Empty, null, valueSelector, errorSelector);

        public static T TryParse<T>(string expression, ParseOptions? options, Func<CrontabSchedule, T> valueSelector, Func<ExceptionProvider, T> errorSelector)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
            if (errorSelector == null) throw new ArgumentNullException(nameof(errorSelector));

            var tokens = expression.Split(StringSeparatorStock.Space, StringSplitOptions.RemoveEmptyEntries);

            var includingSeconds = options is { IncludingSeconds: true };
            var expectedTokenCount = includingSeconds ? 6 : 5;
            if (tokens.Length < expectedTokenCount || tokens.Length > expectedTokenCount)
            {
                return errorSelector(() =>
                {
                    var components =
                        includingSeconds
                        ? "6 components of a schedule in the sequence of seconds, minutes, hours, days, months, and days of week"
                        : "5 components of a schedule in the sequence of minutes, hours, days, months, and days of week";
                    return new CrontabException($"'{expression}' is an invalid crontab expression. It must contain {components}.");
                });
            }

            var fields = new CrontabField[6];

            var offset = includingSeconds ? 0 : 1;
            for (var i = 0; i < tokens.Length; i++)
            {
                var kind = (CrontabFieldKind)i + offset;
                var field = CrontabField.TryParse(kind, tokens[i], v => new { ErrorProvider = (ExceptionProvider?)null, Value = (CrontabField?)v    },
                                                                   e => new { ErrorProvider = (ExceptionProvider?)e   , Value = (CrontabField?)null }) ;

                if (field.ErrorProvider != null)
                    return errorSelector(field.ErrorProvider);
                fields[i + offset] = field.Value!; // non-null by mutual exclusivity!
            }

            return valueSelector(new CrontabSchedule(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5]));
        }

        CrontabSchedule(
            CrontabField? seconds,
            CrontabField minutes, CrontabField hours,
            CrontabField days, CrontabField months,
            CrontabField daysOfWeek)
        {
            _seconds = seconds;
            _minutes = minutes;
            _hours = hours;
            _days = days;
            _months = months;
            _daysOfWeek = daysOfWeek;
        }

        /// <summary>
        /// Enumerates all the occurrences of this schedule starting with a
        /// base time and up to an end time limit. This method uses deferred
        /// execution such that the occurrences are only calculated as they
        /// are enumerated.
        /// </summary>
        /// <remarks>
        /// This method does not return the value of <paramref name="baseTime"/>
        /// itself if it falls on the schedule. For example, if <paramref name="baseTime" />
        /// is midnight and the schedule was created from the expression <c>* * * * *</c>
        /// (meaning every minute) then the next occurrence of the schedule
        /// will be at one minute past midnight and not midnight itself.
        /// The method returns the <em>next</em> occurrence <em>after</em>
        /// <paramref name="baseTime"/>. Also, <param name="endTime" /> is
        /// exclusive.
        /// </remarks>

        public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, DateTime endTime)
        {
            for (var occurrence = TryGetOccurrence(baseTime, endTime, true);
                 occurrence != null && occurrence < endTime;
                 occurrence = TryGetOccurrence(occurrence.Value, endTime, true))
            {
                yield return occurrence.Value;
            }
        }

        public IEnumerable<DateTime> GetPrevOccurrences(DateTime baseTime, DateTime startTime)
        {
            for (var occurrence = TryGetOccurrence(baseTime, startTime, false);
                 occurrence != null && occurrence > startTime;
                 occurrence = TryGetOccurrence(occurrence.Value, startTime, false))
            {
                yield return occurrence.Value;
            }
        }

        /// <summary>
        /// Gets the next occurrence of this schedule starting with a base time.
        /// </summary>

        public DateTime GetNextOccurrence(DateTime baseTime) =>
            GetNextOccurrence(baseTime, DateTime.MaxValue);


        public DateTime GetPrevOccurrence(DateTime baseTime) =>
            GetPrevOccurrence(baseTime, DateTime.MinValue);

        /// <summary>
        /// Gets the next occurrence of this schedule starting with a base
        /// time and up to an end time limit.
        /// </summary>
        /// <remarks>
        /// This method does not return the value of <paramref name="baseTime"/>
        /// itself if it falls on the schedule. For example, if <paramref name="baseTime" />
        /// is midnight and the schedule was created from the expression <c>* * * * *</c>
        /// (meaning every minute) then the next occurrence of the schedule
        /// will be at one minute past midnight and not midnight itself.
        /// The method returns the <em>next</em> occurrence <em>after</em>
        /// <paramref name="baseTime"/>. Also, <param name="endTime" /> is
        /// exclusive.
        /// </remarks>

        public DateTime GetNextOccurrence(DateTime baseTime, DateTime endTime) =>
            TryGetOccurrence(baseTime, endTime, true) ?? endTime;

        /// <summary>
        /// Gets the previous occurrence of this schedule starting with a base
        /// time and up to an end time limit.
        /// </summary>
        /// <remarks>
        /// This method does not return the value of <paramref name="baseTime"/>
        /// itself if it falls on the schedule. For example, if <paramref name="baseTime" />
        /// is midnight and the schedule was created from the expression <c>* * * * *</c>
        /// (meaning every minute) then the previous occurrence of the schedule
        /// will be at one minute before midnight and not midnight itself.
        /// The method returns the <em>previous</em> occurrence <em>before</em>
        /// <paramref name="baseTime"/>. Also, <param name="startTime" /> is
        /// exclusive.
        /// </remarks>
        public DateTime GetPrevOccurrence(DateTime baseTime, DateTime startTime) =>
            TryGetOccurrence(baseTime, startTime, false) ?? startTime;

        DateTime? TryGetOccurrence(DateTime baseTime, DateTime limitTime, bool isNext)
        {
            var runningTime = new DateTimeComponents(baseTime);
            var incrementor = new Incrementor(isNext);
            runningTime.Second = incrementor.Increment(runningTime.Second);

            //
            // Second
            //

            //Move to next second
            var secondsIter = new CrontabField.Iterator(_seconds ?? SecondZero, isNext);
            runningTime.Second = secondsIter.Next(runningTime.Second);

            AdjustForSecondInvalid(runningTime, incrementor, secondsIter);

            //
            // Minute
            //

            //Move to next minute
            var minutesIter = new CrontabField.Iterator(_minutes, isNext);
            runningTime.Minute = minutesIter.Next(runningTime.Minute);

            AdjustForMinuteInvalidOrRollover(baseTime, runningTime, incrementor, secondsIter, minutesIter);

            //
            // Hour
            //

            //Move to next hour
            var hoursIter = new CrontabField.Iterator(_hours, isNext);
            runningTime.Hour = hoursIter.Next(runningTime.Hour);

            //Possible iterations will occur when advancing the day causes an invalid date (depending on which)
            //month is being considered.  Note:  Not all months have the same number of days, unlike the other
            //date/time elements which always have the same range.
            var invalidDayOfMonth = false;
            var daysIter = new CrontabField.Iterator(_days, isNext);
            var monthsIter = new CrontabField.Iterator(_months, isNext);
            do
            {
                //If anything but a repeat iteration of this do-while loop that is moving forward in time.
                //When moving backward in time, we need to consider an hour adjustment to the last hour of the previous day
                if (!invalidDayOfMonth || !isNext)
                {
                    AdjustForHourInvalidOrRollover(baseTime, runningTime, incrementor, secondsIter, minutesIter, hoursIter);

                    //
                    // Day
                    //

                    //Move to next day
                    runningTime.Day = daysIter.Next(runningTime.Day);
                }

                //Reset variable for this iteration and assume valid day until tested down below.
                invalidDayOfMonth = false;

                AdjustForDayInvalidOrRollover(baseTime, runningTime, incrementor, secondsIter, minutesIter, hoursIter, daysIter);

                //
                // Month
                //

                runningTime.Month = monthsIter.Next(runningTime.Month);

                AdjustForMonthInvalidOrRollover(baseTime, isNext, runningTime, incrementor, secondsIter, minutesIter, hoursIter, invalidDayOfMonth, daysIter, monthsIter);

                //
                // Stop processing when year goes beyond the upper bound for the datetime or calendar
                // object. Otherwise we would get an exception.
                //

                var upperBoundYear = isNext ? Calendar.MaxSupportedDateTime.Year : Calendar.MinSupportedDateTime.Year;
                if (incrementor.After(runningTime.Year, upperBoundYear))
                    return null;

                //
                // The day field in a cron expression spans the entire range of days
                // in a month, which is from 1 to 31. However, the number of days in
                // a month tend to be variable depending on the month (and the year
                // in case of February). So a check is needed here to see if the
                // date is a border case. If the day happens to be beyond 28
                // (meaning that we're dealing with the suspicious range of 29-31)
                // and the date part has changed then we need to determine whether
                // the day still makes sense for the given year and month. If the
                // day is beyond the last possible value, then the day/month part
                // for the schedule is re-evaluated. So an expression like "0 0
                // 15,31 * *" will yield the following sequence starting on midnight
                // of Jan 1, 2000:
                //
                //  Jan 15, Jan 31, Feb 15, Mar 15, Apr 15, Apr 31, ...
                //

                var dateChanged = runningTime.Day != baseTime.Day || runningTime.Month != baseTime.Month || runningTime.Year != baseTime.Year;

                if (runningTime.Day > 28 && dateChanged && runningTime.Day > Calendar.GetDaysInMonth(runningTime.Year, runningTime.Month))
                {
                    if (incrementor.AfterOrEqual(runningTime.Year, limitTime.Year) &&
                        incrementor.AfterOrEqual(runningTime.Month, limitTime.Month) &&
                        incrementor.AfterOrEqual(runningTime.Day, limitTime.Day))
                        return limitTime;

                    if (isNext)
                        runningTime.Day = CrontabField.nil;
                    else
                        runningTime.Hour = CrontabField.nil;

                    invalidDayOfMonth = true;
                }

            } while (invalidDayOfMonth);

            var resultantTime = new DateTime(runningTime.Year, runningTime.Month, runningTime.Day, runningTime.Hour, runningTime.Minute, runningTime.Second, 0, baseTime.Kind);

            if (incrementor.AfterOrEqual(resultantTime, limitTime))
                return limitTime;

            //
            // Day of week
            //

            if (_daysOfWeek.Contains((int)resultantTime.DayOfWeek))
                return resultantTime;

            var resultantUpperBoundMinuteOfDay = new DateTime(runningTime.Year, runningTime.Month, runningTime.Day, isNext ? 23 : 0, isNext ? 59 : 0, isNext ? 59 : 0, 0, baseTime.Kind);
            return TryGetOccurrence(resultantUpperBoundMinuteOfDay, limitTime, isNext);
        }

        private static void AdjustForMonthInvalidOrRollover(DateTime baseTime, bool isNext, DateTimeComponents runningTime, Incrementor incrementor, CrontabField.Iterator secondsIter, CrontabField.Iterator minutesIter, CrontabField.Iterator hoursIter, bool invalidDayOfMonth, CrontabField.Iterator daysIter, CrontabField.Iterator monthsIter)
        {
            if (runningTime.Month == CrontabField.nil)
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = minutesIter.LowerBound;
                runningTime.Hour = hoursIter.LowerBound;
                runningTime.Day = daysIter.LowerBound;
                runningTime.Month = monthsIter.LowerBound;
                runningTime.Year = incrementor.Increment(runningTime.Year);
            }
            else if (incrementor.After(runningTime.Month, baseTime.Month))
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = minutesIter.LowerBound;
                runningTime.Hour = hoursIter.LowerBound;

                if (isNext || !invalidDayOfMonth)
                    runningTime.Day = daysIter.LowerBound;
            }
        }

        private static void AdjustForDayInvalidOrRollover(DateTime baseTime, DateTimeComponents runningTime, Incrementor incrementor, CrontabField.Iterator secondsIter, CrontabField.Iterator minutesIter, CrontabField.Iterator hoursIter, CrontabField.Iterator daysIter)
        {
            if (runningTime.Day == CrontabField.nil)
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = minutesIter.LowerBound;
                runningTime.Hour = hoursIter.LowerBound;
                runningTime.Day = daysIter.LowerBound;
                runningTime.Month = incrementor.Increment(runningTime.Month);
            }
            else if (incrementor.After(runningTime.Day, baseTime.Day))
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = minutesIter.LowerBound;
                runningTime.Hour = hoursIter.LowerBound;
            }
        }

        private static void AdjustForHourInvalidOrRollover(DateTime baseTime, DateTimeComponents runningTime, Incrementor incrementor, CrontabField.Iterator secondsIter, CrontabField.Iterator minutesIter, CrontabField.Iterator hoursIter)
        {
            if (runningTime.Hour == CrontabField.nil)
            {
                runningTime.Minute = minutesIter.LowerBound;
                runningTime.Hour = hoursIter.LowerBound;
                runningTime.Day = incrementor.Increment(runningTime.Day);
            }
            else if (incrementor.After(runningTime.Hour, baseTime.Hour))
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = minutesIter.LowerBound;
            }
        }

        private static void AdjustForMinuteInvalidOrRollover(DateTime baseTime, DateTimeComponents runningTime, Incrementor incrementor, CrontabField.Iterator secondsIter, CrontabField.Iterator minutesIter)
        {
            if (runningTime.Minute == CrontabField.nil)
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = minutesIter.LowerBound;
                runningTime.Hour = incrementor.Increment(runningTime.Hour);
            }
            else if (incrementor.After(runningTime.Minute, baseTime.Minute))
            {
                runningTime.Second = secondsIter.LowerBound;
            }
        }

        private static void AdjustForSecondInvalid(DateTimeComponents runningTime, Incrementor incrementor, CrontabField.Iterator secondsIter)
        {
            if (runningTime.Second == CrontabField.nil)
            {
                runningTime.Second = secondsIter.LowerBound;
                runningTime.Minute = incrementor.Increment(runningTime.Minute);
            }
        }

        /// <summary>
        /// Returns a string in crontab expression (expanded) that represents
        /// this schedule.
        /// </summary>
        public override string ToString()
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);

            if (_seconds != null)
            {
                _seconds.Format(writer, true);
                writer.Write(' ');
            }
            _minutes.Format(writer, true); writer.Write(' ');
            _hours.Format(writer, true); writer.Write(' ');
            _days.Format(writer, true); writer.Write(' ');
            _months.Format(writer, true); writer.Write(' ');
            _daysOfWeek.Format(writer, true);

            return writer.ToString();
        }

        static Calendar Calendar => CultureInfo.InvariantCulture.Calendar;
    }
}
