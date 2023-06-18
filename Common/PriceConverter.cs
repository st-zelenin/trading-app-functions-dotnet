using Newtonsoft.Json;

namespace Common;

public class PriceConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(decimal) || objectType == typeof(decimal?) || objectType == typeof(double) || objectType == typeof(double?);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteValue(string.Empty);
            return;
        }

        if (value.GetType() == typeof(double))
        {
            writer.WriteValue(((double)value).ToString("0.################"));
            return;
        }

        if (value.GetType() == typeof(decimal))
        {
            writer.WriteValue(((decimal)value).ToString("0.################"));
            return;
        }

        throw new ArgumentException($"unexpected value type: {value.GetType().Name}");
    }
}

