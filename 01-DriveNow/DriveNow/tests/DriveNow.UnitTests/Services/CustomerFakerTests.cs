
using DriveNow.Domain.Enums;
using DriveNow.UnitTests.Fakers;
using FluentAssertions;

namespace DriveNow.UnitTests.Services;

public class CustomerFakerTests
{
    [Fact]
    public void Should_GenerateCustomerWithRequestedAgeAndTier_When_CustomizationsAreProvided()
    {
        var customer = new CustomerFaker()
            .WithAge(23)
            .WithTier(LoyaltyTier.Silver)
            .Generate();

        customer.DateOfBirth.Year.Should().Be(2003);
        customer.LoyaltyTier.Should().Be(LoyaltyTier.Silver);
        customer.FullName.Should().NotBeNullOrWhiteSpace();
        customer.Email.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Should_GenerateSameCustomerData_When_SeedIsFixed()
    {
        var first = new CustomerFaker().Generate();
        var second = new CustomerFaker().Generate();

        first.FullName.Should().Be(second.FullName);
        first.Email.Should().Be(second.Email);
        first.DateOfBirth.Should().Be(second.DateOfBirth);
    }
}
