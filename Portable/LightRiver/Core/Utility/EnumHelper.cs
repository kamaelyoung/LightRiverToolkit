using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver
{
#if !NET45
    public static class EnumHelper
    {
        /// <summary>
        /// 取得Enum中所有的Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("Type must be enumeration type.");

            return from field in typeof(T).GetFields()
                   where field.IsLiteral && !string.IsNullOrEmpty(field.Name)
                   select (T)field.GetValue(null);
        }
    }
#endif
}
