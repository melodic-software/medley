using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class HandlerSuffixAnalyzerTests : SuffixAnalyzerTestBase<HandlerSuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME003;

    [Fact]
    public async Task HandlerWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = """
            public interface IRequest<TResponse> { }

            public interface IRequestHandler<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
            {
            }

            public class CreateUserCommand : IRequest<int> { }

            public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
            {
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task HandlerWithoutSuffix_ReportsDiagnosticAsync()
    {
        string source = """
            public interface IRequest<TResponse> { }

            public interface IRequestHandler<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
            {
            }

            public class CreateUserCommand : IRequest<int> { }

            public class {|#0:CreateUser|}  : IRequestHandler<CreateUserCommand, int>
            {
            }
            """;

        await VerifyDiagnosticAsync(source, "CreateUser");
    }

    [Fact]
    public async Task ClassNotImplementingHandler_NoDiagnosticAsync()
    {
        string source = """
            public class CreateUser
            {
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }
}
