using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SharedKernel.Analyzers.Analyzers.Architecture;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Architecture;

/// <summary>
/// Tests for <see cref="CrossModuleReferenceAnalyzer"/>.
/// </summary>
public class CrossModuleReferenceAnalyzerTests
{
    [Fact]
    public async Task EmptySource_NoDiagnosticAsync()
    {
        // Arrange
        string source = "";

        // Act & Assert - no diagnostics expected for empty source
        await CSharpAnalyzerVerifier<CrossModuleReferenceAnalyzer>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SimpleClass_NoDiagnosticAsync()
    {
        // Arrange
        string source = """
            namespace Test;

            public class SimpleClass
            {
                public void DoSomething() { }
            }
            """;

        // Act & Assert - no diagnostics expected for simple class
        await CSharpAnalyzerVerifier<CrossModuleReferenceAnalyzer>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SameModuleReference_NoDiagnosticAsync()
    {
        // Arrange - Code within Users module referencing Users.Domain type (same module)
        string source = """
            // Stub type in Users.Domain (same module)
            namespace Modules.Users.Domain
            {
                public class UserEntity { }
            }

            // Type in Users.Application that references it
            namespace Modules.Users.Application.Features
            {
                public class UserService
                {
                    private readonly Modules.Users.Domain.UserEntity _user = null!;
                }
            }
            """;

        // Act & Assert - Same module references are allowed
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source);
    }

    [Fact]
    public async Task ContractsReference_NoDiagnosticAsync()
    {
        // Arrange - Referencing another module's Contracts is allowed
        string source = """
            // Stub type in Orders.Contracts (allowed cross-module reference)
            namespace Modules.Orders.Contracts
            {
                public class OrderDto { }
            }

            // Type in Users.Application that references Orders.Contracts
            namespace Modules.Users.Application.Features
            {
                public class UserService
                {
                    private readonly Modules.Orders.Contracts.OrderDto _order = null!;
                }
            }
            """;

        // Act & Assert - Contracts references are allowed
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source);
    }

    [Fact]
    public async Task SharedKernelReference_NoDiagnosticAsync()
    {
        // Arrange - Referencing SharedKernel is always allowed
        string source = """
            // Stub type in SharedKernel
            namespace SharedKernel
            {
                public class Entity { }
            }

            // Type in Users.Application that references SharedKernel
            namespace Modules.Users.Application.Features
            {
                public class UserService
                {
                    private readonly SharedKernel.Entity _entity = null!;
                }
            }
            """;

        // Act & Assert - SharedKernel references are allowed
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source);
    }

    [Fact]
    public async Task CrossModuleDomainReference_ReportsDiagnosticAsync()
    {
        // Arrange - Users module directly referencing Orders.Domain (violation)
        string source = """
            // Stub type in Orders.Domain (cross-module internal reference)
            namespace Modules.Orders.Domain
            {
                public class OrderEntity { }
            }

            // Type in Users.Application that references Orders.Domain
            namespace Modules.Users.Application.Features
            {
                public class {|#0:UserService|}
                {
                    private readonly Modules.Orders.Domain.OrderEntity _order = null!;
                }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticIds.MDYARCH001, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("Users", "Orders.Domain");

        // Act & Assert
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source, expected);
    }

    [Fact]
    public async Task CrossModuleApplicationReference_ReportsDiagnosticAsync()
    {
        // Arrange - Users module referencing Orders.Application (violation)
        string source = """
            // Stub type in Orders.Application (cross-module internal reference)
            namespace Modules.Orders.Application
            {
                public class OrderService { }
            }

            // Type in Users.Application that references Orders.Application
            namespace Modules.Users.Application.Features
            {
                public class {|#0:UserService|}
                {
                    private readonly Modules.Orders.Application.OrderService _service = null!;
                }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticIds.MDYARCH001, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("Users", "Orders.Application");

        // Act & Assert
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source, expected);
    }

    [Fact]
    public async Task CrossModuleInfrastructureReference_ReportsDiagnosticAsync()
    {
        // Arrange - Users module referencing Orders.Infrastructure (violation)
        string source = """
            // Stub type in Orders.Infrastructure (cross-module internal reference)
            namespace Modules.Orders.Infrastructure
            {
                public class OrderRepository { }
            }

            // Type in Users.Application that references Orders.Infrastructure
            namespace Modules.Users.Application.Features
            {
                public class {|#0:UserService|}
                {
                    private readonly Modules.Orders.Infrastructure.OrderRepository _repo = null!;
                }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticIds.MDYARCH001, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("Users", "Orders.Infrastructure");

        // Act & Assert
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source, expected);
    }

    [Fact]
    public async Task NonModuleAssembly_NoDiagnosticAsync()
    {
        // Arrange - Code not in a module assembly should not trigger diagnostics
        string source = """
            // Stub type in Orders.Domain
            namespace Modules.Orders.Domain
            {
                public class OrderEntity { }
            }

            // Non-module code that references module types - not analyzed
            namespace SomeOtherNamespace
            {
                public class SomeClass
                {
                    private readonly Modules.Orders.Domain.OrderEntity _order = null!;
                }
            }
            """;

        // Act & Assert - Non-module assemblies are not analyzed
        await TestWithAssemblyName("SomeOtherProject")
            .VerifyAsync(source);
    }

    [Fact]
    public async Task CrossModuleBaseClass_ReportsDiagnosticAsync()
    {
        // Arrange - Users module inheriting from Orders.Domain type (violation)
        string source = """
            // Stub base class in Orders.Domain
            namespace Modules.Orders.Domain
            {
                public class OrderBase { }
            }

            // Type in Users.Application that inherits from Orders.Domain
            namespace Modules.Users.Application.Features
            {
                public class {|#0:UserHandler|} : Modules.Orders.Domain.OrderBase { }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticIds.MDYARCH001, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("Users", "Orders.Domain");

        // Act & Assert
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source, expected);
    }

    [Fact]
    public async Task CrossModuleInterface_ReportsDiagnosticAsync()
    {
        // Arrange - Users module implementing interface from Orders.Domain (violation)
        string source = """
            // Stub interface in Orders.Domain
            namespace Modules.Orders.Domain
            {
                public interface IOrderEntity { }
            }

            // Type in Users.Application that implements Orders.Domain interface
            namespace Modules.Users.Application.Features
            {
                public class {|#0:UserService|} : Modules.Orders.Domain.IOrderEntity { }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticIds.MDYARCH001, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("Users", "Orders.Domain");

        // Act & Assert
        await TestWithAssemblyName("Modules.Users.Application")
            .VerifyAsync(source, expected);
    }

    private static AnalyzerTestWithAssembly TestWithAssemblyName(string assemblyName) =>
        new(assemblyName);

    private sealed class AnalyzerTestWithAssembly(string assemblyName)
        : CSharpAnalyzerTest<CrossModuleReferenceAnalyzer, ShouldlyVerifier>
    {
        protected override string DefaultTestProjectName => assemblyName;

        protected override CompilationOptions CreateCompilationOptions()
        {
            var options = base.CreateCompilationOptions();
            return options.WithModuleName(assemblyName);
        }

        protected override ParseOptions CreateParseOptions() =>
            VerifierConfiguration.CreateParseOptions();

        public async Task VerifyAsync(string source, params DiagnosticResult[] expected)
        {
            TestCode = source;
            ExpectedDiagnostics.AddRange(expected);
            ReferenceAssemblies = VerifierConfiguration.ReferenceAssemblies;
            // Ignore compiler errors for namespace references that don't exist in test context
            CompilerDiagnostics = CompilerDiagnostics.None;
            await RunAsync();
        }
    }
}
