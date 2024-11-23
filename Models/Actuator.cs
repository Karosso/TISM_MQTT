public enum ActuatorType
{
    Lampada = 0,
    Motor = 1,
    // Outros tipos
}

public class Actuator
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public int OutputPin { get; set; }
    public ActuatorType TypeActuator { get; set; }
    public string EspId { get; set; }
    public bool IsDigital { get; set; }
}
