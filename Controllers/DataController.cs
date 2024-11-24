using Firebase.Database;
using Firebase.Database.Query;
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

        [HttpGet("actuators/{macAddress}/{actuatorId}")]
        public async Task<IActionResult> GetCurrentActuatorData(string actuatorId, string macAddress)
        {
            // Acessa a coleção de actuators para o macAddress no Firebase
            var currentData = await _firebaseClient
                .Child($"data/{macAddress}/actuators/{actuatorId}")
                .OrderByKey()
                .LimitToLast(1)
                .OnceAsync<ActuatorData>();

            if (currentData == null || !currentData.Any())
            {
                return NotFound("Sem dados para este atuador");
            }

            // Extrai o único item da coleção retornada
            var result = currentData.First().Object;

            return Ok(result);
        }

        [HttpGet("actuators/{macAddress}/{actuatorId}/recent")]
        public async Task<IActionResult> GetRecentActuatorData(string actuatorId, string macAddress)
        {
            // Obtém até 30 registros mais recentes da coleção de sensores
            var recentData = await _firebaseClient
                .Child($"data/{macAddress}/actuators/{actuatorId}")
                .OrderByKey()
                .LimitToLast(30) // Limita a consulta a no máximo 30 itens
                .OnceAsync<ActuatorData>();

            if (recentData == null || !recentData.Any())
            {
                return NotFound("Sem dados recentes para este atuador.");
            }

            // Extrai os objetos da lista retornada
            var results = recentData
                .Select(entry => entry.Object) // Mapeia os objetos para a classe ActuatorData
                .ToList();

            return Ok(results);
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


        [HttpGet("sensors/{macAddress}/{sensorId}")]
        public async Task<IActionResult> GetCurrentSensorData(string sensorId, string macAddress)
        {
            // Acessa a coleção de sensores para o macAddress no Firebase
            var currentData = await _firebaseClient
                .Child($"data/{macAddress}/sensors/{sensorId}")
                .OrderByKey()
                .LimitToLast(1)
                .OnceAsync<SensorData>();

            if (currentData == null || !currentData.Any())
            {
                return NotFound("Sem dados para este sensor");
            }

            // Extrai o único item da coleção retornada
            var result = currentData.First().Object;

            return Ok(result);
        }

        [HttpGet("sensors/{macAddress}/{sensorId}/recent")]
        public async Task<IActionResult> GetRecentSensorData(string sensorId, string macAddress)
        {
            // Obtém até 30 registros mais recentes da coleção de sensores
            var recentData = await _firebaseClient
                .Child($"data/{macAddress}/sensors/{sensorId}")
                .OrderByKey()
                .LimitToLast(30) // Limita a consulta a no máximo 30 itens
                .OnceAsync<SensorData>();

            if (recentData == null || !recentData.Any())
            {
                return NotFound("Sem dados recentes para este sensor.");
            }

            // Extrai os objetos da lista retornada
            var results = recentData
                .Select(entry => entry.Object) // Mapeia os objetos para a classe SensorData
                .ToList();

            return Ok(results);
        }

    }
}
