using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightRiver
{
    public class ObjectDictionary<TKey> : Dictionary<TKey, object>
    {
        public T GetValue<T>(TKey key)
        {
            return (T)this[key];
        }

        public bool TryGetValue<T>(TKey key, out T result)
        {
            result = default(T);
            if (!ContainsKey(key))
                return false;

            result = GetValue<T>(key);
            return true;
        }

        public static Task SaveAsync(ObjectDictionary<TKey> dictionary, IPlatformFile file, string filePath)
        {
            return JsonIoUtility.SerializeAsync(dictionary, file, filePath);
        }

        public static Task<ObjectDictionary<TKey>> LoadAsync(IPlatformFile file, string filePath)
        {
            return JsonIoUtility.DeserializeAsync<ObjectDictionary<TKey>>(file, filePath);
        }
    }
}
