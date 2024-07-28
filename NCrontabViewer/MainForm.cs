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

namespace NCrontabViewer
{
    #region Imports

    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using NCrontab;

    #endregion

    public partial class MainForm : Form
    {
        static readonly char[] Separators = { ' ' };

        DateTime lastChangeTime;
        bool dirty;
        CrontabSchedule? crontab;
        bool isSixPart;
        DateTime startTime;
        int totalOccurrenceCount;

        public MainForm()
        {
            InitializeComponent();
        }

        // ReSharper disable once InconsistentNaming
        void CronBox_Changed(object sender, EventArgs args)
        {
            this.lastChangeTime = DateTime.Now;
            this.dirty = true;
            this.isSixPart = false;
            this.crontab = null;
        }

        // ReSharper disable once InconsistentNaming
        void Timer_Tick(object sender, EventArgs args)
        {
            var changeLapse = DateTime.Now - this.lastChangeTime;

            if (!this.dirty || changeLapse <= TimeSpan.FromMilliseconds(500))
                return;

            this.dirty = false;
            DoCrontabbing();
        }

        void DoCrontabbing()
        {
            this.resultBox.Clear();
            this.errorProvider.SetError(this.cronBox, null);
            this.statusBarPanel.Text = "Ready";
            this.moreButton.Enabled = false;

            const string defaultCustomFormat = "dd/MM/yyyy HH:mm";

            if (this.crontab == null)
            {
                try
                {
                    var expression = this.cronBox.Text.Trim();

                    if (expression.Length == 0)
                        return;

                    this.isSixPart = expression.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Length == 6;
                    this.crontab = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = this.isSixPart });

                    this.totalOccurrenceCount = 0;

                    this.startTime = DateTime.ParseExact(this.startTimePicker.Text,
                                                         this.startTimePicker.CustomFormat ?? defaultCustomFormat, CultureInfo.InvariantCulture,
                                                         DateTimeStyles.AssumeLocal) - (this.isSixPart ? TimeSpan.FromSeconds(1): TimeSpan.FromMinutes(1));
                }
                catch (CrontabException e)
                {
                    this.errorProvider.SetError(this.cronBox, e.Message);

                    var traceBuilder = new StringBuilder();

                    Exception traceException = e;
                    Exception lastException;

                    do
                    {
                        _ = traceBuilder.Append(traceException.Message)
                                        .Append("\r\n");
                        lastException = traceException;
                        traceException = traceException.GetBaseException();
                    }
                    while (lastException != traceException);

                    this.resultBox.Text = traceBuilder.ToString();
                    return;
                }

            }

            var endTime = DateTime.ParseExact(this.endTimePicker.Text,
                this.endTimePicker.CustomFormat ?? defaultCustomFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal);

            var sb = new StringBuilder();

            var count = 0;
            const int maxCount = 500;
            var info = DateTimeFormatInfo.CurrentInfo;
            var dayWidth = info.AbbreviatedDayNames.Max(s => s.Length);
            var monthWidth = info.AbbreviatedMonthNames.Max(s => s.Length);
            var timeComponent = this.isSixPart ? "HH:mm:ss" : "HH:mm";
            var timeFormat = $"{{0,-{dayWidth}:ddd}} {{0:dd}}, {{0,-{monthWidth}:MMM}} {{0:yyyy {timeComponent}}}";
            var lastTimeString = new string('?', string.Format(null, timeFormat, DateTime.MinValue).Length);

            foreach (var occurrence in this.crontab.GetNextOccurrences(this.startTime, endTime))
            {
                if (count + 1 > maxCount)
                    break;

                this.startTime = occurrence;
                this.totalOccurrenceCount++;
                count++;

                var timeString = string.Format(null, timeFormat, occurrence);

                _ = sb.Append(timeString)
                      .Append(" | ");

                var index = Diff(lastTimeString, timeString, 0, dayWidth, sb);
                _ = sb.Append(' ');
                index = Diff(lastTimeString, timeString, index + 1, 2, sb);
                _ = sb.Append(", ");
                index = Diff(lastTimeString, timeString, index + 2, monthWidth, sb);
                _ = sb.Append(' ');
                index = Diff(lastTimeString, timeString, index + 1, 4, sb);
                _ = sb.Append(' ');
                index = Diff(lastTimeString, timeString, index + 1, 2, sb);
                _ = sb.Append(':');
                index = Diff(lastTimeString, timeString, index + 1, 2, sb);
                if (this.isSixPart)
                {
                    _ = sb.Append(':');
                    _ = Diff(lastTimeString, timeString, index + 1, 2, sb);
                }

                lastTimeString = timeString;

                _ = sb.Append("\r\n");
            }

            this.moreButton.Enabled = count == maxCount;

            this.statusBarPanel.Text = $"Last count = {count:N0}, Total = {this.totalOccurrenceCount:N0}";

            this.resultBox.Text = sb.ToString();
            this.resultBox.Select(0, 0);
            this.resultBox.ScrollToCaret();
        }

        static int Diff(string oldString, string newString, int index, int length, StringBuilder builder)
        {
#pragma warning disable IDE0045 // Convert to conditional expression (has side-effects)
            if (string.CompareOrdinal(oldString, index, newString, index, length) == 0)
                _ = builder.Append('-', length);
            else
                _ = builder.Append(newString, index, length);
#pragma warning restore IDE0045 // Convert to conditional expression

            return index + length;
        }

        // ReSharper disable once InconsistentNaming
        void More_Click(object sender, EventArgs e) => DoCrontabbing();
    }
}
