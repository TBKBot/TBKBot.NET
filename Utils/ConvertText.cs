using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBKBot.Utils
{
    public class ConvertText
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
    }
}
