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
    }
}




