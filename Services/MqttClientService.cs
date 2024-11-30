using Firebase.Database;
using Firebase.Database.Query;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text;
using System.Text.Json;
using TISM_MQTT.Models;

namespace TISM_MQTT.Services
{
    // Serviço implementando IHostedService para execução contínua no ciclo de vida da aplicação.
    public class MqttClientService : IHostedService
    {
        private readonly ILogger<MqttClientService> _logger; // Logger para registrar eventos do serviço.
        private readonly FirebaseClient firebaseClient; // Cliente para interação com o Firebase.
        private IMqttClient _mqttClient; // Cliente MQTT para enviar/receber mensagens.
        private IMqttClientOptions _options; // Opções de configuração para o cliente MQTT.

        // Construtor injeta dependências para logging e Firebase.
        public MqttClientService(ILogger<MqttClientService> logger, FirebaseClient firebaseClient)
        {
            _logger = logger;
            this.firebaseClient = firebaseClient;
        }

        // Método chamado ao iniciar o serviço.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient(); // Cria o cliente MQTT.

            _options = new MqttClientOptionsBuilder()
                .WithClientId("7a0ff2fb-c0e7-4755-ac91-4599b6d87ee0") // Identificador único do cliente MQTT.
                .WithTcpServer("test.mosquitto.org", 1883) // Configura o servidor MQTT.
                .WithCleanSession() // Garante que mensagens não persistem entre sessões.
                .Build();

            // Handler executado quando o cliente conecta ao broker.
            _mqttClient.UseConnectedHandler(async e =>
            {
                _logger.LogInformation("Connected to MQTT broker");

                // Subscrição genérica para tópicos de dispositivos ESP32
                await SubscribeToTopicAsync("/esp32/+/sensors/#");
                await SubscribeToTopicAsync("/esp32/+/actuators/#");

                _logger.LogInformation("Subscribed to topics for sensors and actuators");
            });

            // Handler executado ao receber mensagens de um tópico subscrito.
            _mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                var topic = e.ApplicationMessage.Topic; // Tópico da mensagem.
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload); // Conteúdo da mensagem.
                _logger.LogInformation($"Received message from topic {topic}: {message}");

                try
                {
                    var jsonDoc = JsonDocument.Parse(message); // Interpreta a mensagem como JSON.
                    var macAddress = ExtractMacAddressFromTopic(topic); // Extrai o MAC Address do tópico.

                    // Verifica se a mensagem é de sensores ou atuadores e processa.
                    if (topic.Contains("/sensors/"))
                    {
                        await ProcessSensorData(macAddress, jsonDoc.RootElement);
                    }
                    else if (topic.Contains("/actuators/"))
                    {
                        await ProcessActuatorData(macAddress, jsonDoc.RootElement);
                    }
                    else
                    {
                        _logger.LogWarning($"Unknown topic: {topic}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            });

            // Tenta conectar ao broker MQTT.
            try
            {
                await _mqttClient.ConnectAsync(_options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker");
            }
        }

        // Extrai o MAC Address do tópico com base no padrão esperado.
        private string ExtractMacAddressFromTopic(string topic)
        {
            var parts = topic.Split('/');
            return parts.Length > 2 ? parts[2] : string.Empty; // Assume que o MAC Address é a segunda parte do tópico
        }

        // Processa dados recebidos de sensores.
        private async Task ProcessSensorData(string macAddress, JsonElement rootElement)
        {
            try
            {
                var sensorData = JsonSerializer.Deserialize<SensorData>(rootElement.GetRawText()); // Converte o JSON para objeto SensorData.

                if (sensorData == null)
                {
                    _logger.LogWarning("Received message does not contain valid sensor data");
                    return;
                }

                // Verifica se o Timestamp é válido e o converte para milissegundos
                long timestampMilliseconds;
                try
                {
                    // Certifica-se de que o Timestamp é tratado como string antes da conversão
                    var timestampString = sensorData.Timestamp.ToString("o"); // ISO 8601
                    var dateTimeOffset = DateTimeOffset.Parse(timestampString);
                    timestampMilliseconds = dateTimeOffset.ToUnixTimeMilliseconds();
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid timestamp format: {Timestamp}", sensorData.Timestamp);
                    return;
                }

                // Salva no Firebase organizado por MAC Address
                await firebaseClient
                    .Child($"data/{macAddress}/sensors/{sensorData.SensorId}/{timestampMilliseconds}")
                    .PutAsync(sensorData);

                _logger.LogInformation($"Sensor data for {macAddress} saved in Firebase");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sensor data");
            }
        }

        // Processa dados recebidos de atuadores.
        private async Task ProcessActuatorData(string macAddress, JsonElement rootElement)
        {
            try
            {
                // Converte o JSON para objeto ActuatorData.
                var actuatorData = JsonSerializer.Deserialize<ActuatorData>(rootElement.GetRawText());

                if (actuatorData == null)
                {
                    _logger.LogWarning("Received message does not contain valid actuator data");
                    return;
                }

                // Verifica se o Timestamp é válido e o converte para milissegundos
                long timestampMilliseconds;
                try
                {
                    // Certifica-se de que o Timestamp é tratado como string antes da conversão
                    var timestampString = actuatorData.Timestamp.ToString("o"); // ISO 8601
                    var dateTimeOffset = DateTimeOffset.Parse(timestampString);
                    timestampMilliseconds = dateTimeOffset.ToUnixTimeMilliseconds();
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid timestamp format: {Timestamp}", actuatorData.Timestamp);
                    return;
                }

                // Salva os dados no Firebase organizados por MAC Address e ID do atuador.
                await firebaseClient
                    .Child($"data/{macAddress}/actuators/{actuatorData.ActuatorId}/{timestampMilliseconds}")
                    .PutAsync(actuatorData);

                _logger.LogInformation($"Actuator data for {macAddress} saved in Firebase");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing actuator data");
            }
        }

        // Método chamado ao parar o serviço.
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mqttClient != null)
            {
                await _mqttClient.DisconnectAsync(); // Desconecta do broker MQTT.
            }
        }

        // Publica mensagens em um tópico MQTT.
        public async Task<bool> PublishMessageAsync(string topic, string message)
        {
            if (_mqttClient.IsConnected)
            {
                try
                {
                    var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic) // Define o tópico.
                        .WithPayload(message) // Define o conteúdo da mensagem.
                        .WithExactlyOnceQoS() // Define a qualidade de serviço.
                        .Build();

                    await _mqttClient.PublishAsync(mqttMessage); // Publica a mensagem.
                    _logger.LogInformation($"Message published to topic {topic}: {message}");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to publish message to topic {topic}");
                }
            }
            else
            {
                _logger.LogWarning("MQTT client is not connected");
            }
            return false;
        }

        // Subscreve-se a um tópico MQTT.
        public async Task SubscribeToTopicAsync(string topic)
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
                _logger.LogInformation($"Subscribed to topic: {topic}");
            }
        }
    }
}
