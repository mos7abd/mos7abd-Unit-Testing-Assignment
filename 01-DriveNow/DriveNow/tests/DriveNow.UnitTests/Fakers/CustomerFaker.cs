
using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Fakers;

public sealed class CustomerFaker
{
    private const int Seed = 12345;
    private static readonly DateTime ReferenceDate =
        new(2026, 11, 9);

    private int _age = 30;
    private LoyaltyTier _tier = LoyaltyTier.None;

    private readonly Faker<Customer> _faker;

    public CustomerFaker()
    {
        _faker = new Faker<Customer>()
            .UseSeed(Seed)
            .RuleFor(c => c.FullName, f => f.Name.FullName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.DateOfBirth,
                _ => ReferenceDate.AddYears(-_age))
            .RuleFor(c => c.LicenseIssueDate,
                _ => ReferenceDate.AddYears(-5))
            .RuleFor(c => c.LoyaltyTier, _ => _tier)
            .RuleFor(c => c.IsBlacklisted, _ => false)
            .RuleFor(c => c.CompletedRentals, f => f.Random.Int(0, 50));
    }

    public CustomerFaker WithAge(int age)
    {
        _age = age;
        return this;
    }

    public CustomerFaker WithTier(LoyaltyTier tier)
    {
        _tier = tier;
        return this;
    }

    public Customer Generate() => _faker.Generate();
}
