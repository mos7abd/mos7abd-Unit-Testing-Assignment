# RentalMath — Test Review

## 1. Business Rules Coverage

The `RentalMathTests` class contains **22 test cases** covering the three business rules.

| Business Rule | Method | Test Cases | Coverage |
|---|---|---:|---|
| BR-01 | `Should_CalculateAgeCorrectly` | 5 | Before, on, and after the birthday; February 29 in a non-leap year |
| BR-01 | `Should_HandleFebruary29Birthday_InLeapYears` | 1 | February 29 in a leap year |
| BR-02 | `Should_ReturnCorrectLicenseYears_WhenCheckingDateBoundaries` | 5 | Before, on, and after the license anniversary; February 29 in a non-leap year |
| BR-02 | `Should_HandleFebruary29Anniversary_InLeapYears` | 1 | February 29 in a leap year |
| BR-03 | `Should_ReturnCorrectRentalDays_WhenCheckingDurationBoundaries` | 5 | 3 hours, 24 hours and 59 minutes, 25 hours, 25 hours and 1 minute, and 72 hours |
| BR-03 | `Should_ThrowArgumentException_WhenReturnAtEqualsPickup` | 1 | Return time equals pickup time |
| BR-03 | `Should_ThrowArgumentException_WhenReturnAtIsBeforePickup` | 1 | Return time is before pickup time |
| BR-03 | `Should_ReturnAtLeastOneDay_WhenRentalDurationIsLessThan24Hours` | 1 | Minimum one rental day |
| BR-03 | `Should_ReturnOneDay_WhenRentalDurationIsExactly24Hours` | 1 | Exactly 24 hours |
| BR-03 | `Should_AddExtraDay_WhenDurationIs48HoursAnd61Minutes` | 1 | 48 hours and 61 minutes |

### Boundary Coverage

- **BR-01:** Tests the birthday boundary before, on, and after the birthday, including February 29 in leap and non-leap years.
- **BR-02:** Tests the license anniversary boundary and February 29 in leap and non-leap years.
- **BR-03:** Tests durations below 24 hours, exactly 24 hours, and around the additional-day threshold:
  - 24 hours and 59 minutes → 1 day.
  - 25 hours → 1 day.
  - 25 hours and 1 minute → 2 days.
  - 48 hours and 61 minutes → 3 days.
  - 72 hours → 3 days.
  - Return time equal to or before pickup → `ArgumentException`.

## 2. AI Review

The test names and business rules were reviewed using AI without inspecting the production implementation.

### AI Review Findings

The review identified some areas worth checking:

1. Add leap-year cases for February 29 birthdays and license anniversaries.
2. Test rental durations below 24 hours.
3. Test exactly 24 hours.
4. Test 48 hours and 61 minutes.
5. Make sure the 60-minute and 61-minute boundaries are covered.
6. Improve test names where they do not clearly describe the scenario.

### Accepted Suggestions

The following cases were added:

| Suggestion | Decision | Reason |
|---|---|---|
| February 29 birthday in a leap year | Accepted | Covers the leap-year boundary for BR-01. |
| February 29 license anniversary in a leap year | Accepted | Covers the leap-year boundary for BR-02. |
| Rental duration below 24 hours | Accepted | Verifies the minimum one-day rule. |
| Exactly 24 hours | Accepted | Verifies the full 24-hour boundary. |
| 48 hours and 61 minutes | Accepted | Verifies the additional-day rule after multiple full days. |

### Suggestions Already Covered

The 60-minute and 61-minute boundaries were already covered by these cases:

- 25 hours → 1 day.
- 25 hours and 1 minute → 2 days.

Therefore, duplicate tests were not added.

### Suggestions Not Adopted

The review suggested making some test names more descriptive. The existing names were retained for this iteration rather than renaming every test. However, `Should_CalculateAgeCorrectly` could be more specific because it does not explicitly mention the date boundaries.

## 3. Test Execution Results

After adding the accepted test cases, all tests were executed successfully.

| Result | Count |
|---|---:|
| Passed | 22 |
| Failed | 0 |
| Total | 22 |

## 4. Bug Found During Testing

One test exposed an issue in `CalculateRentalDays`.

The business rule says that an additional rental day is counted only when the remaining duration is **strictly greater than 60 minutes**.

The original implementation used:

```csharp
if (remainder.TotalMinutes >= 60)
    days++;
```

This caused the 25-hour test to fail:

- 24 full hours = 1 day.
- Remaining time = exactly 60 minutes.
- Expected result = 1 day.
- Actual result before the fix = 2 days.

The condition was corrected to:

```csharp
if (remainder.TotalMinutes > 60)
    days++;
```

After this correction, the boundary test passed.

## 5. Final Assessment

The final test suite contains 22 passing test cases.

The tests cover:

- Full-year age and license calculations.
- February 29 behavior in leap and non-leap years.
- Rental duration boundaries.
- The minimum rental day.
- Invalid return times.
- The strict 60-minute additional-day condition.

The tests also helped identify and correct a boundary-condition bug in `CalculateRentalDays`.

**Note:** AI assistance was used during test drafting and review. The review was limited to business rules and test names, not the production implementation.