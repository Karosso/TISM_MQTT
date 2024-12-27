using TISM_MQTT.Models;

public enum SensorType
{
    DHT11 = 1,       // Sensor de Temperatura e Umidade (comum)
    DHT22 = 2,       // Sensor de Temperatura e Umidade (comum)
    AM2302 = 3,      // Sensor de Temperatura e Umidade (comum)
    CCS811 = 4,      // Sensor de Qualidade do Ar (comum)
    MH_Z19B = 5,     // Sensor de CO2 (comum)
    MQ135 = 6,       // Sensor de Gás (comum)
    MQ9 = 7,         // Sensor de Gás (comum)
    HC_SR501 = 8,    // Sensor de Movimento PIR (comum)
    RCWL0516 = 9,    // Sensor de Movimento Radar (comum)
    VL53L0X = 10,     // Sensor de Distância a Laser (comum)
    Ultrasonic = 11, // Sensor Ultrassônico de Distância (comum)
    Maxbotix = 12,   // Sensor Ultrassônico de Distância (comum)
    TSL2561 = 13,    // Sensor de Luz Digital (comum)
    APDS9960 = 14,   // Sensor de Cor, Luz e Proximidade (comum)
    SW420 = 15,      // Sensor de Vibração (comum)
    HX711 = 16,      // Sensor de Célula de Carga (comum)
    YFS201 = 17,     // Sensor de Fluxo de Água (comum)
    PH_SENSOR = 18,  // Sensor de pH
    MAX30100 = 19,   // Sensor de Oximetria (comum)
    NTC = 20,        // Sensor de Temperatura NTC (comum)
}

public class Sensor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public SensorType Type { get; set; }
    public string EspId { get; set; }

    // Propriedade para armazenar os últimos dados do sensor
    public SensorData? LastData { get; set; }
}
