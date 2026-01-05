using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class ServiceSuffixAnalyzerTests : SuffixAnalyzerTestBase<ServiceSuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME005;

    [Fact]
    public async Task ServiceWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Application
            {
                public class EmailService
                {
                    public void Send() { }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task ManagerClass_InApplicationNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Application
            {
                public class {|#0:UserManager|}
                {
                    public void CreateUser() { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "UserManager");
    }

    [Fact]
    public async Task ProcessorClass_InDomainNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public class {|#0:OrderProcessor|}
                {
                    public void Process() { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "OrderProcessor");
    }

    [Fact]
    public async Task CoordinatorClass_InServicesNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Services
            {
                public class {|#0:WorkflowCoordinator|}
                {
                    public void Coordinate() { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "WorkflowCoordinator");
    }

    [Fact]
    public async Task ProviderClass_InApplicationNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Application
            {
                public class {|#0:DataProvider|}
                {
                    public object GetData() => new();
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "DataProvider");
    }

    [Fact]
    public async Task ExecutorClass_InDomainNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public class {|#0:TaskExecutor|}
                {
                    public void Execute() { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "TaskExecutor");
    }

    [Fact]
    public async Task DispatcherClass_InServicesNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Services
            {
                public class {|#0:EventDispatcher|}
                {
                    public void Dispatch() { }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "EventDispatcher");
    }

    [Fact]
    public async Task ManagerClass_NotInServiceNamespace_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Infrastructure
            {
                public class UserManager
                {
                    public void CreateUser() { }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task AbstractManagerClass_InDomainNamespace_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public abstract class BaseManager
                {
                    public abstract void Manage();
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task RegularClass_InDomainNamespace_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public class User
                {
                    public string Name { get; set; }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }
}
