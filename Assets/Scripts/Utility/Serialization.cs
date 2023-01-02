using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ZetanStudio.Serialization
{
    public static class Json
    {
        public static string ToJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });
        }
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });
        }
    }

    public class PloyListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> values = new List<T>();
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    values.Add((T)item.Value.ToObject(Type.GetType(item.Name)));
                }
            }
            catch { }
            return values;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<T>)value;
            var obj = new JObject();
            foreach (var item in list)
            {
                obj.Add(item.GetType().AssemblyQualifiedName, JToken.FromObject(item));
            }
            serializer.Serialize(writer, obj);
        }
    }
    public class PloyArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> values = new List<T>();
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    values.Add((T)item.Value.ToObject(Type.GetType(item.Name)));
                }
            }
            catch { }
            return values.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (T[])value;
            var obj = new JObject();
            foreach (var item in list)
            {
                obj.Add(item.GetType().AssemblyQualifiedName, JToken.FromObject(item));
            }
            serializer.Serialize(writer, obj);
        }
    }
    public class PloyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    return item.Value.ToObject(Type.GetType(item.Name));
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JObject() { { value.GetType().AssemblyQualifiedName, JToken.FromObject(value) } });
        }
    }
}