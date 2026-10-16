using MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands;
using MultiShop.Order.Application.Features.Mediator.Validators;

namespace MultiShop.Order.UnitTests.Validators
{
    // Adres gönderiye kopyalanır (K-3): uzunluk sınırları Cargo'nun sütunlarına göre (AddressLimits).
    public class AddressCommandValidatorTests
    {
        private static CreateAddressCommand ValidCommand() => new()
        {
            UserId = "user-1",
            Name = "Ayşe",
            SurName = "Yılmaz",
            Email = "ayse@example.com",
            Phone = "5550000000",
            Country = "Türkiye",
            City = "İstanbul",
            District = "Kadıköy",
            Detail1 = "Moda Cad. 1",
        };

        [Fact]
        public void Valid_address_passes()
        {
            new CreateAddressCommandValidator().Validate(ValidCommand()).IsValid.Should().BeTrue();
        }

        public static TheoryData<string, Action<CreateAddressCommand>> InvalidAddresses => new()
        {
            { nameof(CreateAddressCommand.Name), x => x.Name = "" },
            { nameof(CreateAddressCommand.SurName), x => x.SurName = " " },
            { nameof(CreateAddressCommand.Email), x => x.Email = "gecersiz-eposta" },
            { nameof(CreateAddressCommand.Phone), x => x.Phone = "" },
            { nameof(CreateAddressCommand.Country), x => x.Country = "" },
            { nameof(CreateAddressCommand.City), x => x.City = "" },
            { nameof(CreateAddressCommand.District), x => x.District = "" },
            { nameof(CreateAddressCommand.Detail1), x => x.Detail1 = "" },
            { nameof(CreateAddressCommand.Name), x => x.Name = new string('a', AddressLimits.NameMaxLength + 1) },
            { nameof(CreateAddressCommand.Detail2), x => x.Detail2 = new string('a', AddressLimits.LineMaxLength + 1) },
            { nameof(CreateAddressCommand.ZipCode), x => x.ZipCode = new string('1', AddressLimits.ZipCodeMaxLength + 1) },
            { nameof(CreateAddressCommand.Description), x => x.Description = new string('a', AddressLimits.DescriptionMaxLength + 1) },
        };

        [Theory]
        [MemberData(nameof(InvalidAddresses))]
        public void Invalid_field_is_reported(string property, Action<CreateAddressCommand> breakIt)
        {
            var command = ValidCommand();
            breakIt(command);

            var result = new CreateAddressCommandValidator().Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == property);
        }

        [Fact]
        public void Optional_fields_may_be_empty()
        {
            var command = ValidCommand();
            command.Detail2 = null;
            command.ZipCode = null;
            command.Description = null;

            new CreateAddressCommandValidator().Validate(command).IsValid.Should().BeTrue();
        }

        [Fact]
        public void Update_requires_an_address_id()
        {
            var command = new UpdateAddressCommand
            {
                AddressId = 0, Name = "Ayşe", SurName = "Yılmaz", Email = "ayse@example.com", Phone = "5550000000",
                Country = "Türkiye", City = "İstanbul", District = "Kadıköy", Detail1 = "Moda Cad. 1"
            };

            var result = new UpdateAddressCommandValidator().Validate(command);

            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(UpdateAddressCommand.AddressId));
        }
    }
}
