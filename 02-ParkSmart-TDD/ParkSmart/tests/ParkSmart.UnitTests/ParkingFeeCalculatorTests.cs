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


        [Theory]
        [InlineData(VehicleType.Motorcycle, 180, 15)]
        [InlineData(VehicleType.Truck, 600, 200)]
        [InlineData(VehicleType.Truck, 1800, 350)]
        public void Should_ApplyVehicleSpecificRates_When_VehicleTypeChanges(
            VehicleType vehicle,
            int minutes,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddMinutes(minutes),
                Vehicle = vehicle
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(expectedFee, result);
        }

        [Theory]
        [InlineData(VehicleType.Car, 30)]
        [InlineData(VehicleType.Motorcycle, 15)]
        [InlineData(VehicleType.Truck, 75)]
        public void Should_ApplyNightFlatRate_When_ParkingSessionQualifies(
            VehicleType vehicle,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 12, 23, 0, 0);
            var exitTime = new DateTime(2026, 11, 13, 7, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = exitTime,
                Vehicle = vehicle
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(expectedFee, result);
        }


        [Theory]
        [InlineData(VehicleType.Car, 5, 150)]
        [InlineData(VehicleType.Motorcycle, 5, 75)]
        [InlineData(VehicleType.Truck, 5, 300)]
        [InlineData(VehicleType.Truck, 1800, 350)]
        public void Should_ApplyLostTicketMinimum_When_TicketIsLost(
            VehicleType vehicle,
            int minutes,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 12, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddMinutes(minutes),
                Vehicle = vehicle,
                TicketLost = true
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(expectedFee, result);
        }


        [Theory]
        [InlineData(13, 120, 0)]
        [InlineData(13, 121, 30)]
        [InlineData(12, 20, 10)]
        public void Should_ApplyTwoHoursFree_When_EntryDayIsFriday(
            int day,
            int minutes,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, day, 10, 0, 0);

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


        [Theory]
        [InlineData(9, 17, 0)]
        [InlineData(18, 20, 10)]
        public void Should_ApplySubscriberBenefits_When_WeekdayParking(
            int entryHour,
            int exitHour,
            decimal expectedFee)
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, entryHour, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = new DateTime(2026, 11, 9, exitHour, 0, 0),
                IsSubscriber = true
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(expectedFee, result);
        }

        [Fact]
        public void Should_ApplyFiftyPercentDiscount_When_SubscriberParksOnSaturday()
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 14, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddHours(3),
                IsSubscriber = true
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(15m, result);
        }

        [Fact]
        public void Should_ApplyLostTicketMinimumAfterDiscount_When_SubscriberLosesTicket()
        {
            // Arrange
            var entryTime = new DateTime(2026, 11, 9, 10, 0, 0);

            var session = new ParkingSession
            {
                EntryTime = entryTime,
                ExitTime = entryTime.AddHours(1),
                IsSubscriber = true,
                TicketLost = true
            };

            var calculator = new ParkingFeeCalculator();

            // Act
            var result = calculator.Calculate(session);

            // Assert
            Assert.Equal(150m, result);
        }


    }
}




