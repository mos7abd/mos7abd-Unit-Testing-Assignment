
using DriveNow.Application.Receipts;
using DriveNow.Domain.Entities;
using DriveNow.Domain.ValueObjects;

namespace DriveNow.Application.Services;

public class ReceiptService
{
    private readonly LineItemFactory _lineItems;
    private readonly TaxCalculator _tax;
    private readonly CurrencyFormatter _currency;
    private readonly ReceiptFormatter _receipts;

    public ReceiptService(
        LineItemFactory lineItems,
        TaxCalculator tax,
        CurrencyFormatter currency,
        ReceiptFormatter receipts)
    {
        _lineItems = lineItems;
        _tax = tax;
        _currency = currency;
        _receipts = receipts;
    }

    public string Build(Booking booking, ReturnReport? report)
    {
        var items = _lineItems.Create(booking, report);

        decimal vat = 0m;
        decimal net = 0m;

        foreach (var item in items)
        {
            vat += _tax.CalculateVat(item.Amount);
            net += item.Amount;
        }

        var total = net + vat;

        var formattedAmounts = new List<string>();
        var width = 0;

        foreach (var item in items)
        {
            var amount = _currency.Format(item.Amount);

            formattedAmounts.Add(amount);
            width = Math.Max(width, amount.Length);
        }

        var lines = new List<string>();

        for (var i = 0; i < items.Count; i++)
        {
            var amount = formattedAmounts[i];

            lines.Add(
                items[i].Description.PadRight(30, '.') +
                amount.PadLeft(width));
        }

        lines.Add("VAT 14%".PadRight(30, '.') + _currency.Format(vat).PadLeft(width));
        lines.Add("Total".PadRight(30, '.') + _currency.Format(total).PadLeft(width));

        return _receipts.Render(booking.Reference, lines);
    }
}
