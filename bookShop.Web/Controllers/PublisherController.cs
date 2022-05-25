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
    public class PublisherController : Controller
    {
        private readonly IPublisherService _publisherService;
        public PublisherController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }
        public async Task<IActionResult> Index()
        {
           /* var publisher = await _publisherService.GetAllEntitiesAsyncDto();
            return View(publisher); */

            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/publisher/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var values1 = JsonConvert.DeserializeObject(jsonString).ToString();
            var values = JsonConvert.DeserializeObject<List<PublisherListResponse>>(values1);

            return View(values);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddPublisherRequest publisher)
        {

            if (ModelState.IsValid)
            {
              /*  var success = await _publisherService.AddAsync(publisher);*/

                var json = JsonConvert.SerializeObject(publisher);
                var httpClient = getClient();
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PostAsync("https://localhost:7084/api/publisher/create", httpContent);
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<bool>(jsonString);

                return RedirectToAction(nameof(Index));
           
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //if (await _publisherService.IsExistsAsync(id))
            //{
            //    var book = await _publisherService.GetEntityByIdAsyncDto(id);
            //    return View(book);
            //}
            //return NotFound();

            if (await _publisherService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/publisher/" + id);
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var publisher1 = JsonConvert.DeserializeObject(jsonString).ToString();
                var publisher = JsonConvert.DeserializeObject<UpdatePublisherResponse>(publisher1);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return View(publisher);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Publisher publisher)
        {
            //if (ModelState.IsValid)
            //{
            //    var success = _publisherService.Update(publisher);
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
                var json = JsonConvert.SerializeObject(publisher);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PutAsync("https://localhost:7084/api/publisher/update", content);
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
            //if (await _publisherService.IsExistsAsync(id))
            //{
            //    await _publisherService.SoftDeleteAsync(id);
            //    return Json(true);
            //}

            if (await _publisherService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.DeleteAsync("https://localhost:7084/api/publisher/" + id);
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
