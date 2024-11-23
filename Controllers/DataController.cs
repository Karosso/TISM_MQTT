using Firebase.Database;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TISM_MQTT.Models;

namespace TISM_MQTT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;

        public DataController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        [HttpGet("actuators/{macAddress}")]
        public async Task<IActionResult> GetActuatorData(string macAddress)
        {
            // Acessa a coleção de actuators para o macAddress no Firebase
            var actuatorData = await _firebaseClient
                .Child($"data/{macAddress}/actuators")
                .OnceAsync<object>(); // Desserializa para um objeto genérico

            var result = new List<ActuatorData>();

            // Itera sobre os dados e converte o timestamp em DateTime
            foreach (var item in actuatorData)
            {
                // Desserializa o valor de cada item para ActuatorData
                var actuatorDataItem = JsonConvert.DeserializeObject<ActuatorData>(item.Object.ToString());

                if (actuatorDataItem != null)
                {
                    actuatorDataItem.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item.Key)).UtcDateTime; // Converte o timestamp
                    result.Add(actuatorDataItem);
                }
            }

            return Ok(result);
        }

        [HttpGet("sensors/{macAddress}")]
        public async Task<IActionResult> GetSensorData(string macAddress)
        {
            // Acessa a coleção de sensors para o macAddress no Firebase
            var sensorData = await _firebaseClient
                .Child($"data/{macAddress}/sensors")
                .OnceAsync<object>(); // Desserializa para um objeto genérico

            var result = new List<SensorData>();

            // Itera sobre os dados e converte o timestamp em DateTime
            foreach (var item in sensorData)
            {
                // Desserializa o valor de cada item para SensorData
                var sensorDataItem = JsonConvert.DeserializeObject<SensorData>(item.Object.ToString());

                if (sensorDataItem != null)
                {
                    sensorDataItem.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item.Key)).UtcDateTime; // Converte o timestamp
                    result.Add(sensorDataItem);
                }
            }

            return Ok(result);
        }
    }
}
