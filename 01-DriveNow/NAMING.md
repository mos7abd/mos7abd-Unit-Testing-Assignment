# Naming Conventions

This document defines the naming conventions used across the DriveNow testing projects and supporting test code. These conventions apply to all tests added as part of the Unit Testing & TDD assignment.

## R1 — Test Project, Class, and File Naming

### Test Projects
- Test project names must follow the pattern: `{ProductionProject}.UnitTests`.
- The project name must clearly identify the production project being tested.
- Example: `DriveNow.UnitTests`.

### Test Classes
- Test class names must follow the pattern: `{ClassUnderTest}Tests`.
- Each test class should focus on one production class or one closely related responsibility.
- Example: `PricingServiceTests`.

### Test Files
- Each test class must be placed in a file with exactly the same name as the class.
- File names must use PascalCase and end with `Tests.cs`.
- Example: `PricingServiceTests.cs`.

## R2 — Test Method Naming

The following three naming styles were considered:

### Style 1 — Method_Scenario_ExpectedResult

The method name is divided into three parts: the method being tested, the scenario, and the expected result.

Examples:
- `CalculateRentalDays_25Hours_ReturnsOneDay`
- `CreateBooking_BlacklistedCustomer_ThrowsException`
- `ApplyPromo_LowercaseCode_AppliesDiscount`

### Style 2 — Should_ExpectedResult_When_Scenario

The name describes the expected behavior and the condition under which it should happen.

Examples:
- `Should_ReturnOneDay_WhenRentalDurationIs25Hours`
- `Should_RejectBlacklistedCustomer_WhenCreatingBooking`
- `Should_ApplyPromoCaseInsensitively_WhenCodeExists`

### Style 3 — Given_When_Then

The name describes the initial condition, the action, and the expected outcome.

Examples:
- `Given25HoursDuration_WhenCalculatingRentalDays_ThenReturnsOneDay`
- `GivenBlacklistedCustomer_WhenCreatingBooking_ThenThrowsException`
- `GivenLowercasePromoCode_WhenApplyingPromo_ThenAppliesDiscount`

### Selected Convention

The selected convention is:

`Should_ExpectedResult_When_Scenario`

**Reason for selection:**
- It makes the expected behavior clear from the test name.
- It is easy to read in Test Explorer.
- It focuses on business behavior rather than implementation details.
- It provides consistent and descriptive names across the test suite.

All test methods must use PascalCase, start with `Should_`, and describe one expected behavior under a specific condition.

## R3 — Test Helper Naming

The following naming conventions apply to test helper types:

### Test Doubles
- Fake implementations must start with `Fake`.
- Spy implementations must start with `Spy`.
- Mock objects should use `Mock` when represented by a named helper or field.

Examples:
- `FakeBookingRepository`
- `SpySmsSender`
- `MockPaymentGateway`

### Bogus Fakers
- Bogus faker classes must end with `Faker`.
- Their names must identify the generated entity.

Example:
- `CustomerFaker`

### AutoFixture Customizations
- Customization classes must end with `Customization`.
- Their names must identify the type being customized.

Example:
- `CustomerCustomization`

### AutoData Attributes
- Custom AutoData attributes must end with `AutoDataAttribute`.
- Their names must identify the fixture configuration or auto-mocking behavior.

Example:
- `AutoMoqDataAttribute`

## R4 — Production Code Naming and Namespace Conventions

Production code must use PascalCase for type names. Interfaces must start with `I`, and type names must communicate their responsibility.

### Interfaces
- Interface names must start with `I`.
- The remaining name must describe the contract or responsibility.
- Interfaces related to application services and abstractions belong in the appropriate `DriveNow.Application` namespace.
- Example: `IReceiptFormatter` in `DriveNow.Application.Receipts`.

### Services
- Service classes must end with `Service`.
- Application services belong in `DriveNow.Application.Services`.
- Domain-specific services belong in `DriveNow.Domain.Services`.
- Examples:
  - `PricingService` — `DriveNow.Application.Services`
  - `BookingService` — `DriveNow.Application.Services`

### Repositories
- Repository implementations must end with `Repository`.
- Repository implementations belong in `DriveNow.Infrastructure.Repositories`.
- Examples:
  - `BookingRepository` — `DriveNow.Infrastructure.Repositories`
  - `VehicleRepository` — `DriveNow.Infrastructure.Repositories`

### Exceptions
- Custom exception classes must end with `Exception`.
- Domain-related exceptions belong in `DriveNow.Domain.Exceptions`.
- Examples:
  - `BookingNotFoundException` — `DriveNow.Domain.Exceptions`
  - `InvalidPromoCodeException` — `DriveNow.Domain.Exceptions`

### General Rules
- Use PascalCase for classes, interfaces, methods, and properties.
- Use meaningful names that describe purpose and responsibility.
- Avoid unexplained abbreviations.
- Keep namespaces aligned with the project's existing folder and architectural organization.
- Do not introduce inconsistent naming styles when adding new production or test code.

---

**Enforcement:** These conventions are the naming standard for the assignment. Future naming convention tests must verify the applicable rules defined in this document.