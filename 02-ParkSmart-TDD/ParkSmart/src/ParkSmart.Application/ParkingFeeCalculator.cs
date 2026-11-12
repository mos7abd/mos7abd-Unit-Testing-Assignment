
using ParkSmart.Domain;
using System;

namespace ParkSmart.Application
{
    public class ParkingFeeCalculator
    {


        public decimal Calculate(ParkingSession session)
        {
            if (session.ExitTime < session.EntryTime)
            {
                throw new ArgumentException("Exit time must be after entry time.");
            }

            if (session.ChargedKwh < 0)
            {
                throw new ArgumentException("Charged kWh cannot be negative.");
            }

            var duration = session.ExitTime - session.EntryTime;

            var (hourlyRate, dailyCap) = session.Vehicle switch
            {
                VehicleType.Motorcycle => (5m, 40m),
                VehicleType.Truck => (25m, 200m),
                _ => (10m, 80m)
            };

            var totalMinutes = (decimal)duration.TotalMinutes;
            var fullDays = (int)(totalMinutes / 1440m);
            var remainingMinutes = totalMinutes % 1440m;

            var remainingHours = Math.Ceiling(remainingMinutes / 60m);
            var remainingFee = remainingHours * hourlyRate;

            var normalFee = fullDays * dailyCap + Math.Min(remainingFee, dailyCap);

            if (session.TicketLost)
            {
                normalFee = GetLostTicketFee(normalFee, session.Vehicle);
            }
            else if (duration <= GetFreeDuration(session.EntryTime))
            {
                normalFee = 0m;
            }
            else
            {
                var qualifiesForNightRate =
                    session.EntryTime.TimeOfDay >= new TimeSpan(22, 0, 0) &&
                    session.ExitTime.Date == session.EntryTime.Date.AddDays(1) &&
                    session.ExitTime.TimeOfDay <= new TimeSpan(8, 0, 0);

                if (qualifiesForNightRate)
                {
                    var nightRate = session.Vehicle switch
                    {
                        VehicleType.Motorcycle => 15m,
                        VehicleType.Truck => 75m,
                        _ => 30m
                    };

                    normalFee = Math.Min(normalFee, nightRate);
                }
            }

            if (session.IsSubscriber && !session.TicketLost)
            {
                if (QualifiesForSubscriberFreeParking(session))
                {
                    normalFee = 0m;
                }
                else
                {
                    normalFee *= 0.5m;
                }
            }

            var totalFee = normalFee + CalculateChargingFee(session.ChargedKwh);

            return Math.Round(totalFee, 2, MidpointRounding.AwayFromZero);
        }


        private static decimal GetLostTicketFee(decimal normalFee, VehicleType vehicle)
        {
            var minimumFee = GetLostTicketMinimum(vehicle);
            return Math.Max(normalFee, minimumFee);
        }


        private static decimal GetLostTicketMinimum(VehicleType vehicle)
        {
            return vehicle switch
            {
                VehicleType.Motorcycle => 75m,
                VehicleType.Truck => 300m,
                _ => 150m
            };
        }

        private static TimeSpan GetFreeDuration(DateTime entryTime)
        {
            return entryTime.DayOfWeek == DayOfWeek.Friday
                ? TimeSpan.FromHours(2)
                : TimeSpan.FromMinutes(15);
        }

        private static bool QualifiesForSubscriberFreeParking(ParkingSession session)
        {
            return session.EntryTime.Date == session.ExitTime.Date &&
                   session.EntryTime.DayOfWeek >= DayOfWeek.Monday &&
                   session.EntryTime.DayOfWeek <= DayOfWeek.Friday &&
                   session.EntryTime.TimeOfDay >= new TimeSpan(7, 0, 0) &&
                   session.ExitTime.TimeOfDay <= new TimeSpan(19, 0, 0);
        }

        private static decimal CalculateChargingFee(decimal chargedKwh)
        {
            return chargedKwh * 3.50m;
        }
    }
}
