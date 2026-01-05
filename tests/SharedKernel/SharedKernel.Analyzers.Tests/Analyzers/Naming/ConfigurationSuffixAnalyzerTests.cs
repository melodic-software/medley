using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class ConfigurationSuffixAnalyzerTests : SuffixAnalyzerTestBase<ConfigurationSuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME006;

    private const string EfCoreNamespace = """
        namespace Microsoft.EntityFrameworkCore
        {
            public class EntityTypeBuilder<T> where T : class { }
            public interface IEntityTypeConfiguration<T> where T : class
            {
                void Configure(EntityTypeBuilder<T> builder);
            }
        }
        """;

    [Fact]
    public async Task ConfigurationWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = $$"""
            {{EfCoreNamespace}}

            namespace MyApp.Infrastructure
            {
                using Microsoft.EntityFrameworkCore;

                public class User { }

                public class UserConfiguration : IEntityTypeConfiguration<User>
                {
                    public void Configure(EntityTypeBuilder<User> builder) { }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task ConfigurationWithoutSuffix_ReportsDiagnosticAsync()
    {
        string source = $$"""
            {{EfCoreNamespace}}

            namespace MyApp.Infrastructure
            {
                using Microsoft.EntityFrameworkCore;

                public class User { }

                public class {|#0:UserEntityConfig|} : IEntityTypeConfiguration<User>
                {
                    public void Configure(EntityTypeBuilder<User> builder) { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "UserEntityConfig");
    }

    [Fact]
    public async Task ConfigurationWithCfgSuffix_ReportsDiagnosticAsync()
    {
        string source = $$"""
            {{EfCoreNamespace}}

            namespace MyApp.Infrastructure
            {
                using Microsoft.EntityFrameworkCore;

                public class Order { }

                public class {|#0:OrderCfg|} : IEntityTypeConfiguration<Order>
                {
                    public void Configure(EntityTypeBuilder<Order> builder) { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "OrderCfg");
    }

    [Fact]
    public async Task ClassNotImplementingConfiguration_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Infrastructure
            {
                public class UserConfig
                {
                    public string Setting { get; set; }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task AbstractConfiguration_NoDiagnosticAsync()
    {
        string source = $$"""
            {{EfCoreNamespace}}

            namespace MyApp.Infrastructure
            {
                using Microsoft.EntityFrameworkCore;

                public class BaseEntity { }

                public abstract class BaseConfig : IEntityTypeConfiguration<BaseEntity>
                {
                    public abstract void Configure(EntityTypeBuilder<BaseEntity> builder);
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task MultipleConfigurations_ReportsAllDiagnosticsAsync()
    {
        string source = $$"""
            {{EfCoreNamespace}}

            namespace MyApp.Infrastructure
            {
                using Microsoft.EntityFrameworkCore;

                public class User { }
                public class Order { }

                public class {|#0:UserMap|} : IEntityTypeConfiguration<User>
                {
                    public void Configure(EntityTypeBuilder<User> builder) { }
                }

                public class {|#1:OrderMap|} : IEntityTypeConfiguration<Order>
                {
                    public void Configure(EntityTypeBuilder<Order> builder) { }
                }
            }
            """;

        await VerifyMultipleDiagnosticsAsync(source, "UserMap", "OrderMap");
    }
}
