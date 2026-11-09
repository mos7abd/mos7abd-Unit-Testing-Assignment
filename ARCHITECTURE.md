# Architecture Tests — Task 3

## Overview

This document records the architecture and naming violations detected by the architecture tests when running against the original DriveNow starter code.

The purpose of these tests is to identify dependency and naming issues that may affect maintainability, separation of concerns, and unit test isolation.

No production code was modified during this task.

## R1 — Domain Dependencies

**Result:** Passed

**Rule:** `DriveNow.Domain` must not depend on any other DriveNow project.

`DriveNow.Domain` does not depend on:
- `DriveNow.Application`
- `DriveNow.Infrastructure`
- `DriveNow.ConsoleApp`

No violations were detected.

## R2 — Application Dependencies

**Result:** Failed

**Rule:** `DriveNow.Application` must depend only on `DriveNow.Domain` and must not depend on `DriveNow.Infrastructure` or `DriveNow.ConsoleApp`.

**Offending types:**

- `DriveNow.Application.Services.BookingService`
- `DriveNow.Application.Services.PricingService`
- `DriveNow.Application.Services.ReturnService`
- `DriveNow.Application.Bookings.CancellationService`

**Why it matters for testing:**

Dependencies on Infrastructure can make application services harder to test in isolation. They may cause unit tests to rely on infrastructure implementations instead of abstractions and test doubles.

The separate check for dependencies on `DriveNow.ConsoleApp` passed.

## R3 — Infrastructure Dependencies

**Result:** Passed

**Rule:** `DriveNow.Infrastructure` must not depend on `DriveNow.ConsoleApp`.

No violations were detected.

## R4 — Production Naming Conventions

**Result:** Failed

**Rule:** Production interfaces, services, repositories, and custom exceptions must follow the naming and namespace conventions defined in `NAMING.md`.

**Offending types:**

| Type | Violation |
|---|---|
| `DriveNow.Application.Receipts.ReceiptService` | Service is not in a `Services` namespace. |
| `DriveNow.Application.Bookings.CancellationService` | Service is not in a `Services` namespace. |
| `DriveNow.Infrastructure.Repositories.CustomerRepo` | Repository name must end with `Repository`. |
| `DriveNow.Infrastructure.External.PaymentFailedException` | Custom exception must be in `DriveNow.Domain.Exceptions`. |

**Why it matters for testing:**

Consistent naming and namespaces make production responsibilities easier to discover and enforce. They also make architecture rules more predictable and help identify misplaced responsibilities.

## R5 — Test Naming Conventions

**Result:** Failed

**Rule:** Test classes must end with `Tests`, and every `[Fact]` or `[Theory]` method in `DriveNow.UnitTests` must follow this naming pattern:

`Should_ExpectedResult_When_Scenario`

**Offending methods:**

1. `DriveNow.UnitTests.Services.RentalMathTests.Should_CalculateAgeCorrectly`
2. `DriveNow.UnitTests.Services.RentalMathTests.Should_ReturnCorrectLicenseYears_WhenCheckingDateBoundaries`
3. `DriveNow.UnitTests.Services.RentalMathTests.Should_ReturnCorrectRentalDays_WhenCheckingDurationBoundaries`
4. `DriveNow.UnitTests.Services.RentalMathTests.Should_ThrowArgumentException_WhenReturnAtEqualsPickup`
5. `DriveNow.UnitTests.Services.RentalMathTests.Should_ThrowArgumentException_WhenReturnAtIsBeforePickup`
6. `DriveNow.UnitTests.Services.RentalMathTests.Should_HandleFebruary29Birthday_InLeapYears`
7. `DriveNow.UnitTests.Services.RentalMathTests.Should_HandleFebruary29Anniversary_InLeapYears`
8. `DriveNow.UnitTests.Services.RentalMathTests.Should_ReturnAtLeastOneDay_WhenRentalDurationIsLessThan24Hours`
9. `DriveNow.UnitTests.Services.RentalMathTests.Should_AddExtraDay_WhenDurationIs48HoursAnd61Minutes`
10. `DriveNow.UnitTests.Services.RentalMathTests.Should_ReturnOneDay_WhenRentalDurationIsExactly24Hours`

**Test class naming:** No violations were detected.

**Why it matters for testing:**

Consistent test names make the expected behavior and tested scenario clear in Test Explorer and test reports. Names that do not follow the convention make tests less readable and harder to understand and maintain.

## Summary

| Rule | Result | Violations |
|---|---|---:|
| R1 — Domain dependencies | Passed | 0 |
| R2 — Application dependencies | Failed | 4 |
| R3 — Infrastructure dependencies | Passed | 0 |
| R4 — Production naming | Failed | 4 |
| R5 — Test naming | Failed | 10 |
| **Total** | **Failed** | **18** |

All detected violations are documented for correction in Task 4.

The architecture tests are expected to pass after the required production dependencies and naming conventions are corrected.

