using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBKBot.Utils
{
    public class ConvertString
    {
        public string TimeFormat(TimeSpan time)
        {
            if (time.Days > 0)
            {
                return $"{time.Days}:{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else if (time.Hours > 0)
            {
                return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else
            {
                return $"{time.Minutes}:{time.Seconds:D2}";
            }
        }

        public string GenerateProgressBar(int value, int maxValue, int length, string chars)
        {
            // calculate the ratio of the completed progress bar
            double progressRatio = value / maxValue;

            // calculate the number of filled and empty slots in the progress bar
            int filledSlots = (int)(length * progressRatio);
            int emptySlots = length - filledSlots;

            // generate the progress bar string
            string progressBar = new string(chars[0], filledSlots);
            progressBar += chars[1];
            progressBar += new string(chars[2], emptySlots);

            return progressBar;
        }
    }
}
