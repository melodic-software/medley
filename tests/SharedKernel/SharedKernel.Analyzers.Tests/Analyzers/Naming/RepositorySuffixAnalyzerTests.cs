using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class RepositorySuffixAnalyzerTests : SuffixAnalyzerTestBase<RepositorySuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME001;

    [Fact]
    public async Task RepositoryWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = """
            public interface IRepository<T> { }

            public class UserRepository : IRepository<User>
            {
            }

            public class User { }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task RepositoryWithoutSuffix_ReportsDiagnosticAsync()
    {
        string source = """
            public interface IRepository<T> { }

            public class {|#0:UserStore|} : IRepository<User>
            {
            }

            public class User { }
            """;

        await VerifyDiagnosticAsync(source, "UserStore");
    }

    [Fact]
    public async Task InterfaceImplementingRepository_NoDiagnosticAsync()
    {
        string source = """
            public interface IRepository<T> { }

            public interface IUserRepository : IRepository<User>
            {
            }

            public class User { }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task AbstractRepositoryClass_NoDiagnosticAsync()
    {
        string source = """
            public interface IRepository<T> { }

            public abstract class BaseStore : IRepository<object>
            {
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task ClassNotImplementingRepository_NoDiagnosticAsync()
    {
        string source = """
            public class UserStore
            {
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }
}
