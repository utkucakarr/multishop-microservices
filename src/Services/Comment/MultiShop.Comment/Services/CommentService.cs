using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Comment.Context;
using MultiShop.Comment.Dtos;
using MultiShop.Comment.Entities;
using MultiShop.Contracts.Comments;

namespace MultiShop.Comment.Services
{
    /// <summary>
    /// Yorumlar ve moderasyon (KT-14, COM-01…08). Yeni yorum onay bekler; sitede yalnızca onaylı yorumlar görünür,
    /// admin onaylar ya da reddeder. Ürünün gerçekten var olup olmadığına bakılmaz (COM-06, bilinçli kabul: Catalog'a
    /// senkron çağrı yapmamak için; olmayan ürünün yorumu hiçbir sayfada görünmez).
    /// </summary>
    public class CommentService
    {
        public const int MaxPageSize = 100;

        private readonly CommentContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CommentService(CommentContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<List<ProductCommentDto>> GetApprovedByProductAsync(string productId)
            => await _context.UserComments.AsNoTracking()
                .Where(x => x.ProductId == productId && x.Status == CommentStatus.Approved)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new ProductCommentDto(x.UserCommentId, x.NameSurname, x.CommentDetail, x.Rating, x.CreatedDate))
                .ToListAsync();

        public async Task<CommentPageDto> GetPageAsync(CommentStatus? status, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var query = _context.UserComments.AsNoTracking();
            if (status is { } value)
                query = query.Where(x => x.Status == value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.UserCommentId)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new CommentDto(x.UserCommentId, x.ProductId, x.UserId, x.NameSurname, x.Email,
                    x.CommentDetail, x.Rating, x.CreatedDate, x.Status))
                .ToListAsync();

            return new CommentPageDto(items, totalCount, page, pageSize);
        }

        public async Task<CommentDto> GetAsync(int id)
            => await _context.UserComments.AsNoTracking()
                .Where(x => x.UserCommentId == id)
                .Select(x => new CommentDto(x.UserCommentId, x.ProductId, x.UserId, x.NameSurname, x.Email,
                    x.CommentDetail, x.Rating, x.CreatedDate, x.Status))
                .SingleOrDefaultAsync()
               ?? throw new NotFoundException("Yorum", id);

        public async Task<int> CreateAsync(CreateCommentDto dto, string userId)
        {
            var comment = new UserComment
            {
                UserId = userId,
                ProductId = dto.ProductId,
                NameSurname = dto.NameSurname.Trim(),
                Email = dto.Email.Trim(),
                CommentDetail = dto.CommentDetail.Trim(),
                Rating = dto.Rating,
                CreatedDate = DateTime.UtcNow,
                Status = CommentStatus.Pending
            };

            // Yorum ve CommentCreated tek transaction'da yazılır (transactional outbox): ya ikisi birden kaydedilir ya hiçbiri.
            // Publish RabbitMQ'ya değil OutboxMessage tablosuna yazar; event'te yorum id'si olduğu için önce yorum kaydedilir.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.UserComments.Add(comment);
            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new CommentCreated(
                comment.UserCommentId, comment.ProductId, comment.NameSurname, comment.Rating, comment.CreatedDate));
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return comment.UserCommentId;
        }

        /// <summary>Onaylar ya da reddeder; önceki durumdan bağımsız (reddedilen yorum sonradan onaylanabilir).</summary>
        public async Task SetStatusAsync(int id, CommentStatus status)
        {
            var updated = await _context.UserComments
                .Where(x => x.UserCommentId == id)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.Status, status));
            if (updated == 0)
                throw new NotFoundException("Yorum", id);
        }

        public async Task DeleteAsync(int id)
        {
            var deleted = await _context.UserComments.Where(x => x.UserCommentId == id).ExecuteDeleteAsync();
            if (deleted == 0)
                throw new NotFoundException("Yorum", id);
        }
    }
}
