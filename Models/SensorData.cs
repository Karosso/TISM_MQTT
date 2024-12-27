using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TISM_MQTT.Models
{
    public class SensorData
    {
        public string SensorId { get; set; }
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime Timestamp { get; set; }

        public string Value { get; set; }


    }

    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateTimeString = reader.GetString();
                // Tenta analisar o formato sem frações de segundo e com frações de segundo
                if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
                {
                    return dateTime;
                }
                else if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:ss.ffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime))
                {
                    return dateTime;
                }

                // Se não puder analisar, lança uma exceção
                throw new JsonException($"Invalid date format: {dateTimeString}");
            }
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}
