using MultiShop.Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Interfaces
{
    public interface IOrderingRepository
    {
        /// <summary>Kullanıcının siparişleri, en yeni önce.</summary>
        Task<List<Ordering>> GetOrderingsByUserIdAsync(string userId);
    }
}
