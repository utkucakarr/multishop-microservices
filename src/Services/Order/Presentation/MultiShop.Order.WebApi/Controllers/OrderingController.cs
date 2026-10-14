using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Order.Application.Features.Mediator.Queries.OrderingQueries;
using MultiShop.Order.Application.Sagas;
using MultiShop.Order.Domain.Enums;
using MultiShop.Order.Persistence.Context;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Order.WebApi.Security;

namespace MultiShop.Order.WebApi.Controllers
{
    // Müşteri yalnızca kendi siparişlerine, Admin hepsine erişir; kullanıcı kimliği her zaman token'dan okunur.
    // Sipariş API'den oluşturulmaz ve değiştirilmez: sepet onayı (BasketCheckedOut) saga'yı başlatır, durumu saga yönetir (ORD-02).
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;
        private readonly OrderAccessGuard _accessGuard;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly OrderContext _orderContext;

        public OrderingController(IMediator mediator, ICurrentUserService currentUser, OrderAccessGuard accessGuard, IPublishEndpoint publishEndpoint, OrderContext orderContext)
        {
            _mediator = mediator;
            _currentUser = currentUser;
            _accessGuard = accessGuard;
            _publishEndpoint = publishEndpoint;
            _orderContext = orderContext;
        }

        /// <summary>Tüm siparişler (en yeni önce); <paramref name="status"/> verilirse yalnızca o durumdakiler.</summary>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> OrderingList([FromQuery] OrderStatus? status)
        {
            var values = await _mediator.Send(new GetOrderingQuery { Status = status });
            return Ok(values);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> OrderingById(int id)
        {
            await _accessGuard.EnsureOrderingAccessAsync(id);
            var value = await _mediator.Send(new GetOrderingByIdQuery(id));
            return Ok(value);
        }

        /// <summary>
        /// Sipariş numarasıyla sipariş ve durumu. WebUI sepet onayından sonra bunu sorgulayarak siparişin
        /// ödeme beklemesini (AwaitingPayment) ya da iptal sebebini (Cancelled + FailMessage) bekler.
        /// Saga mesajı henüz işlemediyse sipariş yoktur: 404.
        /// </summary>
        [HttpGet("by-number/{orderNumber:guid}")]
        public async Task<IActionResult> OrderingByNumber(Guid orderNumber)
        {
            var ordering = await _accessGuard.EnsureOrderingAccessAsync(orderNumber);
            var value = await _mediator.Send(new GetOrderingByIdQuery(ordering.OrderingId));
            return Ok(value);
        }

        /// <summary>
        /// Siparişi iptal eder (sahibi ya da Admin). Yalnızca ödeme alınmamış sipariş (Pending, AwaitingPayment) iptal edilir.
        /// İptal saga'ya komut olarak gider: ayrılmış stok bırakılır, Payment bekleyen ödemeyi kapatır.
        /// İşlem arka planda tamamlandığı için 202 döner; sonuç <c>by-number</c> ile izlenir.
        /// </summary>
        [HttpPost("{orderNumber:guid}/cancel")]
        public async Task<IActionResult> CancelOrdering(Guid orderNumber)
        {
            var ordering = await _accessGuard.EnsureOrderingAccessAsync(orderNumber);
            if (ordering.Status is not (OrderStatus.Pending or OrderStatus.AwaitingPayment))
                throw new ConflictException("Yalnızca ödemesi alınmamış siparişler iptal edilebilir.");

            var reason = ordering.UserId == _currentUser.GetRequiredUserId()
                ? "Sipariş müşteri tarafından iptal edildi."
                : "Sipariş yönetici tarafından iptal edildi.";

            // Bus outbox: komut OutboxMessage tablosuna yazılır, RabbitMQ'ya arka planda gider (RabbitMQ kapalıysa da kaybolmaz).
            await _publishEndpoint.Publish(new CancelOrder(orderNumber, reason));
            await _orderContext.SaveChangesAsync();

            return Accepted();
        }

        /// <summary>Giriş yapmış kullanıcının kendi siparişleri ("Siparişlerim").</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> MyOrderings()
        {
            var values = await _mediator.Send(new GetOrderingByUserIdQuery(_currentUser.GetRequiredUserId()));
            return Ok(values);
        }

        /// <summary>Belirli bir kullanıcının siparişleri; yalnızca Admin (müşteri için <c>mine</c>).</summary>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("GetOrderingByUserId")]
        public async Task<IActionResult> GetOrderingByUserId(string id)
        {
            var values = await _mediator.Send(new GetOrderingByUserIdQuery(id));
            return Ok(values);
        }
    }
}
