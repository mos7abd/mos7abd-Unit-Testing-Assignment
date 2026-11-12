
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.UnitTests.TestDoubles;
using Xunit;

namespace DriveNow.UnitTests.Services;

public class BookingServiceHandWrittenDoublesTests
{
    [Fact]
    public void Should_CreateBookingAndNotifyCustomer_When_RequestIsValid()
    {
        // Arrange
        var now = new DateTimeOffset(
            2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

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
            IsBlacklisted = false
        };

        var vehicle = new Vehicle
        {
            Id = 10,
            PlateNumber = "TEST-123",
            Model = "Test Car",
            Category = VehicleCategory.Economy,
            DailyRate = 100,
            Mileage = 5000,
            IsAvailable = true
        };

        var customers = new StubCustomerRepository(customer);
        var vehicles = new StubVehicleRepository(vehicle);
        var bookings = new FakeBookingRepository();
        var promos = new StubPromoCodeRepository();

        var payments = new MockPaymentGateway();
        var sms = new SpySmsSender();
        var email = new DummyEmailSender();
        var audit = new StubAuditLogger();
        var referenceGenerator = new StubReferenceGenerator();

        var pricing = new PricingService(promos, timeProvider);

        var sut = new BookingService(
            customers,
            vehicles,
            bookings,
            promos,
            payments,
            sms,
            email,
            audit,
            referenceGenerator,
            timeProvider,
            pricing);

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = now.DateTime.AddHours(3),
            ReturnAt = now.DateTime.AddDays(1).AddHours(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var result = sut.CreateBooking(request);

        // Assert
        Assert.Equal(BookingStatus.Reserved, result.Status);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(vehicle.Id, result.VehicleId);

        // Fake: the booking was stored.
        Assert.Single(bookings.Bookings);
        Assert.Equal(result.Reference, bookings.Bookings[0].Reference);

        // Mock: the deposit was charged with the expected amount.
        var charge = Assert.Single(payments.Charges);
        Assert.Equal(customer.Id, charge.CustomerId);
        Assert.Equal(1000m, charge.Amount);
        Assert.Equal($"Deposit {result.Reference}", charge.Description);

        // Spy: the customer received an SMS.
        var message = Assert.Single(sms.Messages);
        Assert.Equal(customer.Phone, message.Phone);
        Assert.Equal(
            $"Booking {result.Reference} confirmed",
            message.Text);
    }
}
