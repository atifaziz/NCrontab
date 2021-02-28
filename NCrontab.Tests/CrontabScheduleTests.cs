#region License and Terms
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
// Portions Copyright (c) 2001 The OpenSymphony Group. All rights reserved.
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
    using System;
    using System.Globalization;
    using System.Linq;
    using NUnit.Framework;
    using ParseOptions = CrontabSchedule.ParseOptions;

    [TestFixture]
    public sealed class CrontabScheduleTests
    {
        const string TimeFormat = "dd/MM/yyyy HH:mm:ss";

        static readonly string[] TimeFormats =
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "dd/MM/yyyy HH:mm:ss"
        };

        [Test]
        public void CannotParseNullString()
        {
            var e = Assert.Throws<ArgumentNullException>(() => CrontabSchedule.Parse(null));
            Assert.That(e.ParamName, Is.EqualTo("expression"));
        }

        [Test]
        public void CannotParseEmptyString()
        {
            Assert.Throws<CrontabException>(() => CrontabSchedule.Parse(string.Empty));
        }

        [Test]
        public void TryParseNullString() =>
            Assert.That(CrontabSchedule.TryParse(null), Is.Null);

        [Test]
        public void TryParseEmptyString() =>
            Assert.That(CrontabSchedule.TryParse(string.Empty), Is.Null);

        [Test]
        public void AllTimeString()
        {
            Assert.AreEqual("* * * * *", CrontabSchedule.Parse("* * * * *").ToString());
        }

        [Test]
        public void SixPartAllTimeString()
        {
            Assert.AreEqual("* * * * * *", CrontabSchedule.Parse("* * * * * *", new ParseOptions { IncludingSeconds = true }).ToString());
        }

        [Test]
        public void CannotParseWhenSecondsRequired()
        {
            Assert.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * *", new ParseOptions { IncludingSeconds = true }));
        }

        [TestCase("* 1-3 * * *"            , "* 1-2,3 * * *"                   , false)]
        [TestCase("* * * 1,3,5,7,9,11 *"   , "* * * */2 *"                     , false)]
        [TestCase("10,25,40 * * * *"       , "10-40/15 * * * *"                , false)]
        [TestCase("* * * 1,3,8 1-2,5"      , "* * * Mar,Jan,Aug Fri,Mon-Tue"   , false)]
        [TestCase("1 * 1-3 * * *"          , "1 * 1-2,3 * * *"                 , true )]
        [TestCase("22 * * * 1,3,5,7,9,11 *", "22 * * * */2 *"                  , true )]
        [TestCase("33 10,25,40 * * * *"    , "33 10-40/15 * * * *"             , true )]
        [TestCase("55 * * * 1,3,8 1-2,5"   , "55 * * * Mar,Jan,Aug Fri,Mon-Tue", true )]
        public void Formatting(string format, string expression, bool includingSeconds)
        {
            var options = new ParseOptions { IncludingSeconds = includingSeconds };
            Assert.AreEqual(format, CrontabSchedule.Parse(expression, options).ToString());
        }

        /// <summary>
        /// Tests to see if the cron class can calculate the previous matching
        /// time correctly in various circumstances.
        /// </summary>

        [TestCase("01/01/2003 00:00:00", "* * * * *", "01/01/2003 00:01:00", false)]
        [TestCase("01/01/2003 00:01:00", "* * * * *", "01/01/2003 00:02:00", false)]
        [TestCase("01/01/2003 00:02:00", "* * * * *", "01/01/2003 00:03:00", false)]
        [TestCase("01/01/2003 00:59:00", "* * * * *", "01/01/2003 01:00:00", false)]
        [TestCase("01/01/2003 01:59:00", "* * * * *", "01/01/2003 02:00:00", false)]
        [TestCase("01/01/2003 23:59:00", "* * * * *", "02/01/2003 00:00:00", false)]
        [TestCase("31/12/2003 23:59:00", "* * * * *", "01/01/2004 00:00:00", false)]

        [TestCase("28/02/2003 23:59:00", "* * * * *", "01/03/2003 00:00:00", false)]
        [TestCase("28/02/2004 23:59:00", "* * * * *", "29/02/2004 00:00:00", false)]

        // Second tests

        [TestCase("01/01/2003 00:00:00", "45 * * * * *", "01/01/2003 00:00:45"         , true)]

        [TestCase("01/01/2003 00:00:00", "45-47,48,49 * * * * *", "01/01/2003 00:00:45", true)]
        [TestCase("01/01/2003 00:00:45", "45-47,48,49 * * * * *", "01/01/2003 00:00:46", true)]
        [TestCase("01/01/2003 00:00:46", "45-47,48,49 * * * * *", "01/01/2003 00:00:47", true)]
        [TestCase("01/01/2003 00:00:47", "45-47,48,49 * * * * *", "01/01/2003 00:00:48", true)]
        [TestCase("01/01/2003 00:00:48", "45-47,48,49 * * * * *", "01/01/2003 00:00:49", true)]
        [TestCase("01/01/2003 00:00:49", "45-47,48,49 * * * * *", "01/01/2003 00:01:45", true)]

        [TestCase("01/01/2003 00:00:00", "2/5 * * * * *", "01/01/2003 00:00:02"        , true)]
        [TestCase("01/01/2003 00:00:02", "2/5 * * * * *", "01/01/2003 00:00:07"        , true)]
        [TestCase("01/01/2003 00:00:50", "2/5 * * * * *", "01/01/2003 00:00:52"        , true)]
        [TestCase("01/01/2003 00:00:52", "2/5 * * * * *", "01/01/2003 00:00:57"        , true)]
        [TestCase("01/01/2003 00:00:57", "2/5 * * * * *", "01/01/2003 00:01:02"        , true)]

        // See: https://github.com/atifaziz/NCrontab/issues/90
        [TestCase("24/02/2021 09:50:35", "* 0-1 10 * * *", "24/02/2021 10:00:00"       , true)]

        // Minute tests

        [TestCase("01/01/2003 00:00:00", "45 * * * *", "01/01/2003 00:45:00", false)]

        [TestCase("01/01/2003 00:00:00", "45-47,48,49 * * * *", "01/01/2003 00:45:00", false)]
        [TestCase("01/01/2003 00:45:00", "45-47,48,49 * * * *", "01/01/2003 00:46:00", false)]
        [TestCase("01/01/2003 00:46:00", "45-47,48,49 * * * *", "01/01/2003 00:47:00", false)]
        [TestCase("01/01/2003 00:47:00", "45-47,48,49 * * * *", "01/01/2003 00:48:00", false)]
        [TestCase("01/01/2003 00:48:00", "45-47,48,49 * * * *", "01/01/2003 00:49:00", false)]
        [TestCase("01/01/2003 00:49:00", "45-47,48,49 * * * *", "01/01/2003 01:45:00", false)]

        [TestCase("01/01/2003 00:00:00", "2/5 * * * *", "01/01/2003 00:02:00", false)]
        [TestCase("01/01/2003 00:02:00", "2/5 * * * *", "01/01/2003 00:07:00", false)]
        [TestCase("01/01/2003 00:50:00", "2/5 * * * *", "01/01/2003 00:52:00", false)]
        [TestCase("01/01/2003 00:52:00", "2/5 * * * *", "01/01/2003 00:57:00", false)]
        [TestCase("01/01/2003 00:57:00", "2/5 * * * *", "01/01/2003 01:02:00", false)]

        // See: https://github.com/atifaziz/NCrontab/issues/90
        [TestCase("24/02/2021 09:50:35", "* * 10 * * *", "24/02/2021 10:00:00", true)]
        [TestCase("24/02/2021 09:50:35", "* 55 * * * *", "24/02/2021 09:55:00", true)]

        [TestCase("01/01/2003 00:00:30", "3 45 * * * *", "01/01/2003 00:45:03", true)]

        [TestCase("01/01/2003 00:00:30", "6 45-47,48,49 * * * *", "01/01/2003 00:45:06", true)]
        [TestCase("01/01/2003 00:45:30", "6 45-47,48,49 * * * *", "01/01/2003 00:46:06", true)]
        [TestCase("01/01/2003 00:46:30", "6 45-47,48,49 * * * *", "01/01/2003 00:47:06", true)]
        [TestCase("01/01/2003 00:47:30", "6 45-47,48,49 * * * *", "01/01/2003 00:48:06", true)]
        [TestCase("01/01/2003 00:48:30", "6 45-47,48,49 * * * *", "01/01/2003 00:49:06", true)]
        [TestCase("01/01/2003 00:49:30", "6 45-47,48,49 * * * *", "01/01/2003 01:45:06", true)]

        [TestCase("01/01/2003 00:00:30", "9 2/5 * * * *", "01/01/2003 00:02:09", true)]
        [TestCase("01/01/2003 00:02:30", "9 2/5 * * * *", "01/01/2003 00:07:09", true)]
        [TestCase("01/01/2003 00:50:30", "9 2/5 * * * *", "01/01/2003 00:52:09", true)]
        [TestCase("01/01/2003 00:52:30", "9 2/5 * * * *", "01/01/2003 00:57:09", true)]
        [TestCase("01/01/2003 00:57:30", "9 2/5 * * * *", "01/01/2003 01:02:09", true)]

        // Hour tests

        [TestCase("20/12/2003 10:00:00", " * 3/4 * * *", "20/12/2003 11:00:00", false)]
        [TestCase("20/12/2003 00:30:00", " * 3   * * *", "20/12/2003 03:00:00", false)]
        [TestCase("20/12/2003 01:45:00", "30 3   * * *", "20/12/2003 03:30:00", false)]

        // Day of month tests

        [TestCase("07/01/2003 00:00:00", "30  *  1 * *", "01/02/2003 00:30:00", false)]
        [TestCase("01/02/2003 00:30:00", "30  *  1 * *", "01/02/2003 01:30:00", false)]

        [TestCase("01/01/2003 00:00:00", "10  * 22    * *", "22/01/2003 00:10:00", false)]
        [TestCase("01/01/2003 00:00:00", "30 23 19    * *", "19/01/2003 23:30:00", false)]
        [TestCase("01/01/2003 00:00:00", "30 23 21    * *", "21/01/2003 23:30:00", false)]
        [TestCase("01/01/2003 00:01:00", " *  * 21    * *", "21/01/2003 00:00:00", false)]
        [TestCase("10/07/2003 00:00:00", " *  * 30,31 * *", "30/07/2003 00:00:00", false)]

        // Test month rollovers for months with 28,29,30 and 31 days

        [TestCase("28/02/2002 23:59:59", "* * * 3 *", "01/03/2002 00:00:00", false)]
        [TestCase("29/02/2004 23:59:59", "* * * 3 *", "01/03/2004 00:00:00", false)]
        [TestCase("31/03/2002 23:59:59", "* * * 4 *", "01/04/2002 00:00:00", false)]
        [TestCase("30/04/2002 23:59:59", "* * * 5 *", "01/05/2002 00:00:00", false)]

        // Test month 30,31 days

        [TestCase("01/01/2000 00:00:00", "0 0 15,30,31 * *", "15/01/2000 00:00:00", false)]
        [TestCase("15/01/2000 00:00:00", "0 0 15,30,31 * *", "30/01/2000 00:00:00", false)]
        [TestCase("30/01/2000 00:00:00", "0 0 15,30,31 * *", "31/01/2000 00:00:00", false)]
        [TestCase("31/01/2000 00:00:00", "0 0 15,30,31 * *", "15/02/2000 00:00:00", false)]

        [TestCase("15/02/2000 00:00:00", "0 0 15,30,31 * *", "15/03/2000 00:00:00", false)]

        [TestCase("15/03/2000 00:00:00", "0 0 15,30,31 * *", "30/03/2000 00:00:00", false)]
        [TestCase("30/03/2000 00:00:00", "0 0 15,30,31 * *", "31/03/2000 00:00:00", false)]
        [TestCase("31/03/2000 00:00:00", "0 0 15,30,31 * *", "15/04/2000 00:00:00", false)]

        [TestCase("15/04/2000 00:00:00", "0 0 15,30,31 * *", "30/04/2000 00:00:00", false)]
        [TestCase("30/04/2000 00:00:00", "0 0 15,30,31 * *", "15/05/2000 00:00:00", false)]

        [TestCase("15/05/2000 00:00:00", "0 0 15,30,31 * *", "30/05/2000 00:00:00", false)]
        [TestCase("30/05/2000 00:00:00", "0 0 15,30,31 * *", "31/05/2000 00:00:00", false)]
        [TestCase("31/05/2000 00:00:00", "0 0 15,30,31 * *", "15/06/2000 00:00:00", false)]

        [TestCase("15/06/2000 00:00:00", "0 0 15,30,31 * *", "30/06/2000 00:00:00", false)]
        [TestCase("30/06/2000 00:00:00", "0 0 15,30,31 * *", "15/07/2000 00:00:00", false)]

        [TestCase("15/07/2000 00:00:00", "0 0 15,30,31 * *", "30/07/2000 00:00:00", false)]
        [TestCase("30/07/2000 00:00:00", "0 0 15,30,31 * *", "31/07/2000 00:00:00", false)]
        [TestCase("31/07/2000 00:00:00", "0 0 15,30,31 * *", "15/08/2000 00:00:00", false)]

        [TestCase("15/08/2000 00:00:00", "0 0 15,30,31 * *", "30/08/2000 00:00:00", false)]
        [TestCase("30/08/2000 00:00:00", "0 0 15,30,31 * *", "31/08/2000 00:00:00", false)]
        [TestCase("31/08/2000 00:00:00", "0 0 15,30,31 * *", "15/09/2000 00:00:00", false)]

        [TestCase("15/09/2000 00:00:00", "0 0 15,30,31 * *", "30/09/2000 00:00:00", false)]
        [TestCase("30/09/2000 00:00:00", "0 0 15,30,31 * *", "15/10/2000 00:00:00", false)]

        [TestCase("15/10/2000 00:00:00", "0 0 15,30,31 * *", "30/10/2000 00:00:00", false)]
        [TestCase("30/10/2000 00:00:00", "0 0 15,30,31 * *", "31/10/2000 00:00:00", false)]
        [TestCase("31/10/2000 00:00:00", "0 0 15,30,31 * *", "15/11/2000 00:00:00", false)]

        [TestCase("15/11/2000 00:00:00", "0 0 15,30,31 * *", "30/11/2000 00:00:00", false)]
        [TestCase("30/11/2000 00:00:00", "0 0 15,30,31 * *", "15/12/2000 00:00:00", false)]

        [TestCase("15/12/2000 00:00:00", "0 0 15,30,31 * *", "30/12/2000 00:00:00", false)]
        [TestCase("30/12/2000 00:00:00", "0 0 15,30,31 * *", "31/12/2000 00:00:00", false)]
        [TestCase("31/12/2000 00:00:00", "0 0 15,30,31 * *", "15/01/2001 00:00:00", false)]

        // Other month tests (including year rollover)

        [TestCase("01/12/2003 05:00:00", "10 * * 6 *", "01/06/2004 00:10:00", false)]
        [TestCase("04/01/2003 00:00:00", " 1 2 3 * *", "03/02/2003 02:01:00", false)]
        [TestCase("01/07/2002 05:00:00", "10 * * February,April-Jun *", "01/02/2003 00:10:00", false)]
        [TestCase("01/01/2003 00:00:00", "0 12 1 6 *", "01/06/2003 12:00:00", false)]
        [TestCase("11/09/1988 14:23:00", "* 12 1 6 *", "01/06/1989 12:00:00", false)]
        [TestCase("11/03/1988 14:23:00", "* 12 1 6 *", "01/06/1988 12:00:00", false)]
        [TestCase("11/03/1988 14:23:00", "* 2,4-8,15 * 6 *", "01/06/1988 02:00:00", false)]
        [TestCase("11/03/1988 14:23:00", "20 * * january,FeB,Mar,april,May,JuNE,July,Augu,SEPT-October,Nov,DECEM *", "11/03/1988 15:20:00", false)]

        // Day of week tests

        [TestCase("26/06/2003 10:00:00", "30 6 * * 0",      "29/06/2003 06:30:00", false)]
        [TestCase("26/06/2003 10:00:00", "30 6 * * sunday", "29/06/2003 06:30:00", false)]
        [TestCase("26/06/2003 10:00:00", "30 6 * * SUNDAY", "29/06/2003 06:30:00", false)]
        [TestCase("19/06/2003 00:00:00", "1 12 * * 2",      "24/06/2003 12:01:00", false)]
        [TestCase("24/06/2003 12:01:00", "1 12 * * 2",      "01/07/2003 12:01:00", false)]

        [TestCase("01/06/2003 14:55:00", "15 18 * * Mon", "02/06/2003 18:15:00", false)]
        [TestCase("02/06/2003 18:15:00", "15 18 * * Mon", "09/06/2003 18:15:00", false)]
        [TestCase("09/06/2003 18:15:00", "15 18 * * Mon", "16/06/2003 18:15:00", false)]
        [TestCase("16/06/2003 18:15:00", "15 18 * * Mon", "23/06/2003 18:15:00", false)]
        [TestCase("23/06/2003 18:15:00", "15 18 * * Mon", "30/06/2003 18:15:00", false)]
        [TestCase("30/06/2003 18:15:00", "15 18 * * Mon", "07/07/2003 18:15:00", false)]

        [TestCase("01/01/2003 00:00:00", "* * * * Mon",   "06/01/2003 00:00:00", false)]
        [TestCase("01/01/2003 12:00:00", "45 16 1 * Mon", "01/09/2003 16:45:00", false)]
        [TestCase("01/09/2003 23:45:00", "45 16 1 * Mon", "01/12/2003 16:45:00", false)]

        // Leap year tests

        [TestCase("01/01/2000 12:00:00", "1 12 29 2 *", "29/02/2000 12:01:00", false)]
        [TestCase("29/02/2000 12:01:00", "1 12 29 2 *", "29/02/2004 12:01:00", false)]
        [TestCase("29/02/2004 12:01:00", "1 12 29 2 *", "29/02/2008 12:01:00", false)]

        // Non-leap year tests

        [TestCase("01/01/2000 12:00:00", "1 12 28 2 *", "28/02/2000 12:01:00", false)]
        [TestCase("28/02/2000 12:01:00", "1 12 28 2 *", "28/02/2001 12:01:00", false)]
        [TestCase("28/02/2001 12:01:00", "1 12 28 2 *", "28/02/2002 12:01:00", false)]
        [TestCase("28/02/2002 12:01:00", "1 12 28 2 *", "28/02/2003 12:01:00", false)]
        [TestCase("28/02/2003 12:01:00", "1 12 28 2 *", "28/02/2004 12:01:00", false)]
        [TestCase("29/02/2004 12:01:00", "1 12 28 2 *", "28/02/2005 12:01:00", false)]

        [TestCase("01/01/2000 12:00:00", "40 14/1 * * *", "01/01/2000 14:40:00", false)]
        [TestCase("01/01/2000 14:40:00", "40 14/1 * * *", "01/01/2000 15:40:00", false)]

        public void Evaluations(string startTimeString, string cronExpression, string nextTimeString, bool includingSeconds)
        {
            CronCall(startTimeString, cronExpression, nextTimeString, new ParseOptions
            {
                IncludingSeconds = includingSeconds
            });
        }

        [TestCase(" *  * * * *  ", "01/01/2003 00:00:00", "01/01/2003 00:00:00"   , false)]
        [TestCase(" *  * * * *  ", "31/12/2002 23:59:59", "01/01/2003 00:00:00"   , false)]
        [TestCase(" *  * * * Mon", "31/12/2002 23:59:59", "01/01/2003 00:00:00"   , false)]
        [TestCase(" *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 00:00:00"   , false)]
        [TestCase(" *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 12:00:00"   , false)]
        [TestCase("30 12 * * Mon", "01/01/2003 00:00:00", "06/01/2003 12:00:00"   , false)]

        [TestCase(" *  *  * * * *  ", "01/01/2003 00:00:00", "01/01/2003 00:00:00", true )]
        [TestCase(" *  *  * * * *  ", "31/12/2002 23:59:59", "01/01/2003 00:00:00", true )]
        [TestCase(" *  *  * * * Mon", "31/12/2002 23:59:59", "01/01/2003 00:00:00", true )]
        [TestCase(" *  *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 00:00:00", true )]
        [TestCase(" *  *  * * * Mon", "01/01/2003 00:00:00", "02/01/2003 12:00:00", true )]
        [TestCase("10 30 12 * * Mon", "01/01/2003 00:00:00", "06/01/2003 12:00:10", true )]

        public void FiniteOccurrences(string cronExpression, string startTimeString, string endTimeString, bool includingSeconds)
        {
            CronFinite(cronExpression, startTimeString, endTimeString, new ParseOptions
            {
                IncludingSeconds = includingSeconds
            });
        }

        //
        // Test to check we don't loop indefinitely looking for a February
        // 31st because no such date would ever exist!
        //

        [Category("Performance")]
#if NETCOREAPP1_0
        [Ignore("Timeout attribute missing from NUnit for .NET Core.")]
#else
        [Timeout(1000)]
#endif
        [TestCase("* * 31 Feb *", false)]
        [TestCase("* * * 31 Feb *", true)]
        public void DontLoopIndefinitely(string expression, bool includingSeconds)
        {
            CronFinite(expression, "01/01/2001 00:00:00", "01/01/2010 00:00:00", new ParseOptions
            {
                IncludingSeconds = includingSeconds
            });
        }

        static void BadField(string expression, bool includingSeconds)
        {
            Assert.Throws<CrontabException>(() => CrontabSchedule.Parse(expression, new ParseOptions
            {
                IncludingSeconds = includingSeconds
            }));
            Assert.That(CrontabSchedule.TryParse(expression, new ParseOptions
            {
                IncludingSeconds = includingSeconds
            }), Is.Null);
        }

        [TestCase("bad * * * * *", false)]
        public void BadSecondsField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("bad * * * *", false)]
        [TestCase("* bad * * * *", true)]
        public void BadMinutesField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* bad * * *", false)]
        [TestCase("* * bad * * *", true)]
        public void BadHoursField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* * bad * *", false)]
        [TestCase("* * * bad * *", true)]
        public void BadDayField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* * * bad *", false)]
        [TestCase("* * * * bad *", true)]
        public void BadMonthField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* * * * mon,bad,wed", false)]
        [TestCase("* * * * * mon,bad,wed", true)]
        public void BadDayOfWeekField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* 1,2,3,456,7,8,9 * * *", false)]
        [TestCase("* * 1,2,3,456,7,8,9 * * *", true)]
        public void OutOfRangeField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* 1,Z,3,4 * * *", false)]
        [TestCase("* * 1,Z,3,4 * * *", true)]
        public void NonNumberValueInNumericOnlyField(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* 1/Z * * *", false)]
        [TestCase("* * 1/Z * * *", true)]
        public void NonNumericFieldInterval(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        [TestCase("* 3-l2 * * *", false)]
        [TestCase("* * 3-l2 * * *", true)]
        public void NonNumericFieldRangeComponent(string expression, bool includingSeconds) =>
            BadField(expression, includingSeconds);

        /// <summary>
        /// Test case for
        /// <a href="https://github.com/atifaziz/NCrontab/issues/21">issue
        /// #21</a> (GetNextOccurrence throws if next occurrence produces
        /// invalid time).
        /// </summary>

        [Test]
        public void GetNextOccurrences_NextOccurrenceInvalidTime_ShouldStopAtLastValidTime()
        {
            var schedule = CrontabSchedule.Parse("0 0 29 Feb Mon");
            var occurrences = schedule.GetNextOccurrences(new DateTime(9988, 1, 1), DateTime.MaxValue);
            Assert.AreEqual(new DateTime(9988, 2, 29), occurrences.Last());
        }

        // Instead of using strings and parsing as date,
        // consider NUnit's TestCaseData:
        // https://github.com/nunit/docs/wiki/TestCaseData

        [TestCase("0 0 29 Feb Mon", "2017-01-01", "2017-12-31", "2017-12-31")]
        [TestCase("0 0 29 Feb Mon", "9000-01-01", "9008-12-31", "9008-02-29")]
        public void GetNextOccurrence(string expression, string startDate, string endDate, string expectedValue)
        {
            var schedule = CrontabSchedule.Parse(expression);
            var start = Time(startDate);
            var end = Time(endDate);
            var expected = Time(expectedValue);

            var occurrence = schedule.GetNextOccurrence(start, end);

            Assert.AreEqual(expected, occurrence);
        }


        static void CronCall(string startTimeString, string cronExpression, string nextTimeString, ParseOptions options)
        {
            var schedule = CrontabSchedule.Parse(cronExpression, options);
            var next = schedule.GetNextOccurrence(Time(startTimeString));

            Assert.AreEqual(nextTimeString, TimeString(next),
                "Occurrence of <{0}> after <{1}>.", cronExpression, startTimeString);
        }

        static void CronFinite(string cronExpression, string startTimeString, string endTimeString, ParseOptions options)
        {
            var schedule = CrontabSchedule.Parse(cronExpression, options);
            var occurrence = schedule.GetNextOccurrence(Time(startTimeString), Time(endTimeString));

            Assert.AreEqual(endTimeString, TimeString(occurrence),
                "Occurrence of <{0}> after <{1}> did not terminate with <{2}>.",
                cronExpression, startTimeString, endTimeString);
        }

        static string TimeString(DateTime time) => time.ToString(TimeFormat, CultureInfo.InvariantCulture);
        static DateTime Time(string str) => DateTime.ParseExact(str, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
}
