using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.UnitTests.Services
{

    public class PricingServiceScenarioTests
    {
        public static TheoryData<Customer, Vehicle, BookingRequest, PriceBreakdown>
            WorkedExamples => new()
        {
        // Worked Example A
        {
            new Customer
            {
                DateOfBirth = new DateTime(1992, 11, 8),
                LoyaltyTier = LoyaltyTier.Gold
            },
            new Vehicle
            {
                DailyRate = 600m,
                Category = VehicleCategory.Economy
            },
            new BookingRequest
            {
                PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
                ReturnAt = new DateTime(2026, 11, 11, 10, 0, 0),
                InsurancePlan = InsurancePlan.Standard,
                AdditionalDrivers = 0
            },
            new PriceBreakdown
            {
                RentalDays = 3,
                BaseAmount = 1800m,
                WeekendSurcharge = 0m,
                YoungDriverSurcharge = 0m,
                DurationDiscount = 0m,
                LoyaltyDiscount = 180m,
                PromoDiscount = 0m,
                InsuranceCost = 450m,
                AdditionalDriversCost = 0m,
                Total = 2070m
            }
        },

        // Worked Example B
        {
            new Customer
            {
                DateOfBirth = new DateTime(2003, 11, 5),
                LoyaltyTier = LoyaltyTier.Silver
            },
            new Vehicle
            {
                DailyRate = 1500m,
                Category = VehicleCategory.SUV
            },
            new BookingRequest
            {
                PickupAt = new DateTime(2026, 11, 5, 9, 0, 0),
                ReturnAt = new DateTime(2026, 11, 12, 9, 30, 0),
                InsurancePlan = InsurancePlan.Premium,
                AdditionalDrivers = 1,
                PromoCode = "SAVE15"
            },
            new PriceBreakdown
            {
                RentalDays = 7,
                BaseAmount = 10500m,
                WeekendSurcharge = 600m,
                YoungDriverSurcharge = 1575m,
                DurationDiscount = 1050m,
                LoyaltyDiscount = 525m,
                PromoDiscount = 1050m,
                InsuranceCost = 2100m,
                AdditionalDriversCost = 700m,
                Total = 12850m
            }
        }
        };

        [Theory]
        [MemberData(nameof(WorkedExamples))]
        public void Should_CalculateExpectedBreakdown_When_WorkedExampleIsProvided(
            Customer customer,
            Vehicle vehicle,
            BookingRequest request,
            PriceBreakdown expected)
        {
            var repository = new FakePromoCodeRepository();

            var sut = new PricingService(
                repository,
                new FixedTimeProvider());

            var actual = sut.Calculate(customer, vehicle, request);

            using (new AssertionScope())
            {
                actual.Should().BeEquivalentTo(expected);
            }
        }

        private sealed class FakePromoCodeRepository : IPromoCodeRepository
        {
            private readonly PromoCode _promo = new()
            {
                Code = "SAVE15",
                DiscountPercent = 15,
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 11, 30),
                MinDays = 7,
                AllowedCategories = [VehicleCategory.SUV],
                MaxUses = 100,
                UsedCount = 0
            };

            public PromoCode? GetByCode(string code) =>
                string.Equals(code, _promo.Code, StringComparison.OrdinalIgnoreCase)
                    ? _promo
                    : null;

            public void Update(PromoCode promo) { }
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() =>
                new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
        }
    }

}
