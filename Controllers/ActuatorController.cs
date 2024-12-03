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
                .Child($"/{userUid}/{espId}")
                .OnceSingleAsync<object>();

            return espExists != null;
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

                var actuators = await firebaseClient
                    .Child(userUid)
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

                var actuator = await firebaseClient
                    .Child(userUid)
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
