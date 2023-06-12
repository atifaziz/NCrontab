using System;

namespace NCrontab.Utils
{
    /// <summary>
    /// Used when heavy manipulation of the individual elements of a DateTime are needed.  These elements may even
    /// represent an invalid DateTime during or after manipulation. Resolution is only down to the second.
    /// </summary>
    internal sealed class DateTimeComponents
    {
        public DateTimeComponents(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = dateTime.Month;
            Day = dateTime.Day;
            Hour = dateTime.Hour;
            Minute = dateTime.Minute;
            Second = dateTime.Second;
        }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
    }
}
