
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Abstractions.Logging;
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.UnitTests.TestDoubles;

namespace DriveNow.UnitTests.Services;

public class CancellationServiceTests
{
    // BR-51: Cancellation fee boundaries

    [Theory]
    [InlineData(49, 0)]
    [InlineData(48, 0)]
    [InlineData(47, 50)]
    [InlineData(25, 50)]
    [InlineData(24, 50)]
    [InlineData(23, 100)]
    [InlineData(1, 100)]
    public void Should_CalculateCancellationFee_When_HoursUntilPickupChange(
        double hours,
        decimal expectedFee)
    {
        // Arrange
        var scenario = CreateScenario(hours);

        // Act
        var fee = scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Equal(expectedFee, fee);
    }

    [Theory]
    [InlineData(49, 0)]
    [InlineData(48, 0)]
    [InlineData(47, 0)]
    [InlineData(24, 0)]
    [InlineData(23, 0)]
    [InlineData(1, 0)]
    public void Should_NotChargeCancellationFee_When_CustomerIsPlatinum(
        double hours,
        decimal expectedFee)
    {
        // Arrange
        var scenario = CreateScenario(
            hours,
            loyaltyTier: LoyaltyTier.Platinum);

        // Act
        var fee = scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Equal(expectedFee, fee);
        Assert.Empty(scenario.Payments.Charges);
    }

    // BR-52: Deposit refund and additional charge

    [Theory]
    [InlineData(12, 150, 0, 50)]
    [InlineData(12, 50, 50, 0)]
    [InlineData(12, 100, 0, 0)]
    [InlineData(48, 500, 0, 500)]
    public void Should_HandleDeposit_When_CancellationFeeChanges(
        double hours,
        decimal deposit,
        decimal expectedCharge,
        decimal expectedRefund)
    {
        // Arrange
        var scenario = CreateScenario(
            hours,
            deposit: deposit);

        // Act
        scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Equal(
            expectedCharge,
            scenario.Payments.Charges.Sum(x => x.Amount));

        Assert.Equal(
            expectedRefund,
            scenario.Payments.Refunds.Sum(x => x.Amount));
    }

    // BR-53: Cancellation processing

    [Fact]
    public void Should_UpdateBookingAndPromo_When_BookingIsCancelled()
    {
        // Arrange
        var scenario = CreateScenario(
            hoursUntilPickup: 72,
            promoCode: "SAVE10");

        // Act
        scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Equal(
            BookingStatus.Cancelled,
            scenario.Booking.Status);

        Assert.Equal(2, scenario.Promo.UsedCount);
        Assert.Equal(1, scenario.PromoRepository.UpdateCalls);
    }

    [Fact]
    public void Should_SendSmsAndEmail_When_BookingIsCancelled()
    {
        // Arrange
        var scenario = CreateScenario(72);

        var expectedMessage =
            $"Booking {scenario.Booking.Reference} cancelled";

        // Act
        scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Single(scenario.Sms.Messages);
        Assert.Equal(
            scenario.Customer.Phone,
            scenario.Sms.Messages[0].Phone);

        Assert.Equal(
            expectedMessage,
            scenario.Sms.Messages[0].Text);

        Assert.Single(scenario.Email.Messages);
        Assert.Equal(
            scenario.Customer.Email,
            scenario.Email.Messages[0].To);

        Assert.Equal(
            expectedMessage,
            scenario.Email.Messages[0].Subject);

        Assert.Equal(
            expectedMessage,
            scenario.Email.Messages[0].Body);
    }

    [Fact]
    public void Should_WriteAuditLog_When_BookingIsCancelled()
    {
        // Arrange
        var scenario = CreateScenario(72);

        var expectedMessage =
            $"Booking {scenario.Booking.Reference} cancelled";

        // Act
        scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Contains(
            expectedMessage,
            scenario.Audit.Messages);
    }

    [Fact]
    public void Should_NotUpdatePromo_When_BookingHasNoPromoCode()
    {
        // Arrange
        var scenario = CreateScenario(72);

        // Act
        scenario.Sut.Cancel(scenario.Booking.Id);

        // Assert
        Assert.Equal(0, scenario.PromoRepository.UpdateCalls);
    }

    [Fact]
    public void Should_RejectCancellation_When_BookingIsNotReserved()
    {
        // Arrange
        var scenario = CreateScenario(72);
        scenario.Booking.Status = BookingStatus.Active;

        // Act
        var exception = Assert.Throws<InvalidBookingStateException>(
            () => scenario.Sut.Cancel(scenario.Booking.Id));

        // Assert
        Assert.Equal(
            "Only reserved bookings can be cancelled",
            exception.Message);
    }

    [Fact]
    public void Should_RejectCancellation_When_PickupTimeHasArrived()
    {
        // Arrange
        var scenario = CreateScenario(0);

        // Act
        var exception = Assert.Throws<InvalidBookingStateException>(
            () => scenario.Sut.Cancel(scenario.Booking.Id));

        // Assert
        Assert.Equal(
            "Booking has already started",
            exception.Message);
    }

    private static CancellationScenario CreateScenario(
        double hoursUntilPickup,
        decimal deposit = 0,
        LoyaltyTier loyaltyTier = LoyaltyTier.None,
        string? promoCode = null)
    {
        var now = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var timeProvider = new FixedTimeProvider(now);

        var customer = new Customer
        {
            Id = 1,
            FullName = "Test Customer",
            Email = "test@example.com",
            Phone = "01000000000",
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2010, 1, 1),
            LoyaltyTier = loyaltyTier,
            CompletedRentals = 0
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
            PickupAt = now.AddHours(hoursUntilPickup).DateTime,
            ReturnAt = now.AddDays(1).DateTime,
            Status = BookingStatus.Reserved,
            Deposit = deposit,
            PromoCode = promoCode
        };

        var bookings = new FakeBookingRepository();
        bookings.Bookings.Add(booking);

        var payments = new MockPaymentGateway();
        var sms = new SpySmsSender();
        var email = new RecordingEmailSender();
        var audit = new RecordingAuditLogger();

        var promo = new PromoCode
        {
            Code = "SAVE10",
            DiscountPercent = 10,
            UsedCount = 3,
            MaxUses = 10
        };

        var promoRepository = new RecordingPromoCodeRepository(promo);

        var sut = new CancellationService(
            bookings,
            new StubVehicleRepository(vehicle),
            new StubCustomerRepository(customer),
            promoRepository,
            payments,
            sms,
            email,
            audit,
            timeProvider);

        return new CancellationScenario(
            sut,
            booking,
            customer,
            payments,
            sms,
            email,
            audit,
            promo,
            promoRepository);
    }

    private sealed record CancellationScenario(
        CancellationService Sut,
        Booking Booking,
        Customer Customer,
        MockPaymentGateway Payments,
        SpySmsSender Sms,
        RecordingEmailSender Email,
        RecordingAuditLogger Audit,
        PromoCode Promo,
        RecordingPromoCodeRepository PromoRepository);

    private sealed class RecordingEmailSender : IEmailSender
    {
        public List<(string To, string Subject, string Body)> Messages { get; } = [];

        public void Send(string to, string subject, string body)
        {
            Messages.Add((to, subject, body));
        }
    }

    private sealed class RecordingAuditLogger : IAuditLogger
    {
        public List<string> Messages { get; } = [];

        public void Write(string message)
        {
            Messages.Add(message);
        }
    }

    private sealed class RecordingPromoCodeRepository : IPromoCodeRepository
    {
        private readonly PromoCode _promo;

        public int UpdateCalls { get; private set; }

        public RecordingPromoCodeRepository(PromoCode promo)
        {
            _promo = promo;
        }

        public PromoCode? GetByCode(string code)
        {
            return string.Equals(
                code,
                _promo.Code,
                StringComparison.OrdinalIgnoreCase)
                ? _promo
                : null;
        }

        public void Update(PromoCode promo)
        {
            UpdateCalls++;
        }
    }
}
