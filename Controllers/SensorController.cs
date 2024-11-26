using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private const string CollectionName = "/devices";

        /// <summary>
        /// Inicializa uma nova instância do <see cref="SensorController"/>.
        /// </summary>
        /// <param name="firebaseClient">Instância do cliente Firebase.</param>
        public SensorController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        /// <summary>
        /// Verifica se o ESP32 existe no Firebase.
        /// </summary>
        /// <param name="espId">ID do ESP32.</param>
        /// <returns>True se o ESP32 existir, caso contrário, False.</returns>
        private async Task<bool> DoesEspExist(string espId)
        {
            var espExists = await _firebaseClient
                .Child(CollectionName)
                .Child(espId)
                .OnceSingleAsync<object>();

            return espExists != null;
        }

        /// <summary>
        /// Adiciona um sensor ao Firebase.
        /// </summary>
        /// <param name="sensor">Dados do sensor.</param>
        /// <returns>Resposta indicando sucesso ou erro.</returns>
        [HttpPost]
        public async Task<IActionResult> InsertSensor([FromBody] Sensor sensor)
        {
            if (string.IsNullOrEmpty(sensor.Id) || string.IsNullOrEmpty(sensor.EspId))
            {
                return BadRequest("Sensor ID and EspId is required.");
            }

            try
            {
                if (!await DoesEspExist(sensor.EspId))
                {
                    return NotFound($"ESP32 with ID '{sensor.EspId}' not found.");
                }

                await _firebaseClient
                    .Child("devices")
                    .Child(sensor.EspId)
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

        [HttpGet("{espId}")]
        public async Task<IActionResult> GetSensors(string espId)
        {
            try
            {
                if (!await DoesEspExist(espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var sensors = await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("sensors")
                    .OnceAsync<Sensor>();

                var sensorList = sensors.Select(b => new Sensor
                {
                    Id = b.Key,
                    Name = b.Object.Name,
                    EspId = b.Object.EspId,
                    Pin1 = b.Object.Pin1,
                    Pin2 = b.Object.Pin2,
                    Type = b.Object.Type,
                }).ToList();

                return Ok(sensorList);
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"FireBase error: {ex.Message}");
            }
        }

        [HttpGet("{espId}/{id}")]
        public async Task<IActionResult> GetSensorById(string espId, string id)
        {
            try
            {
                if (!await DoesEspExist(espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var sensor = await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("sensors")
                    .Child(id)
                    .OnceSingleAsync<Sensor>();

                if (sensor == null)
                {
                    return NotFound($"Sensor with ID '{id}' not found.");
                }

                return Ok(sensor);
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Firebase error: {ex.Message}");
            }
        }


        [HttpDelete("{espId}/{id}")]
        public async Task<IActionResult> DeleteSensor(string espId, string id)
        {
            try
            {
                if (!await DoesEspExist(espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var existingSensor = await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("sensors")
                    .Child(id)
                    .OnceSingleAsync<Sensor>();

                if (existingSensor == null)
                {
                    return NotFound($"Sensor with ID '{id}' not found.");
                }

                await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("sensors")
                    .Child(id)
                    .DeleteAsync();

                return NoContent();
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Firebase error: {ex.Message}");
            }

        }

    }
}
