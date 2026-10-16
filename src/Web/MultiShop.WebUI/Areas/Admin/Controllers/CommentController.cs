using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CommentDtos;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.CommentServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Yorum moderasyonu: yeni yorumlar "Onay bekliyor" gelir, sitede yalnızca onaylananlar görünür.
    // Yorum içeriği admin tarafından düzenlenmez; onaylanır, reddedilir ya da silinir.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Comment")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        void CommentViewBagList()
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Yorumlar";
            ViewBag.v3 = "Yorum Listesi";
            ViewBag.v0 = "Yorum İşlemleri";
        }

        /// <summary>Varsayılan filtre "Onay bekliyor": admin'in işi olan yorumlar önce gelsin. <c>status=all</c> hepsini gösterir.</summary>
        [Route("Index")]
        public async Task<IActionResult> Index(string? status = CommentStatuses.Pending, int page = 1)
        {
            CommentViewBagList();
            var filter = CommentStatusDisplay.All.Contains(status) ? status : null;
            ViewBag.status = filter ?? "all";
            var values = await _commentService.GetCommentsAsync(filter, page);
            return View(values);
        }

        [Route("Approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Approve(int id, string? status, int page)
            => RunAsync(() => _commentService.ApproveCommentAsync(id), "Yorum onaylandı; artık ürün sayfasında görünüyor.", status, page);

        [Route("Reject")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Reject(int id, string? status, int page)
            => RunAsync(() => _commentService.RejectCommentAsync(id), "Yorum reddedildi; ürün sayfasında görünmeyecek.", status, page);

        [Route("Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Delete(int id, string? status, int page)
            => RunAsync(() => _commentService.DeleteCommentAsync(id), "Yorum silindi.", status, page);

        // İşlemden sonra aynı filtre ve sayfaya dönülür.
        private async Task<IActionResult> RunAsync(Func<Task<string?>> action, string successMessage, string? status, int page)
        {
            var error = await action();
            if (error is null)
                TempData["SuccessMessage"] = successMessage;
            else
                TempData["ErrorMessage"] = error;
            return RedirectToAction(nameof(Index), new { status, page = Math.Max(page, 1) });
        }
    }
}
