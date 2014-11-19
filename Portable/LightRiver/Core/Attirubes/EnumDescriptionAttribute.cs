using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LightRiver
{
    /// <summary>
    /// 可以用於Enum的Attribute，額外定義Key與Value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : Attribute
    {
        /// <summary>
        /// EnumDescription的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// EnumDescription的Value
        /// </summary>
        public string Value { get; set; }

        public EnumDescriptionAttribute(string key)
            : this(key, string.Empty)
        {
        }

        public EnumDescriptionAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// 如果該Enum有設定EnumDescriptoinAttribute，則可以取得EnumDescription中的Key
        /// </summary>
        /// <param name="enumerationValue"></param>
        /// <returns></returns>
        public static string GetKey(Enum enumerationValue)
        {
            // 取得type
            Type type = enumerationValue.GetType();
            // 取得TypeInfo後才能取得有沒有CustomAttribute
#if !NET45
            var customAttributes = type.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
#else
            FieldInfo fieldInfo = type.GetRuntimeField(enumerationValue.ToString());
            var customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
#endif
            if (customAttributes != null && customAttributes.Length > 0) {
                return customAttributes[0].Key;
            }

            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        /// <summary>
        /// 如果該Enum有設定EnumDescriptoinAttribute，則可以取得EnumDescription中的Value
        /// </summary>
        /// <param name="enumerationValue"></param>
        /// <returns></returns>
        public static string GetValue(Enum enumerationValue)
        {
            // 取得type
            Type type = enumerationValue.GetType();
            // 取得TypeInfo後才能取得有沒有CustomAttribute
#if !NET45
            var customAttributes = type.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
#else
            FieldInfo fieldInfo = type.GetRuntimeField(enumerationValue.ToString());
            var customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
#endif

            if (customAttributes != null && customAttributes.Length > 0) {
                return customAttributes[0].Value;
            }

            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }
    }
}
