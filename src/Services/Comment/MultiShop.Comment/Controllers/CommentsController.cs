using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Comment.Context;
using MultiShop.Comment.Entities;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Contracts.Comments;

namespace MultiShop.Comment.Controllers
{
    // Ziyaretçi bir ürünün yorumlarını okur, giriş yapan yorum yazar; liste, düzenleme, silme ve sayaçlar (moderasyon) Admin.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentContext _commentContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public CommentsController(CommentContext commentContext, IPublishEndpoint publishEndpoint)
        {
            _commentContext = commentContext;
            _publishEndpoint = publishEndpoint;
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
        public async Task<IActionResult> CreateComment(UserComment userComment)
        {
            // Yorum ve CommentCreated tek transaction'da yazılır (transactional outbox): ya ikisi birden kaydedilir ya hiçbiri.
            // Publish RabbitMQ'ya değil OutboxMessage tablosuna yazar; event'te yorum id'si olduğu için önce yorum kaydedilir.
            await using var transaction = await _commentContext.Database.BeginTransactionAsync();

            _commentContext.UserComments.Add(userComment);
            await _commentContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new CommentCreated(
                userComment.UserCommentId, userComment.ProductId, userComment.NameSurname, userComment.Rating, DateTime.UtcNow));
            await _commentContext.SaveChangesAsync();

            await transaction.CommitAsync();
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
    }
}
