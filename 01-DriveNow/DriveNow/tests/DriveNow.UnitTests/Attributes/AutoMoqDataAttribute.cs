
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using DriveNow.UnitTests.Customizations;

namespace DriveNow.UnitTests.Attributes;

public sealed class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture();

            fixture.Customize(
                new AutoMoqCustomization { ConfigureMembers = true });

            fixture.Customize(new ValidCustomerCustomization());

            return fixture;
        })
    {
    }
}
