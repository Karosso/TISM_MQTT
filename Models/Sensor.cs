public enum SensorType
{
    DHT11 = 0,       // Sensor de Temperatura e Umidade (comum)
    DHT22 = 1,       // Sensor de Temperatura e Umidade (comum)
    AM2302 = 2,      // Sensor de Temperatura e Umidade (comum)
    CCS811 = 3,      // Sensor de Qualidade do Ar (comum)
    MH_Z19B = 4,     // Sensor de CO2 (comum)
    MQ135 = 5,       // Sensor de Gás (comum)
    MQ9 = 6,         // Sensor de Gás (comum)
    HC_SR501 = 7,    // Sensor de Movimento PIR (comum)
    RCWL0516 = 8,    // Sensor de Movimento Radar (comum)
    VL53L0X = 9,     // Sensor de Distância a Laser (comum)
    Ultrasonic = 10, // Sensor Ultrassônico de Distância (comum)
    Maxbotix = 11,   // Sensor Ultrassônico de Distância (comum)
    TSL2561 = 12,    // Sensor de Luz Digital (comum)
    APDS9960 = 13,   // Sensor de Cor, Luz e Proximidade (comum)
    SW420 = 14,      // Sensor de Vibração (comum)
    HX711 = 15,      // Sensor de Célula de Carga (comum)
    YFS201 = 16,     // Sensor de Fluxo de Água (comum)
    PH_SENSOR = 17,  // Sensor de pH
    MAX30100 = 18,   // Sensor de Oximetria (comum)
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
