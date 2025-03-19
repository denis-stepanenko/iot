using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using IOTAPI.Hubs;
using IOTAPI.Services;
using IOTAPI.Models;
using IOTAPI.ApiKeyAuthorization;
using IOTAPI.DTO;
using IOTAPI.Data.Repos;

namespace IOTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestHTTPController : ControllerBase
    {
        private readonly TemperatureRepo _repo;
        private readonly IHubContext<TestHTTPHub> _hubContext;
        public TestHTTPController(TemperatureRepo repo, IHubContext<TestHTTPHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;

        }

        /// <summary>
        /// Получение последних 10 отправленных данных
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetTop10")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTop10()
        {
            var items = await _repo.GetLastItemsAsync("test-http", 10);
            return Ok(items);
        }

        /// <summary>
        /// Проверка GET без авторизации
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendGet(decimal temperature)
        {
            await _repo.AddAsync("test-http", (double)temperature);
            
            var newItem = await _repo.GetLastAsync("test-http");

            await _hubContext.Clients.All.SendAsync("TemperatureChanged", newItem);

            return Ok();
        }

        /// <summary>
        /// Проверка POST без авторизации
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendPost(TemperatureDTO temperature)
        {
            await _repo.AddAsync("test-http", (double)temperature.Value);

            var newItem = await _repo.GetLastAsync("test-http");

            await _hubContext.Clients.All.SendAsync("TemperatureChanged", newItem);

            return Ok();
        }

        /// <summary>
        /// Проверка GET с авторизацией через API ключ
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        [ApiKey]
        [HttpGet("Secured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendGetTest(decimal temperature)
        {
            await _repo.AddAsync("test-http", (double)temperature);

            var newItem = await _repo.GetLastAsync("test-http");

            await _hubContext.Clients.All.SendAsync("TemperatureChanged", newItem);

            return Ok();
        }

        /// <summary>
        /// Проверка POST с авторизацией через API ключ
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        [ApiKey]
        [HttpPost("Secured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendPostTest(TemperatureDTO temperature)
        {
            await _repo.AddAsync("test-http", (double)temperature.Value);

            var newItem = await _repo.GetLastAsync("test-http");

            await _hubContext.Clients.All.SendAsync("TemperatureChanged", newItem);

            return Ok();
        }
    }
}
