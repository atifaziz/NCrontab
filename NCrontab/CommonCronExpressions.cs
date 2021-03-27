using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCrontab
{
    /// <summary>
    /// Common cron expressions
    /// </summary>
    public class CronExpressions
    {
        public const string Daily = "0 0 * * *";
        public const string Hourly = "0 * * * *";
        public const string Weekly = "0 0 * * 0";
        public const string Monthly = "0 0 1 * *";
    }
}
