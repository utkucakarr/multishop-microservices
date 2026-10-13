using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.IdentityServer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MultiShop.IdentityServer.Controllers
{
    /// <summary>Admin dashboard'unun Identity kısmı.</summary>
    public record IdentityStatisticsDto(int UserCount);

    // Önceden token'sız erişilebiliyordu; kullanıcı sayısı yalnızca admin paneli içindir.
    // Ocelot /services/admin/dashboard bunu diğer servislerin istatistikleriyle birleştirir.
    [Authorize(MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult GetStatistics()
        {
            int userCount = _userManager.Users.Count();
            return Ok(new IdentityStatisticsDto(userCount));
        }
    }
}
