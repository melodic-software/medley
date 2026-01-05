using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.CodeFixes.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.CodeFixes.Naming;

public class AddSuffixCodeFixProviderTests
{
    [Fact]
    public async Task RepositoryWithoutSuffix_AddsRepositorySuffixAsync()
    {
        string source = """
            public interface IRepository<T> { }

            public class {|#0:UserStore|} : IRepository<User>
            {
            }

            public class User { }
            """;

        string fixedSource = """
            public interface IRepository<T> { }

            public class UserStoreRepository : IRepository<User>
            {
            }

            public class User { }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<RepositorySuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME001)
                .WithLocation(0)
                .WithArguments("UserStore");

        await CSharpCodeFixVerifier<RepositorySuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task ValidatorWithoutSuffix_AddsValidatorSuffixAsync()
    {
        string source = """
            public abstract class AbstractValidator<T> { }

            public class CreateUserCommand { }

            public class {|#0:CreateUserRules|} : AbstractValidator<CreateUserCommand>
            {
            }
            """;

        string fixedSource = """
            public abstract class AbstractValidator<T> { }

            public class CreateUserCommand { }

            public class CreateUserRulesValidator : AbstractValidator<CreateUserCommand>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<ValidatorSuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME002)
                .WithLocation(0)
                .WithArguments("CreateUserRules");

        await CSharpCodeFixVerifier<ValidatorSuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task HandlerWithoutSuffix_AddsHandlerSuffixAsync()
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

        string fixedSource = """
            public interface IRequest<TResponse> { }

            public interface IRequestHandler<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
            {
            }

            public class CreateUserCommand : IRequest<int> { }

            public class CreateUserHandler  : IRequestHandler<CreateUserCommand, int>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<HandlerSuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME003)
                .WithLocation(0)
                .WithArguments("CreateUser");

        await CSharpCodeFixVerifier<HandlerSuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task SpecificationWithoutSuffix_AddsSpecificationSuffixAsync()
    {
        string source = """
            public abstract class Specification<T> { }

            public class User { }

            public class {|#0:ActiveUsers|} : Specification<User>
            {
            }
            """;

        string fixedSource = """
            public abstract class Specification<T> { }

            public class User { }

            public class ActiveUsersSpecification : Specification<User>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<SpecificationSuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME004)
                .WithLocation(0)
                .WithArguments("ActiveUsers");

        await CSharpCodeFixVerifier<SpecificationSuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task ConfigurationWithoutSuffix_AddsConfigurationSuffixAsync()
    {
        // Smart suffix handling: "Config" is recognized as partial for "Configuration"
        // and gets replaced instead of appending (UserEntityConfig → UserEntityConfiguration)
        string source = """
            public interface IEntityTypeConfiguration<T> { }

            public class User { }

            public class {|#0:UserEntityConfig|} : IEntityTypeConfiguration<User>
            {
            }
            """;

        string fixedSource = """
            public interface IEntityTypeConfiguration<T> { }

            public class User { }

            public class UserEntityConfiguration : IEntityTypeConfiguration<User>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<ConfigurationSuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME006)
                .WithLocation(0)
                .WithArguments("UserEntityConfig");

        await CSharpCodeFixVerifier<ConfigurationSuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task SpecificationWithPartialSuffix_ReplacesSpecWithSpecificationAsync()
    {
        // Smart suffix handling: "Spec" is recognized as partial for "Specification"
        // and gets replaced instead of appending (ActiveUsersSpec → ActiveUsersSpecification)
        string source = """
            public abstract class Specification<T> { }

            public class User { }

            public class {|#0:ActiveUsersSpec|} : Specification<User>
            {
            }
            """;

        string fixedSource = """
            public abstract class Specification<T> { }

            public class User { }

            public class ActiveUsersSpecification : Specification<User>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<SpecificationSuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME004)
                .WithLocation(0)
                .WithArguments("ActiveUsersSpec");

        await CSharpCodeFixVerifier<SpecificationSuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task RepositoryWithPartialSuffix_ReplacesRepoWithRepositoryAsync()
    {
        // Smart suffix handling: "Repo" is recognized as partial for "Repository"
        // and gets replaced instead of appending (UserRepo → UserRepository)
        string source = """
            public interface IRepository<T> { }

            public class User { }

            public class {|#0:UserRepo|} : IRepository<User>
            {
            }
            """;

        string fixedSource = """
            public interface IRepository<T> { }

            public class User { }

            public class UserRepository : IRepository<User>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<RepositorySuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME001)
                .WithLocation(0)
                .WithArguments("UserRepo");

        await CSharpCodeFixVerifier<RepositorySuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }

    [Fact]
    public async Task ValidatorWithPartialSuffix_ReplacesValidWithValidatorAsync()
    {
        // Smart suffix handling: "Valid" is recognized as partial for "Validator"
        // and gets replaced instead of appending (CreateUserValid → CreateUserValidator)
        string source = """
            public abstract class AbstractValidator<T> { }

            public class CreateUserCommand { }

            public class {|#0:CreateUserValid|} : AbstractValidator<CreateUserCommand>
            {
            }
            """;

        string fixedSource = """
            public abstract class AbstractValidator<T> { }

            public class CreateUserCommand { }

            public class CreateUserValidator : AbstractValidator<CreateUserCommand>
            {
            }
            """;

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected =
            CSharpCodeFixVerifier<ValidatorSuffixAnalyzer, AddSuffixCodeFixProvider>
                .Diagnostic(DiagnosticIds.MDYNAME002)
                .WithLocation(0)
                .WithArguments("CreateUserValid");

        await CSharpCodeFixVerifier<ValidatorSuffixAnalyzer, AddSuffixCodeFixProvider>
            .VerifyCodeFixAsync(source, [expected], fixedSource);
    }
}
