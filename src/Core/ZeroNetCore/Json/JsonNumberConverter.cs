using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agebull.MicroZero
{

    /// <summary>
    ///     大数字序列化器
    /// </summary>
    internal class JsonNumberConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = JToken.FromObject(value);

            if (t.Type == JTokenType.Integer)
                writer.WriteValue(value.ToString());
            else
                t.WriteTo(writer);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false.The type will skip the converter.");
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(long) || objectType == typeof(decimal);
        }
    }
}