#region License, Terms and Author(s)
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

namespace NCrontabViewer
{
    #region Imports

    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;
    using NCrontab;

    #endregion

    public partial class MainForm : Form
    {
        private DateTime _lastChangeTime;
        private bool _dirty;
        private CrontabSchedule _crontab;
        private DateTime _startTime;
        private int _totalOccurrenceCount;

        public MainForm()
        {
            InitializeComponent();
        }

        private void CronBox_Changed(object sender, EventArgs args)
        {
            _lastChangeTime = DateTime.Now;
            _dirty = true;
            _crontab = null;
        }

        private void Timer_Tick(object sender, EventArgs args)
        {
            var changeLapse = DateTime.Now - _lastChangeTime;

            if (!_dirty || changeLapse <= TimeSpan.FromMilliseconds(500)) 
                return;

            _dirty = false;
            DoCrontabbing();
        }

        private void DoCrontabbing()
        {
            _resultBox.Clear();
            _errorProvider.SetError(_cronBox, null);
            _statusBarPanel.Text = "Ready";
            _moreButton.Enabled = false;

            if (_crontab == null)
            {
                try
                {
                    var expression = _cronBox.Text.Trim();

                    if (expression.Length == 0)
                        return;

                    _crontab = CrontabSchedule.Parse(expression);
                   
                    _totalOccurrenceCount = 0;
                    
                    _startTime = DateTime.ParseExact(_startTimePicker.Text, 
                        _startTimePicker.CustomFormat, CultureInfo.InvariantCulture, 
                        DateTimeStyles.AssumeLocal).AddMinutes(-1);
                }
                catch (CrontabException e)
                {
                    _errorProvider.SetError(_cronBox, e.Message);

                    var traceBuilder = new StringBuilder();

                    Exception traceException = e;
                    Exception lastException;

                    do
                    {
                        traceBuilder.Append(traceException.Message);
                        traceBuilder.Append("\r\n");
                        lastException = traceException;
                        traceException = traceException.GetBaseException();
                    }
                    while (lastException != traceException);

                    _resultBox.Text = traceBuilder.ToString();
                    return;
                }

            }

            var endTime = DateTime.ParseExact(_endTimePicker.Text, 
                _endTimePicker.CustomFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal);

            var sb = new StringBuilder();
            var lastTimeString = "??? ??, ??? ???? ??:??";

            var count = 0;
            const int maxCount = 500;

            foreach (var occurance in _crontab.GetNextOccurrences(_startTime, endTime))
            {
                if (count + 1 > maxCount)
                    break;

                _startTime = occurance;
                _totalOccurrenceCount++;
                count++;

                var timeString = occurance.ToString("ddd dd, MMM yyyy HH:mm");
                sb.Append(timeString);
                sb.Append(" | ");

                Diff(lastTimeString, timeString, 0, 3, sb);
                sb.Append(' ');
                Diff(lastTimeString, timeString, 4, 2, sb);
                sb.Append(", ");
                Diff(lastTimeString, timeString, 8, 3, sb);
                sb.Append(' ');
                Diff(lastTimeString, timeString, 12, 4, sb);
                sb.Append(' ');
                Diff(lastTimeString, timeString, 17, 2, sb);
                sb.Append(':');
                Diff(lastTimeString, timeString, 20, 2, sb);

                lastTimeString = timeString;

                sb.Append("\r\n");
            }

            _moreButton.Enabled = count == maxCount;

            _statusBarPanel.Text = string.Format("Last count = {0}, Total = {1}",
                count.ToString("N0"), _totalOccurrenceCount.ToString("N0"));

            _resultBox.Text = sb.ToString();
            _resultBox.Select(0, 0);
            _resultBox.ScrollToCaret();
        }

        private static void Diff(string oldString, string newString, int index, int length, StringBuilder builder)
        {
            if (string.CompareOrdinal(oldString, index, newString, index, length) == 0)
                builder.Append('-', length);
            else
                builder.Append(newString, index, length);
        }

        private void More_Click(object sender, EventArgs e)
        {
            DoCrontabbing();
        }
    }
}
