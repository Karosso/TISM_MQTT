public enum SensorType
{
    Temperatura = 0,
    Fumaca = 1,
    // Outros tipos
}

public class Sensor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public int Pin1 { get; set; }
    public int? Pin2 { get; set; } // Opcional
    public SensorType Type { get; set; }
    public string EspId { get; set; }
    public bool IsDigital { get; set; }
}
