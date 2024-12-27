using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TISM_MQTT.Models;

namespace TISM_MQTT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly FirebaseClient _firebaseClient;

        public AlertController(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        // Cria um alerta no Firebase
        [HttpPost]
        private async Task<IActionResult> CreateAlert([FromBody] Alert alert)
        {
            if (string.IsNullOrEmpty(alert.SensorId) || alert.Value == null)
            {
                return BadRequest("SensorId and Value are required.");
            }


            try
            {
                // Salva o alerta no Firebase
                await _firebaseClient
                    .Child($"alerts/{alert.SensorId}")
                    .PutAsync(alert);

                return Ok(new { Message = "Alert created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating alert: {ex.Message}");
            }
        }

        // Edita um alerta existente no Firebase
        [HttpPut("{sensorId}")]
        public async Task<IActionResult> EditAlert(string sensorId, [FromBody] Alert alert)
        {
            if (string.IsNullOrEmpty(alert.SensorId) || alert.Value == null)
            {
                return BadRequest("SensorId and Value are required.");
            }

            try
            {
                // Atualiza o alerta no Firebase diretamente no nó de alerts
                await _firebaseClient
                    .Child("alerts")
                    .Child(sensorId)
                    .PutAsync(alert);

                return Ok(new { Message = "Alert updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error editing alert: {ex.Message}");
            }
        }

        // Exclui um alerta do Firebase
        [HttpDelete("{sensorId}")]
        public async Task<IActionResult> DeleteAlert(string sensorId)
        {
            try
            {
                // Exclui o alerta no Firebase
                await _firebaseClient
                    .Child("alerts")
                    .Child(sensorId)
                    .DeleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting alert: {ex.Message}");
            }
        }

        // Recupera todos os alertas do Firebase
        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            try
            {
                // Obtém todos os alertas do Firebase
                var alerts = await _firebaseClient
                    .Child("alerts")
                    .OnceAsync<Alert>();

                // Mapeia os alertas do Firebase para uma lista de objetos Alert
                var alertList = alerts.Select(a => new Alert
                {
                    SensorId = a.Object.SensorId,
                    Value = a.Object.Value,
                    SensorName = a.Object.SensorName
                }).ToList();

                return Ok(alertList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving alerts: {ex.Message}");
            }
        }



    }

}
