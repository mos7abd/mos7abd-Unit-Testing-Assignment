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

## R4 — What the Legacy Tests Actually Checked

After running the legacy tests, I found that they mainly verified implementation details and interactions between collaborators rather than the correctness of the generated receipt.

1. **Factory invocation:** Verified that `ILineItemFactory.Create` was called exactly once.
2. **VAT invocation count:** Verified that `ITaxCalculator.CalculateVat` was called exactly three times for three line items. It did not verify the calculated VAT amounts.
3. **Currency formatter invocation count:** Expected exactly eight calls. After the refactoring, the actual number became five because each amount is now formatted only once.
4. **Collaborator execution order:** Used `Moq.MockSequence` to enforce a specific sequence of calls. The test failed because its expected sequence still required eight currency formatting calls before rendering.
5. **Renderer delegation:** Verified that `Build` returned the value produced by `IReceiptFormatter.Render` and that the renderer was called once.

### Why Did a Behavior-Preserving Change Break the Tests?

The refactoring changed the internal implementation of `ReceiptService.Build` without changing the generated receipt text.

The old tests were tightly coupled to the number and order of collaborator calls. They treated those implementation details as requirements. As a result, a valid optimization caused two tests to fail even though the receipt remained identical.

### Would These Tests Catch an Incorrect VAT Amount?

No. The VAT mock was configured to return `0m` for every input, and the tests only verified the number of calls. They never asserted the actual VAT amount or compared the final receipt against an expected receipt.

Therefore, an incorrect VAT calculation could go unnoticed.

### Interfaces Used by the Legacy Tests

The four interfaces involved were:

- `ILineItemFactory`
- `ITaxCalculator`
- `ICurrencyFormatter`
- `IReceiptFormatter`

Their necessity should be evaluated by checking their real implementations and usage across the application, not merely by whether they make mocking possible. This will be addressed in R7.