
using DriveNow.Domain.Services;
using FluentAssertions;

namespace DriveNow.UnitTests.Services;

public class RentalMathTests
{
    [Theory]
    [InlineData(2000, 11, 10, 2026, 11, 9, 25)]
    [InlineData(2000, 11, 10, 2026, 11, 10, 26)]
    [InlineData(2000, 11, 10, 2026, 11, 11, 26)]
    [InlineData(2000, 2, 29, 2025, 2, 27, 24)]
    [InlineData(2000, 2, 29, 2025, 2, 28, 25)]
    public void Should_CalculateAge_When_GivenBirthDate
(
        int birthYear,
        int birthMonth,
        int birthDay,
        int currentYear,
        int currentMonth,
        int currentDay,
        int expectedAge)
    {
        // Arrange
        var dateOfBirth = new DateTime(birthYear, birthMonth, birthDay);
        var onDate = new DateTime(currentYear, currentMonth, currentDay);

        // Act
        var result = RentalMath.CalculateAge(dateOfBirth, onDate);

        // Assert
        result.Should().Be(expectedAge);
    }

    [Theory]
    [Trait("Rule", "BR-02")]
    [InlineData(2000, 11, 10, 2026, 11, 9, 25)]
    [InlineData(2000, 11, 10, 2026, 11, 10, 26)]
    [InlineData(2000, 11, 10, 2026, 11, 11, 26)]
    [InlineData(2000, 2, 29, 2025, 2, 27, 24)]
    [InlineData(2000, 2, 29, 2025, 2, 28, 25)]
    public void Should_ReturnCorrectLicenseYears_When_CheckingDateBoundaries(
    int issueYear,
    int issueMonth,
    int issueDay,
    int currentYear,
    int currentMonth,
    int currentDay,
    int expectedYears)
    {
        // Arrange
        var issueDate = new DateTime(issueYear, issueMonth, issueDay);
        var onDate = new DateTime(currentYear, currentMonth, currentDay);

        // Act
        var result = RentalMath.CalculateLicenseYears(issueDate, onDate);

        // Assert
        result.Should().Be(expectedYears);
    }

    [Theory]
    [Trait("Rule", "BR-03")]
    [InlineData(3, 0, 1)]
    [InlineData(24, 59, 1)]
    [InlineData(25, 0, 1)]
    [InlineData(25, 1, 2)]
    [InlineData(72, 0, 3)]
    public void Should_ReturnCorrectRentalDays_When_CheckingDurationBoundaries(
    int hours,
    int minutes,
    int expectedDays)
    {
        // Arrange
        var pickup = new DateTime(2026, 11, 8, 10, 0, 0);
        var returnAt = pickup.AddHours(hours).AddMinutes(minutes);

        // Act
        var result = RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        result.Should().Be(expectedDays);
    }

    [Fact]
    [Trait("Rule", "BR-03")]
    public void Should_ThrowArgumentException_When_ReturnAtEqualsPickup()
    {
        // Arrange
        var pickup = new DateTime(2026, 11, 8, 10, 0, 0);

        // Act
        Action act = () => RentalMath.CalculateRentalDays(pickup, pickup);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Return must be after pickup");
    }

    [Fact]
    [Trait("Rule", "BR-03")]
    public void Should_ThrowArgumentException_When_ReturnAtIsBeforePickup()
    {
        // Arrange
        var pickup = new DateTime(2026, 11, 8, 10, 0, 0);
        var returnAt = pickup.AddHours(-1);

        // Act
        Action act = () => RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Return must be after pickup");
    }

    [Fact]
    [Trait("Rule", "BR-01")]
    public void Should_HandleFebruary29Birthday_When_YearIsLeap()
    {
        // Arrange
        var dateOfBirth = new DateTime(2000, 2, 29);
        var onDate = new DateTime(2024, 2, 29);

        // Act
        var result = RentalMath.CalculateAge(dateOfBirth, onDate);

        // Assert
        result.Should().Be(24);
    }

    [Fact]
    [Trait("Rule", "BR-02")]
    public void Should_HandleFebruary29Anniversary_When_YearIsLeap()
    {
        // Arrange
        var issueDate = new DateTime(2000, 2, 29);
        var onDate = new DateTime(2024, 2, 29);

        // Act
        var result = RentalMath.CalculateLicenseYears(issueDate, onDate);

        // Assert
        result.Should().Be(24);
    }

    [Fact]
    [Trait("Rule", "BR-03")]
    public void Should_ReturnAtLeastOneDay_When_DurationIsLessThan24Hours()
    {
        // Arrange
        var pickup = new DateTime(2026, 11, 8, 10, 0, 0);
        var returnAt = pickup.AddMinutes(30);

        // Act
        var result = RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    [Trait("Rule", "BR-03")]
    public void Should_AddExtraDay_When_DurationIs48HoursAnd61Minutes()
    {
        // Arrange
        var pickup = new DateTime(2026, 11, 8, 10, 0, 0);
        var returnAt = pickup.AddHours(48).AddMinutes(61);

        // Act
        var result = RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    [Trait("Rule", "BR-03")]
    public void Should_ReturnOneDay_When_DurationIsExactly24Hours()
    {
        // Arrange
        var pickup = new DateTime(2026, 11, 8, 10, 0, 0);
        var returnAt = pickup.AddHours(24);

        // Act
        var result = RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        result.Should().Be(1);
    }
}
