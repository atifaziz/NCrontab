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

#pragma warning disable CA1852 // Seal internal types (incorrect)
                               // Type 'Program' can be sealed because it has no subtypes in its
                               // containing assembly and is not externally visible.
                               // See: https://github.com/dotnet/roslyn-analyzers/issues/6141

using System;
using System.Collections.Generic;
using System.Globalization;
using NCrontab;

var verbose = false;

try
{
    var argList = new List<string>(args);
    var verboseIndex = argList.IndexOf("--verbose") is var vi and >= 0 ? vi
                     : argList.IndexOf("-v");
    // ReSharper disable once AssignmentInConditionalExpression
    if (verbose = verboseIndex >= 0)
        argList.RemoveAt(verboseIndex);

    var (expression, startTimeString, endTimeString, format) =
        argList.Count switch
        {
            3   => (argList[0], argList[1], argList[2], null),
            > 3 => (argList[0], argList[1], argList[2], argList[3]),
            _ => throw new ApplicationException("Missing required arguments. You must at least supply CRONTAB-EXPRESSION START-DATE END-DATE."),
        };

    expression = expression.Trim();
    var options = new CrontabSchedule.ParseOptions
    {
        IncludingSeconds = expression.Split(' ').Length > 5,
    };

    var start = ParseDateArgument(startTimeString, "start");
    var end = ParseDateArgument(endTimeString, "end");
    format ??= options.IncludingSeconds ? "ddd, dd MMM yyyy HH:mm:ss"
                                        : "ddd, dd MMM yyyy HH:mm";

    var schedule = CrontabSchedule.Parse(expression, options);

    foreach (var occurrence in schedule.GetNextOccurrences(start, end))
        Console.Out.WriteLine(occurrence.ToString(format, null));

    return 0;
}
#pragma warning disable CA1031 // Do not catch general exception types
catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
{
    var error =
        verbose
        ? e.ToString()
        : e is ApplicationException
        ? e.Message
        : e.GetBaseException().Message;
    Console.Error.WriteLine(error);
    return 1;
}

static DateTime ParseDateArgument(string arg, string hint)
    => DateTime.TryParse(arg, null, DateTimeStyles.AssumeLocal, out var v) ? v
     : throw new ApplicationException("Invalid " + hint + " date or date format argument.");

#pragma warning disable CA1064 // Exceptions should be public
sealed class ApplicationException : Exception
#pragma warning restore CA1064 // Exceptions should be public
{
    public ApplicationException() : this(null) { }
    public ApplicationException(string? message) : this(message, null) { }
    public ApplicationException(string? message, Exception? innerException) : base(message, innerException) { }
}
