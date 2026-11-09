
using AutoFixture.Xunit2;
using DriveNow.UnitTests.Customizations;

namespace DriveNow.UnitTests.Attributes;

public sealed class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new AutoMoqDataAttribute(), values)
    {
    }
}
