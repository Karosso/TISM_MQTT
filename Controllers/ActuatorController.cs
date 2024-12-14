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

        public ActuatorController(FirebaseClient firebaseClient)
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
            return new FirebaseClient(
                "https://smart-home-control-98900-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token)
                });
        }

        private async Task<bool> DoesEspExist(string userUid, string espId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var firebaseClient = await GetFirebaseClientWithToken(token);

            var espExists = await firebaseClient
                .Child($"/{userUid}/devices/{espId}")
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
                    .Child($"{userUid}/timestamp")
                    .PutAsync(timestampMilliseconds);

            }
            catch (Exception ex)
            {
                // Log de erro ou tratativa de exceção
                Console.WriteLine($"Error updating timestamp: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertActuator([FromBody] Actuator actuator)
        {
            var authResult = await CheckAuthentication();
            if (authResult != null) return authResult;

            if (string.IsNullOrEmpty(actuator.Id) || string.IsNullOrEmpty(actuator.EspId))
            {
                return BadRequest("Actuator ID and EspId are required.");
            }

            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var firebaseClient = await GetFirebaseClientWithToken(token);

                var userUid = await GetUserUidAsync();

                if (!await DoesEspExist(userUid, actuator.EspId))
                {
                    return NotFound($"ESP32 with ID '{actuator.EspId}' not found.");
                }

                await firebaseClient
                    .Child(userUid)
                    .Child("devices")
                    .Child(actuator.EspId)
                    .Child("actuators")
                    .Child(actuator.Id)
                    .PutAsync(actuator);

                UpdateTimestamp(userUid);

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

        [HttpDelete("{espId}/{id}")]
        public async Task<IActionResult> DeleteActuator(string espId, string id)
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

                var existingActuator = await firebaseClient
                    .Child(userUid)
                    .Child("devices")
                    .Child(espId)
                    .Child("actuators")
                    .Child(id)
                    .OnceSingleAsync<Actuator>();

                if (existingActuator == null)
                {
                    return NotFound($"Actuator with ID '{id}' not found.");
                }

                await firebaseClient
                    .Child(userUid)
                    .Child("devices")
                    .Child(espId)
                    .Child("actuators")
                    .Child(id)
                    .DeleteAsync();

                UpdateTimestamp(userUid);

                return NoContent();
            }
            catch (FirebaseException ex)
            {
                return StatusCode(500, $"Firebase error: {ex.Message}");
            }
        }
    }
}
