using Firebase.Database;
using Firebase.Database.Query;
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
            
            if (string.IsNullOrEmpty(esp32.Name))
            {
                return BadRequest("ESP32 Name is required.");
            }

            try
            {
                await _firebaseClient
                    .Child("devices")
                    .Child("esp32")
                    .Child(esp32.Id)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEspById(string id)
        {
            // Obtém o ESP específico pelo ID
            var esp = await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .OnceSingleAsync<ESP32>(); // Busca o objeto diretamente pelo ID

            if (esp == null)
            {
                return NotFound($"Esp com ID '{id}' não encontrado.");
            }

            // Retorna o sensor com suas propriedades
            var result = new ESP32
            {
                Id = id, // Adiciona o ID manualmente, pois `OnceSingleAsync` não inclui a chave
                Name = esp.Name,
                MacAddress = esp.MacAddress,
            };

            return Ok(result);
        }

        [HttpGet("{id}/sensors")]
        public async Task<IActionResult> GetEspSensorsById(string id)
        {
            // Obtém os sensores do esp específico pelo ID
            var sensors = await _firebaseClient
                .Child("/devices/sensors")
                .OrderBy("EspId")
                .EqualTo(id)
                .OnceAsync<Sensor>();

            if (sensors == null)
            {
                return NotFound($"Esp sem sensores cadastrados");
            }

            // Retorna o a lista sensores com suas propriedades
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

        [HttpGet("{id}/actuators")]
        public async Task<IActionResult> GetEspActuatorsById(string id)
        {
            // Obtém os atuadores do esp específico pelo ID
            var actuators = await _firebaseClient
                .Child("/devices/actuators")
                .OrderBy("EspId")
                .EqualTo(id)
                .OnceAsync<Actuator>();

            if (actuators == null)
            {
                return NotFound($"Esp sem atuadores cadastrados");
            }

            // Retorna o a lista atuadores com suas propriedades
            var actuatorsList = actuators.Select(b => new Actuator
            {
                Id = b.Key,
                Name = b.Object.Name,
                OutputPin = b.Object.OutputPin,
                TypeActuator = b.Object.TypeActuator,
                EspId = b.Object.EspId,
                IsDigital = b.Object.IsDigital,

            }).ToList();

            return Ok(actuators);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEsp(string id)
        {
            // Verifica se o ESP existe
            var existingEsp = await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .OnceSingleAsync<ESP32>();

            if (existingEsp == null)
            {
                return NotFound("ESP não encontrado.");
            }

            // Verifica se existem sensores vinculados ao ESP
            var sensors = await _firebaseClient
                .Child("/devices/sensors")
                .OrderBy("EspId")
                .EqualTo(id)
                .OnceAsync<Sensor>();

            if (sensors != null && sensors.Any())
            {
                return BadRequest("Este ESP não pode ser excluído porque possui sensores vinculados.");
            }

            // Verifica se existem atuadores vinculados ao ESP
            var actuators = await _firebaseClient
                .Child("/devices/actuators")
                .OrderBy("EspId")
                .EqualTo(id)
                .OnceAsync<Actuator>();

            if (actuators != null && actuators.Any())
            {
                return BadRequest("Este ESP não pode ser excluído porque possui atuadores vinculados.");
            }

            // Remove o ESP
            await _firebaseClient
                .Child(CollectionName)
                .Child(id)
                .DeleteAsync();

            return NoContent();
        }

    }
}
