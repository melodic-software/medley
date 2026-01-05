using SharedKernel.Analyzers.Analyzers.Naming;
using SharedKernel.Analyzers.Tests.Infrastructure;
using Xunit;

namespace SharedKernel.Analyzers.Tests.Analyzers.Naming;

public class SpecificationSuffixAnalyzerTests : SuffixAnalyzerTestBase<SpecificationSuffixAnalyzer>
{
    protected override string DiagnosticId => DiagnosticIds.MDYNAME004;

    [Fact]
    public async Task SpecificationWithCorrectSuffix_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public abstract class Specification<T>
                {
                    public abstract bool IsSatisfiedBy(T entity);
                }

                public class User { }

                public class ActiveUserSpecification : Specification<User>
                {
                    public override bool IsSatisfiedBy(User entity) => true;
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task SpecificationWithoutSuffix_InheritingBase_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public abstract class Specification<T>
                {
                    public abstract bool IsSatisfiedBy(T entity);
                }

                public class User { }

                public class {|#0:ActiveUser|} : Specification<User>
                {
                    public override bool IsSatisfiedBy(User entity) => true;
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "ActiveUser");
    }

    [Fact]
    public async Task SpecificationWithoutSuffix_ImplementingInterface_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public interface ISpecification<T>
                {
                    bool IsSatisfiedBy(T entity);
                }

                public class Order { }

                public class {|#0:PendingOrders|} : ISpecification<Order>
                {
                    public bool IsSatisfiedBy(Order entity) => true;
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "PendingOrders");
    }

    [Fact]
    public async Task SpecificationWithSpecSuffix_ReportsDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public abstract class Specification<T>
                {
                    public abstract bool IsSatisfiedBy(T entity);
                }

                public class Product { }

                public class {|#0:AvailableProductSpec|}  : Specification<Product>
                {
                    public override bool IsSatisfiedBy(Product entity) => true;
                }
            }
            """;

        await VerifyDiagnosticAsync(source, "AvailableProductSpec");
    }

    [Fact]
    public async Task ClassNotImplementingSpecification_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public class ActiveUserFilter
                {
                    public bool Apply(object entity) => true;
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task AbstractSpecification_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public abstract class Specification<T>
                {
                    public abstract bool IsSatisfiedBy(T entity);
                }

                public class Entity { }

                public abstract class CompositeSpec : Specification<Entity>
                {
                    public override bool IsSatisfiedBy(Entity entity) => true;
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task SpecificationInterface_NoDiagnosticAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public interface IUserSpec
                {
                    bool IsSatisfiedBy(object entity);
                }
            }
            """;

        await VerifyNoDiagnosticAsync(source);
    }

    [Fact]
    public async Task MultipleSpecifications_ReportsAllDiagnosticsAsync()
    {
        string source = """
            namespace MyApp.Domain
            {
                public interface ISpecification<T>
                {
                    bool IsSatisfiedBy(T entity);
                }

                public class User { }
                public class Order { }

                public class {|#0:ActiveUsers|} : ISpecification<User>
                {
                    public bool IsSatisfiedBy(User entity) => true;
                }

                public class {|#1:CompletedOrders|} : ISpecification<Order>
                {
                    public bool IsSatisfiedBy(Order entity) => true;
                }
            }
            """;

        await VerifyMultipleDiagnosticsAsync(source, "ActiveUsers", "CompletedOrders");
    }
}
