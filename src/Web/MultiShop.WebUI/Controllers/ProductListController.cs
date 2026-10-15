using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CommentDtos;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Controllers
{
    public class ProductListController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;

        public ProductListController(IHttpClientFactory httpClientFactory, ICommentService commentService, IUserService userService)
        {
            _httpClientFactory = httpClientFactory;
            _commentService = commentService;
            _userService = userService;
        }

        public IActionResult Index(string id)
        {
            @ViewBag.directory1 = "Ana Sayfa";
            @ViewBag.directory2 = "Ürünler";
            @ViewBag.directory3 = "Ürün Listesi";
            ViewBag.i = id;
            return View();
        }

        public IActionResult ProductDetail(string id)
        {
            @ViewBag.directory1 = "Ana Sayfa";
            @ViewBag.directory2 = "Ürünler Listesi";
            @ViewBag.directory3 = "Ürün Detayları";
            ViewBag.x = id;
            return View();
        }

        [HttpGet]
        public PartialViewResult AddComment()
        {
            return PartialView();
        }

        /// <summary>
        /// Yorum gönderir (fetch ile, JSON döner). Ad ve e-posta formdan değil giriş yapan kullanıcının profilinden;
        /// durum ve tarih Comment servisinde atanır. Yorum admin onaylayana kadar sitede görünmez.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CreateCommentDto createCommentDto)
        {
            // Yorum yapmak için giriş gerekir; form fetch ile gönderildiği için login sayfasına yönlendirmek yerine 401 dönülür.
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized();

            var user = await _userService.GetUserInfo();
            var fullName = $"{user.Name} {user.Surname}".Trim();
            createCommentDto.NameSurname = string.IsNullOrEmpty(fullName) ? user.UserName : fullName;
            createCommentDto.Email = user.Email;

            var error = await _commentService.CreateCommentAsync(createCommentDto);
            return error is null
                ? Json(new { success = true, message = "Yorumunuz alındı; onaylandıktan sonra yayınlanacak." })
                : BadRequest(new { success = false, message = error });
        }
    }
}
