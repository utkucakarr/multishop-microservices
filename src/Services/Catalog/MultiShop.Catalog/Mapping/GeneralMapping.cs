using AutoMapper;
using MultiShop.Catalog.Dtos.AboutDtos;
using MultiShop.Catalog.Dtos.BrandDtos;
using MultiShop.Catalog.Dtos.CategoryDtos;
using MultiShop.Catalog.Dtos.FeatureDtos;
using MultiShop.Catalog.Dtos.FeatureSliderDtos;
using MultiShop.Catalog.Dtos.OfferDiscountDtos;
using MultiShop.Catalog.Dtos.ProductDetailDtos;
using MultiShop.Catalog.Dtos.ProductDtos;
using MultiShop.Catalog.Dtos.ProductImageDtos;
using MultiShop.Catalog.Dtos.SpecialOfferDtos;
using MultiShop.Catalog.Entities;

namespace MultiShop.Catalog.Mapping
{
    public class GeneralMapping : Profile
    {
        // Yalnızca kullanılan yönler (CAT-12): entity → okuma DTO'su, Create/Update DTO → entity. Önceki ReverseMap'ler
        // her iki yönü de açıyordu; istemci DTO'sundan entity'ye beklenmeyen bir eşleme olmasın.
        public GeneralMapping()
        {
            CreateMap<Category, ResultCategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Category, GetByIdCategoryDto>();

            CreateMap<Product, ResultProductDto>();
            CreateMap<Product, GetByIdProductDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : null));

            CreateMap<Product, ResultProductsWithCategoryDto>()
                .ForMember(
                    dest => dest.Category,
                    opt => opt.MapFrom(src => src.Category)
                )
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : null));

            CreateMap<ProductDetail, ResultProductDetailDto>();
            CreateMap<ProductDetail, GetByIdProductDetailDto>();

            CreateMap<ProductImage, ResultProductImageDto>();
            CreateMap<ProductImage, GetByIdProductImageDto>();

            CreateMap<FeatureSlider, ResultFeatureSliderDto>();
            CreateMap<CreateFeatureSliderDto, FeatureSlider>();
            CreateMap<UpdateFeatureSliderDto, FeatureSlider>();
            CreateMap<FeatureSlider, GetByIdFeatureSliderDto>();

            CreateMap<SpecialOffer, ResultSpecialOfferDto>();
            CreateMap<CreateSpecialOfferDto, SpecialOffer>();
            CreateMap<UpdateSpecialOfferDto, SpecialOffer>();
            CreateMap<SpecialOffer, GetByIdSpecialOfferDto>();

            CreateMap<Feature, ResultFeatureDto>();
            CreateMap<CreateFeatureDto, Feature>();
            CreateMap<UpdateFeatureDto, Feature>();
            CreateMap<Feature, GetByIdFeatureDto>();

            CreateMap<OfferDiscount, ResultOfferDiscountDto>();
            CreateMap<CreateOfferDiscountDto, OfferDiscount>();
            CreateMap<UpdateOfferDiscountDto, OfferDiscount>();
            CreateMap<OfferDiscount, GetByIdOfferDiscountDto>();

            CreateMap<Brand, ResultBrandDto>();
            CreateMap<CreateBrandDto, Brand>();
            CreateMap<UpdateBrandDto, Brand>();
            CreateMap<Brand, GetByIdBrandDto>();

            CreateMap<About, ResultAboutDto>();
            CreateMap<CreateAboutDto, About>();
            CreateMap<UpdateAboutDto, About>();
            CreateMap<About, GetByIdAboutDto>();
        }
    }
}