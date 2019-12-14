using System;

namespace SimpleLyricsMaker.ViewModels.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToLyricsTimeString(this TimeSpan time)
        {
            string result = "";

            int hours = time.Hours + (24 * time.Days);
            if (hours > 0)
                result += $"{hours:D2}:";
            
            return result + $"{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds.ToString("D3").Substring(0, 2)}";
        }
    }
}