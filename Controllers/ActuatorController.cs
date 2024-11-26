using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using TISM_MQTT.Models;

namespace TISM_MQTT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActuatorController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;
        private const string CollectionName = "/devices";

        /// <summary>
        /// Inicializa uma nova instância do <see cref="ActuatorController"/>.
        /// </summary>
        /// <param name="firebaseClient">Instância do cliente Firebase.</param>
        public ActuatorController(FirebaseClient firebaseClient)
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

        [HttpPost]
        public async Task<IActionResult> InsertActuator([FromBody] Actuator actuator)
        {
            if (string.IsNullOrEmpty(actuator.Id) || string.IsNullOrEmpty(actuator.EspId))
            {
                return BadRequest("Actuator ID and EspId are required.");
            }

            try
            {
                if (!await DoesEspExist(actuator.EspId))
                {
                    return NotFound($"ESP32 with ID '{actuator.EspId}' not found.");
                }

                await _firebaseClient
                    .Child("devices")
                    .Child(actuator.EspId)
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

        [HttpGet("{espId}")]
        public async Task<IActionResult> GetActuators(string espId)
        {
            try
            {
                if (!await DoesEspExist(espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var actuators = await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("actuators")
                    .OnceAsync<Actuator>();

                var actuatorList = actuators.Select(b => new Actuator
                {
                    Id = b.Key,
                    Name = b.Object.Name,
                    OutputPin = b.Object.OutputPin,
                    TypeActuator = b.Object.TypeActuator,
                    EspId = b.Object.EspId,
                    IsDigital = b.Object.IsDigital,
                }).ToList();

                return Ok(actuatorList);
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Firebase error: {ex.Message}");
            }
        }

        [HttpGet("{espId}/{id}")]
        public async Task<IActionResult> GetActuatorById(string espId, string id)
        {
            try
            {
                if (!await DoesEspExist(espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var actuator = await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("actuators")
                    .Child(id)
                    .OnceSingleAsync<Actuator>();

                if (actuator == null)
                {
                    return NotFound($"Actuator with ID '{id}' not found.");
                }

                return Ok(actuator);
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Firebase error: {ex.Message}");
            }
        }

        [HttpDelete("{espId}/{id}")]
        public async Task<IActionResult> DeleteActuator(string espId, string id)
        {
            try
            {
                if (!await DoesEspExist(espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var existingActuator = await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("actuators")
                    .Child(id)
                    .OnceSingleAsync<Actuator>();

                if (existingActuator == null)
                {
                    return NotFound($"Actuator with ID '{id}' not found.");
                }

                await _firebaseClient
                    .Child(CollectionName)
                    .Child(espId)
                    .Child("actuators")
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
