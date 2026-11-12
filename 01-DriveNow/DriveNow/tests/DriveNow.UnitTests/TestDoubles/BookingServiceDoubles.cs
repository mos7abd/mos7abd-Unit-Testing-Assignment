
using DriveNow.Application.Abstractions.External;
using DriveNow.Application.Abstractions.Logging;
using DriveNow.Application.Abstractions.Repositories;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.TestDoubles;

// Dummy: exists only to satisfy the dependency.
// Email behavior is irrelevant in tests that use this class.
public class DummyEmailSender : IEmailSender
{
    public void Send(string to, string subject, string body)
    {
    }
}

// Stub: returns predefined data configured by the test.
public class StubCustomerRepository : ICustomerRepository
{
    private readonly Customer? _customer;

    public StubCustomerRepository(Customer? customer)
    {
        _customer = customer;
    }

    public Customer? GetById(int id) =>
        _customer?.Id == id ? _customer : null;

    public List<Customer> GetAll() =>
        _customer is null ? [] : [_customer];

    public void Update(Customer customer)
    {
    }
}

// Spy: records calls so the test can inspect what happened.
public class SpySmsSender : ISmsSender
{
    public List<(string Phone, string Text)> Messages { get; } = [];

    public void Send(string phone, string text)
    {
        Messages.Add((phone, text));
    }
}

// Hand-written Mock: records calls and allows verification.
public class MockPaymentGateway : IPaymentGateway
{
    public List<(int CustomerId, decimal Amount, string Description)> Charges { get; } = [];

    public List<(int CustomerId, decimal Amount, string Description)> Refunds { get; } = [];

    public string Charge(int customerId, decimal amount, string description)
    {
        Charges.Add((customerId, amount, description));
        return "PAY-TEST";
    }

    public void Refund(int customerId, decimal amount, string description)
    {
        Refunds.Add((customerId, amount, description));
    }
}

// Fake: a working in-memory implementation of the booking repository.
public class FakeBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = [];

    private int _nextId = 1;

    public List<Booking> Bookings => _bookings;

    public Booking? GetById(int id) =>
        _bookings.FirstOrDefault(booking => booking.Id == id);

    public List<Booking> GetAll() =>
        _bookings.ToList();

    public void Add(Booking booking)
    {
        booking.Id = _nextId++;
        _bookings.Add(booking);
    }

    public void Update(Booking booking)
    {
        var index = _bookings.FindIndex(item => item.Id == booking.Id);

        if (index >= 0)
            _bookings[index] = booking;
    }

    public List<Booking> GetOpenBookingsForVehicle(int vehicleId) =>
        _bookings
            .Where(booking =>
                booking.VehicleId == vehicleId &&
                booking.Status is BookingStatus.Reserved or BookingStatus.Active)
            .ToList();

    public int CountOpenBookingsForCustomer(int customerId) =>
        _bookings.Count(booking =>
            booking.CustomerId == customerId &&
            booking.Status is BookingStatus.Reserved or BookingStatus.Active);
}

// Supporting stubs for the BookingService test setup.
public class StubVehicleRepository : IVehicleRepository
{
    private readonly Vehicle? _vehicle;

    public StubVehicleRepository(Vehicle? vehicle)
    {
        _vehicle = vehicle;
    }

    public Vehicle? GetById(int id) =>
        _vehicle?.Id == id ? _vehicle : null;

    public List<Vehicle> GetAll() =>
        _vehicle is null ? [] : [_vehicle];

    public void Update(Vehicle vehicle)
    {
    }
}

public class StubPromoCodeRepository : IPromoCodeRepository
{
    public PromoCode? GetByCode(string code) => null;

    public void Update(PromoCode promo)
    {
    }
}
public class SpyPromoCodeRepository : IPromoCodeRepository
{
    private readonly PromoCode _promo;

    public int UpdateCalls { get; private set; }

    public PromoCode? UpdatedPromo { get; private set; }

    public SpyPromoCodeRepository(PromoCode promo)
    {
        _promo = promo;
    }
    public class SpyEmailSender : IEmailSender
    {
        public List<(string To, string Subject, string Body)> Messages { get; } = [];

        public void Send(string to, string subject, string body)
        {
            Messages.Add((to, subject, body));
        }
    }

    public class SpyAuditLogger : IAuditLogger
    {
        public List<string> Messages { get; } = [];

        public void Write(string message)
        {
            Messages.Add(message);
        }
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
        UpdatedPromo = promo;
    }
}
public class StubAuditLogger : IAuditLogger
{
    public void Write(string message)
    {
    }
}

public class StubReferenceGenerator : IReferenceGenerator
{
    public string Generate(int length) =>
        new string('A', length);
}

public class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone =>
        TimeZoneInfo.Utc;
}
public class FailingPaymentGateway : IPaymentGateway
{
    public string Charge(int customerId, decimal amount, string description)
    {
        throw new InvalidOperationException("Payment failed");
    }

    public void Refund(int customerId, decimal amount, string description)
    {
    }
}