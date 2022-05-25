using bookShop.Business.Abstract;
using bookShop.Dtos.Requests;
using bookShop.Entities.Concrete;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace bookShop.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        [Route("GetAllEntitiesAsyncDto")]
        public async Task<IActionResult> GetAllEntitiesAsyncDto()
        {
            var users = await _userService.GetAllEntitiesAsyncDto();
            var json = JsonConvert.SerializeObject(users);
            return Ok(json);
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(AddUserRequest user)
        {

            bool success = await _userService.AddAsync(user);
            return Ok(success);
        }

        [HttpPost]
        [Route("PanelRegister")]
        public async Task<IActionResult> PanelRegister(AddUserRequest user)
        {

            bool success = await _userService.AddAsync(user);
            return Ok(success);

        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(User user, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user2 = await _userService.ValidateUser(user.UserName, user.Password);
                if (user2 != null)
                {
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
                }
                ModelState.AddModelError("login", "kullanıcı adı veya şifre hatalı");

            }
            return Ok();
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _userService.IsExistsAsync(id))
            {
                await _userService.SoftDeleteAsync(id);
                return Ok(true);
            }

            return NotFound();
        }

        [HttpPost]
        [Route("LoginJWT")]
        public async Task<IActionResult> LoginJWT(User model)
        {
            var user = await _userService.ValidateUser(model.UserName, model.Password);

            if (user != null)
            {
                //1.claim bilgileri
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role),
                };

                //2.gizli cümlenin üretilmesi
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Burası çok ama çok gizli bir ifade"));
                var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                //3.tokenin özelliklerini tanımla

                var token = new JwtSecurityToken(
                    issuer: "turkcell.com.tr",
                    audience: "turkcell.com",
                    claims: claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddMinutes(20),
                    signingCredentials: credential
                    );
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return BadRequest(new { message = "Hatalı kullanıcı adı veya şifre" });
        }
    }
}
