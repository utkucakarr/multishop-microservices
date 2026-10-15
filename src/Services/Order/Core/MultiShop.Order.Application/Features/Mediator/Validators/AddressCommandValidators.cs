using FluentValidation;
using MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands;

namespace MultiShop.Order.Application.Features.Mediator.Validators
{
    /// <summary>Adres oluşturma ve güncellemenin ortak kuralları. Mesajlar FluentValidation'ın Türkçe metinleri (ServiceRegistration).</summary>
    public abstract class AddressCommandValidator<T> : AbstractValidator<T> where T : AddressCommandBase
    {
        protected AddressCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(AddressLimits.NameMaxLength).WithName("Ad");
            RuleFor(x => x.SurName).NotEmpty().MaximumLength(AddressLimits.NameMaxLength).WithName("Soyad");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(AddressLimits.EmailMaxLength).WithName("E-posta");
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(AddressLimits.PhoneMaxLength).WithName("Telefon");
            RuleFor(x => x.Country).NotEmpty().MaximumLength(AddressLimits.PlaceMaxLength).WithName("Ülke");
            RuleFor(x => x.City).NotEmpty().MaximumLength(AddressLimits.PlaceMaxLength).WithName("Şehir");
            RuleFor(x => x.District).NotEmpty().MaximumLength(AddressLimits.PlaceMaxLength).WithName("İlçe");
            RuleFor(x => x.Detail1).NotEmpty().MaximumLength(AddressLimits.LineMaxLength).WithName("Adres satırı 1");
            RuleFor(x => x.Detail2).MaximumLength(AddressLimits.LineMaxLength).WithName("Adres satırı 2");
            RuleFor(x => x.ZipCode).MaximumLength(AddressLimits.ZipCodeMaxLength).WithName("Posta kodu");
            RuleFor(x => x.Description).MaximumLength(AddressLimits.DescriptionMaxLength).WithName("Açıklama");
        }
    }

    public class CreateAddressCommandValidator : AddressCommandValidator<CreateAddressCommand>
    {
    }

    public class UpdateAddressCommandValidator : AddressCommandValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator()
        {
            RuleFor(x => x.AddressId).GreaterThan(0);
        }
    }
}
