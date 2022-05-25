using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Dtos.Responses;
using bookShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace bookShop.Web.Controllers
{
    [Authorize(Roles ="Admin,Editor")]
    public class BookController : Controller
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
        [AllowAnonymous]
        public async Task<IActionResult> Index(string[] category, string[] publishers, int page = 1)
        {
            IList<BookListResponse> books;
            if (category.Length != 0 || publishers.Length != 0)
            {
                books = _bookService.SearchEntitiesByNameAsync(category, publishers);
            }
            else
            {
                books = await _bookService.GetAllEntitiesAsyncDto();
            }

            var booksPerPage = 3;
            var paginatedBooks = books.OrderByDescending(p => p.Id)
                                            .Skip((page - 1) * booksPerPage)
                                            .Take(booksPerPage);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = Math.Ceiling((decimal)books.Count / booksPerPage);

            return View(paginatedBooks);
        }
        public async Task<IActionResult> BookList()
        {
            /*
                var books = await _bookService.GetAllEntitiesAsync();
                return View(books); 
            */

            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/book/booklist");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();

            var values1 = JsonConvert.DeserializeObject(jsonString).ToString();
            var values2 = JsonConvert.DeserializeObject<List<BookListResponse>>(values1);

            return View(values2);
        }
        public async Task<IActionResult> DetailBook(int id)
        {
            var book = await _bookService.GetEntityByIdAsync(id);
            return View(book);


        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/book/booklist");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<BookListResponse>>(jsonString);
            ViewBag.SelectedCategory = await GetCategoriesForDropDown();
            ViewBag.SelectedWriter = await GetWritersForDropDown();
            ViewBag.SelectedPublisher = await GetPublishersForDropDown();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddBookRequest book, string categories)
        {

            var success = await _bookService.CreateBook(book, categories);
            if (success)
            {
                return Json(true);
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.SelectedCategory = await GetCategoriesForDropDown();
            ViewBag.SelectedWriter = await GetWritersForDropDown();
            ViewBag.SelectedPublisher = await GetPublishersForDropDown();
            if (await _bookService.IsExistsAsync(id))
            {
               /* var book = await _bookService.GetEntityByIdAsyncDto(id);
                return View(book); */

                var httpClient = getClient();
                var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/book/" + id);
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var book1 = JsonConvert.DeserializeObject(jsonString).ToString();
                var book = JsonConvert.DeserializeObject<UpdateBookResponse>(book1);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return View(book);
                }

                return NotFound();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateBookResponse book)
        {
            //if (ModelState.IsValid)
            //{
            //    var success = _bookService.UpdateDto(book);
            //    if (success)
            //    {
            //        return RedirectToAction(nameof(Index));
            //    }
            //    return BadRequest();
            //}
            //ViewBag.Categories = await GetCategoriesForDropDown();
            //return View();
            if (ModelState.IsValid) 
            {
                var httpClient = getClient();
                var json = JsonConvert.SerializeObject(book);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PutAsync("https://localhost:7084/api/book", content);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                return BadRequest();
            }
            return View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (await _bookService.IsExistsAsync(id))
            {
                await _bookService.SoftDeleteAsync(id);
                return Json(true);
            }

            return NotFound();
        }

        public async Task<List<SelectListItem>> GetCategoriesForDropDown()
        {
            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/category/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<CategoryListResponse>>(JsonConvert.DeserializeObject<string>(jsonString));

            List<SelectListItem> selectedCategory = new List<SelectListItem>();
            //var items = await _categoryService.GetAllEntitiesAsync();
            foreach (var item in items)
            {
                selectedCategory.Add
                    (
                    new SelectListItem { Text = item.Name, Value = item.Id.ToString() }
                    );
            }
            return selectedCategory;
        }
        public async Task<List<SelectListItem>> GetWritersForDropDown()
        {
            var selectedWriter = new List<SelectListItem>();

            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/writer/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<WriterListResponse>>(JsonConvert.DeserializeObject<string>(jsonString));
            //var items = await _writerService.GetAllEntitiesAsync();
            foreach (var item in items)
            {
                selectedWriter.Add
                    (
                    new SelectListItem { Text = item.FullName, Value = item.Id.ToString() }
                    );
            }
            return selectedWriter;
        }
        public async Task<List<SelectListItem>> GetPublishersForDropDown()
        {

            var selectedPublisher = new List<SelectListItem>();

            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/publisher/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<PublisherListResponse>>(JsonConvert.DeserializeObject<string>(jsonString));

            //var items = await _publisherService.GetAllEntitiesAsync();

            foreach (var item in items)
            {
                selectedPublisher.Add
                    (
                    new SelectListItem { Text = item.Name, Value = item.Id.ToString() }
                    );
            }
            return selectedPublisher;
        }
        public HttpClient getClient()
        {
            var httpClient = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(contentType);
            string s = HttpContext.Session.GetString("token");
            JWT jwt = JsonConvert.DeserializeObject<JWT>(s);
            /* httpClient.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue("Bearer", jwt.Token);*/
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt.Token);
            return httpClient;
        }
    }
}
