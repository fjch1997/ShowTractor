using ShowTractor.Properties;
using System;
using System.Text;

namespace ShowTractor.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToDisplayText(this TimeSpan timeSpan)
        {
            var sb = new StringBuilder();
            var seperatorNeeded = false;
            var remainingTime = timeSpan;
            var showHours = true;
            if (remainingTime > TimeSpan.FromDays(365))
            {
                seperatorNeeded = true;
                showHours = false;
                var years = (int)Math.Floor(remainingTime.TotalDays / 365);
                remainingTime -= TimeSpan.FromDays(years * 365);
                if (years == 1)
                    sb.Append(string.Format(Resources.TotalTimeYearFormat, years));
                else
                    sb.Append(string.Format(Resources.TotalTimeYearFormatPlural, years));
            }
            if (remainingTime > TimeSpan.FromDays(30))
            {
                AppendSeperatorIfNeeded();
                var month = (int)Math.Floor(remainingTime.TotalDays / 30);
                remainingTime -= TimeSpan.FromDays(month * 30);
                if (month == 1)
                    sb.Append(string.Format(Resources.TotalTimeMonthFormat, month));
                else
                    sb.Append(string.Format(Resources.TotalTimeMonthFormatPlural, month));
            }
            if (remainingTime > TimeSpan.FromDays(1))
            {
                AppendSeperatorIfNeeded();
                if (remainingTime.Days == 1)
                    sb.Append(string.Format(Resources.TotalTimeDayFormat, remainingTime.Days));
                else
                    sb.Append(string.Format(Resources.TotalTimeDayFormatPlural, remainingTime.Days));
            }
            if (remainingTime.Hours > 0 && showHours)
            {
                AppendSeperatorIfNeeded();
                if (remainingTime.Hours == 1)
                    sb.Append(string.Format(Resources.TotalTimeHourFormat, remainingTime.Hours));
                else
                    sb.Append(string.Format(Resources.TotalTimeHourFormatPlural, remainingTime.Hours));
            }
            return sb.ToString();

            void AppendSeperatorIfNeeded()
            {
                if (seperatorNeeded)
                {
                    sb.Append(Resources.TotalTimeSeperator);
                }
                seperatorNeeded = true;
            }
        }
    }
}
