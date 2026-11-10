# Hidden Inputs Audit

| Class | Line | Hidden Input | Why It Hurts Testing | How It Was Removed |
|---|---:|---|---|---|
| BookingService | Reference generation | Randomness | Tests cannot predict generated booking references. | Injected `IReferenceGenerator`. |
| BookingService | Date/time access | System clock | Tests depend on the actual current date and time. | Injected `TimeProvider`. |
| PricingService | Date/time access | System clock | Pricing behavior may depend on the current date. | Injected `TimeProvider`. |
| CancellationService | Date/time access | System clock | Cancellation behavior is difficult to test deterministically. | Injected `TimeProvider`. |
| ReturnService | Date/time access | System clock | Return timestamps depend on the machine clock. | Injected `TimeProvider`. |
| InMemoryDatabase | Seed data initialization | System clock | Seed data changes depending on when tests run. | Injected `TimeProvider`; seed dates derive from one captured `today` value. |
| PaymentGateway | Delay and failure simulation | Thread.Sleep and randomness | Tests become slow and payment failures are unpredictable. | Injected `IExecutionDelay` and `IPaymentFailurePolicy`. |
| SmsSender | Execution delay | Thread.Sleep | Tests wait unnecessarily for simulated external calls. | Injected `IExecutionDelay`. |
| EmailSender | File system and timestamp | File system and system clock | Tests depend on disk access and real timestamps. | Injected `IEmailOutbox` and `TimeProvider`. |
| AuditLog | File system and timestamp | File system and system clock | Tests depend on file access and real timestamps. | Injected `IAuditLogWriter` and `TimeProvider`. |
| Repositories | Shared data state | Static data store | Tests can affect each other through shared state. | Converted `InMemoryDatabase` into an instance dependency. |

Only production dependencies that are controllable through injected abstractions are considered resolved. Infrastructure implementations may still use system resources internally because they are replaceable at their boundaries.
