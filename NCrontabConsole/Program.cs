#region License and Terms
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
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

namespace NCrontabConsole
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using NCrontab;

    #endregion

    static class Program
    {
        static int Main(string[] args)
        {
            var verbose = false;

            try
            {
                var argList = new List<string>(args);
                var verboseIndex = argList.IndexOf("--verbose");
                // ReSharper disable once AssignmentInConditionalExpression
                if (verbose = verboseIndex >= 0)
                    argList.RemoveAt(verboseIndex);

                if (argList.Count < 3)
                    throw new ApplicationException("Missing required arguments. You must at least supply CRONTAB-EXPRESSION START-DATE END-DATE.");

                var expressions = argList[0].Split(';')
                                            .Select(s => s.Trim())
                                            .Where(s => s.Length > 0)
                                            .ToArray();

                var start = ParseDateArgument(argList[1], "start");
                var end = ParseDateArgument(argList[2], "end");

                var exopts =
                    expressions
                        .Select(expression => new
                        {
                            Expression = expression,
                            Options = new CrontabSchedule.ParseOptions
                            {
                                IncludingSeconds = expression.Split(' ').Length > 5,
                            },
                        })
                        .ToArray();

                var format =
                    argList.Count > 3 ? argList[3]
                    : exopts.Any(e => e.Options.IncludingSeconds)
                    ? "ddd, dd MMM yyyy HH:mm:ss"
                    : "ddd, dd MMM yyyy HH:mm";

                var schedules =
                    from e in exopts
                    select CrontabSchedule.Parse(e.Expression, e.Options);

                foreach (var occurrence in schedules.GetNextOccurrences(start, end))
                    Console.Out.WriteLine(occurrence.ToString(format));

                return 0;
            }
            catch (Exception e)
            {
                var error =
                    verbose
                    ? e.ToString()
                    : e is ApplicationException
                    ? e.Message : e.GetBaseException().Message;
                Console.Error.WriteLine(error);
                return 1;
            }
        }

        static DateTime ParseDateArgument(string arg, string hint)
        {
            try
            {
                return DateTime.Parse(arg, null, DateTimeStyles.AssumeLocal);
            }
            catch (FormatException e)
            {
                throw new ApplicationException("Invalid " + hint + " date or date format argument.", e);
            }
        }

        sealed class ApplicationException : Exception
        {
            public ApplicationException() {}
            public ApplicationException(string message) : base(message) {}
            public ApplicationException(string message, Exception inner) : base(message, inner) {}
        }
    }
}
