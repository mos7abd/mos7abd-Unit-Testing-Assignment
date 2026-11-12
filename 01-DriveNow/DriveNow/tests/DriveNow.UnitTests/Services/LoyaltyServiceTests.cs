
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Services;

namespace DriveNow.UnitTests.Services;

public class LoyaltyServiceTests
{
    [Theory]
    [InlineData(4, LoyaltyTier.None)]
    [InlineData(5, LoyaltyTier.Silver)]
    [InlineData(6, LoyaltyTier.Silver)]
    [InlineData(14, LoyaltyTier.Silver)]
    [InlineData(15, LoyaltyTier.Gold)]
    [InlineData(16, LoyaltyTier.Gold)]
    [InlineData(29, LoyaltyTier.Gold)]
    [InlineData(30, LoyaltyTier.Platinum)]
    [InlineData(31, LoyaltyTier.Platinum)]
    public void Should_UpgradeTier_When_CompletedRentalsChange(
        int completedRentals,
        LoyaltyTier expectedTier)
    {
        // Arrange
        var customer = new Customer
        {
            CompletedRentals = completedRentals,
            LoyaltyTier = LoyaltyTier.None
        };

        // Act
        LoyaltyService.UpgradeTier(customer);

        // Assert
        Assert.Equal(expectedTier, customer.LoyaltyTier);
    }

    [Theory]
    [InlineData(4, LoyaltyTier.Silver)]
    [InlineData(14, LoyaltyTier.Gold)]
    [InlineData(29, LoyaltyTier.Platinum)]
    public void Should_NeverDowngradeTier_When_CompletedRentalsAreBelowThreshold(
        int completedRentals,
        LoyaltyTier currentTier)
    {
        // Arrange
        var customer = new Customer
        {
            CompletedRentals = completedRentals,
            LoyaltyTier = currentTier
        };

        // Act
        LoyaltyService.UpgradeTier(customer);

        // Assert
        Assert.Equal(currentTier, customer.LoyaltyTier);
    }
}
