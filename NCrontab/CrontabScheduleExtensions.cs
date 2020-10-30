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
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        public static IEnumerable<DateTime> GetNextOccurrences(
            this IEnumerable<CrontabSchedule> schedules,
            DateTime baseTime, DateTime endTime) =>
            GetNextOccurrences(schedules, baseTime, endTime, (_, dt) => dt);

        /// <summary>
        /// Generates a sequence of unique next occurrences (in order) between
        /// two dates and based on one or more schedules. An additional
        /// parameter specifies a function that projects the items of the
        /// resulting sequence where each invocation of the function is given
        /// the schedule that caused the occurrence and the occurrence itself.
        /// </summary>
        /// <remarks>
        /// The <paramref name="baseTime"/> and <paramref name="endTime"/>
        /// arguments are exclusive.
        /// </remarks>

        public static IEnumerable<T> GetNextOccurrences<T>(this IEnumerable<CrontabSchedule> schedules,
            DateTime baseTime, DateTime endTime,
            Func<CrontabSchedule, DateTime, T> resultSelector)
        {
            if (schedules == null) throw new ArgumentNullException(nameof(schedules));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            return Merge(schedules, s => s.GetNextOccurrences(baseTime, endTime), resultSelector);
        }

        static IEnumerable<TResult> Merge<T, TSortable, TResult>(
            IEnumerable<T> sources,
            Func<T, IEnumerable<TSortable>> sortablesSelector,
            Func<T, TSortable, TResult> resultSelector)
            where TSortable : IComparable<TSortable>
        {
            var enumerators = new List<KeyValuePair<T, IEnumerator<TSortable>>>();

            try
            {
                enumerators.AddRange(from t in sources
                                     select Pair(t, sortablesSelector(t).GetEnumerator()));
            }
            catch
            {
                foreach (var e in enumerators)
                    e.Value.Dispose();
                throw;
            }

            try
            {
                for (var i = enumerators.Count - 1; i >= 0; i--)
                {
                    if (!enumerators[i].Value.MoveNext())
                    {
                        enumerators[i].Value.Dispose();
                        enumerators.RemoveAt(i);
                    }
                }

                while (enumerators.Count > 0)
                {
                    var i = 0;
                    var e = enumerators[0];

                    for (var xi = 1; xi < enumerators.Count; xi++)
                    {
                        var xe = enumerators[xi];
                        var comparison = xe.Value.Current.CompareTo(e.Value.Current);
                        if (comparison < 0)
                        {
                            i = xi;
                            e = xe;
                        }
                        else if (comparison == 0)
                        {
                            i = xi;
                            goto skip;
                        }
                    }

                    yield return resultSelector(e.Key, e.Value.Current);

                skip:

                    // advance iterator that yielded element, excluding it when consumed
                    if (!enumerators[i].Value.MoveNext())
                    {
                        enumerators[i].Value.Dispose();
                        enumerators.RemoveAt(i);
                    }
                }
            }
            finally
            {
                foreach (var e in enumerators)
                    e.Value.Dispose();
            }
        }

        static KeyValuePair<TKey, TValue> Pair<TKey, TValue>(TKey key, TValue value) =>
            new KeyValuePair<TKey, TValue>(key, value);
    }
}
