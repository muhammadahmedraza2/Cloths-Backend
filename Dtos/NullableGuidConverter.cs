using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClothingErp.Api.Dtos;

/// <summary>
/// Fixes: PUT/POST requests failing with
/// "The JSON value could not be converted to System.Nullable`1[System.Guid]"
/// whenever a client (Angular admin form, Swagger, Postman, etc.) sends an
/// empty string "" for an optional GUID field instead of omitting it or
/// sending null. Apply with [JsonConverter(typeof(NullableGuidConverter))]
/// on any `Guid?` property that can arrive as "".
/// </summary>
public class NullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (Guid.TryParse(value, out var guid))
            {
                return guid;
            }

            throw new JsonException($"\"{value}\" is not a valid GUID.");
        }

        throw new JsonException("Expected a GUID string or null.");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}