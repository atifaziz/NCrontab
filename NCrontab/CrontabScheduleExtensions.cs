#region License and Terms
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
// Portions Copyright (c) 2023 Microsoft Corp. All rights reserved.
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

using System;
using System.Collections.Generic;

namespace NCrontab;

public static class CrontabScheduleExtensions
{
    /// <summary>
    /// Generates a sequence of unique next occurrences (in order) between
    /// two dates based on one or more schedules.
    /// </summary>
    /// <remarks>
    /// The <paramref name="baseTime"/> and <paramref name="endTime"/>
    /// arguments are exclusive.
    /// </remarks>

    public static IEnumerable<DateTime>
        GetNextOccurrences(this IEnumerable<CrontabSchedule> schedules,
                           DateTime baseTime, DateTime endTime)
    {
        if (schedules == null) throw new ArgumentNullException(nameof(schedules));

        return GetNextOccurrences(schedules, baseTime, endTime, static (_, dt) => dt).DistinctUntilChanged();
    }

    /// <summary>
    /// Generates a sequence of next occurrences (in order) between
    /// two dates and based on one or more schedules. An additional
    /// parameter specifies a function that projects the items of the
    /// resulting sequence where each invocation of the function is given
    /// the schedule that caused the occurrence and the occurrence itself.
    /// </summary>
    /// <remarks>
    /// The <paramref name="baseTime"/> and <paramref name="endTime"/>
    /// arguments are exclusive.
    /// </remarks>

    public static IEnumerable<T>
        GetNextOccurrences<T>(this IEnumerable<CrontabSchedule> schedules,
                              DateTime baseTime, DateTime endTime,
                              Func<CrontabSchedule, DateTime, T> resultSelector)
    {
        if (schedules == null) throw new ArgumentNullException(nameof(schedules));
        if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

        return Schedule.GetOccurrences(schedules,
                                       s => s.GetNextOccurrences(baseTime, endTime),
                                       resultSelector);
    }

    enum Sides { None, First, Second, Both }

    internal static IEnumerable<KeyValuePair<T, DateTime>>
        Merge<T>(this IEnumerable<KeyValuePair<T, DateTime>> schedule1,
                 IEnumerable<KeyValuePair<T, DateTime>> schedule2)
    {
        // Initialize two enumerators for each input sequence.

        using var enumerator1 = schedule1.GetEnumerator();
        using var enumerator2 = schedule2.GetEnumerator();

        // Initialize the flags to determine if each enumerator has a value.

        var have1 = enumerator1.MoveNext();
        var have2 = enumerator2.MoveNext();

        // Enumerate and yield the items in order of smallest due time.

        for (;;)
        {
            // Determine which sequence contains the next smallest due time.

            var sides = have1 && have2 ? Sides.Both
                      : have1 ? Sides.First
                      : have2 ? Sides.Second
                      : Sides.None;

#pragma warning disable IDE0010 // Add missing cases (false negative)
            switch (sides)
#pragma warning restore IDE0010 // Add missing cases
            {
                case Sides.First: // Only first sequence has a value.
                {
                    // Either MoveNext value exists at 1.
                    yield return enumerator1.Current;
                    have1 = enumerator1.MoveNext();
                    break;
                }
                case Sides.Second: // Only second sequence has a value.
                {
                    yield return enumerator2.Current;
                    have2 = enumerator2.MoveNext();
                    break;
                }
                case Sides.Both: // Both sequences have a value.
                {
                    // Determine which enumerator has the next smallest due time.

                    var occurrence1 = enumerator1.Current;
                    var occurrence2 = enumerator2.Current;

                    if (occurrence1.Value.CompareTo(occurrence2.Value) > 0)
                    {
                        // Second has smaller due time, yield it and progress to
                        // the next value in the second sequence.

                        yield return occurrence2;
                        have2 = enumerator2.MoveNext();
                    }
                    else
                    {
                        // First has smaller due time, yield it and progress to
                        // the next value in the first sequence.

                        yield return occurrence1;
                        have1 = enumerator1.MoveNext();
                    }

                    break;
                }
                case Sides.None:
                {
                    // No value left in either sequence so end enumerating.

                    yield break;
                }
            }
        }
    }
}
