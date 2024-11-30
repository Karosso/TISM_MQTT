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

        public EspController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        private async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Simulação de validação do token. Substitua conforme necessário.
                if (string.IsNullOrEmpty(token))
                    return false;

                // Aqui você pode validar o token, por exemplo, verificando no Firebase Authentication.
                return true; // Retornar true se o token for válido.
            }
            catch
            {
                return false;
            }
        }

        private async Task<IActionResult> CheckAuthentication()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            var token = authorizationHeader?.Replace("Bearer ", string.Empty);

            if (string.IsNullOrEmpty(token) || !await ValidateTokenAsync(token))
            {
                return Unauthorized(new { Message = "Token de autenticação inválido ou ausente." });
            }

            return null; // Indica que a validação foi bem-sucedida.
        }

        private async Task<string> GetUserUidAsync()
        {
            var userUid = Request.Headers["user_uid"].ToString();
            if (string.IsNullOrEmpty(userUid))
            {
                throw new ArgumentException("O cabeçalho 'user_uid' é obrigatório.");
            }

            return userUid;
        }

        private async Task<FirebaseClient> GetFirebaseClientWithToken(string token)
        {
            var firebaseClient = new FirebaseClient(
                "https://smart-home-control-98900-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token)
                });

            return firebaseClient;
        }

        [HttpPost]
        public async Task<IActionResult> InsertESP32([FromBody] ESP32 esp32)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            if (string.IsNullOrEmpty(esp32.MacAddress))
                return BadRequest("ESP32 MAC address is required.");

            if (string.IsNullOrEmpty(esp32.Name))
                return BadRequest("ESP32 Name is required.");

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                var collectionPath = $"{userUid}/{esp32.Id}";

                var existingESP = await firebaseClient
                    .Child(collectionPath)
                    .OnceSingleAsync<ESP32>();

                if (existingESP != null)
                {
                    return Conflict(new { Error = "An ESP32 with the same id already exists." });
                }

                await firebaseClient
                    .Child(collectionPath)
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
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                var esps = await firebaseClient
                    .Child(userUid)
                    .OnceAsync<ESP32>();

                if (esps == null || !esps.Any())
                {
                    return NotFound(new { Message = "Nenhum ESP32 encontrado na coleção." });
                }

                var espList = esps.Select(b => new ESP32
                {
                    Id = b.Key,
                    Name = b.Object.Name,
                    MacAddress = b.Object.MacAddress,
                    Actuators = b.Object.Actuators,
                    Sensors = b.Object.Sensors,
                }).ToList();

                return Ok(espList);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEspById(string id)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);
                var userUid = await GetUserUidAsync();

                var collectionPath = $"{userUid}/{id}";

                var esp = await firebaseClient
                    .Child(collectionPath)
                    .Child(id)
                    .OnceSingleAsync<ESP32>();

                if (esp == null)
                {
                    return NotFound($"Esp com ID '{id}' não encontrado.");
                }

                var result = new ESP32
                {
                    Id = id,
                    Name = esp.Name,
                    MacAddress = esp.MacAddress,
                };

                return Ok(result);
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

        [HttpGet("{id}/sensors")]
        public async Task<IActionResult> GetEspSensorsById(string id)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                var collectionPath = $"{userUid}/{id}";

                var esp = await firebaseClient
                    .Child(collectionPath)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEsp(string id)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                var esp = await firebaseClient
                    .Child(userUid)
                    .Child(id)
                    .OnceSingleAsync<ESP32>();

                if (esp == null)
                {
                    return NotFound("ESP não encontrado.");
                }

                if ((esp.Sensors != null && esp.Sensors.Any()) || (esp.Actuators != null && esp.Actuators.Any()))
                {
                    return StatusCode(StatusCodes.Status409Conflict,
                        "Este ESP não pode ser excluído porque possui sensores ou atuadores vinculados.");
                }

                await firebaseClient
                    .Child(userUid)
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
