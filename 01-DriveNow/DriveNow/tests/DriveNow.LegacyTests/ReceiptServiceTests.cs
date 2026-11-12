
using DriveNow.Application.Receipts;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;
using VerifyTests;

namespace DriveNow.LegacyTests;

public class ReceiptServiceTests
{
    [Fact]
    public async Task Should_MatchReceiptSnapshot_When_CompletedRentalHasLateFeeAndDamage()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            Reference = "DN-TEST-0001",
            CustomerId = 1,
            VehicleId = 7,
            PickupAt = new DateTime(2026, 11, 7, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ActualReturnAt = new DateTime(2026, 11, 10, 12, 0, 0),
            Status = BookingStatus.Completed,
            Price = new PriceBreakdown
            {
                RentalDays = 3,
                BaseAmount = 12000m,
                InsuranceCost = 500m,
                Total = 12500m
            }
        };

        var report = new ReturnReport
        {
            LateMinutes = 120,
            LateFee = 800m,
            DamageCharge = 1000m,
            TotalExtra = 1800m
        };

        var sut = new ReceiptService(
            new LineItemFactory(),
            new TaxCalculator(),
            new CurrencyFormatter(),
            new ReceiptFormatter());

        // Act
        var receipt = sut.Build(booking, report);

        // Assert
        await Verifier.Verify(receipt);
    }
}
