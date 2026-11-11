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

            var startedHours = (int)Math.Ceiling(duration.TotalMinutes / 60);

            return startedHours * 10m;
        }
    }
}
