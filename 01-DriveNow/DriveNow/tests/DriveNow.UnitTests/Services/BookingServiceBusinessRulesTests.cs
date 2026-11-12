
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.UnitTests.TestDoubles;
using static DriveNow.UnitTests.TestDoubles.SpyPromoCodeRepository;

namespace DriveNow.UnitTests.Services;

public class BookingServiceBusinessRulesTests
{
    [Fact]
    public void Should_AcceptLuxuryVehicle_When_CustomerIsExactly25YearsOld()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Luxury,
            new DateTime(2001, 11, 9));

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(10, booking.VehicleId);
        Assert.Equal(BookingStatus.Reserved, booking.Status);
    }

    [Fact]
    public void Should_AcceptBackToBackBooking_When_PickupEqualsExistingReturn()
    {
        // Arrange
        var now = new DateTime(2026, 11, 9, 9, 0, 0);
        var pickup = now.AddHours(3);

        var existingBooking = new Booking
        {
            Id = 50,
            CustomerId = 2,
            VehicleId = 10,
            PickupAt = pickup.AddDays(-1),
            ReturnAt = pickup,
            Status = BookingStatus.Reserved
        };

        var (sut, bookings, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            existingBooking);

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(2, bookings.Bookings.Count);
        Assert.NotNull(booking);
    }
    [Fact]
    public void Should_RejectBooking_When_LicenseIsLessThanOneYear()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            licenseIssueDate: new DateTime(2026, 1, 1));

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal(
            "License must be held for at least 1 year",
            exception.Message);
    }

    [Fact]
    public void Should_ThrowCustomerNotFoundException_When_CustomerDoesNotExist()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.CustomerId = 999;

        // Act
        var exception = Assert.Throws<CustomerNotFoundException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Customer #999 not found", exception.Message);
    }
    [Fact]
    public void Should_RejectBooking_When_CustomerIsBlacklisted()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            isBlacklisted: true);

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Customer is blacklisted", exception.Message);
    }
    [Fact]
    public void Should_ThrowVehicleNotFoundException_When_VehicleDoesNotExist()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.VehicleId = 999;

        // Act
        var exception = Assert.Throws<VehicleNotFoundException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Vehicle #999 not found", exception.Message);
    }
    [Fact]
    public void Should_RejectBooking_When_VehicleIsUnavailable()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            vehicleAvailable: false);

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Vehicle is not available", exception.Message);
    }

    [Fact]
    public void Should_RejectVan_When_DriverHasLessThanTwoYearsOfLicense()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Van,
            new DateTime(1990, 1, 1),
            licenseIssueDate: new DateTime(2025, 11, 9));

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal(
            "Vans require 2 years of license",
            exception.Message);
    }
    [Fact]
    public void Should_RejectBooking_When_PickupIsLessThanTwoHoursAway()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.PickupAt = new DateTime(2026, 11, 9, 10, 59, 0);
        request.ReturnAt = request.PickupAt.AddDays(1);

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal(
            "Pickup must be at least 2 hours from now",
            exception.Message);
    }
    [Fact]
    public void Should_AcceptBooking_When_PickupIsExactlyTwoHoursAway()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.PickupAt = new DateTime(2026, 11, 9, 11, 0, 0);
        request.ReturnAt = request.PickupAt.AddDays(1);

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.NotNull(booking);
    }

    [Fact]
    public void Should_RejectBooking_When_ReturnEqualsPickup()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.ReturnAt = request.PickupAt;

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Rental period is invalid", exception.Message);
    }

    [Fact]
    public void Should_RejectBooking_When_RentalPeriodExceeds30Days()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.ReturnAt = request.PickupAt.AddDays(31);

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Rental period is invalid", exception.Message);
    }

    [Fact]
    public void Should_AcceptBooking_When_RentalPeriodIsExactly30Days()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.ReturnAt = request.PickupAt.AddDays(30);

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(30, booking.Price.RentalDays);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void Should_RejectBooking_When_AdditionalDriversIsOutOfRange(
    int additionalDrivers)
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.AdditionalDrivers = additionalDrivers;

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal(
            "Up to 3 additional drivers allowed",
            exception.Message);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Should_AcceptBooking_When_AdditionalDriversIsWithinRange(
    int additionalDrivers)
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        request.AdditionalDrivers = additionalDrivers;

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(additionalDrivers, booking.AdditionalDrivers);
    }

    [Fact]
    public void Should_RejectBooking_When_PickupOverlapsExistingBooking()
    {
        // Arrange
        var now = new DateTime(2026, 11, 9, 9, 0, 0);
        var pickup = now.AddHours(3);

        var existingBooking = new Booking
        {
            Id = 50,
            CustomerId = 2,
            VehicleId = 10,
            PickupAt = pickup.AddDays(-1),
            ReturnAt = pickup.AddDays(1),
            Status = BookingStatus.Reserved
        };

        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            existingBooking);

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal(
            "Vehicle is already booked for these dates",
            exception.Message);
    }
    [Fact]
    public void Should_RejectBooking_When_CustomerAlreadyHasTwoOpenBookings()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            openBookingsCount: 2);

        // Act
        var exception = Assert.Throws<BookingRejectedException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal(
            "Customer already has 2 active bookings",
            exception.Message);
    }

    [Fact]
    public void Should_UsePricingServiceResult_When_BookingIsCreated()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.NotNull(booking.Price);
        Assert.Equal(1, booking.Price.RentalDays);
        Assert.Equal(100m, booking.Price.BaseAmount);
        Assert.Equal(100m, booking.Price.Total);
    }

    [Fact]
    public void Should_SetBookingDefaultsAndReference_When_BookingIsCreated()
    {
        // Arrange
        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1));

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(BookingStatus.Reserved, booking.Status);

        Assert.Equal(
            new DateTime(2026, 11, 9, 9, 0, 0),
            booking.CreatedAt);

        Assert.Equal(5000, booking.StartMileage);

        Assert.Equal("DN-20261109-AAAA", booking.Reference);

        Assert.Matches(
            @"^DN-\d{8}-[A-Za-z0-9]{4}$",
            booking.Reference);
    }
    [Theory]
    [InlineData(VehicleCategory.Economy, 1000)]
    [InlineData(VehicleCategory.Compact, 1000)]
    [InlineData(VehicleCategory.SUV, 2500)]
    [InlineData(VehicleCategory.Van, 2500)]
    [InlineData(VehicleCategory.Luxury, 5000)]
    public void Should_CalculateExpectedDeposit_When_VehicleCategoryChanges(
    VehicleCategory category,
    decimal expectedDeposit)
    {
        // Arrange
        var (sut, _, request, payments) = CreateScenario(
            category,
            new DateTime(1990, 1, 1));

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(expectedDeposit, booking.Deposit);
        Assert.Single(payments.Charges);
        Assert.Equal(expectedDeposit, payments.Charges[0].Amount);
    }

    [Theory]
    [InlineData(VehicleCategory.Economy, 500)]
    [InlineData(VehicleCategory.SUV, 1250)]
    [InlineData(VehicleCategory.Luxury, 2500)]
    public void Should_HalveDeposit_When_PremiumInsuranceIsSelected(
    VehicleCategory category,
    decimal expectedDeposit)
    {
        // Arrange
        var (sut, _, request, payments) = CreateScenario(
            category,
            new DateTime(1990, 1, 1));

        request.InsurancePlan = InsurancePlan.Premium;

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(expectedDeposit, booking.Deposit);
        Assert.Single(payments.Charges);
        Assert.Equal(expectedDeposit, payments.Charges[0].Amount);
    }

    [Fact]
    public void Should_NotChargeDeposit_When_CustomerIsPlatinum()
    {
        // Arrange
        var (sut, _, request, payments) = CreateScenario(
            VehicleCategory.Luxury,
            new DateTime(1990, 1, 1),
            loyaltyTier: LoyaltyTier.Platinum);

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(0m, booking.Deposit);
        Assert.Empty(payments.Charges);
    }

    [Fact]
    public void Should_NotSaveBooking_When_DepositPaymentFails()
    {
        // Arrange
        var (sut, bookings, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            paymentFails: true);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => sut.CreateBooking(request));

        // Assert
        Assert.Equal("Payment failed", exception.Message);
        Assert.Empty(bookings.Bookings);
    }

    [Fact]
    public void Should_IncrementPromoUsage_When_BookingUsesValidPromoCode()
    {
        // Arrange
        var now = new DateTime(2026, 11, 9, 9, 0, 0);

        var promo = new PromoCode
        {
            Code = "SAVE15",
            DiscountPercent = 15,
            ValidFrom = now.Date,
            ValidUntil = now.Date.AddDays(30),
            MinDays = 1,
            AllowedCategories = null,
            MaxUses = 100,
            UsedCount = 5
        };

        var promos = new SpyPromoCodeRepository(promo);

        var (sut, _, request, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            promoRepository: promos);

        request.PromoCode = "SAVE15";

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        Assert.Equal(6, promo.UsedCount);
        Assert.Equal(1, promos.UpdateCalls);
        Assert.Same(promo, promos.UpdatedPromo);
        Assert.Equal("SAVE15", booking.PromoCode);
    }

    [Fact]
    public void Should_SendNotificationsAndAuditLog_When_BookingIsCreated()
    {
        // Arrange
        var now = new DateTime(2026, 11, 9, 9, 0, 0);

        var customer = new Customer
        {
            Id = 1,
            FullName = "Test Customer",
            Email = "test@example.com",
            Phone = "01000000000",
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2010, 1, 1)
        };

        var vehicle = new Vehicle
        {
            Id = 10,
            Category = VehicleCategory.Economy,
            DailyRate = 100,
            Mileage = 5000,
            IsAvailable = true
        };

        var customers = new StubCustomerRepository(customer);
        var vehicles = new StubVehicleRepository(vehicle);
        var bookings = new FakeBookingRepository();
        var promos = new StubPromoCodeRepository();

        var sms = new SpySmsSender();
        var email = new SpyEmailSender();
        var audit = new SpyAuditLogger();

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 11, 9, 9, 0, 0, TimeSpan.Zero));

        var sut = new BookingService(
            customers,
            vehicles,
            bookings,
            promos,
            new MockPaymentGateway(),
            sms,
            email,
            audit,
            new StubReferenceGenerator(),
            timeProvider,
            new PricingService(promos, timeProvider));

        var request = new BookingRequest
        {
            CustomerId = 1,
            VehicleId = 10,
            PickupAt = now.AddHours(3),
            ReturnAt = now.AddDays(1).AddHours(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var booking = sut.CreateBooking(request);

        // Assert
        var confirmation = $"Booking {booking.Reference} confirmed";

        Assert.Single(bookings.Bookings);
        Assert.Same(booking, bookings.Bookings[0]);

        Assert.Single(sms.Messages);
        Assert.Equal("01000000000", sms.Messages[0].Phone);
        Assert.Equal(confirmation, sms.Messages[0].Text);

        Assert.Single(email.Messages);
        Assert.Equal("test@example.com", email.Messages[0].To);
        Assert.Equal(confirmation, email.Messages[0].Subject);
        Assert.Equal(confirmation, email.Messages[0].Body);

        Assert.Contains($"Booking {booking.Reference} created", audit.Messages);
    }


    [Theory]
    [InlineData(10, 59, false)]
    [InlineData(11, 0, true)]
    [InlineData(11, 1, true)]
    public void Should_ValidateRentalStart_When_CurrentTimeChanges(
    int hour,
    int minute,
    bool shouldStart)
    {
        // Arrange
        var currentTime = new DateTimeOffset(
            2026, 11, 9, hour, minute, 0, TimeSpan.Zero);

        var (sut, bookings, _, _) = CreateScenario(
            VehicleCategory.Economy,
            new DateTime(1990, 1, 1),
            currentTime: currentTime);

        var booking = new Booking
        {
            Id = 50,
            CustomerId = 1,
            VehicleId = 10,
            PickupAt = new DateTime(2026, 11, 9, 12, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 12, 0, 0),
            Status = BookingStatus.Reserved,
            Reference = "DN-TEST-0001"
        };

        bookings.Bookings.Add(booking);

        // Act
        if (shouldStart)
        {
            sut.StartRental(booking.Id);
        }
        else
        {
            var exception = Assert.Throws<InvalidBookingStateException>(
                () => sut.StartRental(booking.Id));

            Assert.Equal("Too early to start this rental", exception.Message);
        }

        // Assert
        Assert.Equal(
            shouldStart ? BookingStatus.Active : BookingStatus.Reserved,
            booking.Status);

        Assert.Same(booking, bookings.GetById(booking.Id));
    }


    private static (
        BookingService Sut,
        FakeBookingRepository Bookings,
        BookingRequest Request,
        MockPaymentGateway Payments)
        CreateScenario(
    VehicleCategory category,
    DateTime dateOfBirth,
    Booking? existingBooking = null,
    DateTime? licenseIssueDate = null,
    bool isBlacklisted = false,
    bool vehicleAvailable = true,
    int openBookingsCount = 0,
    LoyaltyTier loyaltyTier = LoyaltyTier.None,
    bool paymentFails = false,
    IPromoCodeRepository? promoRepository = null,
    DateTimeOffset? currentTime = null)
    {
        var now = new DateTimeOffset(
            2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

        var timeProvider = new FixedTimeProvider(currentTime ?? now);
        
        var customer = new Customer
        {
            Id = 1,
            FullName = "Test Customer",
            Email = "test@example.com",
            Phone = "01000000000",
            DateOfBirth = dateOfBirth,
            LicenseIssueDate = licenseIssueDate ?? new DateTime(2010, 1, 1),
            LoyaltyTier = loyaltyTier,
            IsBlacklisted = isBlacklisted,
        };

        var vehicle = new Vehicle
        {
            Id = 10,
            PlateNumber = "TEST-123",
            Model = "Test Car",
            Category = category,
            DailyRate = 100,
            Mileage = 5000,
            IsAvailable = vehicleAvailable,
        };

        var customers = new StubCustomerRepository(customer);
        var vehicles = new StubVehicleRepository(vehicle);
        var bookings = new FakeBookingRepository();
        var promos = promoRepository ?? new StubPromoCodeRepository();
        for (var i = 0; i < openBookingsCount; i++)
        {
            bookings.Bookings.Add(new Booking
            {
                Id = 100 + i,
                CustomerId = customer.Id,
                VehicleId = 100 + i,
                PickupAt = now.DateTime.AddDays(-2),
                ReturnAt = now.DateTime.AddDays(2),
                Status = BookingStatus.Reserved
            });
        }
        if (existingBooking is not null)
            bookings.Bookings.Add(existingBooking);
        var payments = new MockPaymentGateway();

        IPaymentGateway paymentGateway = paymentFails
    ? new FailingPaymentGateway()
    : payments;

        var sut = new BookingService(
            customers,
            vehicles,
            bookings,
            promos,
            paymentGateway,
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

        return (sut, bookings, request, payments);
    }
}
