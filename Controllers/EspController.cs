using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TISM_MQTT.Models;

namespace TISM_MQTT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;
        private const string CollectionName = "/devices/esp32";

        /// <summary>
        /// Inicializa uma nova instância do <see cref="EspController"/>.
        /// </summary>
        /// <param name="firebaseClient">Instância do cliente Firebase.</param>
        public EspController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        /// <summary>
        /// Adiciona um ESP32 ao Firebase.
        /// </summary>
        /// <param name="esp32">Dados do dispositivo ESP32.</param>
        /// <returns>Resposta indicando sucesso ou erro.</returns>
        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> GetEsps()
        {
            var esps = await _firebaseClient
                .Child(CollectionName)
                .OnceAsync<ESP32>();

            var espList = esps.Select(b => new ESP32
            {
                Id = b.Key,
                Name = b.Object.Name,
                MacAddress = b.Object.MacAddress,
            }).ToList();

            return Ok(espList);
        }

        // Fazer regra de deleção, validar se tem sensor relacionado a este esp32
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteEsp(string id)
        {
            var existngEsp = await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .OnceSingleAsync<ESP32>();

            if (existngEsp == null)
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
