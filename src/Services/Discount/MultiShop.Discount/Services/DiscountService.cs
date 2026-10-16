using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Discount.Context;
using MultiShop.Discount.Dtos;

namespace MultiShop.Discount.Services
{
    public class DiscountService : IDiscountService
    {
        // SQL Server unique index ihlali hata numaraları.
        private static readonly int[] UniqueViolationErrors = [2601, 2627];

        // Kupon geçerliliği tek yerde: aktif ve son gün dahil süresi dolmamış (ValidDate gün bazında karşılaştırılır).
        private const string ValidCouponCondition = "IsActive = 1 And ValidDate >= CAST(GETDATE() AS date)";

        private readonly IDbConnectionFactory _connectionFactory;

        public DiscountService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> CreateDiscountCouponAsync(CreateDiscountCouponDto createCouponDto)
        {
            // Oluşan kuponun id'si aynı komutla döner (201 yanıtındaki id).
            string query = "Insert Into Coupons (Code, Rate, IsActive, ValidDate) Output Inserted.CouponId Values (@code, @rate, @isActive, @validDate)";
            var parameters = new DynamicParameters();
            parameters.Add("@code", createCouponDto.Code);
            parameters.Add("@rate", createCouponDto.Rate);
            parameters.Add("@isActive", createCouponDto.IsActive);
            parameters.Add("@validDate", createCouponDto.ValidDate);
            return await WithUniqueCodeAsync(connection => connection.ExecuteScalarAsync<int>(query, parameters), createCouponDto.Code);
        }

        public async Task DeleteDiscountCouponAsync(int id)
        {
            // Silme işlemi
            string query = "Delete From Coupons Where CouponId = @couponId";
            var parameters = new DynamicParameters();
            parameters.Add("couponId", id);
            using var connection = _connectionFactory.CreateConnection();
            if (await connection.ExecuteAsync(query, parameters) == 0)
                throw new NotFoundException("Kupon", id);
        }

        public async Task<List<ResultDiscountCouponDto>> GetAllDiscountCouponAsync()
        {
            // Bütün verileri getirme işlemi
            string query = "Select * From Coupons";
            using (var connection = _connectionFactory.CreateConnection())
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
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<GetByIdDiscountCouponDto>(query, parameters)
                    ?? throw new NotFoundException("Kupon", id);
            }
        }

        public async Task<DiscountStatisticsDto> GetStatisticsAsync()
        {
            // Tek sorgu; tablo boşken SUM null döndüğü için ISNULL.
            string query = "Select Count(*) As CouponCount, " +
                $"ISNULL(Sum(Case When {ValidCouponCondition} Then 1 Else 0 End), 0) As ActiveCouponCount " +
                "From Coupons";
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QuerySingleAsync<DiscountStatisticsDto>(query);
            }
        }

        public async Task UpdateDiscountCouponAsync(UpdateDiscountCouponDto updateCouponDto)
        {
            // Güncelleme işlemi
            string query = "Update Coupons Set Code=@code, Rate=@rate, IsActive=@isActive, ValidDate=@validDate Where CouponId=@couponId";
            var parameters = new DynamicParameters();
            parameters.Add("@code", updateCouponDto.Code);
            parameters.Add("@rate", updateCouponDto.Rate);
            parameters.Add("@isActive", updateCouponDto.IsActive);
            parameters.Add("@validDate", updateCouponDto.ValidDate);
            parameters.Add("@couponId", updateCouponDto.CouponId);
            var updated = await WithUniqueCodeAsync(connection => connection.ExecuteAsync(query, parameters), updateCouponDto.Code);
            if (updated == 0)
                throw new NotFoundException("Kupon", updateCouponDto.CouponId);
        }

        public async Task<CouponValidationDto> ValidateCouponAsync(string code)
        {
            string query = $"Select Code, Rate From Coupons Where Code = @code And {ValidCouponCondition}";
            var parameters = new DynamicParameters();
            parameters.Add("@code", code);
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CouponValidationDto>(query, parameters)
                    ?? throw new NotFoundException("Kupon geçersiz veya süresi dolmuş.");
            }
        }

        // Aynı kod ikinci kez eklenirse veritabanındaki unique index reddeder; kontrol + ekleme arasında yarış olmaz.
        private async Task<T> WithUniqueCodeAsync<T>(Func<IDbConnection, Task<T>> command, string code)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                return await command(connection);
            }
            catch (SqlException ex) when (UniqueViolationErrors.Contains(ex.Number))
            {
                throw new ConflictException($"'{code}' kodlu bir kupon zaten var.", ex);
            }
        }
    }
}
