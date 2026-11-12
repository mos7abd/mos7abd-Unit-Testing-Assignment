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

---

# Task 4 — Architecture Decisions

## 1. Project Dependency Direction

After the Task 4 refactoring, the solution follows Dependency Inversion and keeps business logic independent of infrastructure details.

| Project | Depends On | Responsibility |
|---|---|---|
| DriveNow.Domain | None | Domain entities, enums, value objects, domain services, and exceptions. |
| DriveNow.Application | DriveNow.Domain | Application services, use cases, models, and abstractions. |
| DriveNow.Infrastructure | DriveNow.Application, DriveNow.Domain | Repository implementations, external services, file system operations, and persistence. |
| DriveNow.ConsoleApp | Application, Infrastructure, Domain | Composition root and console user interface. |

## 2. Interface Placement Decisions

### Application.Abstractions

Interfaces consumed by Application services are declared in the Application project.

This includes:

- Repository interfaces: `ICustomerRepository`, `IVehicleRepository`, `IBookingRepository`, and `IPromoCodeRepository`.
- External service interfaces: `IPaymentGateway`, `ISmsSender`, `IEmailSender`, `IReferenceGenerator`, `IPaymentFailurePolicy`, and `IExecutionDelay`.
- Logging interface: `IAuditLogger`.

These interfaces belong to Application because application services depend on their contracts, not their concrete implementations.

Infrastructure implements these interfaces. This allows unit tests to replace infrastructure dependencies with test doubles.

### Infrastructure.Abstractions

The following interfaces belong to Infrastructure:

- `IEmailOutbox`
- `IAuditLogWriter`

They are internal infrastructure boundaries used to isolate file system operations. Application services do not consume them, so they do not belong in Application.

## 3. Dependency Inversion

The dependency direction is:

- Domain has no dependency on other DriveNow projects.
- Application depends only on Domain.
- Infrastructure depends on Application and Domain to implement their contracts.
- ConsoleApp references Application and Infrastructure to compose the application.

Application does not reference Infrastructure. This prevents business logic from depending on implementation details.

## 4. Manual Dependency Injection

The ConsoleApp acts as the composition root.

It creates the shared `TimeProvider`, database instance, repositories, external service implementations, and application services.

Dependencies are passed through constructors manually. No DI container is used.

## 5. Testability

Hidden dependencies are exposed through injectable abstractions, including:

- `TimeProvider` for controlling time.
- `IReferenceGenerator` for controlling generated references.
- Repository interfaces for replacing data access.
- External service interfaces for replacing payment, SMS, email, delay, and failure behavior.
- Infrastructure-specific interfaces for isolating file system operations.

The in-memory database is an instance rather than a static shared store, allowing tests to use isolated data.

These decisions preserve the architecture rules established in Task 3 while making application behavior controllable in tests.