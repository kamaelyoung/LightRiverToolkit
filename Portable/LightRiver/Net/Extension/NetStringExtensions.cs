using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightRiver
{
    internal static class NetStringExtensions
    {
        /// <summary>
        /// 把字串轉成C++有0結尾的byte陣列
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] ToCppByteArray(this string src)
        {
            byte[] cppByteArray = new byte[Encoding.UTF8.GetByteCount(src) + 1];
            MemoryStream stream = new MemoryStream(cppByteArray);
            stream.Write(Encoding.UTF8.GetBytes(src), 0, cppByteArray.Length - 1);
            stream.WriteByte(0); // 寫入c/c++字串結尾的0
            stream.Dispose();

            return cppByteArray;
        }
    }
}
