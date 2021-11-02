using System;

namespace ImageSorter
{
    public class Time
    {
        public string timestamp { get; set; }
        public string formatted { get; set; }

        public DateTime ToDateTime()
        {
            // Unix timestamp is seconds past epoch
            var unixTimeStamp = Double.Parse(timestamp);
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}