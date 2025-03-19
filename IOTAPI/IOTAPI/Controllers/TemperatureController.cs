using IOTAPI.Data.Repos;
using IOTAPI.Hubs;
using IOTAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IOTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureController : ControllerBase
    {
        private readonly TemperatureRepo _repo;
        private readonly IHubContext<TemperatureHub> _hubContext;

        public TemperatureController(TemperatureRepo repo, IHubContext<TemperatureHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Очистить все данные о температуре
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteAll")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAll()
        {
            await _repo.DeleteSeries("temperature");

            return NoContent();
        }

        /// <summary>
        /// Отправить данные о температуре
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        [HttpGet("Send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Send(double temperature)
        {
            await _repo.CreateSeriesIfDosentExistAsync("temperature");

            Random random = new Random();
            await _repo.AddAsync("temperature", temperature);

            Temperature? newItem = await _repo.GetLastAsync("temperature");

            await _hubContext.Clients.All.SendAsync("TemperatureChanged", newItem);

            return Ok();
        }

        /// <summary>
        /// Получить последнее значение температуры
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetLast")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLast()
        {
            Temperature? item = await _repo.GetLastAsync("temperature");

            return Ok(item);
        }

        /// <summary>
        /// Получить последние 10 значений температуры
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetTop10")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTop10()
        {
            var items = await _repo.GetLastItemsAsync("temperature", 10);

            return Ok(items);
        }

        /// <summary>
        /// Получить среднюю часовую темпратуру за сегодня
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetForToday")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetForToday()
        {
            DateTime now = DateTime.Now;

            DateTime startDate = new DateTime(now.Year, now.Month, now.Day);
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            double duration = TimeSpan.FromHours(1).TotalMilliseconds;
            var items = await _repo.GetAverageByDuration("temperature", startDate, endDate, (long)duration);

            return Ok(items);
        }

        /// <summary>
        /// Получить среднюю дневную температуру за последние 7 дней
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetForWeek")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetForWeek()
        {
            DateTime now = DateTime.Now;

            DateTime startDate = now.AddDays(-7);
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            double duration = TimeSpan.FromDays(1).TotalMilliseconds;
            var items = await _repo.GetAverageByDuration("temperature", startDate, endDate, (long)duration);

            return Ok(items);
        }

        /// <summary>
        /// Получить среднюю дневную температуру за последний месяц
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetForMonth")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetForMonth()
        {
            DateTime now = DateTime.Now;

            DateTime startDate = now.AddMonths(-1);
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            double duration = TimeSpan.FromDays(1).TotalMilliseconds;
            var items = await _repo.GetAverageByDuration("temperature", startDate, endDate, (long)duration);

            return Ok(items);
        }
    }
}
