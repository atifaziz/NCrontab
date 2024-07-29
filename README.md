# NCrontab: Crontab for .NET

[![Build Status][build-badge]][builds]
[![NuGet][nuget-badge]][nuget-pkg]

NCrontab is a library written in C# targeting [.NET Standard Library][netstd]
1.0 and that provides the following facilities:

* Parsing of crontab expressions
* Formatting of crontab expressions
* Calculation of occurrences of time based on a crontab schedule

This library does not provide any scheduler or is not a scheduling facility like
cron from Unix platforms. What it provides is parsing, formatting and an algorithm
to produce occurrences of time based on a give schedule expressed in the crontab
format:

    * * * * *
    - - - - -
    | | | | |
    | | | | +----- day of week (0 - 6) (Sunday=0)
    | | | +------- month (1 - 12)
    | | +--------- day of month (1 - 31)
    | +----------- hour (0 - 23)
    +------------- min (0 - 59)

or a six-part format that allows for seconds:

    * * * * * *
    - - - - - -
    | | | | | |
    | | | | | +--- day of week (0 - 6) (Sunday=0)
    | | | | +----- month (1 - 12)
    | | | +------- day of month (1 - 31)
    | | +--------- hour (0 - 23)
    | +----------- min (0 - 59)
    +------------- sec (0 - 59)

Star (`*`) in the value field above means all legal values as in parentheses for
that column. The value column can have a `*` or a list of elements separated by
commas. An element is either a number in the ranges shown above or two numbers in
the range separated by a hyphen (meaning an inclusive range). For more, see
[CrontabExpression].

The default format parsed by `CrontabSchedule.Parse` is the five-part cron
format. In order to use the six-part format that includes seconds, pass a
`CrontabSchedule.ParseOptions` to `Parse` with `IncludingSeconds` set to
`true`. For example:

```csharp
var s = CrontabSchedule.Parse("0,30 * * * * *",
                              new CrontabSchedule.ParseOptions
                              {
                                  IncludingSeconds = true
                              });
```

Below is an example in [IronPython][ipy] of how to use `CrontabSchedule` class
from NCrontab to generate occurrences of the schedule `0 12 * */2 Mon`
(meaning, *12:00 PM on Monday of every other month, starting with January*)
throughout the year 2000:

    IronPython 1.1 (1.1) on .NET 2.0.50727.1434
    Copyright (c) Microsoft Corporation. All rights reserved.
    >>> import clr
    >>> clr.AddReferenceToFileAndPath(r'C:\NCrontab\bin\Release\NCrontab.dll')
    >>> from System import DateTime
    >>> from NCrontab import CrontabSchedule
    >>> s = CrontabSchedule.Parse('0 12 * */2 Mon')
    >>> start = DateTime(2000, 1, 1)
    >>> end = start.AddYears(1)
    >>> occurrences = s.GetNextOccurrences(start, end)
    >>> print '\n'.join([t.ToString('ddd, dd MMM yyyy HH:mm') for t in occurrences])
    Mon, 03 Jan 2000 12:00
    Mon, 10 Jan 2000 12:00
    Mon, 17 Jan 2000 12:00
    Mon, 24 Jan 2000 12:00
    Mon, 31 Jan 2000 12:00
    Mon, 06 Mar 2000 12:00
    Mon, 13 Mar 2000 12:00
    Mon, 20 Mar 2000 12:00
    Mon, 27 Mar 2000 12:00
    Mon, 01 May 2000 12:00
    Mon, 08 May 2000 12:00
    Mon, 15 May 2000 12:00
    Mon, 22 May 2000 12:00
    Mon, 29 May 2000 12:00
    Mon, 03 Jul 2000 12:00
    Mon, 10 Jul 2000 12:00
    Mon, 17 Jul 2000 12:00
    Mon, 24 Jul 2000 12:00
    Mon, 31 Jul 2000 12:00
    Mon, 04 Sep 2000 12:00
    Mon, 11 Sep 2000 12:00
    Mon, 18 Sep 2000 12:00
    Mon, 25 Sep 2000 12:00
    Mon, 06 Nov 2000 12:00
    Mon, 13 Nov 2000 12:00
    Mon, 20 Nov 2000 12:00
    Mon, 27 Nov 2000 12:00

Below is the same example in [F#][f#] Interactive (`fsi.exe`):

    Microsoft (R) F# 2.0 Interactive build 4.0.40219.1
    Copyright (c) Microsoft Corporation. All Rights Reserved.

    For help type #help;;

    > #r "NCrontab.dll"
    -
    - open NCrontab
    - open System
    -
    - let schedule = CrontabSchedule.Parse("0 12 * */2 Mon")
    - let startDate = DateTime(2000, 1, 1)
    - let endDate = startDate.AddYears(1)
    -
    - let occurrences = schedule.GetNextOccurrences(startDate, endDate)
    - occurrences |> Seq.map (fun t -> t.ToString("ddd, dd MMM yyy HH:mm"))
    -             |> String.concat "\n"
    -             |> printfn "%s";;

    --> Referenced 'C:\NCrontab\bin\Release\NCrontab.dll'

    Mon, 03 Jan 2000 12:00
    Mon, 10 Jan 2000 12:00
    Mon, 17 Jan 2000 12:00
    Mon, 24 Jan 2000 12:00
    Mon, 31 Jan 2000 12:00
    Mon, 06 Mar 2000 12:00
    Mon, 13 Mar 2000 12:00
    Mon, 20 Mar 2000 12:00
    Mon, 27 Mar 2000 12:00
    Mon, 01 May 2000 12:00
    Mon, 08 May 2000 12:00
    Mon, 15 May 2000 12:00
    Mon, 22 May 2000 12:00
    Mon, 29 May 2000 12:00
    Mon, 03 Jul 2000 12:00
    Mon, 10 Jul 2000 12:00
    Mon, 17 Jul 2000 12:00
    Mon, 24 Jul 2000 12:00
    Mon, 31 Jul 2000 12:00
    Mon, 04 Sep 2000 12:00
    Mon, 11 Sep 2000 12:00
    Mon, 18 Sep 2000 12:00
    Mon, 25 Sep 2000 12:00
    Mon, 06 Nov 2000 12:00
    Mon, 13 Nov 2000 12:00
    Mon, 20 Nov 2000 12:00
    Mon, 27 Nov 2000 12:00

Below is the same example in C# Interactive (`csi.exe`):

    Microsoft (R) Visual C# Interactive Compiler version 1.2.0.60317
    Copyright (C) Microsoft Corporation. All rights reserved.

    Type "#help" for more information.
    > #r "NCrontab.dll"
    > using NCrontab;
    > var s = CrontabSchedule.Parse("0 12 * */2 Mon");
    > var start = new DateTime(2000, 1, 1);
    > var end = start.AddYears(1);
    > var occurrences = s.GetNextOccurrences(start, end);
    > Console.WriteLine(string.Join(Environment.NewLine,
    .     from t in occurrences
    .     select $"{t:ddd, dd MMM yyyy HH:mm}"));
    Mon, 03 Jan 2000 12:00
    Mon, 10 Jan 2000 12:00
    Mon, 17 Jan 2000 12:00
    Mon, 24 Jan 2000 12:00
    Mon, 31 Jan 2000 12:00
    Mon, 06 Mar 2000 12:00
    Mon, 13 Mar 2000 12:00
    Mon, 20 Mar 2000 12:00
    Mon, 27 Mar 2000 12:00
    Mon, 01 May 2000 12:00
    Mon, 08 May 2000 12:00
    Mon, 15 May 2000 12:00
    Mon, 22 May 2000 12:00
    Mon, 29 May 2000 12:00
    Mon, 03 Jul 2000 12:00
    Mon, 10 Jul 2000 12:00
    Mon, 17 Jul 2000 12:00
    Mon, 24 Jul 2000 12:00
    Mon, 31 Jul 2000 12:00
    Mon, 04 Sep 2000 12:00
    Mon, 11 Sep 2000 12:00
    Mon, 18 Sep 2000 12:00
    Mon, 25 Sep 2000 12:00
    Mon, 06 Nov 2000 12:00
    Mon, 13 Nov 2000 12:00
    Mon, 20 Nov 2000 12:00
    Mon, 27 Nov 2000 12:00

Below is the same example in C# using [`dotnet-script`][dotnet-script]:

    > #r "nuget:NCrontab"
    > using NCrontab;
    > var s = CrontabSchedule.Parse("0 12 * */2 Mon");
    > var start = new DateTime(2000, 1, 1);
    > var end = start.AddYears(1);
    > var occurrences = s.GetNextOccurrences(start, end);
    > Console.WriteLine(string.Join(Environment.NewLine,
    *     from t in occurrences
    *     select $"{t:ddd, dd MMM yyyy HH:mm}"));
    Mon, 03 Jan 2000 12:00
    Mon, 10 Jan 2000 12:00
    Mon, 17 Jan 2000 12:00
    Mon, 24 Jan 2000 12:00
    Mon, 31 Jan 2000 12:00
    Mon, 06 Mar 2000 12:00
    Mon, 13 Mar 2000 12:00
    Mon, 20 Mar 2000 12:00
    Mon, 27 Mar 2000 12:00
    Mon, 01 May 2000 12:00
    Mon, 08 May 2000 12:00
    Mon, 15 May 2000 12:00
    Mon, 22 May 2000 12:00
    Mon, 29 May 2000 12:00
    Mon, 03 Jul 2000 12:00
    Mon, 10 Jul 2000 12:00
    Mon, 17 Jul 2000 12:00
    Mon, 24 Jul 2000 12:00
    Mon, 31 Jul 2000 12:00
    Mon, 04 Sept 2000 12:00
    Mon, 11 Sept 2000 12:00
    Mon, 18 Sept 2000 12:00
    Mon, 25 Sept 2000 12:00
    Mon, 06 Nov 2000 12:00
    Mon, 13 Nov 2000 12:00
    Mon, 20 Nov 2000 12:00
    Mon, 27 Nov 2000 12:00

Some complex schedules cannot be expressed in a single crontab expression so
NCrontab can produce _distinct occurrences_ given a sequence of
`CrontabSchedule` instances. In the C# example below, two schedules are merged
to produce a single set of occurrences over a week. The first schedule occurs
every 6 hours on weekdays while the second occurs every 12 hours on weekends.

    Microsoft (R) Visual C# Interactive Compiler version 1.2.0.60317
    Copyright (C) Microsoft Corporation. All rights reserved.

    Type "#help" for more information.
    > using NCrontab;
    > var s1 = CrontabSchedule.Parse("0 */6 * * Mon-Fri");
    > var s2 = CrontabSchedule.Parse("0 */12 * * Sat,Sun");
    > var s = new[] { s1, s2 };
    > var start = new DateTime(2000, 1, 1);
    > var end = start.AddDays(7);
    > var occurrences = s.GetNextOccurrences(start, end);
    > // `Sat, 01 Jan 2000 10:00` won't appear because `start` is exclusive
    > Console.WriteLine(string.Join(Environment.NewLine,
    .     from t in occurrences
    .     select $"{t:ddd, dd MMM yyyy HH:mm}"));
    Sat, 01 Jan 2000 12:00
    Sun, 02 Jan 2000 00:00
    Sun, 02 Jan 2000 12:00
    Mon, 03 Jan 2000 00:00
    Mon, 03 Jan 2000 06:00
    Mon, 03 Jan 2000 12:00
    Mon, 03 Jan 2000 18:00
    Tue, 04 Jan 2000 00:00
    Tue, 04 Jan 2000 06:00
    Tue, 04 Jan 2000 12:00
    Tue, 04 Jan 2000 18:00
    Wed, 05 Jan 2000 00:00
    Wed, 05 Jan 2000 06:00
    Wed, 05 Jan 2000 12:00
    Wed, 05 Jan 2000 18:00
    Thu, 06 Jan 2000 00:00
    Thu, 06 Jan 2000 06:00
    Thu, 06 Jan 2000 12:00
    Thu, 06 Jan 2000 18:00
    Fri, 07 Jan 2000 00:00
    Fri, 07 Jan 2000 06:00
    Fri, 07 Jan 2000 12:00
    Fri, 07 Jan 2000 18:00

If one or more schedules produce the same occurrence then only one of them
if returned.

## Merging Schedules

NCrontab can merge the timeline of one or more schedules. This can sometimes
come handy when it's impossible to express a schedule with a single crontab
expression like _every 6 hours from 9 AM to 5 PM, on weekdays, but at noon on
weekends_. By breaking it up into two schedules:

- `0 12 * * Sat-Sun`: at noon on weekends
- `0 9-17/6 * * Mon-Fri`: every 6 hours from 9 AM to 5 PM on weekdays

you can merge them to produce a single timeline:

```csharp
using System;
using NCrontab;

var start = new DateTime(2000, 1, 1);
var end = start.AddYears(1);
var schedules = new[]
{
    CrontabSchedule.Parse("0 12 * * Sat-Sun"),
    CrontabSchedule.Parse("0 9-17/6 * * Mon-Fri")
};
var occurrences = schedules.GetNextOccurrences(start, end);
Console.WriteLine(string.Join(Environment.NewLine,
                              from t in occurrences
                              select $"{t:ddd, dd MMM yyyy HH:mm}"));
```

The output from a run will:

    Sat, 01 Jan 2000 12:00
    Sun, 02 Jan 2000 12:00
    Mon, 03 Jan 2000 09:00
    Mon, 03 Jan 2000 12:00
    Mon, 03 Jan 2000 15:00
    Tue, 04 Jan 2000 09:00
    Tue, 04 Jan 2000 12:00
    Tue, 04 Jan 2000 15:00
    Wed, 05 Jan 2000 09:00
    Wed, 05 Jan 2000 12:00
    Wed, 05 Jan 2000 15:00
    Thu, 06 Jan 2000 09:00
    Thu, 06 Jan 2000 12:00
    Thu, 06 Jan 2000 15:00
    Fri, 07 Jan 2000 09:00
    Fri, 07 Jan 2000 12:00
    Fri, 07 Jan 2000 15:00
    Sat, 08 Jan 2000 12:00
    Sun, 09 Jan 2000 12:00
    ...

If two or more schedules produce the same occurrence then only one of them
is returned.

---

This product includes software developed by the [OpenSymphony Group].


  [CrontabExpression]: https://github.com/atifaziz/NCrontab/wiki/Crontab-Expression
  [ipy]: http://en.wikipedia.org/wiki/IronPython
  [f#]: http://msdn.microsoft.com/en-us/fsharp/cc742182
  [build-badge]: https://img.shields.io/appveyor/ci/raboof/ncrontab/master.svg
  [nuget-badge]: https://img.shields.io/nuget/v/ncrontab.svg
  [nuget-pkg]: https://www.nuget.org/packages/ncrontab
  [builds]: https://ci.appveyor.com/project/raboof/ncrontab
  [netstd]: https://docs.microsoft.com/en-us/dotnet/articles/standard/library
  [dotnet-script]: https://github.com/dotnet-script/dotnet-script
  [OpenSymphony Group]: http://www.opensymphony.com/
