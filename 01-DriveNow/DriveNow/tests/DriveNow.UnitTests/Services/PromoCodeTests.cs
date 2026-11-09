using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.UnitTests.Services
{



    public class PromoCodeTests
    {
        [Theory]
        [ClassData(typeof(PromoCodeTestData))]
        public void Should_ValidatePromoCode_When_PromoScenarioIsProvided(
            string code,
            PromoCode? promo,
            int days,
            VehicleCategory category,
            decimal expectedDiscount,
            string? expectedError)
        {
            var repository = new FakePromoCodeRepository(promo);

            var sut = new PricingService(repository, new FixedTimeProvider());

            var customer = new Customer
            {
                DateOfBirth = new DateTime(1990, 1, 1),
                LoyaltyTier = LoyaltyTier.None
            };

            var vehicle = new Vehicle
            {
                DailyRate = 1000m,
                Category = category
            };

            var request = new BookingRequest
            {
                PickupAt = new DateTime(2026, 11, 9, 10, 0, 0),
                ReturnAt = new DateTime(2026, 11, 9, 10, 0, 0).AddDays(days),
                PromoCode = code,
                InsurancePlan = InsurancePlan.Basic
            };

            if (expectedError is not null)
            {
                var act = () => sut.Calculate(customer, vehicle, request);

                act.Should()
                    .Throw<InvalidPromoCodeException>()
                    .WithMessage(expectedError);

                return;
            }

            var actual = sut.Calculate(customer, vehicle, request);

            actual.PromoDiscount.Should().Be(expectedDiscount);
        }

        private sealed class FakePromoCodeRepository(PromoCode? promo)
            : IPromoCodeRepository
        {
            public PromoCode? GetByCode(string code)
            {
                if (promo is null)
                    return null;

                return string.Equals(
                    code,
                    promo.Code,
                    StringComparison.OrdinalIgnoreCase)
                    ? promo
                    : null;
            }

            public void Update(PromoCode promo) { }
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() =>
                new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
        }
    }

    public class PromoCodeTestData : TheoryData<
        string,
        PromoCode?,
        int,
        VehicleCategory,
        decimal,
        string?>
    {
        public PromoCodeTestData()
        {
            // Valid promo: also verifies case-insensitive lookup.
            Add(
                "save15",
                CreatePromo(),
                7,
                VehicleCategory.SUV,
                1050m,
                null);

            // BR-36: Promo code does not exist.
            Add(
                "UNKNOWN",
                null,
                7,
                VehicleCategory.SUV,
                0m,
                "Promo code 'UNKNOWN' does not exist");

            // BR-36: Promo has expired.
            Add(
                "SAVE15",
                CreatePromo(validUntil: new DateTime(2026, 11, 8)),
                7,
                VehicleCategory.SUV,
                0m,
                "Promo code 'SAVE15' has expired");

            // BR-36: Rental duration is below minimum.
            Add(
                "SAVE15",
                CreatePromo(minDays: 3),
                2,
                VehicleCategory.SUV,
                0m,
                "Promo code 'SAVE15' requires at least 3 days");

            // BR-36: Vehicle category is not allowed.
            Add(
                "SAVE15",
                CreatePromo(allowedCategory: VehicleCategory.SUV),
                7,
                VehicleCategory.Economy,
                0m,
                "Promo code 'SAVE15' is not valid for Economy");

            // BR-36: Promo has reached its usage limit.
            Add(
                "SAVE15",
                CreatePromo(maxUses: 10, usedCount: 10),
                7,
                VehicleCategory.SUV,
                0m,
                "Promo code 'SAVE15' has been fully used");

            // BR-36: The first failing validation must win.
            Add(
                "SAVE15",
                CreatePromo(
                    validUntil: new DateTime(2026, 11, 8),
                    minDays: 10,
                    allowedCategory: VehicleCategory.SUV,
                    maxUses: 10,
                    usedCount: 10),
                2,
                VehicleCategory.Economy,
                0m,
                "Promo code 'SAVE15' has expired");
        }

        private static PromoCode CreatePromo(
            DateTime? validUntil = null,
            int minDays = 7,
            VehicleCategory? allowedCategory = null,
            int maxUses = 100,
            int usedCount = 0)
        {
            return new PromoCode
            {
                Code = "SAVE15",
                DiscountPercent = 15,
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = validUntil ?? new DateTime(2026, 11, 30),
                MinDays = minDays,
                AllowedCategories = allowedCategory is null
                    ? [VehicleCategory.SUV]
                    : [allowedCategory.Value],
                MaxUses = maxUses,
                UsedCount = usedCount
            };
        }
    }

}
