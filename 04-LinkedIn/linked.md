# LinkedIn Technical Posts

**LinkedIn Profile:** https://www.linkedin.com/in/mo-abdella

## Publication Note

The required LinkedIn technical posts for this assignment have been prepared and included below as drafts.

As mentioned in the previous assignment, I prefer to publish LinkedIn posts when they offer meaningful value to my professional development and allow me to share practical experiences with the developer community. Therefore, I have decided not to publish these posts on LinkedIn at this stage.

However, all four topics have been covered, including the required technical explanations, C# code examples, and practical applications based on my work throughout this assignment.

---

## 1. Test Doubles

During my Unit Testing assignment, I worked with different types of Test Doubles, and I realized that they are not all used for the same purpose.

At first, Dummy, Stub, Spy, Mock, and Fake seemed very similar to me. But after using them in tests, the differences became clearer.

- **Dummy:** An object passed to a method just because a dependency is required, but it is not actually used.
- **Stub:** Returns predefined data so we can control the scenario being tested.
- **Spy:** Records information about how it was used, such as how many times a method was called.
- **Mock:** Helps verify that specific interactions happened as expected.
- **Fake:** A working but simplified implementation, such as an in-memory repository instead of a real database.

For example, in my DriveNow tests, I used Moq to control repository behavior and verify interactions.

```csharp
repository
    .Setup(x => x.GetById(It.IsAny<int>()))
    .Returns(existingBooking);

// Execute the service method
var result = service.CancelBooking(bookingId);

// Verify the expected interaction
repository.Verify(
    x => x.Update(It.IsAny<Booking>()),
    Times.Once);
```

Here, I don't need a real database to test the service behavior. I can control the repository response and verify whether the service called Update as expected.

The main thing I learned is that choosing the right test double depends on what I want to test: data, behavior, interaction, or a simplified dependency.

#SimulationAcademy #DotNet #CSharp #UnitTesting

---

## 2. Bogus

One thing I noticed while writing unit tests is that creating test data manually can make tests longer and harder to read, especially when the same entities are needed in many different scenarios.

That's where Bogus became useful in my assignment.

Bogus is a library for generating realistic fake data. Instead of manually creating every Customer with a name, email, phone number, and other properties, I can use a Faker class to generate that data for me.

I used the idea of a CustomerFaker to keep test data creation separate from the actual test logic.

For example:

```csharp
var faker = new Faker<Customer>()
    .RuleFor(x => x.Name, f => f.Name.FullName())
    .RuleFor(x => x.Email, f => f.Internet.Email());

var customer = faker.Generate();
```

This makes the test easier to read because the test can focus on the behavior instead of spending many lines preparing objects.

But there is something important: random data should be reproducible.

Using a fixed seed helps generate the same data again, which makes debugging much easier. If a test fails today, I want to be able to reproduce the same scenario tomorrow.

Random data without a fixed seed can be useful for exploring different inputs, but it is not always a good choice for deterministic unit tests, especially when testing specific business rules or boundary conditions.

My takeaway is that test data should make tests clearer, not make failures harder to understand.

#SimulationAcademy #DotNet #CSharp #UnitTesting

---

## 3. TDD

Working on the ParkSmart part of my Unit Testing assignment gave me a better understanding of Test-Driven Development (TDD).

Before this experience, I was more used to writing the implementation first and then adding tests to check whether it worked. TDD changes that order.

The process is based on three steps:

**Red:** Write a test for a behavior that is not implemented yet, then run it and make sure it fails.

**Green:** Write the minimum production code needed to make that test pass.

**Refactor:** Improve the code structure without changing its behavior, while keeping all tests passing.

For example, when implementing parking fee calculation, I needed to consider business rules such as the free parking period, hourly fees, and the daily fee cap.

A test for the daily cap could look like this:

```csharp
var fee = calculator.Calculate(
    entryTime,
    exitTime,
    VehicleType.Car);

fee.Should().Be(80m);
```

The important part is not just writing this assertion. It is first proving that the test fails for the expected reason, then implementing the behavior and running the tests again.

The hardest part for me was thinking about the expected behavior and edge cases before writing the implementation. It requires more discipline than simply coding the solution first.

TDD helped me think about requirements as observable behaviors instead of jumping directly into implementation details.

#SimulationAcademy #DotNet #CSharp #UnitTesting

---

## 4. Coverage

Code coverage is one of the things I explored during Task 9 of my Unit Testing assignment.

I used Coverlet to measure how much of my application code was executed by the tests. But I learned that coverage has different measurements, and understanding the difference is important.

**Line Coverage** measures the percentage of executable code lines that were executed during testing.

**Branch Coverage** measures how many decision branches were executed, such as the true and false paths of an if statement.

For example:

```csharp
if (parkingDuration > freeMinutes)
{
    return CalculateFee(parkingDuration);
}

return 0m;
```

Executing the method does not automatically mean both branches were tested. I need tests for a duration greater than the free period and another duration within the free period.

In my Task 9 results, I got:

- ParkSmart.Application: 95.52% Line Coverage and 91.3% Branch Coverage.
- ParkSmart.Domain: 100% Line Coverage and 100% Branch Coverage.

These numbers helped me identify areas that still needed attention.

However, 100% coverage does not mean the code is correct. A test can execute every line and still have incorrect assertions, miss important business scenarios, or fail to detect a logical bug.

For me, coverage is a useful measurement tool, not proof of quality. The real value comes from writing meaningful tests that verify the expected behavior.

#SimulationAcademy #DotNet #CSharp #UnitTesting
