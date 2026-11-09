
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Abstractions.Logging;
using DriveNow.Application.Models;
using DriveNow.Application.Receipts;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Services;
using DriveNow.Domain.ValueObjects;
using DriveNow.Infrastructure.Abstractions;
using DriveNow.Infrastructure.External;
using DriveNow.Infrastructure.Logging;
using DriveNow.Infrastructure.Persistence;
using DriveNow.Infrastructure.Repositories;
using System.Globalization;

namespace DriveNow.ConsoleApp;

internal static class Program
{
    // =========================================================
    // Shared Dependencies
    // =========================================================

    private static readonly TimeProvider Clock = TimeProvider.System;

    private static readonly InMemoryDatabase Database = new(Clock);

    // =========================================================
    // Repositories
    // =========================================================

    private static readonly CustomerRepository Customers = new(Database);
    private static readonly VehicleRepository Vehicles = new(Database);
    private static readonly BookingRepository Bookings = new(Database);
    private static readonly PromoCodeRepository Promos = new(Database);

    // =========================================================
    // External Services
    // =========================================================

    private static readonly IReferenceGenerator ReferenceGenerator =
        new RandomReferenceGenerator();

    private static readonly IExecutionDelay ExecutionDelay =
        new ExecutionDelay();

    private static readonly IPaymentFailurePolicy FailurePolicy =
        new PaymentFailurePolicy();

    private static readonly IPaymentGateway Payments =
        new PaymentGateway(
            ReferenceGenerator,
            FailurePolicy,
            ExecutionDelay);

    private static readonly ISmsSender Sms =
        new SmsSender(ExecutionDelay);

    private static readonly IEmailOutbox EmailOutbox =
        new FileEmailOutbox(AppContext.BaseDirectory);

    private static readonly IEmailSender Email =
        new EmailSender(EmailOutbox, Clock);

    // =========================================================
    // Logging
    // =========================================================

    private static readonly IAuditLogWriter AuditWriter =
        new FileAuditLogWriter(AppContext.BaseDirectory);

    private static readonly IAuditLogger Audit =
        new AuditLog(AuditWriter, Clock);

    // =========================================================
    // Application Services
    // =========================================================

    private static readonly PricingService Pricing =
        new(Promos, Clock);

    private static readonly ReceiptService ReceiptService =
        new(
            new LineItemFactory(),
            new TaxCalculator(),
            new CurrencyFormatter(),
            new ReceiptFormatter());

    private static readonly BookingService BookingService =
        new(
            Customers,
            Vehicles,
            Bookings,
            Promos,
            Payments,
            Sms,
            Email,
            Audit,
            ReferenceGenerator,
            Clock,
            Pricing);

    private static readonly ReturnService ReturnService =
        new(
            Bookings,
            Vehicles,
            Customers,
            Payments,
            Email,
            Audit,
            Clock,
            ReceiptService);

    private static readonly CancellationService CancellationService =
        new(
            Bookings,
            Vehicles,
            Customers,
            Promos,
            Payments,
            Sms,
            Email,
            Audit,
            Clock);

    // =========================================================
    // Main
    // =========================================================

    private static void Main()
    {
        PriceSeedBookings();

        while (true)
        {
            Console.Clear();

            Console.WriteLine("==================================");
            Console.WriteLine("          DriveNow System");
            Console.WriteLine("==================================");
            Console.WriteLine("1. List Customers");
            Console.WriteLine("2. List Vehicles");
            Console.WriteLine("3. List Bookings");
            Console.WriteLine("4. Create Booking");
            Console.WriteLine("5. Return Vehicle");
            Console.WriteLine("6. Cancel Booking");
            Console.WriteLine("0. Exit");
            Console.WriteLine("==================================");

            Console.Write("Select an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ListCustomers();
                    break;

                case "2":
                    ListVehicles();
                    break;

                case "3":
                    ListBookings();
                    break;

                case "4":
                    CreateBooking();
                    break;

                case "5":
                    ReturnVehicle();
                    break;

                case "6":
                    CancelBooking();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    Pause();
                    break;
            }
        }
    }

    // =========================================================
    // Seed Data
    // =========================================================

    private static void PriceSeedBookings()
    {
        foreach (var booking in Bookings.GetAll())
        {
            if (booking.Status != BookingStatus.Reserved)
                continue;

            var customer = Customers.GetById(booking.CustomerId);
            var vehicle = Vehicles.GetById(booking.VehicleId);

            if (customer is null || vehicle is null)
                continue;

            var request = new BookingRequest
            {
                CustomerId = booking.CustomerId,
                VehicleId = booking.VehicleId,
                PickupAt = booking.PickupAt,
                ReturnAt = booking.ReturnAt,
                InsurancePlan = booking.InsurancePlan,
                AdditionalDrivers = booking.AdditionalDrivers,
                PromoCode = booking.PromoCode
            };

            booking.Price = Pricing.Calculate(customer, vehicle, request);

            Bookings.Update(booking);
        }
    }

    // =========================================================
    // Customers
    // =========================================================

    private static void ListCustomers()
    {
        Console.Clear();

        Console.WriteLine("========== Customers ==========");

        var today = Clock.GetLocalNow().Date;

        foreach (var customer in Customers.GetAll())
        {
            var age = RentalMath.CalculateAge(
                customer.DateOfBirth,
                today);

            Console.WriteLine(
                $"ID: {customer.Id} | " +
                $"Name: {customer.FullName} | " +
                $"Age: {age} | " +
                $"Phone: {customer.Phone}");
        }

        Pause();
    }

    // =========================================================
    // Vehicles
    // =========================================================

    private static void ListVehicles()
    {
        Console.Clear();

        Console.WriteLine("========== Vehicles ==========");

        foreach (var vehicle in Vehicles.GetAll())
        {
            Console.WriteLine(
                $"ID: {vehicle.Id} | " +
                $"Plate: {vehicle.PlateNumber} | " +
                $"Model: {vehicle.Model} | " +
                $"Category: {vehicle.Category} | " +
                $"Daily Rate: {vehicle.DailyRate:N2} | " +
                $"Mileage: {vehicle.Mileage} | " +
                $"Available: {(vehicle.IsAvailable ? "Yes" : "No")}");
        }

        Pause();
    }

    // =========================================================
    // Bookings
    // =========================================================

    private static void ListBookings()
    {
        Console.Clear();

        Console.WriteLine("========== Bookings ==========");

        foreach (var booking in Bookings.GetAll())
        {
            Console.WriteLine(
                $"ID: {booking.Id} | " +
                $"Reference: {booking.Reference} | " +
                $"Customer ID: {booking.CustomerId} | " +
                $"Vehicle ID: {booking.VehicleId} | " +
                $"Pickup: {booking.PickupAt:g} | " +
                $"Return: {booking.ReturnAt:g} | " +
                $"Total: {booking.Price.Total:N2} | " +
                $"Status: {booking.Status}");
        }

        Pause();
    }

    // =========================================================
    // Create Booking
    // =========================================================

    private static void CreateBooking()
    {
        Console.Clear();

        Console.WriteLine("========== Create Booking ==========");

        Console.Write("Customer ID: ");

        if (!int.TryParse(Console.ReadLine(), out var customerId))
        {
            Console.WriteLine("Invalid customer ID.");
            Pause();
            return;
        }

        Console.Write("Vehicle ID: ");

        if (!int.TryParse(Console.ReadLine(), out var vehicleId))
        {
            Console.WriteLine("Invalid vehicle ID.");
            Pause();
            return;
        }

        Console.Write("Pickup date (yyyy-MM-dd): ");

        if (!DateTime.TryParseExact(
                Console.ReadLine(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var pickupAt))
        {
            Console.WriteLine("Invalid pickup date.");
            Pause();
            return;
        }

        Console.Write("Return date (yyyy-MM-dd): ");

        if (!DateTime.TryParseExact(
                Console.ReadLine(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var returnAt))
        {
            Console.WriteLine("Invalid return date.");
            Pause();
            return;
        }

        Console.Write("Promo code (optional): ");
        var promoCode = Console.ReadLine();

        try
        {
            var request = new BookingRequest
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                PickupAt = pickupAt,
                ReturnAt = returnAt,
                PromoCode = promoCode,
                InsurancePlan = InsurancePlan.Basic,
                AdditionalDrivers = 0
            };

            var booking = BookingService.CreateBooking(request);

            Console.WriteLine();
            Console.WriteLine("Booking created successfully.");
            Console.WriteLine($"Booking ID: {booking.Id}");
            Console.WriteLine($"Reference: {booking.Reference}");
            Console.WriteLine($"Total Price: {booking.Price.Total:N2}");
            Console.WriteLine($"Status: {booking.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Booking failed: {ex.Message}");
        }

        Pause();
    }

    // =========================================================
    // Return Vehicle
    // =========================================================

    private static void ReturnVehicle()
    {
        Console.Clear();

        Console.WriteLine("========== Return Vehicle ==========");

        Console.Write("Booking ID: ");

        if (!int.TryParse(Console.ReadLine(), out var bookingId))
        {
            Console.WriteLine("Invalid booking ID.");
            Pause();
            return;
        }

        Console.Write("End Mileage: ");

        if (!int.TryParse(Console.ReadLine(), out var endMileage))
        {
            Console.WriteLine("Invalid mileage.");
            Pause();
            return;
        }

        Console.Write("Fuel Quarters (0-4): ");

        if (!int.TryParse(Console.ReadLine(), out var fuelQuarters))
        {
            Console.WriteLine("Invalid fuel quarters.");
            Pause();
            return;
        }

        Console.Write("Damage Cost: ");

        if (!decimal.TryParse(
                Console.ReadLine(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var damageCost))
        {
            Console.WriteLine("Invalid damage cost.");
            Pause();
            return;
        }

        try
        {
            var report = ReturnService.ProcessReturn(
                bookingId,
                endMileage,
                fuelQuarters,
                damageCost);

            Console.WriteLine();
            Console.WriteLine("Vehicle returned successfully.");
            Console.WriteLine($"Booking ID: {bookingId}");
            Console.WriteLine($"Extra Charges: {report.TotalExtra:N2}");

            var booking = Bookings.GetById(bookingId);

            if (booking is not null)
            {
                var receipt = ReceiptService.Build(booking, report);

                Console.WriteLine();
                Console.WriteLine(receipt);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Return failed: {ex.Message}");
        }

        Pause();
    }

    // =========================================================
    // Cancel Booking
    // =========================================================

    private static void CancelBooking()
    {
        Console.Clear();

        Console.WriteLine("========== Cancel Booking ==========");

        Console.Write("Booking ID: ");

        if (!int.TryParse(Console.ReadLine(), out var bookingId))
        {
            Console.WriteLine("Invalid booking ID.");
            Pause();
            return;
        }

        try
        {
            var refundAmount = CancellationService.Cancel(bookingId);

            Console.WriteLine();
            Console.WriteLine("Booking cancelled successfully.");
            Console.WriteLine($"Refund Amount: {refundAmount:N2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cancellation failed: {ex.Message}");
        }

        Pause();
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
