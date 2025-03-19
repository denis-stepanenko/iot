using IOTAPI.Data.Repos;
using IOTAPI.DTO;
using IOTAPI.Hubs;
using IOTAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IOTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestWSController : ControllerBase
    {
        private readonly WebSocketService _websocketService;
        private readonly TemperatureRepo _repo;
        private readonly IHubContext<TestWSHub> _hubContext;

        public TestWSController(
            WebSocketService webSocketService, 
            TemperatureRepo repo, 
            IHubContext<TestWSHub> hubContext)
        {
            _websocketService = webSocketService;
            _repo = repo;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Получить список подключенных устройств
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("GetDevices")]
        public IActionResult GetDevices()
        {
            var devices = _websocketService.WebSockets.Select(x => new DeviceDTO { Name = x.Key });

            return Ok(devices);
        }

        /// <summary>
        /// Подключиться
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task Connect(string deviceName)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"Device {deviceName} connected.");

                var newDevice = new DeviceDTO { Name = deviceName };
                await _hubContext.Clients.All.SendAsync("Connected", newDevice);

                _websocketService.OnDisconnectedEvent += _websocketService_OnDisconnectedEvent;
                await _websocketService.HandleConnection(ws, deviceName);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsync("Expected a WebSocket request");
            }
        }

        private async Task _websocketService_OnDisconnectedEvent(object sender, DisconnectedEventArgs e)
        {
            await _hubContext.Clients.All.SendAsync("Disconnected", new DeviceDTO { Name = e.DeviceName });
        }

        /// <summary>
        /// Отправить команду клиенту
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("SendCommandToClient")]
        public async Task<IActionResult> SendCommandToClient(WSCommandDTO command)
        {
            if (!_websocketService.WebSockets.ContainsKey(command.DeviceName))
                return BadRequest("Нет подлюченного устройства с таким именем");

            var webSocket = _websocketService.WebSockets[command.DeviceName];

            await _websocketService.SendMessageAsync(webSocket, command.Text);

            return NoContent();
        }
    }
}

