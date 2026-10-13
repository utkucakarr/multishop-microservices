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
            await ExecuteWithUniqueCodeAsync(query, parameters, createCouponDto.Code);
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

        public async Task<DiscountStatisticsDto> GetStatisticsAsync()
        {
            // Tek sorgu; tablo boşken SUM null döndüğü için ISNULL.
            string query = "Select Count(*) As CouponCount, " +
                $"ISNULL(Sum(Case When {ValidCouponCondition} Then 1 Else 0 End), 0) As ActiveCouponCount " +
                "From Coupons";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleAsync<DiscountStatisticsDto>(query);
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
            await ExecuteWithUniqueCodeAsync(query, parameters, updateCouponDto.Code);
        }

        public async Task<CouponValidationDto> ValidateCouponAsync(string code)
        {
            string query = $"Select Code, Rate From Coupons Where Code = @code And {ValidCouponCondition}";
            var parameters = new DynamicParameters();
            parameters.Add("@code", code);
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CouponValidationDto>(query, parameters)
                    ?? throw new NotFoundException("Kupon geçersiz veya süresi dolmuş.");
            }
        }

        // Aynı kod ikinci kez eklenirse veritabanındaki unique index reddeder; kontrol + ekleme arasında yarış olmaz.
        private async Task ExecuteWithUniqueCodeAsync(string query, DynamicParameters parameters, string code)
        {
            try
            {
                using var connection = _context.CreateConnection();
                await connection.ExecuteAsync(query, parameters);
            }
            catch (SqlException ex) when (UniqueViolationErrors.Contains(ex.Number))
            {
                throw new ConflictException($"'{code}' kodlu bir kupon zaten var.", ex);
            }
        }
    }
}
