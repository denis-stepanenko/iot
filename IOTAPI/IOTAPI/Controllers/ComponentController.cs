using IOTAPI.Data.Repos;
using IOTAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOTAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentController : ControllerBase
    {
        private readonly ComponentRepo _repo;
        private readonly ReadingRepo _readingRepo;

        public ComponentController(ComponentRepo repo, ReadingRepo readingRepo)
        {
            _repo = repo;
            _readingRepo = readingRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Component> components = await _repo.GetAllAsync();

            foreach (Component component in components)
            {
                if(component is Indicator indicator)
                {
                    var readings = await _readingRepo.GetAllAsync(component.Topic, 10);
                    indicator.Readings = readings;
                }
            }

            return Ok(components);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Component item)
        {
            var component = await _repo.GetAsync(item.Topic);
            if(component != null)
                return BadRequest("Элемент с такой темой уже существует");

            await _readingRepo.CreateSeriesAsync(item.Topic);

            item.CreatedAt = DateTime.Now;
            
            await _repo.CreateAsync(item);
            
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            await _readingRepo.DeleteSeriesAsync(id);

            await _repo.DeleteAsync(id);
            
            return NoContent();
        }
    }
}
