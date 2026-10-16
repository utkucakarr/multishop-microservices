using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Cargo.DataAccessLayer.Concrete;
using MultiShop.Cargo.DtoLayer.Dtos.CargoCompanyDtos;
using MultiShop.Cargo.EntityLayer.Concrete;

namespace MultiShop.Cargo.BusinessLayer.Services
{
    /// <summary>
    /// Kargo firmaları. Önceki generic Manager/Dal katmanları yalnızca çağrıyı aktarıyordu (CAR-07) ve olmayan id'de
    /// 500 veriyordu (CAR-03); kurallar artık burada: gönderisi olan firma silinmez, varsayılan firma tek.
    /// </summary>
    public class CargoCompanyService
    {
        private readonly CargoContext _context;

        public CargoCompanyService(CargoContext context)
        {
            _context = context;
        }

        public async Task<List<CargoCompanyDto>> GetAllAsync()
            => await _context.CargoCompanies.AsNoTracking()
                .OrderBy(x => x.CargoCompanyId)
                .Select(x => new CargoCompanyDto(x.CargoCompanyId, x.CargoCompanyName, x.IsDefault))
                .ToListAsync();

        public async Task<CargoCompanyDto> GetAsync(int id)
            => await _context.CargoCompanies.AsNoTracking()
                .Where(x => x.CargoCompanyId == id)
                .Select(x => new CargoCompanyDto(x.CargoCompanyId, x.CargoCompanyName, x.IsDefault))
                .SingleOrDefaultAsync()
               ?? throw new NotFoundException("Kargo firması", id);

        public async Task<int> CreateAsync(SaveCargoCompanyDto dto)
        {
            var company = new CargoCompany { CargoCompanyName = dto.CargoCompanyName.Trim() };
            _context.CargoCompanies.Add(company);
            await _context.SaveChangesAsync();
            return company.CargoCompanyId;
        }

        /// <summary>Yalnızca adı değişir; varsayılanlık korunur (<see cref="SetDefaultAsync"/> ile değişir).</summary>
        public async Task UpdateAsync(int id, SaveCargoCompanyDto dto)
        {
            var updated = await _context.CargoCompanies
                .Where(x => x.CargoCompanyId == id)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.CargoCompanyName, dto.CargoCompanyName.Trim()));
            if (updated == 0)
                throw new NotFoundException("Kargo firması", id);
        }

        /// <summary>Eskiden firma silinince bağlı kargo kayıtları da (cascade) siliniyordu; gönderisi olan firma artık silinemez.</summary>
        public async Task DeleteAsync(int id)
        {
            if (await _context.CargoDetails.AnyAsync(x => x.CargoCompanyId == id))
                throw new ConflictException("Bu firmaya bağlı gönderiler var; firma silinemez.");

            var deleted = await _context.CargoCompanies.Where(x => x.CargoCompanyId == id).ExecuteDeleteAsync();
            if (deleted == 0)
                throw new NotFoundException("Kargo firması", id);
        }

        /// <summary>Firmayı varsayılan yapar: yeni gönderilere otomatik atanır. Aynı anda yalnızca bir firma varsayılandır.</summary>
        public async Task SetDefaultAsync(int id)
        {
            if (!await _context.CargoCompanies.AnyAsync(x => x.CargoCompanyId == id))
                throw new NotFoundException("Kargo firması", id);

            await _context.CargoCompanies.ExecuteUpdateAsync(set => set.SetProperty(x => x.IsDefault, x => x.CargoCompanyId == id));
        }
    }
}
