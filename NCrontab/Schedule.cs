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

using System.Collections.Generic;
using System.Linq;
using System;

namespace NCrontab;

static class Schedule
{
    /// <summary>
    /// Generates a sequence of occurrences based on one or more schedules.
    /// </summary>

    internal static IEnumerable<TResult>
        GetOccurrences<T, TResult>(IEnumerable<T> source,
                                   Func<T, IEnumerable<DateTime>> occurrencesSelector,
                                   Func<T, DateTime, TResult> resultSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (occurrencesSelector == null) throw new ArgumentNullException(nameof(occurrencesSelector));
        if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

        return from e in source.Aggregate(Enumerable.Empty<KeyValuePair<T, DateTime>>(),
                                           (a, s) => a.Merge(from e in occurrencesSelector(s)
                                                             select Pair(s, e)))
               select resultSelector(e.Key, e.Value);

        static KeyValuePair<TKey, TValue> Pair<TKey, TValue>(TKey key, TValue value) => new(key, value);
    }
}
