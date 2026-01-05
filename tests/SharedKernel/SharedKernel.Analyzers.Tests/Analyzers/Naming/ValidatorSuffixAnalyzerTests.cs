using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class ValidatorSuffixAnalyzerTests : SuffixAnalyzerTestBase<ValidatorSuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME002;

    [Fact]
    public async Task ValidatorWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = """
            public abstract class AbstractValidator<T> { }

            public class CreateUserCommand { }

            public class CreateUserValidator : AbstractValidator<CreateUserCommand>
            {
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task ValidatorWithoutSuffix_ReportsDiagnosticAsync()
    {
        string source = """
            public abstract class AbstractValidator<T> { }

            public class CreateUserCommand { }

            public class {|#0:CreateUserRules|} : AbstractValidator<CreateUserCommand>
            {
            }
            """;

        await VerifyDiagnosticAsync(source, "CreateUserRules");
    }

    [Fact]
    public async Task ClassNotExtendingAbstractValidator_NoDiagnosticAsync()
    {
        string source = """
            public class CreateUserRules
            {
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }
}
