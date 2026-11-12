
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Receipts;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
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

    // Return validation tests

    [Fact]
    public void Should_ThrowBookingNotFoundException_When_BookingDoesNotExist()
    {
        // Arrange
        var (sut, _) = CreateScenario(0);

        // Act
        var act = () => sut.ProcessReturn(
            999,
            endMileage: 5250,
            fuelQuarters: 4,
            damageCost: 0);

        // Assert
        var exception = Assert.Throws<BookingNotFoundException>(act);
        Assert.Equal("Booking #999 not found", exception.Message);
    }

    [Fact]
    public void Should_ThrowInvalidBookingStateException_When_BookingIsNotActive()
    {
        // Arrange
        var (sut, booking) = CreateScenario(0);
        booking.Status = BookingStatus.Reserved;

        // Act
        var act = () => sut.ProcessReturn(
            booking.Id,
            endMileage: 5250,
            fuelQuarters: 4,
            damageCost: 0);

        // Assert
        var exception = Assert.Throws<InvalidBookingStateException>(act);
        Assert.Equal("Only active bookings can be returned", exception.Message);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_EndMileageIsBelowStartMileage()
    {
        // Arrange
        var (sut, booking) = CreateScenario(0);

        // Act
        var act = () => sut.ProcessReturn(
            booking.Id,
            endMileage: 4999,
            fuelQuarters: 4,
            damageCost: 0);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("End mileage cannot be below the start mileage", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void Should_ThrowArgumentException_When_FuelQuartersAreOutsideValidRange(
        int fuelQuarters)
    {
        // Arrange
        var (sut, booking) = CreateScenario(0);

        // Act
        var act = () => sut.ProcessReturn(
            booking.Id,
            endMileage: 5250,
            fuelQuarters: fuelQuarters,
            damageCost: 0);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("Fuel quarters must be between 0 and 4", exception.Message);
    }


    // BR-46: Return side effects

    [Fact]
    public void Should_UpdateBookingVehicleAndCustomer_When_ReturnIsProcessed()
    {
        // Arrange
        var scenario = CreateScenarioWithSideEffects(
            completedRentals: 4,
            deposit: 500m);

        const int endMileage = 5251;

        // Act
        var report = scenario.Sut.ProcessReturn(
            scenario.Booking.Id,
            endMileage,
            fuelQuarters: 3,
            damageCost: 1000m);

        // Assert - Booking
        Assert.Equal(BookingStatus.Completed, scenario.Booking.Status);
        Assert.Equal(endMileage, scenario.Booking.EndMileage);
        Assert.Equal(scenario.Now.UtcDateTime, scenario.Booking.ActualReturnAt);
        Assert.Equal(report.TotalExtra, scenario.Booking.ExtraCharges);

        // Vehicle
        Assert.Equal(endMileage, scenario.Vehicle.Mileage);

        // Customer
        Assert.Equal(5, scenario.Customer.CompletedRentals);
        Assert.Equal(LoyaltyTier.Silver, scenario.Customer.LoyaltyTier);

        // Extra charges
        Assert.True(report.TotalExtra > 0);
        var charge = Assert.Single(scenario.Payments.Charges);
        Assert.Equal(scenario.Customer.Id, charge.CustomerId);
        Assert.Equal(report.TotalExtra, charge.Amount);
        Assert.Equal($"Extras {scenario.Booking.Reference}", charge.Description);

        // Deposit refund
        var refund = Assert.Single(scenario.Payments.Refunds);
        Assert.Equal(scenario.Customer.Id, refund.CustomerId);
        Assert.Equal(500m, refund.Amount);
        Assert.Equal(
            $"Deposit refund {scenario.Booking.Reference}",
            refund.Description);

        // Receipt email
        var email = Assert.Single(scenario.Email.Messages);
        Assert.Equal(scenario.Customer.Email, email.To);
        Assert.Equal($"Receipt {scenario.Booking.Reference}", email.Subject);
        Assert.False(string.IsNullOrWhiteSpace(email.Body));

        // Audit log
        Assert.Contains(
            $"Booking {scenario.Booking.Reference} returned",
            scenario.AuditLogger.Messages);
    }

    [Fact]
    public void Should_NotChargeOrRefund_When_ExtraChargesAndDepositAreZero()
    {
        // Arrange
        var scenario = CreateScenarioWithSideEffects(
            completedRentals: 0,
            deposit: 0m);

        // Act
        var report = scenario.Sut.ProcessReturn(
            scenario.Booking.Id,
            endMileage: 5250,
            fuelQuarters: 4,
            damageCost: 0m);

        // Assert
        Assert.Equal(0m, report.TotalExtra);
        Assert.Empty(scenario.Payments.Charges);
        Assert.Empty(scenario.Payments.Refunds);
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
    private static (
    ReturnService Sut,
    Booking Booking,
    Vehicle Vehicle,
    Customer Customer,
    MockPaymentGateway Payments,
    RecordingEmailSender Email,
    RecordingAuditLogger AuditLogger,
    DateTimeOffset Now)
    CreateScenarioWithSideEffects(
        int completedRentals,
        decimal deposit)
    {
        var returnAt = new DateTime(
            2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);

        var now = new DateTimeOffset(
            returnAt,
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
            CompletedRentals = completedRentals
        };

        var vehicle = new Vehicle
        {
            Id = 10,
            PlateNumber = "TEST-123",
            Model = "Test Car",
            Category = VehicleCategory.Economy,
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
            InsurancePlan = InsurancePlan.Basic,
            Deposit = deposit,
            Price = new PriceBreakdown
            {
                RentalDays = 1,
                BaseAmount = 100m,
                Total = 100m
            }
        };

        var bookings = new FakeBookingRepository();
        bookings.Bookings.Add(booking);

        var payments = new MockPaymentGateway();
        var email = new RecordingEmailSender();
        var auditLogger = new RecordingAuditLogger();

        var receiptService = new ReceiptService(
            new LineItemFactory(),
            new TaxCalculator(),
            new CurrencyFormatter(),
            new ReceiptFormatter());

        var sut = new ReturnService(
            bookings,
            new StubVehicleRepository(vehicle),
            new StubCustomerRepository(customer),
            payments,
            email,
            auditLogger,
            timeProvider,
            receiptService);

        return (
            sut,
            booking,
            vehicle,
            customer,
            payments,
            email,
            auditLogger,
            now);
    }
    private sealed class RecordingEmailSender : IEmailSender
    {
        public List<(string To, string Subject, string Body)> Messages { get; } = [];

        public void Send(string to, string subject, string body)
        {
            Messages.Add((to, subject, body));
        }
    }

    private sealed class RecordingAuditLogger
        : DriveNow.Application.Abstractions.Logging.IAuditLogger
    {
        public List<string> Messages { get; } = [];

        public void Write(string message)
        {
            Messages.Add(message);
        }
    }
}
