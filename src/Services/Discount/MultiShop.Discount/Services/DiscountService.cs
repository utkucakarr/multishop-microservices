using Dapper;
using MultiShop.Discount.Context;
using MultiShop.Discount.Dtos;
using System.Reflection.Metadata;

namespace MultiShop.Discount.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly DapperContext _context;

        public DiscountService(DapperContext context)
        {
            _context = context;
        }

        public async Task CreateDiscountCouponAsync(CreateDiscountCouponDto createCouponDto)
        {
            // Yeni indirim cuponu eklemek için kullanılan kodlar.
            string query = "Insert Into Coupons (Code, Rate, IsActive, ValidDate) Values (@code, @rate, @isActive, @validDate)";
            var parameters = new DynamicParameters();
            parameters.Add("@code", createCouponDto.Code);
            parameters.Add("@rate", createCouponDto.Rate);
            parameters.Add("@isActive", createCouponDto.IsActive);
            parameters.Add("@validDate", createCouponDto.ValidDate);
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async Task DeleteDiscountCouponAsync(int id)
        {
            // Silme işlemi
            string query = "Delete From Coupons Where CouponId = @couponId";
            var parameters = new DynamicParameters();
            parameters.Add("couponId", id);
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async Task<List<ResultDiscountCouponDto>> GetAllDiscountCouponAsync()
        {
            // Bütün verileri getirme işlemi
            string query = "Select * From Coupons";
            using (var connection = _context.CreateConnection())
            {
                var values = await connection.QueryAsync<ResultDiscountCouponDto>(query);
                return values.ToList();
            }
        }

        public async Task<GetByIdDiscountCouponDto> GetByIdDiscountCouponAsync(int id)
        {
            // Id'ye göre veri getirme işlemi
            var query = "Select * From Coupons Where CouponId = @couponId";
            var parameters = new DynamicParameters();
            parameters.Add("@couponId", id);
            using (var connection = _context.CreateConnection())
            {
                var values = await connection.QueryFirstOrDefaultAsync<GetByIdDiscountCouponDto>(query, parameters);
                return values;
            }
        }

        public async Task<ResultDiscountCouponDto> GetCodeDetailByCodeAsync(string code)
        {
            string query = "Select * From Coupons Where Code = @code";
            var parameters = new DynamicParameters();
            parameters.Add("@code", code);
            using (var connection = _context.CreateConnection())
            {
                var values = await connection.QueryFirstOrDefaultAsync<ResultDiscountCouponDto>(query, parameters);
                return values;
            }
        }

        public async Task<int> GetDiscountCouponCountAsync()
        {
            string query = "Select Count(*) From Coupons";
            using (var connection = _context.CreateConnection())
            {
                var values = await connection.QueryFirstOrDefaultAsync<int>(query);
                return values;
            }
        }

        public int GetDiscountCouponCountRate(string code)
        {
            string query = "Select Rate From Coupons Where Code = @code";
            var parameters = new DynamicParameters();
            parameters.Add("@code", code);
            using (var connection = _context.CreateConnection())
            {
                var values = connection.QueryFirstOrDefault<int>(query, parameters);
                return values;
            }
        }

        public async Task UpdateDiscountCouponAync(UpdateDiscountCouponDto updateCouponDto)
        {
            // Güncelleme işlemi
            string query = "Update Coupons Set Code=@code, Rate=@rate, IsActive=@isActive, ValidDate=@validDate Where CouponId=@couponId";
            var parameters = new DynamicParameters();
            parameters.Add("@code", updateCouponDto.Code);
            parameters.Add("@rate", updateCouponDto.Rate);
            parameters.Add("@isActive", updateCouponDto.IsActive);
            parameters.Add("@validDate", updateCouponDto.ValidDate);
            parameters.Add("@couponId", updateCouponDto.CouponId);
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }
    }
}
