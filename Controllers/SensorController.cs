using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Threading.Tasks;
using TISM_MQTT.Models;

namespace TISM_MQTT.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar a comunicação com o Firebase.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;
        private const string CollectionName = "/devices/sensors";

        /// <summary>
        /// Inicializa uma nova instância do <see cref="SensorController"/>.
        /// </summary>
        /// <param name="firebaseClient">Instância do cliente Firebase.</param>
        public SensorController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        /// <summary>
        /// Adiciona um sensor ao Firebase.
        /// </summary>
        /// <param name="sensor">Dados do sensor.</param>
        /// <returns>Resposta indicando sucesso ou erro.</returns>
        [HttpPost("sensor")]
        public async Task<IActionResult> InsertSensor([FromBody] Sensor sensor)
        {
            if (string.IsNullOrEmpty(sensor.Id))
            {
                return BadRequest("Sensor ID is required.");
            }

            try
            {
                await _firebaseClient
                    .Child("devices")
                    .Child("sensors")
                    .Child(sensor.Id)
                    .PutAsync(sensor);

                return Ok(new { Message = "Sensor added successfully." });
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Firebase error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSensors()
        {
            var sensors = await _firebaseClient
                .Child(CollectionName)
                .OnceAsync<Sensor>();

            var sensorList = sensors.Select(b => new Sensor
            {
                Id = b.Key,
                Name = b.Object.Name,
                Pin1 = b.Object.Pin1,
                Pin2 = b.Object.Pin2,
                Type = b.Object.Type,
                EspId = b.Object.EspId,
                IsDigital = b.Object.IsDigital,

            }).ToList();

            return Ok(sensorList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSensorById(string id)
        {
            // Obtém o sensor específico pelo ID
            var sensor = await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .OnceSingleAsync<Sensor>(); // Busca o objeto diretamente pelo ID

            if (sensor == null)
            {
                return NotFound($"Sensor com ID '{id}' não encontrado.");
            }

            // Retorna o sensor com suas propriedades
            var result = new Sensor
            {
                Id = id, // Adiciona o ID manualmente, pois `OnceSingleAsync` não inclui a chave
                Name = sensor.Name,
                Pin1 = sensor.Pin1,
                Pin2 = sensor.Pin2,
                Type = sensor.Type,
                EspId = sensor.EspId,
                IsDigital = sensor.IsDigital
            };

            return Ok(result);
        }


        [HttpDelete("id")]
        public async Task<IActionResult> DeleteSensor(string id)
        {
            var existngSensor = await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .OnceSingleAsync<Sensor>();

            if (existngSensor == null)
            {
                return NotFound();
            }

            await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .DeleteAsync();

            return NoContent();

        }

    }
}
