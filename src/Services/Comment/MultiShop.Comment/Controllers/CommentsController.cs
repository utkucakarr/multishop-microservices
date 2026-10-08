using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Comment.Context;
using MultiShop.Comment.Entities;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Comment.Controllers
{
    // Ziyaretçi bir ürünün yorumlarını okur, giriş yapan yorum yazar; liste, düzenleme, silme ve sayaçlar (moderasyon) Admin.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentContext _commentContext;

        public CommentsController(CommentContext commentContext)
        {
            _commentContext = commentContext;
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public IActionResult CommentList()
        {
            var values = _commentContext.UserComments.ToList();
            return Ok(values);
        }

        [Authorize(Policy = MultiShopPolicies.Authenticated)]
        [HttpPost]
        public IActionResult CreateComment(UserComment userComment)
        {
            _commentContext.UserComments.Add(userComment);
            _commentContext.SaveChanges();
            return Ok("Yorum başarıyla eklendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public IActionResult DeleteComment(int id)
        {
            var values = _commentContext.UserComments.Find(id);
            _commentContext.UserComments.Remove(values);
            _commentContext.SaveChanges();
            return Ok("Yorum başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("{id}")]
        public IActionResult GetCommentById(int id)
        {
            var value = _commentContext.UserComments.Find(id);
            return Ok(value);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public IActionResult UpdateComment(UserComment userComment)
        {
            _commentContext.UserComments.Update(userComment);
            _commentContext.SaveChanges();
            return Ok("Yorum başarıyla güncellendi");
        }

        [Authorize(Policy = MultiShopPolicies.Read)]
        [HttpGet("CommentListByProductId/{id}")]
        public IActionResult CommentListByProductId(string id)
        {
            var value = _commentContext.UserComments.Where(x => x.ProductId == id);
            return Ok(value);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("GetActiveCommentCount")]
        public IActionResult GetActiveCommentCount()
        {
            int value = _commentContext.UserComments.Where(x => x.Status == true).Count();
            return Ok(value);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("GetPassiveCommentCount")]
        public IActionResult GetPassiveCommentCount()
        {
            int value = _commentContext.UserComments.Where(x => x.Status == false).Count();
            return Ok(value);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("GetTotalCommentCount")]
        public IActionResult GetTotalCommentCount()
        {
            int value = _commentContext.UserComments.Count();
            return Ok(value);
        }
    }
}
