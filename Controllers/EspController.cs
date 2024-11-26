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
        private const string CollectionName = "/devices";

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
                var existingESP = await _firebaseClient
                    .Child("devices")
                    .Child(esp32.Id)
                    .OnceSingleAsync<ESP32>();

                if (existingESP != null)
                {
                    return Conflict(new { Error = "An ESP32 with the same id already exists." });
                }

                // Adiciona o novo ESP32 ao Firebase
                await _firebaseClient
                    .Child("devices")
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
            try
            {
                var esp = await _firebaseClient
                    .Child(CollectionName)
                    .Child(id)
                    .OnceSingleAsync<ESP32>();

                if (esp == null)
                {
                    return NotFound($"ESP com ID '{id}' não encontrado.");
                }

                var sensorList = esp.GetSensorList();
                if (!sensorList.Any())
                {
                    return NotFound($"Nenhum sensor encontrado para o ESP com ID '{id}'.");
                }

                return Ok(sensorList);
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Erro no Firebase: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no servidor: {ex.Message}");
            }
        }



        [HttpGet("{id}/actuators")]
        public async Task<IActionResult> GetEspActuatorsById(string id)
        {
            try
            {
                var esp = await _firebaseClient
                    .Child(CollectionName)
                    .Child(id)
                    .OnceSingleAsync<ESP32>();

                if (esp == null)
                {
                    return NotFound($"ESP com ID '{id}' não encontrado.");
                }

                var actuatorList = esp.GetActuatorList();
                if (!actuatorList.Any())
                {
                    return NotFound($"Nenhum atuador encontrado para o ESP com ID '{id}'.");
                }

                return Ok(actuatorList);
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Erro no Firebase: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no servidor: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEsp(string id)
        {
            try
            {
                var esp = await _firebaseClient
                    .Child(CollectionName)
                    .Child(id)
                    .OnceSingleAsync<ESP32>();

                if (esp == null)
                {
                    return NotFound("ESP não encontrado.");
                }

                if ((esp.Sensors != null && esp.Sensors.Any()) || (esp.Actuators != null && esp.Actuators.Any()))
                {
                    return BadRequest("Este ESP não pode ser excluído porque possui sensores ou atuadores vinculados.");
                }

                await _firebaseClient
                    .Child(CollectionName)
                    .Child(id)
                    .DeleteAsync();

                return NoContent();
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Erro no Firebase: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no servidor: {ex.Message}");
            }
        }


    }
}
