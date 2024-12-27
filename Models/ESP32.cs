namespace TISM_MQTT.Models
{
    public class ESP32
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public Dictionary<string, Sensor>? Sensors { get; set; }
        public Dictionary<string, Actuator>? Actuators { get; set; }

        public List<Sensor> GetSensorList()
        {
            return Sensors?.Values.ToList() ?? new List<Sensor>();
        }

        public List<Actuator> GetActuatorList()
        {
            return Actuators?.Values.ToList() ?? new List<Actuator>();
        }

    }
}
