using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace bookShop.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;
        private readonly IWriterService _writerService;
        private readonly IPublisherService _publisherService;

        public BookController(IBookService bookService, ICategoryService categoryService, IWriterService writerService, IPublisherService publisherService)
        {
            _bookService = bookService;
            _categoryService = categoryService;
            _writerService = writerService;
            _publisherService = publisherService;
        }

        [HttpGet]
        [Route("BookList")]

        public async Task<IActionResult> BookList()
        {
            var books = await _bookService.GetAllEntitiesAsync();
            var json = JsonConvert.SerializeObject(books, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return Ok(json);
        }
        [HttpGet]
        [Route("DetailBook/{id}")]
        public async Task<IActionResult> DetailBook(int id)
        {
            var book = await _bookService.GetEntityByIdAsync(id);
            var json = JsonConvert.SerializeObject(book, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return Ok(json);
        }


        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(AddBookRequest book, string categories)
        {
            var success = await _bookService.CreateBook(book, categories);
            if (success)
            {
                return Ok(true);
            }

            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntityByIdAsyncDto(int id)
        {
            var book = await _bookService.GetEntityByIdAsyncDto(id);
            if (book != null)
            {
                var json = JsonConvert.SerializeObject(book, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Ok(json);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPut]
        public IActionResult UpdateDto(UpdateBookResponse book)
        {
            var success = _bookService.UpdateDto(book);
            if (success)
            {
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _bookService.IsExistsAsync(id))
            {
                await _bookService.SoftDeleteAsync(id);
                return Ok(true);
            }

            return NotFound();
        }

     
        [HttpGet]
        [Route("GetPublisher")]
        public async Task<IActionResult> GetPublishersForDropDown()
        {

            var selectedPublisher = new List<SelectListItem>();
            var items = await _publisherService.GetAllEntitiesAsync();
            foreach (var item in items)
            {
                selectedPublisher.Add
                    (
                    new SelectListItem { Text = item.Name, Value = item.Id.ToString() }
                    );
            }
            var json = JsonConvert.SerializeObject(selectedPublisher);
            return Ok(json);
        }
    }
}
