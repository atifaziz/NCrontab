# NCrontab: Crontab for .NET

NCrontab is a library written in C# 3.0 that provides the following facilities:

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

Star (`*`) in the value field above means all legal values as in parentheses for
that column. The value column can have a `*` or a list of elements separated by
commas. An element is either a number in the ranges shown above or two numbers in
the range separated by a hyphen (meaning an inclusive range). For more, see
[CrontabExpression](https://code.google.com/p/ncrontab/wiki/CrontabExpression).

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
    >>> print '\n'.join([t.ToString('ddd, dd MMM yyyy hh:mm') for t in occurrences])
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
    - occurrences |> Seq.map (fun t -> t.ToString("ddd, dd MMM yyy hh:mm"))
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

    val schedule : NCrontab.CrontabSchedule = 0 12 * 1,3,5,7,9,11 1
    val startDate : System.DateTime = 1/1/2000 12:00:00 AM
    val endDate : System.DateTime = 1/1/2001 12:00:00 AM
    val occurrences : seq<System.DateTime>


  [ipy]: http://en.wikipedia.org/wiki/IronPython
  [f#]: http://msdn.microsoft.com/en-us/fsharp/cc742182
