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