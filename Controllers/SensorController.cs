using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;

namespace TISM_MQTT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;

        public SensorController(FirebaseClient firebaseClient)
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

        private async Task<bool> DoesEspExist(string userUid, string espId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var firebaseClient = await GetFirebaseClientWithToken(token);

            var espExists = await firebaseClient
                .Child($"users/{userUid}/devices/{espId}")
                //.Child(espId)
                .OnceSingleAsync<object>();

            return espExists != null;
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

        [HttpPost]
        public async Task<IActionResult> InsertSensor([FromBody] Sensor sensor)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            if (string.IsNullOrEmpty(sensor.Id) || string.IsNullOrEmpty(sensor.EspId))
            {
                return BadRequest("Sensor ID and EspId is required.");
            }

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                if (!await DoesEspExist(userUid, sensor.EspId))
                {
                    return NotFound($"ESP32 with ID '{sensor.EspId}' not found.");
                }

                await firebaseClient
                    .Child("users/")
                    .Child(userUid)
                    .Child("devices")
                    .Child(sensor.EspId)
                    .Child("sensors")
                    .Child(sensor.Id)
                    .PutAsync(sensor);

                UpdateTimestamp(userUid);

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

        [HttpDelete("{espId}/{id}")]
        public async Task<IActionResult> DeleteSensor(string espId, string id)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                if (!await DoesEspExist(userUid, espId))
                {
                    return NotFound($"ESP32 with ID '{espId}' not found.");
                }

                var existingSensor = await firebaseClient
                    .Child("users/")
                    .Child(userUid)
                    .Child("devices")
                    .Child(espId)
                    .Child("sensors")
                    .Child(id)
                    .OnceSingleAsync<Sensor>();

                UpdateTimestamp(userUid);

                if (existingSensor == null)
                {
                    return NotFound($"Sensor with ID '{id}' not found.");
                }

                await firebaseClient
                    .Child("users/")
                    .Child(userUid)
                    .Child("devices")
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
