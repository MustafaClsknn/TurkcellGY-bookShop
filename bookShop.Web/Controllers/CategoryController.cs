using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Dtos.Responses;
using bookShop.Entities.Concrete;
using bookShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace bookShop.Web.Controllers
{
    [Authorize(Roles ="Admin,Editor")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            /*var categories = await _categoryService.GetAllEntitiesAsyncDto();
            return View(categories);*/

            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/category/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            //     var values1 = JsonConvert.DeserializeObject(jsonString).ToString();
            var values = JsonConvert.DeserializeObject<List<CategoryListResponse>>(JsonConvert.DeserializeObject<string>(jsonString));

            return View(values);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddCategoryRequest category)
        {

            if (ModelState.IsValid)
            {
                /* var success = await _categoryService.AddAsync(category); */
                if (true)
                {
                    var json = JsonConvert.SerializeObject(category);
                    var httpClient = getClient();
                    StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var responseMessage = await httpClient.PostAsync("https://localhost:7084/api/category/create", httpContent);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                return BadRequest();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
             /* if (await _categoryService.IsExistsAsync(id))
              {
                  var book = await _categoryService.GetEntityByIdAsyncDto(id);
                  return View(book);
              }
              return NotFound();*/
            if (await _categoryService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/category/" + id);
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var category1 = JsonConvert.DeserializeObject(jsonString).ToString();
                var category = JsonConvert.DeserializeObject<UpdateCategoryResponse>(category1);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return View(category);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            //if (ModelState.IsValid)
            //{
            //    var success = _categoryService.Update(category);
            //    if (success)
            //    {
            //        return RedirectToAction(nameof(Index));
            //    }
            //    return BadRequest();
            //}
            //return View();

            if (ModelState.IsValid)
            {
                var httpClient = getClient();
                var json = JsonConvert.SerializeObject(category);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PutAsync("https://localhost:7084/api/category/update", content);
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
            //if (await _categoryService.IsExistsAsync(id))
            //{
            //    if (await _categoryService.SoftDeleteAsync(id))
            //    {
            //        return Json(true);
            //    }

            //}
            if (await _categoryService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.DeleteAsync("https://localhost:7084/api/category/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return Json(true);
                }
            }

            return NotFound();
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
