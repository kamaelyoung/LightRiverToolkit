using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightRiver
{
    public static class JsonIoUtility
    {
        public static async Task SerializeAsync(Object obj, IPlatformFile file, string filePath)
        {
            var stream = await file.OpenWriteStreamAsync(filePath);
            await SerializeAsync(obj, stream);
        }

        public static async Task SerializeAsync(Object obj, Stream stream)
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            var sw = new StreamWriter(stream);
            await sw.WriteAsync(jsonString);
            sw.Dispose();
        }

        public static void Serialize(Object obj, Stream stream)
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            var sw = new StreamWriter(stream);
            sw.Write(jsonString);
            sw.Dispose();
        }

        public static async Task<T> DeserializeAsync<T>(IPlatformFile file, string filePath)
        {
            var stream = await file.OpenReadStreamAsync(filePath);
            return await DeserializeAsync<T>(stream);
        }

        public static async Task<T> DeserializeAsync<T>(Stream stream)
        {
            var sr = new StreamReader(stream);
            var jsonString = await sr.ReadToEndAsync();
            sr.Dispose();

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static T Deserialize<T>(Stream stream)
        {
            var sr = new StreamReader(stream);
            var jsonString = sr.ReadToEnd();
            sr.Dispose();

            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
