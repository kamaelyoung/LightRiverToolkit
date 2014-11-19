using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Reflection
{
    public static class AssemblyExtensions
    {
        public static string GetTitle(this Assembly assembly)
        {
            // 取得這個組件的所有 Title 屬性
            var attribute = assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            // 如果至少有一個 Title 屬性
            if (attribute != null) {
                return attribute.Title;
            }
            // 如果沒有 Title 屬性，或 Title 屬性為空字串，則傳回 .exe 名稱
            return System.IO.Path.GetFileNameWithoutExtension(assembly.FullName);
        }

        public static string GetVersion(this Assembly assembly)
        {
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            return assemblyName.Version.ToString();
        }
    }
}
