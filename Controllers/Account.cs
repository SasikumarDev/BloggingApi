using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BloggingAPI.BlogModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace BloggingAPI.Controllers
{
    [ApiController]
    public class Account : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private BloggingContext _bContext;
        public Account(
           IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/Login")]
        public IActionResult Login(Users Usr)
        {
            try
            {
                _bContext = new BloggingContext();
                Usr.Password = EncryptPassword(Usr.Password.ToString());
                var result = _bContext.Users.FirstOrDefault(x => x.EmailId == Usr.EmailId && x.Password == Usr.Password);
                if (result != null)
                {
                    var token = GenerateJwtToken(result);
                    return new OkObjectResult(new { Token = token, Message = "Loged IN", Status = HttpStatusCode.OK });
                }
                return new OkObjectResult(new { Token = "", Message = "Invalid Username Or Password", Status = HttpStatusCode.InternalServerError });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new { Token = "", Message = ex.Message.ToString(), Status = HttpStatusCode.InternalServerError });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/Register")]
        public async Task<IActionResult> Register(Users model)
        {
            try
            {
                _bContext = new BloggingContext();
                if (ModelState.IsValid)
                {
                    model.Password = EncryptPassword(model.Password.ToString());
                    _bContext.Users.Add(model);
                    await _bContext.SaveChangesAsync();
                    var token = GenerateJwtToken(model);
                    return new OkObjectResult(new { Token = token, Message = "Registration Successfully", Status = HttpStatusCode.OK });
                }
                return new OkObjectResult(new { Message = "Registration Successfully", currentDate = DateTime.Now });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { Message = ex.Message.ToString(), currentDate = DateTime.Now });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/CheckUserAvailable")]
        public async Task<bool> CheckUserAvailable(DParameter param)
        {
            try
            {
                _bContext = new BloggingContext();
                var usrs = await _bContext.Users.FirstOrDefaultAsync(x => x.EmailId == param.Id.ToString());
                if (usrs != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
        [NonAction]
        private object GenerateJwtToken(Users usercred)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, usercred.UsId.ToString()),
                        new Claim("UserMail", usercred.EmailId),
                        new Claim("UserID", usercred.UsId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [NonAction]
        private string EncryptPassword(string Value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (SHA256 sHA = SHA256.Create())
            {
                byte[] btPass = sHA.ComputeHash(Encoding.UTF8.GetBytes(Value));
                foreach (byte x in btPass)
                {
                    stringBuilder.Append(x.ToString("X2"));
                }
            }
            return stringBuilder.ToString();
        }
    }
}