using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TISM_MQTT.Models;

namespace TISM_MQTT.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar a comunicação com o Firebase.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FirebaseController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="FirebaseController"/>.
        /// </summary>
        /// <param name="firebaseClient">Instância do cliente Firebase.</param>
        public FirebaseController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        /// <summary>
        /// Adiciona um atuador ao Firebase.
        /// </summary>
        /// <param name="actuator">Dados do atuador.</param>
        /// <returns>Resposta indicando sucesso ou erro.</returns>
        [HttpPost("actuator")]
        public async Task<IActionResult> InsertActuator([FromBody] Actuator actuator)
        {
            // Verifica se o ID do atuador foi fornecido
            if (string.IsNullOrEmpty(actuator.Id))
            {
                return BadRequest("Actuator ID is required.");
            }

            try
            {
                // Insere o atuador no caminho especificado
                await _firebaseClient
                    .Child("devices")
                    .Child("actuators")
                    .Child(actuator.Id)
                    .PutAsync(actuator);

                return Ok(new { Message = "Actuator added successfully." });
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

        /// <summary>
        /// Adiciona um ESP32 ao Firebase.
        /// </summary>
        /// <param name="esp32">Dados do dispositivo ESP32.</param>
        /// <returns>Resposta indicando sucesso ou erro.</returns>
        [HttpPost("esp32")]
        public async Task<IActionResult> InsertESP32([FromBody] ESP32 esp32)
        {
            if (string.IsNullOrEmpty(esp32.MacAddress))
            {
                return BadRequest("ESP32 MAC address is required.");
            }

            try
            {
                await _firebaseClient
                    .Child("devices")
                    .Child("esp32")
                    .Child(esp32.MacAddress)
                    .PutAsync(esp32);

                return Ok(new { Message = "ESP32 added successfully." });
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
    }
}
