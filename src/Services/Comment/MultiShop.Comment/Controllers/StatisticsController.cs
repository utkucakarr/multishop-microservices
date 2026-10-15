using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Comment.Context;
using MultiShop.Comment.Entities;

namespace MultiShop.Comment.Controllers
{
    /// <summary>Admin dashboard'unun Comment kısmı (moderasyon durumlarına göre).</summary>
    public record CommentStatisticsDto(int Total, int Approved, int Pending, int Rejected);

    // Admin dashboard istatistikleri: tek uç, Ocelot /services/admin/dashboard bunu diğer servislerle birleştirir.
    [Authorize(Policy = MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly CommentContext _commentContext;

        public StatisticsController(CommentContext commentContext)
        {
            _commentContext = commentContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            // Tek sorgu: onay durumuna göre gruplanmış sayılar.
            var counts = await _commentContext.UserComments
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int Count(CommentStatus status) => counts.Where(x => x.Status == status).Sum(x => x.Count);
            return Ok(new CommentStatisticsDto(counts.Sum(x => x.Count), Count(CommentStatus.Approved),
                Count(CommentStatus.Pending), Count(CommentStatus.Rejected)));
        }
    }
}
