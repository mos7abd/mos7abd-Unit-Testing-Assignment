# FIRST Audit — Naive BookingService Tests

**Group:** 4

**Target:** `BookingService.CreateBooking`

## 1. Test Execution Results

The complete available test suite was executed 5 times before changing any production code.

| Run | UnitTests Passed | UnitTests Failed | UnitTests Duration | LegacyTests Passed | LegacyTests Failed | LegacyTests Duration |
|---|---:|---:|---:|---:|---:|---:|
| 1 | 24 | 1 | 2.1 sec | 5 | 0 | 75 ms |
| 2 | 24 | 1 | 2.1 sec | 5 | 0 | 70 ms |
| 3 | 24 | 1 | 2.1 sec | 5 | 0 | 75 ms |
| 4 | 24 | 1 | 2.0 sec | 5 | 0 | 77 ms |
| 5 | 24 | 1 | 2.1 sec | 5 | 0 | 75 ms |

**Total per run:** 30 tests — 29 passed and 1 failed.

The same test failed in all five runs:

`CreateBooking_WithPlatinumCustomer_ReturnsZeroDeposit`

Failure message:

`BookingRejectedException: Vehicle is already booked for these dates`

## 2. FIRST Analysis

### F — Fast

**Status: Violated**

The naive tests execute real operations that introduce unnecessary delays.

Evidence:
- `PaymentGateway.PauseAndMaybeFail()` calls `Thread.Sleep(1500)` at `PaymentGateway.cs:21`.
- `SmsSender.Send()` also introduces a delay before completing.
- The naive test group took approximately 2 seconds in the observed execution.

These delays make the tests slower than necessary.

### I — Independent

**Status: Violated**

The tests share mutable static data through `InMemoryDatabase`.

Evidence:
- `InMemoryDatabase.cs:12` contains the static `Bookings` list.
- `BookingRepository.Add()` adds bookings to this shared list.
- There is no reset between test executions.

The Happy Path test creates a booking for vehicle 1 using dates 10 to 13 days from the current date.

The Platinum test uses the same vehicle and dates.

When the first booking remains in the shared database, the Platinum test fails with:

`Vehicle is already booked for these dates`

The Platinum test passed when executed individually (518 ms), but failed when executed with the test group.

This demonstrates test interference caused by shared state.

### R — Repeatable

**Status: Violated**

The payment gateway can fail randomly.

Evidence:
- `PaymentGateway.cs:21` introduces a delay.
- `PaymentGateway.cs:22` uses `new Random().Next(10) == 0`.
- This creates a 10% chance of throwing `PaymentFailedException`.

The Happy Path test failed individually with:

`PaymentFailedException: Gateway timeout`

The failure occurred inside `PaymentGateway.PauseAndMaybeFail()`.

Therefore, the same test can produce different outcomes without any change to its input.

### S — Self-validating

**Status: Satisfied**

The three naive tests contain explicit assertions.

Evidence:
- Happy Path checks that the returned booking is not null, has `Reserved` status, and contains the expected customer and vehicle IDs.
- Platinum checks that the deposit equals zero and the booking status is `Reserved`.
- Overlap rejection uses `Assert.Throws<BookingRejectedException>()`.

The tests report their results through assertions rather than requiring manual inspection.

### T — Timely

**Status: Violated**

`BookingService` directly creates its dependencies instead of receiving them through dependency injection.

Evidence:
- `BookingService.cs:14-21` creates repositories, pricing service, payment gateway, SMS sender, and email sender using `new()`.

This tightly couples the service to concrete implementations.

As a result, tests cannot easily replace external dependencies with controlled test doubles, making it harder to write isolated tests early in development.

## 3. Conclusion

The naive tests revealed several FIRST-related problems:

- **Fast:** Real delays from external operations slow down tests.
- **Independent:** Shared static booking data causes interference between tests.
- **Repeatable:** Random payment failures make test outcomes unpredictable.
- **Self-validating:** The tests use assertions to verify their expected outcomes.
- **Timely:** Direct construction of concrete dependencies makes isolated testing difficult.

No production code was modified during this audit.