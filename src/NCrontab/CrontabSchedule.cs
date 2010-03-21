#region License, Terms and Author(s)
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
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
	#region Imports

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using Debug = System.Diagnostics.Debug;

    #endregion

    /// <summary>
    /// Represents a schedule initialized from the crontab expression.
    /// </summary>

    [ Serializable ]
    public sealed class CrontabSchedule
    {
        private readonly CrontabField _minutes;
        private readonly CrontabField _hours;
        private readonly CrontabField _days;
        private readonly CrontabField _months;
        private readonly CrontabField _daysOfWeek;

        private static readonly char[] _separators = new[] {' '};

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

        public static CrontabSchedule Parse(string expression)
        {
            return TryParse(expression, ErrorHandling.Throw).Value;
        }

        public static ValueOrError<CrontabSchedule> TryParse(string expression)
        {
            return TryParse(expression, null);
        }

        private static ValueOrError<CrontabSchedule> TryParse(string expression, ExceptionHandler onError)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var tokens = expression.Split(_separators, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 5)
            {
                return ErrorHandling.OnError(() => new CrontabException(string.Format(
                           "'{0}' is not a valid crontab expression. It must contain at least 5 components of a schedule "
                           + "(in the sequence of minutes, hours, days, months, days of week).", 
                           expression)), onError);
            }

            var fields = new CrontabField[5];

            for (var i = 0; i < fields.Length; i++)
            {
                var field = CrontabField.TryParse((CrontabFieldKind) i, tokens[i], onError);
                if (field.IsError)
                    return field.ErrorProvider;

                fields[i] = field.Value;
            }

            return new CrontabSchedule(fields[0], fields[1], fields[2], fields[3], fields[4]);
        }

        private CrontabSchedule(
            CrontabField minutes, CrontabField hours, 
            CrontabField days, CrontabField months, 
            CrontabField daysOfWeek)
        {
            Debug.Assert(minutes != null);
            Debug.Assert(hours != null);
            Debug.Assert(days != null);
            Debug.Assert(months != null);
            Debug.Assert(daysOfWeek != null);

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
            for (var occurrence = GetNextOccurrence(baseTime, endTime);
                 occurrence < endTime; 
                 occurrence = GetNextOccurrence(occurrence, endTime))
            {
                yield return occurrence;
            }
        }

        /// <summary>
        /// Gets the next occurrence of this schedule starting with a base time.
        /// </summary>

        public DateTime GetNextOccurrence(DateTime baseTime)
        {
            return GetNextOccurrence(baseTime, DateTime.MaxValue);
        }

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
        
        public DateTime GetNextOccurrence(DateTime baseTime, DateTime endTime) 
        {
            const int nil = -1;

            var baseYear = baseTime.Year;
            var baseMonth = baseTime.Month;
            var baseDay = baseTime.Day;
            var baseHour = baseTime.Hour;
            var baseMinute = baseTime.Minute;

            var endYear = endTime.Year;
            var endMonth = endTime.Month;
            var endDay = endTime.Day;

            var year = baseYear;
            var month = baseMonth;
            var day = baseDay;
            var hour = baseHour;
            var minute = baseMinute + 1;

            //
            // Minute
            //

            minute = _minutes.Next(minute);

            if (minute == nil) 
            {
                minute = _minutes.GetFirst();
                hour++;
            }

            //
            // Hour
            //

            hour = _hours.Next(hour);
            
            if (hour == nil) 
            {
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day++;
            }
            else if (hour > baseHour)
            {
                minute = _minutes.GetFirst();
            }

            //
            // Day
            //

            day = _days.Next(day);

            RetryDayMonth:
        
            if (day == nil) 
            {
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _days.GetFirst();
                month++;
            }
            else if (day > baseDay)
            {
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
            }

            //
            // Month
            //

            month = _months.Next(month);

            if (month == nil) 
            {
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _days.GetFirst();
                month = _months.GetFirst();
                year++;
            }
            else if (month > baseMonth)
            {
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _days.GetFirst();
            }

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

            var dateChanged = day != baseDay || month != baseMonth || year != baseYear;

            if (day > 28 && dateChanged && day > Calendar.GetDaysInMonth(year, month))
            {
                if (year >= endYear && month >= endMonth && day >= endDay)
                    return endTime;

                day = nil;
                goto RetryDayMonth;
            }

            var nextTime = new DateTime(year, month, day, hour, minute, 0, 0, baseTime.Kind);

            if (nextTime >= endTime)
                return endTime;

            //
            // Day of week
            //

            if (_daysOfWeek.Contains((int) nextTime.DayOfWeek)) 
                return nextTime;

            return GetNextOccurrence(new DateTime(year, month, day, 23, 59, 0, 0, baseTime.Kind), endTime);
        }

        /// <summary>
        /// Returns a string in crontab expression (expanded) that represents 
        /// this schedule.
        /// </summary>

        public override string ToString()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);

            _minutes.Format(writer, true); writer.Write(' ');
            _hours.Format(writer, true); writer.Write(' ');
            _days.Format(writer, true); writer.Write(' ');
            _months.Format(writer, true); writer.Write(' ');
            _daysOfWeek.Format(writer, true);

            return writer.ToString();
        }

        private static Calendar Calendar
        {
            get { return CultureInfo.InvariantCulture.Calendar; }
        }
    }
}
