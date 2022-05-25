using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Dtos.Responses;
using bookShop.Entities.Concrete;
using bookShop.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace bookShop.Web.Controllers
{
    [AllowAnonymous]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            /*var users = await _userService.GetAllEntitiesAsyncDto();
            return View(users);*/
            var httpClient = getClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/user/GetAllEntitiesAsyncDto");
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var values1 = JsonConvert.DeserializeObject(jsonString).ToString();
            var values = JsonConvert.DeserializeObject<List<UserListResponse>>(values1);

            return View(values);

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(AddUserRequest user)
        {
            if (ModelState.IsValid)
            {
                /*bool success = await _userService.AddAsync(user);
                return RedirectToAction(nameof(Login));*/
                var json = JsonConvert.SerializeObject(user);
                var httpClient = new HttpClient();
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PostAsync("https://localhost:7084/api/user/create", httpContent);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login","Users");
                }
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id) 
        {
            ViewBag.UserRole = GetUserRoleDropDown();
            // if (await _userService.IsExistsAsync(id))
            //{
            //    var user = await _userService.GetEntityByIdAsync(id);
            //    return View(user);
            //}
            //return NotFound();
            if (await _userService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.GetAsync("https://localhost:7084/api/user/" + id);
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var user1 = JsonConvert.DeserializeObject(jsonString).ToString();
                var user = JsonConvert.DeserializeObject<User>(user1);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return View(user);
                }
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(User user) 
        {
            //if (ModelState.IsValid)
            //{
            //    var success = _userService.Update(user);
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
                var json = JsonConvert.SerializeObject(user);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PutAsync("https://localhost:7084/api/user", content);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                return BadRequest();
            }
            return View();
        }

        [HttpGet]
        public IActionResult PanelRegister()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PanelRegister(AddUserRequest user)
        {
            if (ModelState.IsValid)
            {
                //bool success = await _userService.AddAsync(user);
                //return RedirectToAction(nameof(PanelLogin));

                var json = JsonConvert.SerializeObject(user);
                var httpClient = new HttpClient();
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PostAsync("https://localhost:7084/api/user/panelcreate", httpContent);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("PanelLogin", "Users");
                }
            }

            return View();
        }
        public IActionResult PanelLogin()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(User user, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                var user2 = await _userService.ValidateUser(user.UserName, user.Password);
                if (user2 != null)
                {
                    var httpClient = new HttpClient();
                    string stringData = JsonConvert.SerializeObject(user2);
                    var contentData = new StringContent(stringData, Encoding.UTF8, "application/json");
                    var responseMessage = await httpClient.PostAsync("https://localhost:7084/api/user/loginjwt", contentData);
                    var json = await responseMessage.Content.ReadAsStringAsync();
                    HttpContext.Session.SetString("token", json);


                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user2.UserName),
                        new Claim(ClaimTypes.Email, user2.UserMail),
                        new Claim(ClaimTypes.Role, user2.Role),
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Book");
                    }
                }
                ModelState.AddModelError("login", "kullanıcı adı veya şifre hatalı");

            }
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        public async Task<IActionResult> Delete(int id)
        {
            //if (await _userService.IsExistsAsync(id))
            //{
            //    await _userService.SoftDeleteAsync(id);
            //    return Json(true);
            //}

            //return NotFound();

            if (await _userService.IsExistsAsync(id))
            {
                var httpClient = getClient();
                var responseMessage = await httpClient.DeleteAsync("https://localhost:7084/api/user/" + id);
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
        public List<SelectListItem> GetUserRoleDropDown()
        {

            var userRole = new List<SelectListItem>();

            userRole.Add(new SelectListItem { Text = "Admin", Value = "Admin" });
            userRole.Add(new SelectListItem { Text = "Editor", Value = "Editor" });
            userRole.Add(new SelectListItem { Text = "Client", Value = "Client" });

            return userRole;
        }
    }
}
