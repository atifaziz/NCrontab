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
    using System.Diagnostics;
    using System.Globalization;
    using NCrontab;

    #endregion
    
    static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                    throw new ApplicationException("Missing required arguments. You must at least supply CRONTAB-EXPRESSION START-DATE END-DATE.");

                var expression = args[0];
                var start = ParseDateArgument(args[1], "start");
                var end = ParseDateArgument(args[2], "end");
                var format = args.Length > 3 ? args[3] : "f";
                var schedule = CrontabSchedule.Parse(expression);

                foreach (var occurrence in schedule.GetNextOccurrences(start, end))
                    Console.Out.WriteLine(occurrence.ToString(format));

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Trace.WriteLine(e.ToString());
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
    }
}
