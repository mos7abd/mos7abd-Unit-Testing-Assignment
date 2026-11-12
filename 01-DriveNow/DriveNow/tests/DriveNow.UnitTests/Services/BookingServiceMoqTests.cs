
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace DriveNow.UnitTests.Services;

public class BookingServiceMoqTests
{
    [Fact]
    public void Should_ReturnCustomer_When_CustomerExists()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            FullName = "Test Customer",
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2010, 1, 1),
            LoyaltyTier = LoyaltyTier.Gold
        };

        var customerMock = new Mock<ICustomerRepository>();

        customerMock
            .Setup(repository => repository.GetById(1))
            .Returns(customer);

        // Act
        var result = customerMock.Object.GetById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Customer", result.FullName);

        customerMock.Verify(
            repository => repository.GetById(1),
            Times.Once);
    }


    [Fact]
    public void Should_SendConfirmationSms_When_BookingIsCreated()
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
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            Id = 10,
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

        var paymentMock = new Mock<IPaymentGateway>();
        paymentMock
            .Setup(x => x.Charge(
                customer.Id,
                1000m,
                It.IsAny<string>()))
            .Returns("PAY-TEST");

        var smsMock = new Mock<ISmsSender>();

        var pricing = new PricingService(promos, timeProvider);

        var sut = new BookingService(
            customers,
            vehicles,
            bookings,
            promos,
            paymentMock.Object,
            smsMock.Object,
            new DummyEmailSender(),
            new StubAuditLogger(),
            new StubReferenceGenerator(),
            timeProvider,
            pricing);

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = now.UtcDateTime.AddHours(3),
            ReturnAt = now.UtcDateTime.AddDays(1).AddHours(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var result = sut.CreateBooking(request);

        // Assert
        smsMock.Verify(
            x => x.Send(
                customer.Phone,
                $"Booking {result.Reference} confirmed"),
            Times.Once);
    }

    [Fact]
    public void Should_NotChargeDeposit_When_CustomerIsPlatinum()
    {
        // Arrange
        var now = new DateTimeOffset(
            2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

        var timeProvider = new FixedTimeProvider(now);

        var customer = new Customer
        {
            Id = 1,
            FullName = "Platinum Customer",
            Email = "platinum@example.com",
            Phone = "01000000000",
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2010, 1, 1),
            LoyaltyTier = LoyaltyTier.Platinum
        };

        var vehicle = new Vehicle
        {
            Id = 10,
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

        var paymentMock = new Mock<IPaymentGateway>();

        var pricing = new PricingService(promos, timeProvider);

        var sut = new BookingService(
            customers,
            vehicles,
            bookings,
            promos,
            paymentMock.Object,
            new SpySmsSender(),
            new DummyEmailSender(),
            new StubAuditLogger(),
            new StubReferenceGenerator(),
            timeProvider,
            pricing);

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = now.UtcDateTime.AddHours(3),
            ReturnAt = now.UtcDateTime.AddDays(1).AddHours(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var result = sut.CreateBooking(request);

        // Assert
        Assert.Equal(0m, result.Deposit);

        paymentMock.Verify(
            x => x.Charge(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()),
            Times.Never);
    }

}
