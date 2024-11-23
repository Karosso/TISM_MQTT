namespace TISM_MQTT.Models
{
    public class ESP32
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string MacAddress { get; set; }

    }
}
