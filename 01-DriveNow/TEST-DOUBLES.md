# Test Doubles

## Hand-written Test Doubles

The BookingService tests use five hand-written test doubles without relying on a mocking library.

| Double | Kind | Test that uses it | Why this kind instead of another? |
|---|---|---|---|
| DummyEmailSender | Dummy | `Should_CreateBookingAndNotifyCustomer_When_RequestIsValid` | Email behavior is irrelevant to this test. The object only satisfies the BookingService dependency. |
| StubCustomerRepository | Stub | `Should_CreateBookingAndNotifyCustomer_When_RequestIsValid` | The test needs a predefined customer to control the input and avoid depending on a real repository. |
| SpySmsSender | Spy | `Should_CreateBookingAndNotifyCustomer_When_RequestIsValid` | The test needs to record SMS calls and inspect the phone number and message afterward. |
| MockPaymentGateway | Mock | `Should_CreateBookingAndNotifyCustomer_When_RequestIsValid` | The test needs to verify that a payment was requested with the expected customer, amount, and description. |
| FakeBookingRepository | Fake | `Should_CreateBookingAndNotifyCustomer_When_RequestIsValid` | The test needs working in-memory repository behavior, including storing bookings and querying open bookings. |

## Supporting Test Doubles

Additional supporting doubles are used to construct BookingService in isolation:

- `StubVehicleRepository`
- `StubPromoCodeRepository`
- `StubAuditLogger`
- `StubReferenceGenerator`
- `FixedTimeProvider`

These support the test setup but are not counted among the five required hand-written double types.

## Moq vs Hand-written Test Doubles

Three BookingService tests were implemented using Moq:

- `Should_ReturnCustomer_When_CustomerExists`
- `Should_SendConfirmationSms_When_BookingIsCreated`
- `Should_NotChargeDeposit_When_CustomerIsPlatinum`

### Comparison

| Aspect | Hand-written Doubles | Moq |
|---|---|---|
| Setup | Requires writing a dedicated class and implementing its methods. | Uses `Setup()` and `Returns()` directly in the test. |
| Verification | Requires manually recording calls and inspecting stored values. | Uses `Verify()` and `Times.Once` or `Times.Never`. |
| Readability | Can be easier to understand when the fake behavior is substantial. | More concise for simple interactions and call verification. |
| Brittleness | Custom behavior is explicit, but changes to interfaces require updating the handwritten classes. | Tests can become coupled to exact method calls and arguments. |
| Reusability | Reusable classes can serve many tests. | Mock configurations are usually written close to each test. |

### Conclusion

Hand-written doubles are useful when a test needs meaningful in-memory behavior, such as `FakeBookingRepository`.

Moq is useful when a test mainly needs to configure a dependency or verify interactions, such as payment and SMS calls.

Both approaches are valuable. The choice depends on the behavior being tested.


## R4 — AutoFixture Random Customer Data

### The Problem

AutoFixture generates anonymous Customer objects with random property values.

Some generated values may violate BookingService business rules, such as:
- Customer is younger than 21.
- Driving license is less than one year old.
- Customer is blacklisted.

This can cause tests to fail because of invalid test data rather than the behavior being tested.

### The Solution

Created `ValidCustomerCustomization` implementing `ICustomization`.

It configures every generated Customer with:
- DateOfBirth: 1990-01-01
- LicenseIssueDate: 2015-01-01
- IsBlacklisted: false

Registered this customization inside `AutoMoqDataAttribute`, so tests using this attribute receive valid Customer objects automatically.

### Verification

Added a test that verifies generated customers meet the minimum age and license requirements and are not blacklisted.



## R5 — InlineAutoData

Used `InlineAutoMoqDataAttribute` to combine inline test values with AutoFixture and AutoMoq.

Tested the minimum customer age rule with three explicit ages:

| Age | Expected Result |
|---:|---|
| 20 | Booking rejected |
| 21 | Booking accepted |
| 25 | Booking accepted |

The age is provided explicitly through InlineAutoData, while the remaining test dependencies and objects are generated automatically.

This keeps boundary-value scenarios readable without manually constructing every test object.

## R6 — AutoFixture vs Bogus

Both libraries help create test data, but they solve different problems.

### AutoFixture

AutoFixture focuses on creating anonymous objects and reducing Arrange boilerplate.

In `BookingServiceAutoFixtureTests`, AutoFixture creates:
- BookingService and its dependencies.
- Customer, Vehicle, and BookingRequest objects.
- Moq objects for interfaces through AutoMoqCustomization.

`[Frozen]` ensures that the configured mock instance is shared with the automatically created BookingService.

A custom `ValidCustomerCustomization` ensures generated customers satisfy the minimum age, license, and blacklist requirements.

### Bogus

Bogus focuses on generating realistic and controlled fake data.

In `CustomerFaker` and `VehicleFaker`, Bogus generates realistic values such as:
- Customer names, emails, and phone numbers.
- Vehicle models and plate numbers.
- Random mileage and daily rates.

Fixed seeds and a fixed reference date make generated data reproducible.

The fluent methods `WithAge`, `WithTier`, `WithCategory`, and `WithDailyRate` allow tests to control specific business properties.

### Comparison

| Aspect | AutoFixture | Bogus |
|---|---|---|
| Main purpose | Automatic object creation | Realistic fake data generation |
| Dependencies | Can create mocks using AutoMoq | Does not create dependency mocks |
| Data control | Customizations and Frozen parameters | Faker rules and fluent methods |
| Best use | Reducing test setup boilerplate | Generating realistic domain data |
| Example | BookingServiceAutoFixtureTests | CustomerFakerTests and VehicleFakerTests |

### Conclusion

AutoFixture is useful when a test needs many objects and dependencies with minimal manual setup.

Bogus is useful when tests need realistic, repeatable, and customizable domain data.

They complement each other and can be used together when a test needs both automatic dependency creation and realistic test data.


## R7 — Booking Snapshot with Verify

Added `BookingServiceSnapshotTests` to capture the complete Booking created by `BookingService.CreateBooking`.

The test uses deterministic test dependencies to keep the snapshot stable:

- `FixedTimeProvider` provides a fixed date and time.
- `StubReferenceGenerator` generates a predictable booking reference.
- `FakeBookingRepository` assigns predictable booking IDs.

This prevents changing timestamps, references, and IDs from causing unnecessary snapshot failures.

The verified snapshot is stored in:

`BookingServiceSnapshotTests.Should_MatchCreatedBooking_Snapshot.verified.txt`

The test passed after the snapshot was reviewed and approved.
