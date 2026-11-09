
using AutoFixture.Xunit2;
using AutoFixture;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Abstractions.Logging;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Services;
using Moq;
using DriveNow.UnitTests.Attributes;
using Xunit;

namespace DriveNow.UnitTests.Services;

public class BookingServiceAutoFixtureTests
{
    [Theory]
    [AutoMoqData]
    public void Should_NotChargeDeposit_When_CustomerIsPlatinum(
        [Frozen] Mock<ICustomerRepository> customers,
        [Frozen] Mock<IVehicleRepository> vehicles,
        [Frozen] Mock<IBookingRepository> bookings,
        [Frozen] Mock<IPaymentGateway> payments,
        [Frozen] Mock<ISmsSender> sms,
        [Frozen] Mock<IEmailSender> email,
        [Frozen] Mock<IAuditLogger> auditLogger,
        [Frozen] Mock<IReferenceGenerator> referenceGenerator,
        [Frozen] TimeProvider timeProvider,
        Customer customer,
        Vehicle vehicle,
        BookingService sut)
    {
        // Arrange
        customer.Id = 1;
        customer.DateOfBirth = timeProvider.GetLocalNow().DateTime.AddYears(-30);
        customer.LicenseIssueDate = timeProvider.GetLocalNow().DateTime.AddYears(-5);
        customer.LoyaltyTier = LoyaltyTier.Platinum;
        customer.IsBlacklisted = false;

        vehicle.Id = 10;
        vehicle.Category = VehicleCategory.Economy;
        vehicle.IsAvailable = true;

        customers
            .Setup(x => x.GetById(customer.Id))
            .Returns(customer);

        vehicles
            .Setup(x => x.GetById(vehicle.Id))
            .Returns(vehicle);

        bookings
            .Setup(x => x.GetOpenBookingsForVehicle(vehicle.Id))
            .Returns([]);

        bookings
            .Setup(x => x.CountOpenBookingsForCustomer(customer.Id))
            .Returns(0);

        referenceGenerator
            .Setup(x => x.Generate(It.IsAny<int>()))
            .Returns("ABCD");

        var now = timeProvider.GetLocalNow().DateTime;

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = now.AddDays(1),
            ReturnAt = now.AddDays(2),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var result = sut.CreateBooking(request);

        // Assert
        Assert.Equal(0, result.Deposit);

        payments.Verify(
            x => x.Charge(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()),
            Times.Never);

        bookings.Verify(
            x => x.Add(It.IsAny<Booking>()),
            Times.Once);
    }
}
