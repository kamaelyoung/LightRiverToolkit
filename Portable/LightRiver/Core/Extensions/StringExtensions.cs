using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver
{
    public static class StringExtensions
    {
        public static string SubStringWithDefault(this string src, int startidx, int lenth, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(src))
                return defaultValue;
            
            int len = 0;
            len = Math.Min(lenth, src.Length - startidx);

            if (lenth == -1)
                len = src.Length - startidx;

            return len > 0 ? src.Substring(startidx, len) : defaultValue;
        }

        public static string SubStringWithDefault(this string src, int startidx, string defaultValue = "")
        {
            if (!string.IsNullOrEmpty(src))
                return defaultValue;
            
            if (startidx >= src.Length)
                return defaultValue;

            int lenth = src.Length - startidx;
            return src.SubStringWithDefault(startidx, lenth, defaultValue);
        }
    }
}
