namespace TISM_MQTT.Models
{
    public class ActuatorData
    {
        public string ActuatorId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Command { get; set; }

    }
}
