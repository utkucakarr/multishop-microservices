using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Comment.Dtos;
using MultiShop.Comment.Entities;
using MultiShop.Comment.Services;

namespace MultiShop.Comment.Controllers
{
    // Ziyaretçi bir ürünün onaylı yorumlarını okur, giriş yapan yorum yazar (onay bekler); liste, onay/ret ve silme Admin.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly ICurrentUserService _currentUser;

        public CommentsController(CommentService commentService, ICurrentUserService currentUser)
        {
            _commentService = commentService;
            _currentUser = currentUser;
        }

        /// <summary>Tüm yorumlar (en yeni önce, sayfalı); <paramref name="status"/> verilirse yalnızca o durumdakiler.</summary>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> CommentList([FromQuery] CommentStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => Ok(await _commentService.GetPageAsync(status, page, pageSize));

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCommentById(int id)
            => Ok(await _commentService.GetAsync(id));

        /// <summary>Ürün sayfası: yalnızca onaylı yorumlar (COM-07); e-posta dönmez.</summary>
        [Authorize(Policy = MultiShopPolicies.Read)]
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> CommentListByProductId(string productId)
            => Ok(await _commentService.GetApprovedByProductAsync(productId));

        /// <summary>Yeni yorum; admin onaylayana kadar sitede görünmez.</summary>
        [Authorize(Policy = MultiShopPolicies.Authenticated)]
        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentDto dto)
        {
            var id = await _commentService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetCommentById), new { id }, new { userCommentId = id });
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _commentService.SetStatusAsync(id, CommentStatus.Approved);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            await _commentService.SetStatusAsync(id, CommentStatus.Rejected);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _commentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
