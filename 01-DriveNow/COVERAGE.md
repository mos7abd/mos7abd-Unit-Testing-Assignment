# Task 9 — Code Coverage

## 1. Coverage Measurement

Coverage was measured using Coverlet (`coverlet.collector`) and ReportGenerator.

The following production projects are included:
- DriveNow.Application
- DriveNow.Domain
- DriveNow.Infrastructure

`DriveNow.ConsoleApp` is excluded using `coverage.runsettings`.

## 2. Coverage Progress

| Stage | Project | Line Coverage | Branch Coverage |
|---|---|---:|---:|
| Baseline* | DriveNow.Application | 8.47% | 3.22% |
| Baseline* | DriveNow.Domain | 3.50% | 0% |
| Baseline* | DriveNow.Infrastructure | 0% | 0% |
| Task 1 | DriveNow.Application | 8.48% | 3.23% |
| Task 1 | DriveNow.Domain | 24.56% | 66.67% |
| Task 1 | DriveNow.Infrastructure | 0% | 0% |
| Task 5 | DriveNow.Application | 30.09% | 28.26% |
| Task 5 | DriveNow.Domain | 55.56% | 66.67% |
| Task 5 | DriveNow.Infrastructure | 0% | 0% |
| Task 6 | DriveNow.Application | 54.87% | 57.07% |
| Task 6 | DriveNow.Domain | 76.07% | 66.67% |
| Task 6 | DriveNow.Infrastructure | 0% | 0% |
| Task 7 | DriveNow.Application | 99.12% | 93.48% |
| Task 7 | DriveNow.Domain | 97.46% | 100% |
| Task 7 | DriveNow.Infrastructure | 0% | 0% |
| Task 8 | DriveNow.Application | 99.13% | 93.48% |
| Task 8 | DriveNow.Domain | 97.46% | 100% |
| Task 8 | DriveNow.Infrastructure | 0% | 0% |

\* **Baseline note:** The available Git history does not contain a verifiable snapshot from immediately before Task 1. The baseline therefore uses the earliest available legacy-only snapshot (`7fcc515`). It should not be interpreted as an exact pre-Task-1 measurement.

## 3. Exclusions

Only `DriveNow.ConsoleApp` is excluded through the Coverlet filter:

`[DriveNow.ConsoleApp]*`

The console application is the interactive presentation and composition layer. The coverage assessment focuses on the application, domain, and infrastructure projects.

No other production project was excluded to improve the reported percentages. `DriveNow.Infrastructure` is included, and its measured coverage is reported as 0%.

## 4. Coverage Target

The required targets are:
- Line coverage: at least 90%.
- Branch coverage: at least 85%.

After Task 8:

| Project | Line Coverage | Branch Coverage | Result |
|---|---:|---:|---|
| DriveNow.Application | 99.13% | 93.48% | Meets target |
| DriveNow.Domain | 97.46% | 100% | Meets target |

Both required projects meet the coverage targets.

## 5. R5 — 100% Covered Line with a Bug

The `PricingService` duration-discount calculation contained a boundary bug.

The original condition used `days > 7`, which incorrectly excluded rentals of exactly 7 days from the 10% duration discount.

The condition was corrected to `days >= 7`.

This demonstrates that code coverage alone does not guarantee correctness. A line can execute successfully while still producing an incorrect result if the test assertions do not verify the relevant business boundary.

## 6. R6 — Uncovered Branch Intentionally Not Tested

Some defensive branches are intentionally left uncovered when they represent exceptional or unlikely scenarios that do not add meaningful value to the current business-flow tests.

Testing effort was prioritized toward the main business rules, boundary conditions, and expected application behavior rather than pursuing 100% coverage at the expense of meaningful tests.