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

namespace NCrontab.Tests
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using NUnit.Framework;
    using ParseOptions = CrontabSchedule.ParseOptions;

    #endregion
 
    [ TestFixture ]
    public sealed class CrontabScheduleTests
    {
        const string TimeFormat = "dd/MM/yyyy HH:mm:ss";

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotParseNullString()
        {
            CrontabSchedule.Parse(null);
        }

        [ Test, ExpectedException(typeof(CrontabException)) ]
        public void CannotParseEmptyString()
        {
            CrontabSchedule.Parse(string.Empty);
        }

        [ Test ]
        public void AllTimeString()
        {
            Assert.AreEqual("* * * * *", CrontabSchedule.Parse("* * * * *").ToString());
        }

        [Test]
        public void SixPartAllTimeString()
        {
            Assert.AreEqual("* * * * * *", CrontabSchedule.Parse("* * * * * *", new ParseOptions { IncludingSeconds = true }).ToString());
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void CannotParseWhenSecondsRequired()
        {
            CrontabSchedule.Parse("* * * * *", new ParseOptions { IncludingSeconds = true });
        }

        [Test]
        public void Formatting()
        {
            Assert.AreEqual("* 1-3 * * *", CrontabSchedule.Parse("* 1-2,3 * * *").ToString());
            Assert.AreEqual("* * * 1,3,5,7,9,11 *", CrontabSchedule.Parse("* * * */2 *").ToString());
            Assert.AreEqual("10,25,40 * * * *", CrontabSchedule.Parse("10-40/15 * * * *").ToString());
            Assert.AreEqual("* * * 1,3,8 1-2,5", CrontabSchedule.Parse("* * * Mar,Jan,Aug Fri,Mon-Tue").ToString());
            var includingSeconds = new ParseOptions { IncludingSeconds = true };
            Assert.AreEqual("1 * 1-3 * * *", CrontabSchedule.Parse("1 * 1-2,3 * * *", includingSeconds).ToString());
            Assert.AreEqual("22 * * * 1,3,5,7,9,11 *", CrontabSchedule.Parse("22 * * * */2 *", includingSeconds).ToString());
            Assert.AreEqual("33 10,25,40 * * * *", CrontabSchedule.Parse("33 10-40/15 * * * *", includingSeconds).ToString());
            Assert.AreEqual("55 * * * 1,3,8 1-2,5", CrontabSchedule.Parse("55 * * * Mar,Jan,Aug Fri,Mon-Tue", includingSeconds).ToString());
        }

        /// <summary>
        /// Tests to see if the cron class can calculate the previous matching
        /// time correctly in various circumstances.
        /// </summary>

        [ Test ]
        public void Evaluations()
        {
            CronCall("01/01/2003 00:00:00", "* * * * *", "01/01/2003 00:01:00", false);
            CronCall("01/01/2003 00:01:00", "* * * * *", "01/01/2003 00:02:00", false);
            CronCall("01/01/2003 00:02:00", "* * * * *", "01/01/2003 00:03:00", false);
            CronCall("01/01/2003 00:59:00", "* * * * *", "01/01/2003 01:00:00", false);
            CronCall("01/01/2003 01:59:00", "* * * * *", "01/01/2003 02:00:00", false);
            CronCall("01/01/2003 23:59:00", "* * * * *", "02/01/2003 00:00:00", false);
            CronCall("31/12/2003 23:59:00", "* * * * *", "01/01/2004 00:00:00", false);

            CronCall("28/02/2003 23:59:00", "* * * * *", "01/03/2003 00:00:00", false);
            CronCall("28/02/2004 23:59:00", "* * * * *", "29/02/2004 00:00:00", false);

            // Second tests

            var cronCall = CronCall(new ParseOptions { IncludingSeconds = true });

            cronCall("01/01/2003 00:00:00", "45 * * * * *", "01/01/2003 00:00:45", false);

            cronCall("01/01/2003 00:00:00", "45-47,48,49 * * * * *", "01/01/2003 00:00:45", false);
            cronCall("01/01/2003 00:00:45", "45-47,48,49 * * * * *", "01/01/2003 00:00:46", false);
            cronCall("01/01/2003 00:00:46", "45-47,48,49 * * * * *", "01/01/2003 00:00:47", false);
            cronCall("01/01/2003 00:00:47", "45-47,48,49 * * * * *", "01/01/2003 00:00:48", false);
            cronCall("01/01/2003 00:00:48", "45-47,48,49 * * * * *", "01/01/2003 00:00:49", false);
            cronCall("01/01/2003 00:00:49", "45-47,48,49 * * * * *", "01/01/2003 00:01:45", false);

            cronCall("01/01/2003 00:00:00", "2/5 * * * * *", "01/01/2003 00:00:02", false);
            cronCall("01/01/2003 00:00:02", "2/5 * * * * *", "01/01/2003 00:00:07", false);
            cronCall("01/01/2003 00:00:50", "2/5 * * * * *", "01/01/2003 00:00:52", false);
            cronCall("01/01/2003 00:00:52", "2/5 * * * * *", "01/01/2003 00:00:57", false);
            cronCall("01/01/2003 00:00:57", "2/5 * * * * *", "01/01/2003 00:01:02", false);

            // Minute tests

            CronCall("01/01/2003 00:00:00", "45 * * * *", "01/01/2003 00:45:00", false);

            CronCall("01/01/2003 00:00:00", "45-47,48,49 * * * *", "01/01/2003 00:45:00", false);
            CronCall("01/01/2003 00:45:00", "45-47,48,49 * * * *", "01/01/2003 00:46:00", false);
            CronCall("01/01/2003 00:46:00", "45-47,48,49 * * * *", "01/01/2003 00:47:00", false);
            CronCall("01/01/2003 00:47:00", "45-47,48,49 * * * *", "01/01/2003 00:48:00", false);
            CronCall("01/01/2003 00:48:00", "45-47,48,49 * * * *", "01/01/2003 00:49:00", false);
            CronCall("01/01/2003 00:49:00", "45-47,48,49 * * * *", "01/01/2003 01:45:00", false);

            CronCall("01/01/2003 00:00:00", "2/5 * * * *", "01/01/2003 00:02:00", false);
            CronCall("01/01/2003 00:02:00", "2/5 * * * *", "01/01/2003 00:07:00", false);
            CronCall("01/01/2003 00:50:00", "2/5 * * * *", "01/01/2003 00:52:00", false);
            CronCall("01/01/2003 00:52:00", "2/5 * * * *", "01/01/2003 00:57:00", false);
            CronCall("01/01/2003 00:57:00", "2/5 * * * *", "01/01/2003 01:02:00", false);

            cronCall("01/01/2003 00:00:30", "3 45 * * * *", "01/01/2003 00:45:03", false);

            cronCall("01/01/2003 00:00:30", "6 45-47,48,49 * * * *", "01/01/2003 00:45:06", false);
            cronCall("01/01/2003 00:45:30", "6 45-47,48,49 * * * *", "01/01/2003 00:46:06", false);
            cronCall("01/01/2003 00:46:30", "6 45-47,48,49 * * * *", "01/01/2003 00:47:06", false);
            cronCall("01/01/2003 00:47:30", "6 45-47,48,49 * * * *", "01/01/2003 00:48:06", false);
            cronCall("01/01/2003 00:48:30", "6 45-47,48,49 * * * *", "01/01/2003 00:49:06", false);
            cronCall("01/01/2003 00:49:30", "6 45-47,48,49 * * * *", "01/01/2003 01:45:06", false);

            cronCall("01/01/2003 00:00:30", "9 2/5 * * * *", "01/01/2003 00:02:09", false);
            cronCall("01/01/2003 00:02:30", "9 2/5 * * * *", "01/01/2003 00:07:09", false);
            cronCall("01/01/2003 00:50:30", "9 2/5 * * * *", "01/01/2003 00:52:09", false);
            cronCall("01/01/2003 00:52:30", "9 2/5 * * * *", "01/01/2003 00:57:09", false);
            cronCall("01/01/2003 00:57:30", "9 2/5 * * * *", "01/01/2003 01:02:09", false);

            // Hour tests

            CronCall("20/12/2003 10:00:00", " * 3/4 * * *", "20/12/2003 11:00:00", false);
            CronCall("20/12/2003 00:30:00", " * 3   * * *", "20/12/2003 03:00:00", false);
            CronCall("20/12/2003 01:45:00", "30 3   * * *", "20/12/2003 03:30:00", false);

            // Day of month tests

            CronCall("07/01/2003 00:00:00", "30  *  1 * *", "01/02/2003 00:30:00", false);
            CronCall("01/02/2003 00:30:00", "30  *  1 * *", "01/02/2003 01:30:00", false);

            CronCall("01/01/2003 00:00:00", "10  * 22    * *", "22/01/2003 00:10:00", false);
            CronCall("01/01/2003 00:00:00", "30 23 19    * *", "19/01/2003 23:30:00", false);
            CronCall("01/01/2003 00:00:00", "30 23 21    * *", "21/01/2003 23:30:00", false);
            CronCall("01/01/2003 00:01:00", " *  * 21    * *", "21/01/2003 00:00:00", false);
            CronCall("10/07/2003 00:00:00", " *  * 30,31 * *", "30/07/2003 00:00:00", false);

            // Test month rollovers for months with 28,29,30 and 31 days

            CronCall("28/02/2002 23:59:59", "* * * 3 *", "01/03/2002 00:00:00", false);
            CronCall("29/02/2004 23:59:59", "* * * 3 *", "01/03/2004 00:00:00", false);
            CronCall("31/03/2002 23:59:59", "* * * 4 *", "01/04/2002 00:00:00", false);
            CronCall("30/04/2002 23:59:59", "* * * 5 *", "01/05/2002 00:00:00", false);

            // Test month 30,31 days

            CronCall("01/01/2000 00:00:00", "0 0 15,30,31 * *", "15/01/2000 00:00:00", false);
            CronCall("15/01/2000 00:00:00", "0 0 15,30,31 * *", "30/01/2000 00:00:00", false);
            CronCall("30/01/2000 00:00:00", "0 0 15,30,31 * *", "31/01/2000 00:00:00", false);
            CronCall("31/01/2000 00:00:00", "0 0 15,30,31 * *", "15/02/2000 00:00:00", false);

            CronCall("15/02/2000 00:00:00", "0 0 15,30,31 * *", "15/03/2000 00:00:00", false);

            CronCall("15/03/2000 00:00:00", "0 0 15,30,31 * *", "30/03/2000 00:00:00", false);
            CronCall("30/03/2000 00:00:00", "0 0 15,30,31 * *", "31/03/2000 00:00:00", false);
            CronCall("31/03/2000 00:00:00", "0 0 15,30,31 * *", "15/04/2000 00:00:00", false);

            CronCall("15/04/2000 00:00:00", "0 0 15,30,31 * *", "30/04/2000 00:00:00", false);
            CronCall("30/04/2000 00:00:00", "0 0 15,30,31 * *", "15/05/2000 00:00:00", false);

            CronCall("15/05/2000 00:00:00", "0 0 15,30,31 * *", "30/05/2000 00:00:00", false);
            CronCall("30/05/2000 00:00:00", "0 0 15,30,31 * *", "31/05/2000 00:00:00", false);
            CronCall("31/05/2000 00:00:00", "0 0 15,30,31 * *", "15/06/2000 00:00:00", false);

            CronCall("15/06/2000 00:00:00", "0 0 15,30,31 * *", "30/06/2000 00:00:00", false);
            CronCall("30/06/2000 00:00:00", "0 0 15,30,31 * *", "15/07/2000 00:00:00", false);

            CronCall("15/07/2000 00:00:00", "0 0 15,30,31 * *", "30/07/2000 00:00:00", false);
            CronCall("30/07/2000 00:00:00", "0 0 15,30,31 * *", "31/07/2000 00:00:00", false);
            CronCall("31/07/2000 00:00:00", "0 0 15,30,31 * *", "15/08/2000 00:00:00", false);

            CronCall("15/08/2000 00:00:00", "0 0 15,30,31 * *", "30/08/2000 00:00:00", false);
            CronCall("30/08/2000 00:00:00", "0 0 15,30,31 * *", "31/08/2000 00:00:00", false);
            CronCall("31/08/2000 00:00:00", "0 0 15,30,31 * *", "15/09/2000 00:00:00", false);

            CronCall("15/09/2000 00:00:00", "0 0 15,30,31 * *", "30/09/2000 00:00:00", false);
            CronCall("30/09/2000 00:00:00", "0 0 15,30,31 * *", "15/10/2000 00:00:00", false);

            CronCall("15/10/2000 00:00:00", "0 0 15,30,31 * *", "30/10/2000 00:00:00", false);
            CronCall("30/10/2000 00:00:00", "0 0 15,30,31 * *", "31/10/2000 00:00:00", false);
            CronCall("31/10/2000 00:00:00", "0 0 15,30,31 * *", "15/11/2000 00:00:00", false);

            CronCall("15/11/2000 00:00:00", "0 0 15,30,31 * *", "30/11/2000 00:00:00", false);
            CronCall("30/11/2000 00:00:00", "0 0 15,30,31 * *", "15/12/2000 00:00:00", false);

            CronCall("15/12/2000 00:00:00", "0 0 15,30,31 * *", "30/12/2000 00:00:00", false);
            CronCall("30/12/2000 00:00:00", "0 0 15,30,31 * *", "31/12/2000 00:00:00", false);
            CronCall("31/12/2000 00:00:00", "0 0 15,30,31 * *", "15/01/2001 00:00:00", false);

            // Other month tests (including year rollover)

            CronCall("01/12/2003 05:00:00", "10 * * 6 *", "01/06/2004 00:10:00", false);
            CronCall("04/01/2003 00:00:00", " 1 2 3 * *", "03/02/2003 02:01:00", false);
            CronCall("01/07/2002 05:00:00", "10 * * February,April-Jun *", "01/02/2003 00:10:00", false);
            CronCall("01/01/2003 00:00:00", "0 12 1 6 *", "01/06/2003 12:00:00", false);
            CronCall("11/09/1988 14:23:00", "* 12 1 6 *", "01/06/1989 12:00:00", false);
            CronCall("11/03/1988 14:23:00", "* 12 1 6 *", "01/06/1988 12:00:00", false);
            CronCall("11/03/1988 14:23:00", "* 2,4-8,15 * 6 *", "01/06/1988 02:00:00", false);
            CronCall("11/03/1988 14:23:00", "20 * * january,FeB,Mar,april,May,JuNE,July,Augu,SEPT-October,Nov,DECEM *", "11/03/1988 15:20:00", false);

            // Day of week tests

            CronCall("26/06/2003 10:00:00", "30 6 * * 0",      "29/06/2003 06:30:00", false);
            CronCall("26/06/2003 10:00:00", "30 6 * * sunday", "29/06/2003 06:30:00", false);
            CronCall("26/06/2003 10:00:00", "30 6 * * SUNDAY", "29/06/2003 06:30:00", false);
            CronCall("19/06/2003 00:00:00", "1 12 * * 2",      "24/06/2003 12:01:00", false);
            CronCall("24/06/2003 12:01:00", "1 12 * * 2",      "01/07/2003 12:01:00", false);

            CronCall("01/06/2003 14:55:00", "15 18 * * Mon", "02/06/2003 18:15:00", false);
            CronCall("02/06/2003 18:15:00", "15 18 * * Mon", "09/06/2003 18:15:00", false);
            CronCall("09/06/2003 18:15:00", "15 18 * * Mon", "16/06/2003 18:15:00", false);
            CronCall("16/06/2003 18:15:00", "15 18 * * Mon", "23/06/2003 18:15:00", false);
            CronCall("23/06/2003 18:15:00", "15 18 * * Mon", "30/06/2003 18:15:00", false);
            CronCall("30/06/2003 18:15:00", "15 18 * * Mon", "07/07/2003 18:15:00", false);

            CronCall("01/01/2003 00:00:00", "* * * * Mon",   "06/01/2003 00:00:00", false);
            CronCall("01/01/2003 12:00:00", "45 16 1 * Mon", "01/09/2003 16:45:00", false);
            CronCall("01/09/2003 23:45:00", "45 16 1 * Mon", "01/12/2003 16:45:00", false);

            // Leap year tests

            CronCall("01/01/2000 12:00:00", "1 12 29 2 *", "29/02/2000 12:01:00", false);
            CronCall("29/02/2000 12:01:00", "1 12 29 2 *", "29/02/2004 12:01:00", false);
            CronCall("29/02/2004 12:01:00", "1 12 29 2 *", "29/02/2008 12:01:00", false);

            // Non-leap year tests

            CronCall("01/01/2000 12:00:00", "1 12 28 2 *", "28/02/2000 12:01:00", false);
            CronCall("28/02/2000 12:01:00", "1 12 28 2 *", "28/02/2001 12:01:00", false);
            CronCall("28/02/2001 12:01:00", "1 12 28 2 *", "28/02/2002 12:01:00", false);
            CronCall("28/02/2002 12:01:00", "1 12 28 2 *", "28/02/2003 12:01:00", false);
            CronCall("28/02/2003 12:01:00", "1 12 28 2 *", "28/02/2004 12:01:00", false);
            CronCall("29/02/2004 12:01:00", "1 12 28 2 *", "28/02/2005 12:01:00", false);
        }

        [ Test ]
        public void FiniteOccurrences()
        {
            CronFinite(" *  * * * *  ", "01/01/2003 00:00:00", "01/01/2003 00:00:00");
            CronFinite(" *  * * * *  ", "31/12/2002 23:59:59", "01/01/2003 00:00:00");
            CronFinite(" *  * * * Mon", "31/12/2002 23:59:59", "01/01/2003 00:00:00");
            CronFinite(" *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 00:00:00");
            CronFinite(" *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 12:00:00");
            CronFinite("30 12 * * Mon", "01/01/2003 00:00:00", "06/01/2003 12:00:00");

            var cronFinite = CronFinite(new ParseOptions { IncludingSeconds = true });
            cronFinite(" *  *  * * * *  ", "01/01/2003 00:00:00", "01/01/2003 00:00:00");
            cronFinite(" *  *  * * * *  ", "31/12/2002 23:59:59", "01/01/2003 00:00:00");
            cronFinite(" *  *  * * * Mon", "31/12/2002 23:59:59", "01/01/2003 00:00:00");
            cronFinite(" *  *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 00:00:00");
            cronFinite(" *  *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 12:00:00");
            cronFinite("10 30 12 * * Mon", "01/01/2003 00:00:00", "06/01/2003 12:00:10");
        }

        [ Test, Category("Performance") ]
        public void DontLoopIndefinitely()
        {
            //
            // Test to check we don't loop indefinitely looking for a February
            // 31st because no such date would ever exist!
            //

            TimeCron(TimeSpan.FromSeconds(1), () =>
                CronFinite("* * 31 Feb *", "01/01/2001 00:00:00", "01/01/2010 00:00:00"));
            TimeCron(TimeSpan.FromSeconds(1), () =>
                CronFinite(new ParseOptions { IncludingSeconds = true })("* * * 31 Feb *", "01/01/2001 00:00:00", "01/01/2010 00:00:00"));
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void BadSecondsField()
        {
            CrontabSchedule.Parse("bad * * * * *");
        }

        [ Test, ExpectedException(typeof(CrontabException)) ]
        public void BadMinutesField()
        {
            CrontabSchedule.Parse("bad * * * *");
            CrontabSchedule.Parse("* bad * * * *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void BadHoursField()
        {
            CrontabSchedule.Parse("* bad * * *");
            CrontabSchedule.Parse("* * bad * * *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void BadDayField()
        {
            CrontabSchedule.Parse("* * bad * *");
            CrontabSchedule.Parse("* * * bad * *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void BadMonthField()
        {
            CrontabSchedule.Parse("* * * bad *");
            CrontabSchedule.Parse("* * * * bad *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void BadDayOfWeekField()
        {
            CrontabSchedule.Parse("* * * * mon,bad,wed");
            CrontabSchedule.Parse("* * * * * mon,bad,wed");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void OutOfRangeField()
        {
            CrontabSchedule.Parse("* 1,2,3,456,7,8,9 * * *");
            CrontabSchedule.Parse("* * 1,2,3,456,7,8,9 * * *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void NonNumberValueInNumericOnlyField()
        {
            CrontabSchedule.Parse("* 1,Z,3,4 * * *");
            CrontabSchedule.Parse("* * 1,Z,3,4 * * *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void NonNumericFieldInterval()
        {
            CrontabSchedule.Parse("* 1/Z * * *");
            CrontabSchedule.Parse("* * 1/Z * * *");
        }

        [Test, ExpectedException(typeof(CrontabException))]
        public void NonNumericFieldRangeComponent()
        {
            CrontabSchedule.Parse("* 3-l2 * * *");
            CrontabSchedule.Parse("* * 3-l2 * * *");
        }

        static void TimeCron(TimeSpan limit, ThreadStart test)
        {
            Debug.Assert(test != null);

            Exception e = null;

            var worker = new Thread(() => { try { test(); } catch (Exception ee) { e = ee; } });

            worker.Start();

            if (worker.Join(!Debugger.IsAttached ? (int) limit.TotalMilliseconds : Timeout.Infinite))
            {
                if (e != null)
                    throw new Exception(e.Message, e);

                return;
            }

            worker.Abort();

            Assert.Fail("The test did not complete in the allocated time ({0}). " +
                        "Check there is not an infinite loop somewhere.", limit);
        }

        delegate void CronCallHandler(string startTimeString, string cronExpression, string nextTimeString, bool expectException);

        static void CronCall(string startTimeString, string cronExpression, string nextTimeString, bool expectException)
        {
            CronCall(null)(startTimeString, cronExpression, nextTimeString, expectException);
        }

        static CronCallHandler CronCall(ParseOptions options)
        {
            return (startTimeString, cronExpression, nextTimeString, expectException) =>
            {
                var start = Time(startTimeString);

                try 
                {
                    var schedule = CrontabSchedule.Parse(cronExpression, options);

                    if (expectException) 
                        Assert.Fail("The expression <{0}> cannot be valid.", cronExpression);

                    var next = schedule.GetNextOccurrence(start);

                    Assert.AreEqual(nextTimeString, TimeString(next),
                        "Occurrence of <{0}> after <{1}>.", cronExpression, startTimeString);
                } 
                catch (CrontabException e) 
                {
                    if (!expectException) 
                        Assert.Fail("Unexpected ParseException while parsing <{0}>: {1}", cronExpression, e.ToString());
                }                      
            };
        }

        static void CronFinite(string cronExpression, string startTimeString, string endTimeString)
        {
            CronFinite(null)(cronExpression, startTimeString, endTimeString);
        }

        delegate void CronFiniteHandler(string cronExpression, string startTimeString, string endTimeString);

        static CronFiniteHandler CronFinite(ParseOptions options)
        {
            return (cronExpression, startTimeString, endTimeString) =>
            {
                var schedule = CrontabSchedule.Parse(cronExpression, options);
                var occurrence = schedule.GetNextOccurrence(Time(startTimeString), Time(endTimeString));

                Assert.AreEqual(endTimeString, TimeString(occurrence),
                    "Occurrence of <{0}> after <{1}> did not terminate with <{2}>.",
                    cronExpression, startTimeString, endTimeString);
            };
        }

        static string TimeString(DateTime time)
        {
            return time.ToString(TimeFormat, CultureInfo.InvariantCulture);
        }

        static DateTime Time(string str)
        {
            return DateTime.ParseExact(str, TimeFormat, CultureInfo.InvariantCulture);
        }
    }
}