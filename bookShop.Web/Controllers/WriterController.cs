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
    [Authorize(Roles = "Admin,Editor")]
    public class WriterController : Controller
    {
        private readonly IWriterService _writerService;
        public WriterController(IWriterService writerService)
        {
            _writerService = writerService;
        }
        public async Task<IActionResult> Index()
        {
            /*var writers = await _writerService.GetAllEntitiesAsyncDto();
            return View(writers);*/

            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/writer/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var values1 = JsonConvert.DeserializeObject(jsonString).ToString();
            var values = JsonConvert.DeserializeObject<List<WriterListResponse>>(values1);

            return View(values);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddWriterRequest writer)
        {

            if (ModelState.IsValid)
            {
                /*var success = await _writerService.AddAsync(writer);*/

                var httpClient = getClient();
                var json = JsonConvert.SerializeObject(writer);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PostAsync("https://localhost:7084/api/writer/create",content);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
              //  return BadRequest();
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
        //    if (await _writerService.IsExistsAsync(id))
        //    {
        //        var book = await _writerService.GetEntityByIdAsyncDto(id);
        //        return View(book);
        //    }

            if (await _writerService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/writer/" + id);
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var writer1 = JsonConvert.DeserializeObject(jsonString).ToString();
                var writer = JsonConvert.DeserializeObject<UpdateWriterResponse>(writer1);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return View(writer);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Writer writer)
        {
            //if (ModelState.IsValid)
            //{
            //    var success = _writerService.Update(writer);
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
                var json = JsonConvert.SerializeObject(writer);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PutAsync("https://localhost:7084/api/writer/update", content);
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
            //if (await _writerService.IsExistsAsync(id))
            //{
            //    await _writerService.SoftDeleteAsync(id);
            //    return Json(true);
            //}

            if (await _writerService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.DeleteAsync("https://localhost:7084/api/writer/" + id);
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
