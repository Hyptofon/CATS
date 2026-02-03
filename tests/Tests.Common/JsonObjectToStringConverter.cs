using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests.Common;

public class JsonObjectToStringConverter : JsonConverter<string>
{
    public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonToken.String)
        {
            return (string?)reader.Value;
        }
        
        var token = JToken.Load(reader);
        return token.ToString(Formatting.None);
    }

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        try
        {
            var token = JToken.Parse(value);
            token.WriteTo(writer);
        }
        catch
        {
            writer.WriteValue(value);
        }
    }
}
