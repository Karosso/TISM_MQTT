using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TISM_MQTT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActuatorController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;
        private const string CollectionName = "/devices/actuators";

        /// <summary>
        /// Inicializa uma nova instância do <see cref="ActuatorController"/>.
        /// </summary>
        /// <param name="firebaseClient">Instância do cliente Firebase.</param>
        public ActuatorController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        /// <summary>
        /// Adiciona um atuador ao Firebase.
        /// </summary>
        /// <param name="actuator">Dados do atuador.</param>
        /// <returns>Resposta indicando sucesso ou erro.</returns>
        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> GetActuators()
        {
            var actuators = await _firebaseClient
                .Child(CollectionName)
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

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteActuator(string id)
        {
            var existngActuator = await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .OnceSingleAsync<Actuator>();

            if (existngActuator == null)
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
