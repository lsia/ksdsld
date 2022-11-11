using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Util
{
    static class DictionaryExtensions
    {
        public static void AddCount<T>(this Dictionary<T,int> dictionary, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                if (!dictionary.ContainsKey(value))
                    dictionary.Add(value, 0);

                dictionary[value]++;
            }
        }
        
        public static void AddObservations<T, V>(this Dictionary<T, List<V>> dictionary, IEnumerable<KeyValuePair<T,V>> values, bool add_new_categories = true)
            where V : new()
        {
            foreach (var value in values)
            {
                if (add_new_categories && !dictionary.ContainsKey(value.Key))
                    dictionary.Add(value.Key, new List<V>());

                if (dictionary.ContainsKey(value.Key))
                    dictionary[value.Key].Add(value.Value);
            }
        }
    }
}
