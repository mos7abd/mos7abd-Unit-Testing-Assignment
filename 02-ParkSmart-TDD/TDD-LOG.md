
# ParkSmart TDD Log

## Project Overview

- **Project:** ParkSmart
- **Approach:** Test-Driven Development (Red → Green → Refactor)
- **Stages:** S1–S10
- **Required Refactoring:** At least 4 genuine refactors across 4 different stages.
- **Completed Refactoring Stages:** S7, S8, S9, S10.

---

## Red / Green Cycle Log

### S1 – Free Parking

**Red Commit:**
`red(S1): free parking up to 15 minutes`

**Tested Behavior:**
- Parking sessions up to 15 minutes are free.
- Parking beyond 15 minutes is chargeable.

**Why It Failed:**
The initial implementation did not support the 15-minute free parking rule.

**Green Commit:**
`green(S1): free parking up to 15 minutes`

**Implementation:**
Added the initial free parking duration rule.

---

### S2 – Started Hourly Rate

**Red Commit:**
`red(S2): charge per started hour`

**Tested Behavior:**
- 16 minutes → 10 EGP.
- 60 minutes → 10 EGP.
- 61 minutes → 20 EGP.
- 180 minutes → 30 EGP.

**Why It Failed:**
The calculator did not yet calculate started hours after the free parking period.

**Green Commit:**
`green(S2): charge per started hour`

**Implementation:**
Added started-hour fee calculation.

---

### S3 – Time Validation

**Red Commit:**
`red(S3): validate parking session times`

**Tested Behavior:**
- Exit time before entry time throws `ArgumentException`.
- Equal entry and exit times result in zero fees.

**Why It Failed:**
The calculator did not validate invalid parking session times.

**Green Commit:**
`green(S3): validate parking session times`

**Implementation:**
Added validation with the message:

`Exit time must be after entry time.`

---

### S4 – Daily Parking Cap

**Red Commit:**
`red(S4): apply daily parking fee cap`

**Tested Behavior:**
- Maximum 80 EGP per 24-hour block.
- Multi-day sessions apply the cap to each block.

**Why It Failed:**
The previous implementation calculated hourly fees without applying a daily cap.

**Green Commit:**
`green(S4): apply daily parking fee cap`

**Implementation:**
Added full-day and remaining-time calculations with a daily cap.

---

### S5 – Vehicle-Specific Rates

**Red Commit:**
`red(S5): apply vehicle-specific parking rates`

**Tested Behavior:**

| Vehicle | Hourly Rate | Daily Cap |
|---|---:|---:|
| Motorcycle | 5 EGP | 40 EGP |
| Car | 10 EGP | 80 EGP |
| Truck | 25 EGP | 200 EGP |

**Why It Failed:**
The calculator used the original car pricing instead of selecting rates based on vehicle type.

**Green Commit:**
`green(S5): apply vehicle-specific parking rates`

**Implementation:**
Added vehicle-specific hourly rates and daily caps.

**Additional Test Correction:**

`test(S5): correct truck daily cap expectation`

The expected fee for a 30-hour truck session was corrected from 600 EGP to 350 EGP.

The correct calculation is:
- First 24 hours: 200 EGP.
- Remaining 6 hours: 150 EGP.
- Total: 350 EGP.

---

### S6 – Night Flat Rates

**Red Commit:**
`red(S6): apply night flat rates`

**Tested Behavior:**
- Night parking starts at or after 22:00.
- Exit must be by 08:00 the following day.
- The lower amount between normal fees and the night rate is charged.

**Night Rates:**
- Motorcycle: 15 EGP.
- Car: 30 EGP.
- Truck: 75 EGP.

**Why It Failed:**
The calculator did not recognize qualifying overnight sessions.

**Green Commit:**
`green(S6): apply night flat rates`

**Implementation:**
Added overnight eligibility validation and vehicle-specific night rates.

---

### S7 – Lost Ticket Minimum Fees

**Red Commit:**
`red(S7): apply lost ticket minimum fees`

**Tested Behavior:**
- Motorcycle minimum: 75 EGP.
- Car minimum: 150 EGP.
- Truck minimum: 300 EGP.
- Lost tickets bypass the normal free parking benefit.
- If normal fees exceed the minimum, normal fees are charged.

**Why It Failed:**
The calculator did not enforce minimum fees for lost tickets.

**Green Commit:**
`green(S7): apply lost ticket minimum fees`

**Implementation:**
Added vehicle-specific lost-ticket minimum fees.

**Refactor Commit:**
`refactor(S7): extract lost ticket fee calculation`

**Refactoring:**
Extracted lost-ticket minimum calculation into:

`GetLostTicketMinimum(VehicleType vehicle)`

This improved separation of responsibilities and kept vehicle-specific minimum values in one method.

---

### S8 – Friday Free Parking

**Red Commit:**
`red(S8): apply Friday free parking period`

**Tested Behavior:**
- Friday parking up to 2 hours is free.
- Friday parking beyond 2 hours is chargeable.
- Other weekdays retain the 15-minute free period.

**Test Method:**
`Should_ApplyTwoHoursFree_When_EntryDayIsFriday`

**Why It Failed:**
The existing implementation always used a 15-minute free period.

**Green Commit:**
`green(S8): apply Friday free parking period`

**Implementation:**
Added a day-dependent free parking duration.

**Refactor Commit:**
`refactor(S8): extract free parking duration`

**Refactoring:**
Extracted free-duration calculation into:

`GetFreeDuration(DateTime entryTime)`

This made the free parking rule easier to maintain and extend.

---

### S9 – Subscriber Parking Benefits

**Red Commit:**
`red(S9): apply subscriber parking benefits`

**Tested Behavior:**
- Subscribers receive free parking for qualifying same-day weekday sessions between 07:00 and 19:00.
- Other subscriber parking sessions receive a 50% discount.
- Lost-ticket minimum fees remain enforced.

**Test Methods:**
- `Should_ApplySubscriberBenefits_When_WeekdayParking`
- `Should_ApplyFiftyPercentDiscount_When_SubscriberParksOnSaturday`
- `Should_ApplyLostTicketMinimumAfterDiscount_When_SubscriberLosesTicket`

**Why It Failed:**
The calculator did not yet apply subscriber-specific free parking or discounts.

**Green Commit:**
`green(S9): apply subscriber parking benefits`

**Implementation:**
Added subscriber eligibility rules and a 50% discount for non-qualifying subscriber sessions.

**Refactor Commit:**
`refactor(S9): extract subscriber free parking eligibility`

**Refactoring:**
Extracted subscriber free parking eligibility into:

`QualifiesForSubscriberFreeParking(ParkingSession session)`

This separated the eligibility condition from the main calculation method.

---

### S10 – EV Charging Fees

**Red Commit:**
`red(S10): add EV charging fee tests`

**Tested Behavior:**
- EV charging costs 3.50 EGP per kWh.
- Charging fees are added after parking discounts.
- Charging fees are not discounted.
- Negative charged kWh values throw `ArgumentException`.
- Final fees are rounded to two decimal places using `MidpointRounding.AwayFromZero`.

**Test Methods:**
- `Should_AddChargingFee_When_VehicleUsesElectricCharging`
- `Should_NotDiscountChargingFee_When_SubscriberGetsFreeParking`
- `Should_ThrowArgumentException_When_ChargedKwhIsNegative`

**Why It Failed:**
The calculator did not calculate EV charging fees or validate negative charging values.

**Green Commit:**
`green(S10): add EV charging fees`

**Implementation:**
Added negative kWh validation, EV charging fee calculation, and final fee rounding.

**Refactor Commit:**
`refactor(S10): extract EV charging fee calculation`

**Refactoring:**
Extracted EV charging fee calculation into:

`CalculateChargingFee(decimal chargedKwh)`

This isolated the charging calculation and improved maintainability.

---

## Refactoring Summary

Four genuine refactors were completed across four different stages.

| Stage | Refactoring | Commit |
|---|---|---|
| S7 | Extract lost ticket fee calculation | `refactor(S7): extract lost ticket fee calculation` |
| S8 | Extract free parking duration | `refactor(S8): extract free parking duration` |
| S9 | Extract subscriber free parking eligibility | `refactor(S9): extract subscriber free parking eligibility` |
| S10 | Extract EV charging fee calculation | `refactor(S10): extract EV charging fee calculation` |

All refactoring changes were followed by successful test runs without adding new test cases.

---

## Final Reflection

### Which stage forced the biggest change to earlier code, and what did it teach about designing up front?

Stage S5 forced one of the biggest changes to the earlier implementation.

Initially, the parking fee calculator was designed around a single vehicle type with a fixed hourly rate and daily cap.

When motorcycle and truck pricing requirements were introduced, the implementation had to become flexible enough to select different rates and caps depending on the vehicle type, while preserving the existing started-hour and daily-cap behavior.

This demonstrated that designing too early around one specific scenario can make future changes harder.

TDD helped reveal these limitations gradually. Instead of trying to predict every possible requirement in advance, each new test exposed the next behavior the system needed to support.

The later stages reinforced this lesson by introducing night rates, lost-ticket minimums, Friday free parking, subscriber benefits, and EV charging without breaking the previously implemented rules.
