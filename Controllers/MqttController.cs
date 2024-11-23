using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TISM_MQTT.Services;

namespace TISM_MQTT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MqttController : ControllerBase
    {
        private readonly MqttClientService _mqttClientService;

        public MqttController(MqttClientService mqttClientService)
        {
            _mqttClientService = mqttClientService;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessageAsync([FromBody] PublishMessageRequest request)
        {
            await _mqttClientService.PublishMessageAsync(request.Topic, request.Message);
            return Ok();
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToTopicAsync([FromBody] SubscribeRequest request)
        {
            await _mqttClientService.SubscribeToTopicAsync(request.Topic);
            return Ok();
        }
    }

    public class PublishMessageRequest
    {
        public string Topic { get; set; }
        public string Message { get; set; }
    }

    public class SubscribeRequest
    {
        public string Topic { get; set; }
    }
}
