using AutoMapper;
using MultiShop.Catalog.Dtos.ProductImageDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Entities.ValueObject;
using MultiShop.Catalog.Extensions;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.BuildingBlocks.Storage;

namespace MultiShop.Catalog.Services.ProductImageServices
{
    public class ProductImageService : IProductImageService
    {
        // Görseller depolamada bu klasöre yazılır: {PublicBaseUrl}/products/{guid}.jpg
        private const string StorageFolder = "products";
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;

        public ProductImageService(IProductImageRepository productImageRepository,
            IProductRepository productRepository,
            IFileStorage fileStorage,
            IMapper mapper)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
            _fileStorage = fileStorage;
            _mapper = mapper;
        }

        public async Task CreateProductImageAsync(CreateProductImageDto createProductImageDto)
        {
            var productImage = createProductImageDto.ToEntity();
            if (productImage.IsMain)
                await _productImageRepository.ClearMainAsync(productImage.ProductId);
            await _productImageRepository.CreateAsync(productImage);
        }

        public async Task<GetByIdProductImageDto> UploadProductImageAsync(UploadProductImageDto uploadProductImageDto)
        {
            var file = uploadProductImageDto.File;
            if (file is null || file.Length == 0)
                throw new BadRequestException("Görsel dosyası seçilmedi.");
            if (file.Length > MaxImageBytes)
                throw new BadRequestException($"Görsel en fazla {MaxImageBytes / (1024 * 1024)} MB olabilir.");

            if (string.IsNullOrWhiteSpace(uploadProductImageDto.ProductId))
                throw new BadRequestException("Ürün seçilmedi.");
            var productId = uploadProductImageDto.ProductId;
            if (await _productRepository.GetByIdAsync(productId) is null)
                throw new NotFoundException("Ürün", productId);

            await using var stream = file.OpenReadStream();
            var format = await ImageFormats.DetectAsync(stream)
                ?? throw new BadRequestException("Yalnızca JPG, PNG ya da WebP görsel yüklenebilir.");

            // Ürünün ilk görseli, işaretlenmese de ana görsel olur.
            var lastDisplayOrder = await _productImageRepository.GetMaxDisplayOrderAsync(productId);
            var isMain = uploadProductImageDto.IsMain || lastDisplayOrder == 0;

            var imageUrl = await _fileStorage.SaveAsync(stream, StorageFolder, format.Extension, format.ContentType);
            var productImage = ProductImage.Create(new ProductImageCreationParameters(
                productId: productId,
                imageUrl: imageUrl,
                displayOrder: lastDisplayOrder + 1,
                isMain: isMain));

            try
            {
                if (isMain)
                    await _productImageRepository.ClearMainAsync(productId);
                await _productImageRepository.CreateAsync(productImage);
            }
            catch
            {
                // Kayıt oluşmadıysa dosya sahipsiz kalmasın.
                await _fileStorage.DeleteAsync(imageUrl);
                throw;
            }

            return _mapper.Map<GetByIdProductImageDto>(productImage);
        }

        public async Task SetMainProductImageAsync(string id)
        {
            var productImage = await _productImageRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Ürün görseli", id);

            await _productImageRepository.ClearMainAsync(productImage.ProductId);
            productImage.MarkAsMain();
            await _productImageRepository.UpdateAsync(productImage.ProductImageId, productImage);
        }

        public async Task DeleteProductImageAsync(string id)
        {
            var productImage = await _productImageRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Ürün görseli", id);

            await _productImageRepository.DeleteAsync(id);
            // Önce kayıt silinir: dosya silinemezse en kötü ihtimalle sahipsiz bir dosya kalır, kırık bir görsel değil.
            await _fileStorage.DeleteAsync(productImage.ImageUrl);

            // Ana görsel silindiyse sıradaki görsel ana görsel olur.
            if (productImage.IsMain)
            {
                var next = (await _productImageRepository.GetImagesByProductIdAsync(productImage.ProductId)).FirstOrDefault();
                if (next is not null)
                {
                    next.MarkAsMain();
                    await _productImageRepository.UpdateAsync(next.ProductImageId, next);
                }
            }
        }

        public async Task DeleteByProductIdAsync(string productId)
        {
            var productImages = await _productImageRepository.GetImagesByProductIdAsync(productId);
            await _productImageRepository.DeleteByProductIdAsync(productId);

            foreach (var productImage in productImages)
                await _fileStorage.DeleteAsync(productImage.ImageUrl);
        }

        public async Task<IEnumerable<ResultProductImageDto>> GetAllProductImageAsync()
        {
            var values = await _productImageRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ResultProductImageDto>>(values);
        }

        public async Task<GetByIdProductImageDto> GetByIdProductImageAsync(string id)
        {
            var productImage = await _productImageRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Ürün görseli", id);
            return _mapper.Map<GetByIdProductImageDto>(productImage);
        }

        public async Task<IEnumerable<GetByIdProductImageDto>> GetByProductIdProductImageAsync(string id)
        {
            var productImages = await _productImageRepository
                           .GetImagesByProductIdAsync(id);

            return _mapper.Map<IEnumerable<GetByIdProductImageDto>>(productImages);
        }

        public async Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto)
        {
            var productImage = await _productImageRepository
                            .GetByIdAsync(updateProductImageDto.ProductImageId);

            if (productImage is null)
                throw new NotFoundException("Ürün görseli", updateProductImageDto.ProductImageId);

            var oldImageUrl = productImage.ImageUrl;
            productImage.UpdateImage(
                updateProductImageDto.ImageUrl,
                updateProductImageDto.DisplayOrder,
                updateProductImageDto.IsMain
            );

            if (productImage.IsMain)
                await _productImageRepository.ClearMainAsync(productImage.ProductId);

            await _productImageRepository.UpdateAsync(
                productImage.ProductImageId,
                productImage
            );

            // Görsel başka bir URL ile değiştirildiyse eski dosya (bizdeyse) silinir.
            if (oldImageUrl != productImage.ImageUrl)
                await _fileStorage.DeleteAsync(oldImageUrl);
        }
    }
}
