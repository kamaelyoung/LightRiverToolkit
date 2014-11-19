using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver
{
    public enum DateTimeConvertType
    {
        Full,
        Date,
        Time,
    };

    /// <summary>
    /// 有例外處理的Convert class
    /// </summary>
    public static class Convert2
    {
        public static DateTime ToDateTime(string source, DateTimeConvertType converType)
        {
            DateTime dateTime = DateTime.Now;

            switch (converType) {
                case DateTimeConvertType.Full:
                    dateTime = FromDateTimeFull(source);
                    break;
                case DateTimeConvertType.Date:
                    dateTime = FromDatePart(source);
                    break;
                case DateTimeConvertType.Time:
                    dateTime = FromTimePart(source);
                    break;
            };

            return dateTime;
        }

        /// <summary>
        /// 從日期時間格式字串格式化成DateTime
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static DateTime FromDateTimeFull(string source)
        {
            if (string.IsNullOrEmpty(source))
                return DateTime.Now;

            if (source.Length < 12)
                return DateTime.Now;

            DateTime result = DateTime.Now;
            if (source.Length > 12) {
                DateTime.TryParseExact(source, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out result);
            }
            else {
                DateTime.TryParseExact(source, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out result);
            }

            return result;
        }

        /// <summary>
        /// 從日期格式字串格式化成DateTime
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static DateTime FromDatePart(string source)
        {
            if (string.IsNullOrEmpty(source))
                return DateTime.Now;

            if (source.Length < 4)
                return DateTime.Now;

            DateTime result = DateTime.Now;
            if (source.Length == 4) {
                DateTime.TryParseExact(source, "MMdd", null, System.Globalization.DateTimeStyles.None, out result);
            }
            else if (source.Length == 8) {
                DateTime.TryParseExact(source, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out result);
            }

            return result;
        }


        /// <summary>
        /// 從時間格式字串格式化成DateTime
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static DateTime FromTimePart(string source)
        {
            if (string.IsNullOrEmpty(source))
                return DateTime.Now;

            if (source.Length < 4)
                return DateTime.Now;

            DateTime result = DateTime.Now;
            if (source.Length > 4) {
                DateTime.TryParseExact(source, "HHmmss", null, System.Globalization.DateTimeStyles.None, out result);
            }
            else {
                DateTime.TryParseExact(source, "HHmm", null, System.Globalization.DateTimeStyles.None, out result);
            }

            return result;
        }

        /// <summary>
        /// 將字串轉換成指定小數點位數的字串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pointDigits"></param>
        /// <returns></returns>
        public static string ToString(double source, int pointDigits)
        {
            string digitFormat = string.Format("{{0:F{0}}}", pointDigits);
            return string.Format(digitFormat, source);
        }

        /// <summary>
        /// 將字串轉換成Double（使用TryParse避免Exception）
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static double ToDouble(string src)
        {
            double result = 0;
            double.TryParse(src, out result);
            return result;
        }

        /// <summary>
        /// 將字串轉換成Int（使用TryParse避免Exception）
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static int ToInt(string src)
        {
            int result = 0;
            int.TryParse(src, out result);
            return result;
        }

        /// <summary>
        /// 將字串轉換成short（使用TryParse避免Exception）
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static short ToShort(string src)
        {
            short result = 0;
            short.TryParse(src, out result);
            return result;
        }

        /// <summary>
        /// 將字串轉換成byte（使用TryParse避免Exception）
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte ToByte(string src)
        {
            byte result = 0;
            byte.TryParse(src, out result);
            return result;
        }
    }
}
