using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace bookShop.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WriterController : ControllerBase
    {
        private readonly IWriterService _writerService;
        private readonly IDistributedCache _cache;
        public WriterController(IWriterService writerService, IDistributedCache cahce)
        {
            _writerService = writerService;
            _cache = cahce;
        }
        [HttpGet]
        [Route("GetAllEntitiesAsyncDto")]
        public async Task<IActionResult> GetAllEntitiesAsyncDto()
        {
            var values = await _cache.GetStringAsync("writers");
            if (values == null)
            {
                var writers = await _writerService.GetAllEntitiesAsyncDto();
                values = JsonConvert.SerializeObject(writers);
                var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
                _cache.SetString("writers", values, option);
            }
            return Ok(values);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(AddWriterRequest writer)
        {

            var success = await _writerService.AddAsync(writer);
            if (success)
            {
                var writers = await _writerService.GetAllEntitiesAsyncDto();
                var values = JsonConvert.SerializeObject(writers);
                var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
                _cache.SetString("writers", values, option);
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntityByIdAsyncDto(int id)
        {
            if (await _writerService.IsExistsAsync(id))
            {
                var book = await _writerService.GetEntityByIdAsyncDto(id);
                var json = JsonConvert.SerializeObject(book);
                return Ok(json);
            }
            return NotFound();
        }

        [HttpPut]
        [Route("Update")]
        public async Task<IActionResult> Update(Writer writer)
        {

            var success = _writerService.Update(writer);
            if (success)
            {
                var writers = await _writerService.GetAllEntitiesAsyncDto();
                var values = JsonConvert.SerializeObject(writers);
                var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
                _cache.SetString("writers", values, option);
                return Ok();
            }
            return BadRequest();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _writerService.IsExistsAsync(id))
            {
                await _writerService.SoftDeleteAsync(id);
                var writers = await _writerService.GetAllEntitiesAsyncDto();
                var values = JsonConvert.SerializeObject(writers);
                var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
                _cache.SetString("writers", values, option);
                return Ok(true);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("GetWriters")]
        public async Task<IActionResult> GetWritersForDropDown()
        {
            var selectedWriter = new List<SelectListItem>();
            var items = await _writerService.GetAllEntitiesAsync();
            foreach (var item in items)
            {
                selectedWriter.Add
                    (
                    new SelectListItem { Text = item.FullName, Value = item.Id.ToString() }
                    );
            }
            var json = JsonConvert.SerializeObject(selectedWriter);
            return Ok(json);
        }
    }
}
