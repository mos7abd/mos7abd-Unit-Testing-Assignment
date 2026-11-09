
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;
using FluentAssertions;

namespace DriveNow.UnitTests.Services;

public class PricingServiceTests
{
    private static readonly DateTime Pickup =
        new(2026, 11, 8, 10, 0, 0);

    private readonly PricingService _sut =
        new(new FakePromoCodeRepository(), TimeProvider.System);

    private static Customer CreateCustomer(
        int age = 30,
        LoyaltyTier tier = LoyaltyTier.None)
    {
        return new Customer
        {
            DateOfBirth = Pickup.AddYears(-age),
            LoyaltyTier = tier
        };
    }

    private static Vehicle CreateVehicle(
        decimal dailyRate = 1000m,
        VehicleCategory category = VehicleCategory.Economy)
    {
        return new Vehicle
        {
            DailyRate = dailyRate,
            Category = category
        };
    }

    private static BookingRequest CreateRequest(
        int days = 1,
        DateTime? pickup = null,
        InsurancePlan insurance = InsurancePlan.Basic,
        int additionalDrivers = 0)
    {
        var start = pickup ?? Pickup;

        return new BookingRequest
        {
            PickupAt = start,
            ReturnAt = start.AddDays(days),
            InsurancePlan = insurance,
            AdditionalDrivers = additionalDrivers
        };
    }

    // BR-31: Weekend surcharge

    [Theory]
    [InlineData(100, 1, 0)]
    [InlineData(100, 4, 40)]
    [InlineData(200, 4, 80)]
    public void Should_CalculateWeekendSurcharge_When_RentalIncludesWeekendDays(
        decimal dailyRate,
        int days,
        decimal expected)
    {
        // Arrange
        var thursday = new DateTime(2026, 11, 5, 10, 0, 0);

        var customer = CreateCustomer();
        var vehicle = CreateVehicle(dailyRate);
        var request = CreateRequest(days, thursday);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.WeekendSurcharge.Should().Be(expected);
    }

    // BR-32: Young driver surcharge

    [Theory]
    [InlineData(20, 0)]
    [InlineData(21, 150)]
    [InlineData(24, 150)]
    [InlineData(25, 0)]
    public void Should_CalculateYoungDriverSurcharge_When_AgeIsWithinEligibleRange(
        int age,
        decimal expected)
    {
        // Arrange
        var customer = CreateCustomer(age);
        var vehicle = CreateVehicle();
        var request = CreateRequest();

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.YoungDriverSurcharge.Should().Be(expected);
    }

    // BR-33: Duration discount

    [Theory]
    [InlineData(6, 0)]
    [InlineData(7, 700)]
    [InlineData(13, 1300)]
    [InlineData(14, 2100)]
    public void Should_CalculateDurationDiscount_When_RentalDurationChanges(
        int days,
        decimal expected)
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle = CreateVehicle();
        var request = CreateRequest(days);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.DurationDiscount.Should().Be(expected);
    }

    // BR-34: Loyalty discount

    [Theory]
    [InlineData(LoyaltyTier.None, 0)]
    [InlineData(LoyaltyTier.Silver, 50)]
    [InlineData(LoyaltyTier.Gold, 100)]
    [InlineData(LoyaltyTier.Platinum, 150)]
    public void Should_CalculateLoyaltyDiscount_When_CustomerHasLoyaltyTier(
        LoyaltyTier tier,
        decimal expected)
    {
        // Arrange
        var customer = CreateCustomer(tier: tier);
        var vehicle = CreateVehicle();
        var request = CreateRequest();

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.LoyaltyDiscount.Should().Be(expected);
    }

    // BR-38: Insurance cost

    [Theory]
    [InlineData(InsurancePlan.Basic, VehicleCategory.Economy, 0)]
    [InlineData(InsurancePlan.Standard, VehicleCategory.Economy, 150)]
    [InlineData(InsurancePlan.Premium, VehicleCategory.Economy, 300)]
    [InlineData(InsurancePlan.Basic, VehicleCategory.Luxury, 0)]
    [InlineData(InsurancePlan.Standard, VehicleCategory.Luxury, 300)]
    [InlineData(InsurancePlan.Premium, VehicleCategory.Luxury, 600)]
    public void Should_CalculateInsuranceCost_When_PlanAndCategoryChange(
        InsurancePlan plan,
        VehicleCategory category,
        decimal expected)
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(category: category);
        var request = CreateRequest(insurance: plan);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.InsuranceCost.Should().Be(expected);
    }

    // BR-39: Additional drivers

    [Theory]
    [InlineData(LoyaltyTier.None, 100)]
    [InlineData(LoyaltyTier.Silver, 100)]
    [InlineData(LoyaltyTier.Gold, 0)]
    [InlineData(LoyaltyTier.Platinum, 0)]
    public void Should_CalculateAdditionalDriversCost_When_LoyaltyTierChanges(
        LoyaltyTier tier,
        decimal expected)
    {
        // Arrange
        var customer = CreateCustomer(tier: tier);
        var vehicle = CreateVehicle();
        var request = CreateRequest(additionalDrivers: 1);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.AdditionalDriversCost.Should().Be(expected);
    }
    // BR-30: Base amount

    [Theory]
    [InlineData(1000, 1, 1000)]
    [InlineData(600, 3, 1800)]
    [InlineData(1500, 7, 10500)]
    public void Should_CalculateBaseAmount_When_DailyRateAndDurationChange(
        decimal dailyRate,
        int days,
        decimal expected)
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(dailyRate);
        var request = CreateRequest(days);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.BaseAmount.Should().Be(expected);
    }

    [Theory]
    [InlineData(LoyaltyTier.None, 200)]
    [InlineData(LoyaltyTier.Silver, 200)]
    [InlineData(LoyaltyTier.Gold, 100)]
    [InlineData(LoyaltyTier.Platinum, 100)]
    public void Should_ChargeAdditionalDriversAfterFirstFreeDriver_When_LoyaltyTierChanges(
    LoyaltyTier tier,
    decimal expected)
    {
        // Arrange
        var customer = CreateCustomer(tier: tier);
        var vehicle = CreateVehicle();

        var request = CreateRequest(additionalDrivers: 2);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.AdditionalDriversCost.Should().Be(expected);
    }
    [Theory]
    [InlineData(LoyaltyTier.None, 200)]
    [InlineData(LoyaltyTier.Silver, 200)]
    [InlineData(LoyaltyTier.Gold, 100)]
    [InlineData(LoyaltyTier.Platinum, 100)]
    public void Should_CalculateRemainingDriversCost_When_FirstDriverIsFree(
    LoyaltyTier tier,
    decimal expected)
    { 
        // Arrange
        var customer = CreateCustomer(tier: tier);
        var vehicle = CreateVehicle();

        var request = CreateRequest(additionalDrivers: 2);

        // Act
        var result = _sut.Calculate(customer, vehicle, request);

        // Assert
        result.AdditionalDriversCost.Should().Be(expected);
    }
    private sealed class FakePromoCodeRepository : IPromoCodeRepository
    {
        public PromoCode? GetByCode(string code) => null;

        public void Update(PromoCode promo)
        {
        }
    }
}
