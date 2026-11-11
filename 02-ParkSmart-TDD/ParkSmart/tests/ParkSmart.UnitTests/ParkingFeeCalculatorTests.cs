using ParkSmart.Application;
using ParkSmart.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkSmart.UnitTests
{


    public class ParkingFeeCalculatorTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        public void Should_ReturnZero_When_ParkingDurationIsUpTo15Minutes(int minutes)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddMinutes(minutes)
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(0m, result);
        }


        [Theory]
        [InlineData(16, 10)]
        [InlineData(60, 10)]
        [InlineData(61, 20)]
        [InlineData(180, 30)]
        public void Should_ChargeTenPoundsPerStartedHour_When_ParkingExceedsFreePeriod(
            int minutes,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddMinutes(minutes)
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(expectedFee, result);
        }


        [Fact]
        public void Should_ThrowArgumentException_When_ExitTimeIsBeforeEntryTime()
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddMinutes(-1)
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var exception = Assert.Throws<ArgumentException>(
                () => calculator.Calculate(session));

            // Assert
            Assert.Equal("Exit time must be after entry time.", exception.Message);
        }

        [Fact]
        public void Should_ReturnZero_When_EntryTimeEqualsExitTime()
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(0m, result);
        }


        [Theory]
        [InlineData(600, 80)]
        [InlineData(1800, 140)]
        [InlineData(2880, 160)]
        [InlineData(2890, 170)]
        public void Should_ApplyDailyCap_When_ParkingExceedsDailyLimit(
            int minutes,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddMinutes(minutes)
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(expectedFee, result);
        }


    }
}




