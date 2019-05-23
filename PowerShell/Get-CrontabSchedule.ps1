<#
MIT License

Copyright (c) 2019 Atif Aziz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
#>

function Get-CrontabSchedule() {

    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Expression,
        [datetime]$Start,
        [datetime]$End,
        [int]$Count,
        [switch]$NoOccurrences = $false)

    # Assume NCrontab has been loaded before:
    # Add-Type -Path NCrontab.dll

    $options = [NCrontab.CrontabSchedule+ParseOptions]@{
        IncludingSeconds = ($expression -split ' +', 6).Count -gt 5
    }
    $schedule = [NCrontab.CrontabSchedule]::Parse($expression, $options)

    if (!$start) {
        $start = Get-Date
    }

    if (!$end) {
        $end = [datetime]::MaxValue
        if (!$count) {
            $count = 20
        }
    }

    if ($noOccurrences) {
        $schedule
    }
    else {
        $occurrences = $schedule.GetNextOccurrences($start - ((New-TimeSpan) - 1), $end)
        if ($count) {
            $occurrences | Select-Object -First $count
        } else {
            $occurrences
        }
    }
}
