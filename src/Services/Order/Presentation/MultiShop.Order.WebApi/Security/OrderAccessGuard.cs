using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.WebApi.Security
{
    /// <summary>
    /// Sipariş ve adres kayıtlarına erişim kontrolü: müşteri yalnızca kendi kaydına, Admin hepsine erişir.
    /// Başkasının kaydı istendiğinde 403 yerine 404 döner; böylece o numarada bir kayıt olduğu da belli olmaz (IDOR, G-14).
    /// </summary>
    public class OrderAccessGuard
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IRepository<Ordering> _orderingRepository;
        private readonly IRepository<Address> _addressRepository;

        public OrderAccessGuard(ICurrentUserService currentUser, IRepository<Ordering> orderingRepository, IRepository<Address> addressRepository)
        {
            _currentUser = currentUser;
            _orderingRepository = orderingRepository;
            _addressRepository = addressRepository;
        }

        public async Task<Ordering> EnsureOrderingAccessAsync(int orderingId)
        {
            var ordering = await _orderingRepository.GetByIdAsync(orderingId);
            if (ordering is null || !CanAccess(ordering.UserId))
                throw new NotFoundException("Sipariş", orderingId);
            return ordering;
        }

        public async Task<Address> EnsureAddressAccessAsync(int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address is null || !CanAccess(address.UserId))
                throw new NotFoundException("Adres", addressId);
            return address;
        }

        private bool CanAccess(string ownerUserId)
            => _currentUser.IsAdmin || ownerUserId == _currentUser.GetRequiredUserId();
    }
}
