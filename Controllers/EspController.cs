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

        private async Task<IActionResult> CheckAuthentication()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            var token = authorizationHeader?.Replace("Bearer ", string.Empty);

            if (string.IsNullOrEmpty(token))
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

        private async Task UpdateTimestamp(string userUid)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                long timestampMilliseconds;
                var timestampString = DateTime.UtcNow.ToString("o");
                var dateTimeOffset = DateTimeOffset.Parse(timestampString);
                timestampMilliseconds = dateTimeOffset.ToUnixTimeMilliseconds();

                await firebaseClient
                    .Child($"users/{userUid}/timestamp")
                    .PutAsync(timestampMilliseconds);

            }
            catch (Exception ex)
            {
                // Log de erro ou tratativa de exceção
                Console.WriteLine($"Error updating timestamp: {ex.Message}");
            }
        }

        [HttpGet("timestamp")]
        public async Task<IActionResult> GetTimestamp()
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                // Obtém o timestamp armazenado no Firebase
                var timestampMilliseconds = await firebaseClient
                    .Child($"{userUid}/timestamp")
                    .OnceSingleAsync<long>();

                if (timestampMilliseconds == 0)
                {
                    return NotFound(new { Error = "Timestamp not found." });
                }

                return Ok(timestampMilliseconds);
            }
            catch (Exception ex)
            {
                // Log de erro ou tratativa de exceção
                Console.WriteLine($"Error retrieving timestamp: {ex.Message}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
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

                var collectionPath = $"users/{userUid}/devices/{esp32.Id}";

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

                UpdateTimestamp(userUid);

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
                    .Child($"users/{userUid}/devices")
                    .OnceAsync<ESP32>();

                if (esps == null || !esps.Any())
                {
                    return NotFound(new { Message = "Nenhum ESP32 encontrado na coleção." });
                }

                var espList = esps.Select(b => new
                {
                    Id = b.Key,
                    Name = b.Object.Name,
                    MacAddress = b.Object.MacAddress,
                    Actuators = b.Object.Actuators?.Values.ToList(),
                    Sensors = b.Object.Sensors?.Values.ToList(),
                }).ToList();

                return Ok(espList);
            }
            catch (FirebaseException ex)
            {
                if (ex.InnerException.Message.Contains("401"))
                {
                    return Unauthorized(new { Message = "Token inválido ou sem permissão para acessar os dados." });
                }
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
                    .Child("users/")
                    .Child(userUid)
                    .Child("devices")
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
                    .Child("users/")
                    .Child(userUid)
                    .Child("devices")
                    .Child(id)
                    .DeleteAsync();

                UpdateTimestamp(userUid);

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

        [HttpGet("devices")]
        public async Task<IActionResult> GetDevices()
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                // Busca todos os ESPs do usuário
                var esps = await firebaseClient
                    .Child("users/")
                    .Child(userUid)
                    .Child("devices")
                    .OnceAsync<ESP32>();

                if (esps == null || !esps.Any())
                {
                    return NotFound(new { Message = "Nenhum ESP32 encontrado." });
                }

                // Consolida listas de sensores e atuadores
                var sensors = new List<Sensor>();
                var actuators = new List<Actuator>();

                foreach (var esp in esps)
                {
                    if (esp.Object.Sensors != null)
                    {
                        foreach (var sensor in esp.Object.Sensors.Values)
                        {
                            // Busca os dados mais recentes para o sensor
                            var sensorDataPath = $"data/{esp.Object.MacAddress}/sensors/{sensor.Id}";
                            var sensorData = await firebaseClient
                                .Child(sensorDataPath)
                                .OnceAsync<SensorData>();

                            var latestSensorData = sensorData?
                                .OrderByDescending(sd => sd.Object.Timestamp)
                                .FirstOrDefault()?.Object;

                            // Atualiza o sensor com os últimos dados
                            sensor.LastData = latestSensorData;
                            sensors.Add(sensor);
                        }
                    }

                    if (esp.Object.Actuators != null)
                    {
                        foreach (var actuator in esp.Object.Actuators.Values)
                        {
                            // Busca os dados mais recentes para o atuador
                            var actuatorDataPath = $"data/{esp.Object.MacAddress}/actuators/{actuator.Id}";
                            var actuatorData = await firebaseClient
                                .Child(actuatorDataPath)
                                .OnceAsync<ActuatorData>();

                            var latestActuatorData = actuatorData?
                                .OrderByDescending(ad => ad.Object.Timestamp)
                                .FirstOrDefault()?.Object;

                            // Atualiza o atuador com os últimos dados
                            actuator.LastData = latestActuatorData;
                            actuator.macAddress = esp.Object.MacAddress;
                            actuators.Add(actuator);
                        }
                    }
                }

                return Ok(new
                {
                    Sensors = sensors,
                    Actuators = actuators
                });
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

        [HttpGet("sensors")]
        public async Task<IActionResult> GetSensors()
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                // Busca todos os ESPs do usuário
                var esps = await firebaseClient
                    .Child($"users/{userUid}/devices")
                    .OnceAsync<ESP32>();

                if (esps == null || !esps.Any())
                {
                    return NotFound(new { Message = "Nenhum sensor encontrado, pois não há ESP32s cadastrados." });
                }

                // Consolida todos os sensores
                var sensors = esps
                    .Where(esp => esp.Object.Sensors != null)
                    .SelectMany(esp => esp.Object.Sensors.Values)
                    .ToList();

                if (!sensors.Any())
                {
                    return NotFound(new { Message = "Nenhum sensor encontrado nos ESP32s cadastrados." });
                }

                return Ok(sensors);
            }
            catch (FirebaseException ex)
            {
                if (ex.InnerException?.Message.Contains("401") == true)
                {
                    return Unauthorized(new { Message = "Token inválido ou sem permissão para acessar os dados." });
                }
                return StatusCode(500, $"Erro no Firebase: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no servidor: {ex.Message}");
            }
        }


    }
}
