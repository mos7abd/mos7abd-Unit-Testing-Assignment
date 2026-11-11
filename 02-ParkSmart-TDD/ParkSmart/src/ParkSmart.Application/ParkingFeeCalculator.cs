using ParkSmart.Domain;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (duration <= TimeSpan.FromMinutes(15))
            {
                return 0m;
            }

            var totalMinutes = (decimal)duration.TotalMinutes;
            var fullDays = (int)(totalMinutes / 1440m);
            var remainingMinutes = totalMinutes % 1440m;

            var remainingHours = Math.Ceiling(remainingMinutes / 60m);

            var dailyFee = 80m;
            var remainingFee = remainingHours * 10m;

            return fullDays * dailyFee + Math.Min(remainingFee, dailyFee);
        }

    }
}
