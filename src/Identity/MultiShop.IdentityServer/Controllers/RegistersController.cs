using IdentityServer4.Hosting.LocalApiAuthentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiShop.IdentityServer.Dtos;
using MultiShop.IdentityServer.Models;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace MultiShop.IdentityServer.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> UserRegister(UserRegisterDto userRegisterDto)
        {
            var values = new ApplicationUser()
            {
                UserName = userRegisterDto.UserName,
                Email = userRegisterDto.Email,
                Name = userRegisterDto.Name,
                Surname = userRegisterDto.Surname,
            };
            var result = await _userManager.CreateAsync(values,userRegisterDto.Password);
            if (!result.Succeeded)
            {
                // Hata nedeni (ör. şifre kuralları, kullanılan kullanıcı adı) istemciye 400 ile döner.
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            // Yeni kayıt olan herkes müşteridir; Admin rolü yalnızca IdentitySeeder ile verilir.
            await _userManager.AddToRoleAsync(values, Roles.Customer);
            return Ok("Kullanıcı başarıyla eklendi");
        }
    }
}
