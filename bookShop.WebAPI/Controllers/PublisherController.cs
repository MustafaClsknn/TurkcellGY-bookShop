using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace bookShop.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PublisherController : ControllerBase
    {
        private readonly IPublisherService _publisherService;
        private readonly IDistributedCache _cache;
        public PublisherController(IPublisherService publisherService, IDistributedCache cache)
        {
            _publisherService = publisherService;
            _cache = cache;
        }
        [HttpGet]
        [Route("GetAllEntitiesAsyncDto")]
        public async Task<IActionResult> GetAllEntitiesAsyncDto()
        {
            var values = await _cache.GetStringAsync("publishers");
            if (values == null)
            {
                var publishers = await _publisherService.GetAllEntitiesAsyncDto();
                values = JsonConvert.SerializeObject(publishers);
                var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
                _cache.SetString("publishers", values, option);
            }
            return Ok(values);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(AddPublisherRequest publisher)
        {

            var success = await _publisherService.AddAsync(publisher);
            if (success)
            {
                return Ok(true);
            }
            return BadRequest();

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntityByIdAsyncDto(int id)
        {
            if (await _publisherService.IsExistsAsync(id))
            {
                var book = await _publisherService.GetEntityByIdAsyncDto(id);
                var json = JsonConvert.SerializeObject(book);
                return Ok(json);
            }
            return NotFound();
        }

        [HttpPut]
        [Route("Update")]
        public IActionResult Update(Publisher publisher)
        {

            var success = _publisherService.Update(publisher);
            if (success)
            {
                return Ok(true);
            }
            return BadRequest();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _publisherService.IsExistsAsync(id))
            {
                await _publisherService.SoftDeleteAsync(id);
                return Ok(true);
            }

            return NotFound();
        }
    }
}
