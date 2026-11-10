
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Receipts;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;
using DriveNow.UnitTests.TestDoubles;

namespace DriveNow.UnitTests.Services;

public class ReturnServiceTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(59, 0)]
    [InlineData(60, 10)]
    [InlineData(61, 20)]
    [InlineData(119, 20)]
    [InlineData(120, 20)]
    [InlineData(239, 40)]
    [InlineData(240, 40)]
    [InlineData(241, 100)]
    [InlineData(1439, 100)]
    [InlineData(1440, 100)]
    [InlineData(1441, 200)]
    [InlineData(2879, 200)]
    [InlineData(2880, 200)]
    [InlineData(2881, 300)]
    public void Should_CalculateLateFee_When_LateMinutesChange(
        int lateMinutes,
        decimal expectedLateFee)
    {
        // Arrange
        var (sut, booking) = CreateScenario(lateMinutes);

        // Act
        var report = sut.ProcessReturn(
            booking.Id,
            endMileage: 5250,
            fuelQuarters: 4,
            damageCost: 0);

        // Assert
        Assert.Equal(lateMinutes, report.LateMinutes);
        Assert.Equal(expectedLateFee, report.LateFee);
    }


    // BR-42: Extra mileage charges

    [Theory]
    [InlineData(VehicleCategory.Economy, 0, 0)]
    [InlineData(VehicleCategory.Economy, 1, 0.75)]
    [InlineData(VehicleCategory.Economy, 2, 1.50)]
    [InlineData(VehicleCategory.Luxury, 1, 1.50)]
    public void Should_CalculateMileageFee_When_ExtraKilometersChange(
        VehicleCategory category,
        int extraKm,
        decimal expectedFee)
    {
        // Arrange
        var (sut, booking) = CreateScenario(
            0,
            category: category);

        // Act
        var report = sut.ProcessReturn(
            booking.Id,
            endMileage: 5250 + extraKm,
            fuelQuarters: 4,
            damageCost: 0);

        // Assert
        Assert.Equal(extraKm, report.ExtraKm);
        Assert.Equal(expectedFee, report.MileageFee);
    }

    // BR-43: Missing fuel charges

    [Theory]
    [InlineData(VehicleCategory.Economy, 4, 0)]
    [InlineData(VehicleCategory.Economy, 3, 200)]
    [InlineData(VehicleCategory.Economy, 2, 400)]
    [InlineData(VehicleCategory.Economy, 1, 600)]
    [InlineData(VehicleCategory.Economy, 0, 800)]
    [InlineData(VehicleCategory.SUV, 3, 300)]
    [InlineData(VehicleCategory.Van, 2, 600)]
    public void Should_CalculateFuelFee_When_FuelQuartersChange(
        VehicleCategory category,
        int fuelQuarters,
        decimal expectedFee)
    {
        // Arrange
        var (sut, booking) = CreateScenario(
            0,
            category: category);

        // Act
        var report = sut.ProcessReturn(
            booking.Id,
            endMileage: 5250,
            fuelQuarters: fuelQuarters,
            damageCost: 0);

        // Assert
        Assert.Equal(4 - fuelQuarters, report.MissingFuelQuarters);
        Assert.Equal(expectedFee, report.FuelFee);
    }

    // BR-44: Damage charges

    [Theory]
    [InlineData(InsurancePlan.Basic, 9999, 9999)]
    [InlineData(InsurancePlan.Basic, 10000, 10000)]
    [InlineData(InsurancePlan.Basic, 12000, 10000)]
    [InlineData(InsurancePlan.Standard, 2999, 2999)]
    [InlineData(InsurancePlan.Standard, 3000, 3000)]
    [InlineData(InsurancePlan.Standard, 5000, 3000)]
    [InlineData(InsurancePlan.Premium, 5000, 0)]
    public void Should_CalculateDamageCharge_When_DamageCostChanges(
        InsurancePlan insurancePlan,
        decimal damageCost,
        decimal expectedCharge)
    {
        // Arrange
        var (sut, booking) = CreateScenario(
            0,
            insurancePlan: insurancePlan);

        // Act
        var report = sut.ProcessReturn(
            booking.Id,
            endMileage: 5250,
            fuelQuarters: 4,
            damageCost: damageCost);

        // Assert
        Assert.Equal(expectedCharge, report.DamageCharge);
    }
    [Fact]
    public void Should_NotRefundUnusedRentalDays_When_BookingIsReturnedEarly()
    {
        // Arrange
        var (sut, booking) = CreateScenario(-60);

        // Act
        var report = sut.ProcessReturn(
            booking.Id,
            endMileage: 5250,
            fuelQuarters: 4,
            damageCost: 0);

        // Assert
        Assert.Equal(0, report.LateMinutes);
        Assert.Equal(0m, report.LateFee);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.Equal(
            new DateTime(2026, 11, 9, 11, 0, 0, DateTimeKind.Utc),
            booking.ActualReturnAt);
    }
    private static (ReturnService Sut, Booking Booking) CreateScenario(
    int lateMinutes,
    VehicleCategory category = VehicleCategory.Economy,
    InsurancePlan insurancePlan = InsurancePlan.Basic)
    {
        var returnAt = new DateTime(
            2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);

        var now = new DateTimeOffset(
            returnAt.AddMinutes(lateMinutes),
            TimeSpan.Zero);

        var timeProvider = new FixedTimeProvider(now);

        var customer = new Customer
        {
            Id = 1,
            FullName = "Test Customer",
            Email = "test@example.com",
            Phone = "01000000000",
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2010, 1, 1),
            LoyaltyTier = LoyaltyTier.None,
            CompletedRentals = 0
        };

        var vehicle = new Vehicle
        {
            Id = 10,
            PlateNumber = "TEST-123",
            Model = "Test Car",
            Category = category,
            DailyRate = 100m,
            Mileage = 5000,
            IsAvailable = true
        };

        var booking = new Booking
        {
            Id = 50,
            Reference = "DN-TEST-0001",
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = returnAt.AddDays(-1),
            ReturnAt = returnAt,
            StartMileage = 5000,
            Status = BookingStatus.Active,
            InsurancePlan = insurancePlan,
            Deposit = 0,
            Price = new PriceBreakdown
            {
                RentalDays = 1,
                BaseAmount = 100m,
                Total = 100m
            }
        };

        var bookings = new FakeBookingRepository();
        bookings.Bookings.Add(booking);

        var receiptService = new ReceiptService(
            new LineItemFactory(),
            new TaxCalculator(),
            new CurrencyFormatter(),
            new ReceiptFormatter());

        var sut = new ReturnService(
            bookings,
            new StubVehicleRepository(vehicle),
            new StubCustomerRepository(customer),
            new MockPaymentGateway(),
            new RecordingEmailSender(),
            new StubAuditLogger(),
            timeProvider,
            receiptService);

        return (sut, booking);
    }

    private sealed class RecordingEmailSender : IEmailSender
    {
        public void Send(string to, string subject, string body)
        {
        }
    }
}
