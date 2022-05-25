using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace bookShop.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IDistributedCache _cache;
        public CategoryController(ICategoryService categoryService, IDistributedCache cache)
        {
            _categoryService = categoryService;
            _cache = cache;
        }
        [HttpGet]
        [Route("GetAllEntitiesAsyncDto")]
        public async Task<IActionResult> GetAllEntitiesAsyncDto()
        {
            var values = await _cache.GetStringAsync("categories");
            if (values == null)
            {
                var categories = await _categoryService.GetAllEntitiesAsyncDto();
                values = JsonConvert.SerializeObject(categories);
                var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
                _cache.SetString("categories", values, option);
            }
            return Ok(values);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(AddCategoryRequest category)
        {

            var success = await _categoryService.AddAsync(category);
            if (success)
            {
                return Ok();
            }
            return NotFound();

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntityByIdAsyncDto(int id)
        {
            if (await _categoryService.IsExistsAsync(id))
            {
                var book = await _categoryService.GetEntityByIdAsyncDto(id);
                var json = JsonConvert.SerializeObject(book);
                return Ok(json);
            }
            return NotFound();
        }

        [HttpPut]
        [Route("Update")]
        public IActionResult Update(Category category)
        {

                var success = _categoryService.Update(category);
                if (success)
                {
                    return Ok();
                }
                return BadRequest();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _categoryService.IsExistsAsync(id))
            {
                if (await _categoryService.SoftDeleteAsync(id))
                {
                    return Ok(true);
                }

            }

            return NotFound();
        }
        [HttpGet]
        [Route("GetCategories")]
        public async Task<IActionResult> GetCategoriesForDropDown()
        {

            List<SelectListItem> selectedCategory = new List<SelectListItem>();
            var items = await _categoryService.GetAllEntitiesAsync();
            foreach (var item in items)
            {
                selectedCategory.Add
                    (
                    new SelectListItem { Text = item.Name, Value = item.Id.ToString() }
                    );
            }
            var json = JsonConvert.SerializeObject(selectedCategory);
            return Ok(json);
        }
    }
}
