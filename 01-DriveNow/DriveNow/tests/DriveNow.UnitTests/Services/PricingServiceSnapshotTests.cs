
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;
using VerifyTests;

namespace DriveNow.UnitTests.Services;

public class PricingServiceSnapshotTests
{
    [Fact]
    public async Task Should_MatchPricingSnapshot_When_WorkedExampleAIsUsed()
    {
        var customer = new Customer
        {
            DateOfBirth = new DateTime(1992, 11, 15),
            LoyaltyTier = LoyaltyTier.Gold
        };

        var vehicle = new Vehicle
        {
            DailyRate = 600m,
            Category = VehicleCategory.Economy
        };

        var request = new BookingRequest
        {
            PickupAt = new DateTime(2026, 11, 15, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 18, 10, 0, 0),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        var service = CreatePricingService();

        var breakdown = service.Calculate(customer, vehicle, request);

        await Verifier.Verify(breakdown);
    }

    [Fact]
    public async Task Should_MatchPricingSnapshot_When_WorkedExampleBIsUsed()
    {
        var customer = new Customer
        {
            DateOfBirth = new DateTime(2003, 11, 12),
            LoyaltyTier = LoyaltyTier.Silver
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1500m,
            Category = VehicleCategory.SUV
        };

        var request = new BookingRequest
        {
            PickupAt = new DateTime(2026, 11, 12, 9, 0, 0),
            ReturnAt = new DateTime(2026, 11, 19, 9, 30, 0),
            InsurancePlan = InsurancePlan.Premium,
            AdditionalDrivers = 1,
            PromoCode = "SAVE15"
        };

        var service = CreatePricingService();

        var breakdown = service.Calculate(customer, vehicle, request);

        await Verifier.Verify(breakdown);
    }

    private static PricingService CreatePricingService()
    {
        var promo = new PromoCode
        {
            Code = "SAVE15",
            DiscountPercent = 15,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidUntil = new DateTime(2026, 12, 31),
            MinDays = 1,
            AllowedCategories = null,
            MaxUses = 100,
            UsedCount = 0
        };

        return new PricingService(
            new FakePromoCodeRepository(promo),
            new FixedTimeProvider(
                new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.Zero)));
    }

    private sealed class FakePromoCodeRepository(PromoCode promo)
        : IPromoCodeRepository
    {
        public PromoCode? GetByCode(string code) =>
            string.Equals(code, promo.Code,
                StringComparison.OrdinalIgnoreCase)
                ? promo
                : null;

        public void Update(PromoCode promo)
        {
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
