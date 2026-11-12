
using DriveNow.Domain.Enums;
using DriveNow.UnitTests.Fakers;
using FluentAssertions;

namespace DriveNow.UnitTests.Services;

public class VehicleFakerTests
{
    [Fact]
    public void Should_GenerateVehicleWithRequestedValues_When_CustomizationsAreProvided()
    {
        var vehicle = new VehicleFaker()
            .WithCategory(VehicleCategory.Luxury)
            .WithDailyRate(2500m)
            .Generate();

        vehicle.Category.Should().Be(VehicleCategory.Luxury);
        vehicle.DailyRate.Should().Be(2500m);
        vehicle.Model.Should().NotBeNullOrWhiteSpace();
        vehicle.PlateNumber.Should().NotBeNullOrWhiteSpace();
    }
}
