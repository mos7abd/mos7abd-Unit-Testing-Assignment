
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
                return Math.Max(normalFee, GetLostTicketMinimum(session.Vehicle));
            }

            if (duration <= TimeSpan.FromMinutes(15))
            {
                return 0m;
            }

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

                return Math.Min(normalFee, nightRate);
            }

            return normalFee;
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
    }
}
