using NetArchTest.Rules;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace DriveNow.ArchitectureTests
{
    public class ArchitectureTests
    {
        //R1
        [Fact]
        public void Should_NotDependOnOtherDriveNowProjects_WhenCheckingDomain()
        {
            var result = Types
                .InAssembly(typeof(DriveNow.Domain.Entities.Customer).Assembly)
                .ShouldNot()
                .HaveDependencyOn("DriveNow.Application")
                .And()
                .NotHaveDependencyOn("DriveNow.Infrastructure")
                .And()
                .NotHaveDependencyOn("DriveNow.ConsoleApp")
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        //R2
        [Fact]
        public void Should_OnlyDependOnDomain_WhenCheckingApplication()
        {
            var result = Types
                .InAssembly(typeof(DriveNow.Application.Services.BookingService).Assembly)
                .ShouldNot()
                .HaveDependencyOn("DriveNow.Infrastructure")
                .And()
                .NotHaveDependencyOn("DriveNow.ConsoleApp")
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                string.Join(", ", result.FailingTypes.Select(t => t.FullName))
            );
        }
        //R3
        [Fact]
        public void Should_NotDependOnConsoleApp_WhenCheckingInfrastructure()
        {
            var result = Types
                .InAssembly(typeof(DriveNow.Infrastructure.Repositories.BookingRepository).Assembly)
                .ShouldNot()
                .HaveDependencyOn("DriveNow.ConsoleApp")
                .GetResult();

            Assert.True(result.IsSuccessful);
        }
        //R4
        [Fact]
        public void Should_FollowProductionNamingConventions_WhenCheckingTypes()
        {
            var assemblies = new[]
            {
        typeof(DriveNow.Domain.Entities.Customer).Assembly,
        typeof(DriveNow.Application.Services.BookingService).Assembly,
        typeof(DriveNow.Infrastructure.Repositories.BookingRepository).Assembly
    };

            var types = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass || type.IsInterface)
                .ToList();

            var failures = new List<string>();

            foreach (var type in types)
            {
                var name = type.Name;
                var ns = type.Namespace ?? "";

                if (type.IsInterface && !name.StartsWith("I"))
                    failures.Add($"{type.FullName}: Interface must start with I");

                if (name.EndsWith("Repo") ||
     name.Contains("Repository"))
                {
                    if (!name.EndsWith("Repository"))
                    {
                        failures.Add($"{type.FullName}: Repository must end with Repository");
                    }

                    if (!ns.EndsWith(".Repositories"))
                    {
                        failures.Add($"{type.FullName}: Repository must be in a Repositories namespace");
                    }
                }

                if (name.EndsWith("Service") && !ns.EndsWith(".Services"))
                    failures.Add($"{type.FullName}: Service must be in a Services namespace");

                if (typeof(Exception).IsAssignableFrom(type) &&
                    type != typeof(Exception) &&
                    (!name.EndsWith("Exception") || ns != "DriveNow.Domain.Exceptions"))
                    failures.Add($"{type.FullName}: Exception naming or namespace is invalid");
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }
        // R5
        [Fact]
        public void Should_FollowTestNamingConventions_WhenCheckingUnitTests()
        {
            var assembly = Assembly.Load("DriveNow.UnitTests");

            var testClasses = assembly.GetTypes()
                .Where(type => type.IsClass &&
                    type.GetMethods(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static
                    ).Any(method => method.GetCustomAttributes(false)
                        .Any(attribute =>
                            attribute.GetType().FullName == "Xunit.FactAttribute" ||
                            attribute.GetType().FullName == "Xunit.TheoryAttribute"
                        )
                    ))
                .ToList();

            var failures = new List<string>();

            var methodPattern = new Regex(
                    @"^Should_[A-Z][a-zA-Z0-9]*_When_[A-Z][a-zA-Z0-9]*$"
                );

            foreach (var testClass in testClasses)
            {
                if (!testClass.Name.EndsWith("Tests"))
                    failures.Add($"{testClass.FullName}: Test class must end with Tests");

                var testMethods = testClass.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Static
                );

                foreach (var method in testMethods)
                {
                    var isTestMethod = method.GetCustomAttributes(false)
                        .Any(attribute =>
                            attribute.GetType().FullName == "Xunit.FactAttribute" ||
                            attribute.GetType().FullName == "Xunit.TheoryAttribute"
                        );

                    if (isTestMethod && !methodPattern.IsMatch(method.Name))
                        failures.Add($"{testClass.FullName}.{method.Name}: Invalid test method name");
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }
    }
}