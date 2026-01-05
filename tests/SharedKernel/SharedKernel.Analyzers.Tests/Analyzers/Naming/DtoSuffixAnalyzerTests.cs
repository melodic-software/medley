using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class DtoSuffixAnalyzerTests : SuffixAnalyzerTestBase<DtoSuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME007;

    [Fact]
    public async Task DtoWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public class UserResponseDto
                {
                    public string Name { get; set; }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task DtoWithUppercaseDTOSuffix_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public class UserResponseDTO
                {
                    public string Name { get; set; }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task ResponseClass_InContractsNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public class {|#0:UserResponse|}
                {
                    public string Name { get; set; }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "UserResponse");
    }

    [Fact]
    public async Task RequestClass_InContractsNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public class {|#0:CreateUserRequest|}
                {
                    public string Email { get; set; }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "CreateUserRequest");
    }

    [Fact]
    public async Task ModelClass_InContractsNamespace_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public class {|#0:UserModel|}
                {
                    public string Name { get; set; }
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "UserModel");
    }

    [Fact]
    public async Task ResponseClass_NotInContractsNamespace_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Application
            {
                public class UserResponse
                {
                    public string Name { get; set; }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task RecordInContracts_WithDtoLikeSuffix_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public record {|#0:UserDetails|}(string Name, string Email);
            }
            """;

        await VerifyDiagnosticAsync(source, "UserDetails");
    }

    [Fact]
    public async Task AbstractClass_InContracts_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public abstract class BaseResponse
                {
                    public string Message { get; set; }
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task RegularClass_InContracts_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Contracts
            {
                public class Constants
                {
                    public const string Version = "1.0";
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }
}
