
using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Fakers;

public sealed class VehicleFaker
{
    private const int Seed = 54321;

    private VehicleCategory? _category;
    private decimal? _dailyRate;

    private readonly Faker<Vehicle> _faker;

    public VehicleFaker()
    {
        _faker = new Faker<Vehicle>()
            .UseSeed(Seed)
            .RuleFor(v => v.PlateNumber, f => f.Random.Replace("???-####"))
            .RuleFor(v => v.Model, f => f.Vehicle.Model())
            .RuleFor(v => v.Category,
                f => _category ?? f.PickRandom<VehicleCategory>())
            .RuleFor(v => v.DailyRate,
                f => _dailyRate ?? f.Random.Decimal(300, 3000))
            .RuleFor(v => v.Mileage, f => f.Random.Int(0, 150000))
            .RuleFor(v => v.IsAvailable, _ => true);
    }

    public VehicleFaker WithCategory(VehicleCategory category)
    {
        _category = category;
        return this;
    }

    public VehicleFaker WithDailyRate(decimal dailyRate)
    {
        _dailyRate = dailyRate;
        return this;
    }

    public Vehicle Generate() => _faker.Generate();
}
