using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using Xunit;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceTests
{
    [Fact]
    public void CreateBooking_WithValidRequest_ReturnsReservedBooking()
    {
        // Arrange
        var service = new BookingService();

        var request = new BookingRequest
        {
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = DateTime.Now.AddDays(10),
            ReturnAt = DateTime.Now.AddDays(13),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        // Act
        var booking = service.CreateBooking(request);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(BookingStatus.Reserved, booking.Status);
        Assert.Equal(1, booking.CustomerId);
        Assert.Equal(1, booking.VehicleId);
    }

    [Fact]
    public void CreateBooking_WithPlatinumCustomer_ReturnsZeroDeposit()
    {
        // Arrange
        var service = new BookingService();

        var request = new BookingRequest
        {
            CustomerId = 5,
            VehicleId = 1,
            PickupAt = DateTime.Now.AddDays(10),
            ReturnAt = DateTime.Now.AddDays(13),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        // Act
        var booking = service.CreateBooking(request);

        // Assert
        Assert.Equal(0m, booking.Deposit);
        Assert.Equal(BookingStatus.Reserved, booking.Status);
    }

    [Fact]
    public void CreateBooking_WithOverlappingDates_ThrowsBookingRejectedException()
    {
        // Arrange
        var service = new BookingService();

        var request = new BookingRequest
        {
            CustomerId = 1,
            VehicleId = 5,
            PickupAt = DateTime.Today.AddDays(4).AddHours(10),
            ReturnAt = DateTime.Today.AddDays(5).AddHours(10),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        // Act & Assert
        Assert.Throws<BookingRejectedException>(
            () => service.CreateBooking(request));
    }
}