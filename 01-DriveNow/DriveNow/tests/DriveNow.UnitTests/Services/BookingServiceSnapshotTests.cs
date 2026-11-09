
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.UnitTests.TestDoubles;
using VerifyTests;

namespace DriveNow.UnitTests.Services;

public class BookingServiceSnapshotTests
{
    [Fact]
    public async Task Should_MatchCreatedBooking_Snapshot()
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

        var sut = new BookingService(
            customers,
            vehicles,
            bookings,
            promos,
            new MockPaymentGateway(),
            new SpySmsSender(),
            new DummyEmailSender(),
            new StubAuditLogger(),
            new StubReferenceGenerator(),
            timeProvider,
            new PricingService(promos, timeProvider));

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
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Single(bookings.Bookings);

        await Verifier.Verify(booking);
    }
}
