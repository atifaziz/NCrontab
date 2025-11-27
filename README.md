# NCrontab: Crontab for .NET

[![Build Status][build-badge]][builds]
[![NuGet][nuget-badge]][nuget-pkg]

NCrontab is a library targeting [.NET Standard Library][netstd] 1.0 that
facilitates:

* Parsing of crontab expressions
* Formatting of crontab expressions
* Calculation of occurrences of time based on a crontab schedule

It does _not_ provide any scheduler or is not a scheduling facility like cron from
Unix platforms. What it provides is parsing, formatting and an algorithm to
produce occurrences of time based on a give schedule expressed in the crontab
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

Star (`*`) in a value field above means all legal values as in parentheses for
that column. The value field can therefore have a `*` or a list of elements
separated by commas. An element is either a number in the ranges shown above or
two numbers in the range separated by a hyphen (meaning an inclusive range). For
more, see [CrontabExpression].

Example schedules:

| Expression        | Description                  |
| ----------------- | ---------------------------- |
| `* * * * *`       | Every minute                 |
| `0 * * * *`       | Every hour                   |
| `0 0 * * *`       | Every day at midnight        |
| `0 9 * * Mon-Fri` | Weekdays at 9 AM             |
| `0 0 1 * *`       | First day of every month     |
| `0 */6 * * *`     | Every 6 hours                |
| `0 9-17 * * *`    | Every hour from 9 AM to 5 PM |

## Installation

Add the package to your .NET project with:

    dotnet add package NCrontab

## Quick Start

```csharp
using NCrontab;

var schedule = CrontabSchedule.Parse("0 12 * * Mon"); // Every Monday at noon
var next = schedule.GetNextOccurrence(DateTime.Now);
```

Note that the start time passed to `GetNextOccurrence` is _exclusive_, so if the
start time is exactly on a scheduled occurrence, the next occurrence will be the
one after that. This design allows you to feed the result of `GetNextOccurrence`
back into itself to get subsequent occurrences, for example, as part of a loop.

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

### Getting Occurrences

`CrontabSchedule.GetNextOccurrences` generates occurrences of a schedule. The
following example:

```csharp
using System;
using NCrontab;

var s = CrontabSchedule.Parse("0 12 * */2 Mon"); // Monday of every other month
var start = new DateTime(2000, 1, 1);            // Starting January 1, 2000
var end = start.AddYears(1);                     // Through the year 2000

var occurrences = s.GetNextOccurrences(start, end);

Console.WriteLine(string.Join(Environment.NewLine,
                  from t in occurrences
                  select $"{t:ddd, dd MMM yyyy HH:mm}"));
```

will print occurrences of the schedule `0 12 * */2 Mon` (meaning, 12:00 PM on
Monday of every other month), starting with January 2000 and throughout that
year:

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

### Merging Schedules

NCrontab can merge the _distinct_ timeline of one or more schedules. This can
sometimes come handy when it's impossible to express a schedule with a single
crontab expression like _every 6 hours from 9 AM to 5 PM, on weekdays, but at
noon on weekends_. By breaking it up into two schedules:

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
  [build-badge]: https://img.shields.io/appveyor/ci/raboof/ncrontab/master.svg
  [nuget-badge]: https://img.shields.io/nuget/v/ncrontab.svg
  [nuget-pkg]: https://www.nuget.org/packages/ncrontab
  [builds]: https://ci.appveyor.com/project/raboof/ncrontab
  [netstd]: https://docs.microsoft.com/en-us/dotnet/articles/standard/library
  [OpenSymphony Group]: http://www.opensymphony.com/
