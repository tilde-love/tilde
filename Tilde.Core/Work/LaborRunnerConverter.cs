using System;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tilde.Core.Work
{
    public class LaborRunnerConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<string, Type> converters = new ConcurrentDictionary<string, Type>(); 
            
        public static void RegisterConverter<TRunner>() where TRunner : ILaborRunner
        {
            converters[typeof(TRunner).FullName] = typeof(TRunner); 
        }

        /// <inheritdoc />
        public override bool CanWrite => false; 

        public override bool CanConvert(Type objectType)
        {
            return converters.Values.Aggregate(false, (current, type) => current | type.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            
            string type = item["type"].Value<string>(); 
            
            if (converters.TryGetValue(type, out Type converterType))
            {
                return item.ToObject(converterType, serializer); 
            }

            return existingValue; 
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}