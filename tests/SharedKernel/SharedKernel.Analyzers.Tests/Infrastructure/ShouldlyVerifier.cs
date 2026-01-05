using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Shouldly;

namespace SharedKernel.Analyzers.Tests.Infrastructure;

// Custom IVerifier implementation using Shouldly assertions.
// Microsoft.CodeAnalysis.Testing has compatibility issues with xUnit 2.5+
// (see GitHub issue dotnet/roslyn-sdk#1099). This custom verifier bridges
// the testing infrastructure with Shouldly's assertion library.
public sealed class ShouldlyVerifier : IVerifier
{
    public void Empty<T>(string collectionName, IEnumerable<T> collection)
    {
        collection.ShouldBeEmpty($"{collectionName} should be empty");
    }

    public void Equal<T>(T expected, T actual, string? message = null)
    {
        actual.ShouldBe(expected, message);
    }

    public void True([DoesNotReturnIf(false)] bool assert, string? message = null)
    {
        assert.ShouldBeTrue(message);
    }

    public void False([DoesNotReturnIf(true)] bool assert, string? message = null)
    {
        assert.ShouldBeFalse(message);
    }

    [DoesNotReturn]
    public void Fail(string? message = null)
    {
        throw new ShouldAssertException(message ?? "Test failed");
    }

    public void LanguageIsSupported(string language)
    {
        language.ShouldBe(LanguageNames.CSharp, "Only C# is supported");
    }

    public void NotEmpty<T>(string collectionName, IEnumerable<T> collection)
    {
        collection.ShouldNotBeEmpty($"{collectionName} should not be empty");
    }

    public void SequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        IEqualityComparer<T>? equalityComparer = null,
        string? message = null)
    {
        List<T> expectedList = expected.ToList();
        List<T> actualList = actual.ToList();

        actualList.Count.ShouldBe(expectedList.Count, message ?? "Sequence lengths differ");

        for (int i = 0; i < expectedList.Count; i++)
        {
            if (equalityComparer is not null)
            {
                equalityComparer.Equals(expectedList[i], actualList[i])
                    .ShouldBeTrue($"Element at index {i} differs. {message}");
            }
            else
            {
                actualList[i].ShouldBe(expectedList[i], $"Element at index {i} differs. {message}");
            }
        }
    }

    public IVerifier PushContext(string context)
    {
        return this;
    }
}
