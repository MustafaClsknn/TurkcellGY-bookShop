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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (await _userService.IsExistsAsync(id))
            {
                var user = await _userService.GetEntityByIdAsync(id);
                var json = JsonConvert.SerializeObject(user);
                return Ok(json);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(AddUserRequest user)
        {

            bool success = await _userService.AddAsync(user);
            if (success) 
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("PanelCreate")]
        public async Task<IActionResult> PanelCreate(AddUserRequest user)
        {

            bool success = await _userService.AddAsync(user);
            return Ok(success);

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
        [HttpPut]
        public IActionResult Update(User user) 
        {
            bool success = _userService.Update(user);
            if (success)
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("LoginJWT")]
        public async Task<IActionResult> LoginJWT(User model)
        {
            var user = await _userService.ValidateUser(model.UserName, model.Password);

            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role),
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Gizli Key"));
                var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


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
