## R1 — Expected Test Protection (Before Running)

Before running the legacy tests, I reviewed the five tests and identified what I expected each one to protect:

1. **Build_CallsLineItemFactoryOnce**
   - Expected to protect against calling `ILineItemFactory.Create` more than once during a receipt build.

2. **Build_CallsTaxCalculatorOncePerLine**
   - Expected to protect against calculating VAT an incorrect number of times. It expects one VAT calculation for each of the three line items.

3. **Build_CallsCurrencyFormatterTwicePerLinePlusTwo**
   - Expected to protect the currency formatter's invocation count. It expects exactly eight calls for three line items.

4. **Build_CallsCollaboratorsInOrder**
   - Expected to protect the execution order of collaborators: line-item factory, tax calculator, currency formatter, and receipt formatter.

5. **Build_ReturnsWhatTheRendererReturns**
   - Expected to protect the delegation to `IReceiptFormatter.Render` and ensure that `Build` returns the renderer's result.

These are initial expectations based on the test names, assertions, and mock setups. I will compare them with the actual behavior after running the legacy tests.
