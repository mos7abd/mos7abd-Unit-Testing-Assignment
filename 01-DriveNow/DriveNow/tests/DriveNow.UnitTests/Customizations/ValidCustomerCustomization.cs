
using AutoFixture;
using DriveNow.Domain.Entities;

namespace DriveNow.UnitTests.Customizations;

public class ValidCustomerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Customer>(composer => composer
            .With(x => x.DateOfBirth, new DateTime(1990, 1, 1))
            .With(x => x.LicenseIssueDate, new DateTime(2015, 1, 1))
            .With(x => x.IsBlacklisted, false));
    }
}
