using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class GitHubAdvisoryClientTests
{
    private static GitHubAdvisoryClient CreateClient(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = null };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        return new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);
    }

    private static HttpResponseMessage JsonResponse(object body, string? linkHeader = null)
    {
        var json = JsonSerializer.Serialize(body);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        };
        if (linkHeader is not null)
            response.Headers.Add("Link", linkHeader);
        return response;
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_ReturnsAdvisoriesForUmbracoPackages()
    {
        var body = new[]
        {
            new
            {
                ghsa_id = "GHSA-test-0001-0001",
                summary = "Test advisory",
                severity = "high",
                published_at = "2024-01-15T00:00:00Z",
                html_url = "https://github.com/advisories/GHSA-test-0001-0001",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "Umbraco.Cms" },
                        vulnerable_version_range = "< 10.6.1",
                    },
                },
            },
        };

        var client = CreateClient(JsonResponse(body));

        var results = await client.GetUmbracoAdvisoriesAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("GHSA-test-0001-0001", results[0].GhsaId);
        Assert.Equal("High", results[0].Severity); // capitalised
        Assert.Equal("Umbraco.Cms", results[0].Vulnerabilities[0].PackageName);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_FiltersOutNonUmbracoPackages()
    {
        var body = new[]
        {
            new
            {
                ghsa_id = "GHSA-test-0002-0002",
                summary = "Non-Umbraco advisory",
                severity = "low",
                published_at = "2024-01-15T00:00:00Z",
                html_url = "https://github.com/advisories/GHSA-test-0002-0002",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "Newtonsoft.Json" },
                        vulnerable_version_range = "< 13.0.1",
                    },
                },
            },
        };

        var client = CreateClient(JsonResponse(body));

        var results = await client.GetUmbracoAdvisoriesAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_CapitalizesSeverity()
    {
        var body = new[]
        {
            new
            {
                ghsa_id = "GHSA-test-0003-0003",
                summary = "Critical advisory",
                severity = "critical",
                published_at = "2024-01-15T00:00:00Z",
                html_url = "https://github.com/advisories/GHSA-test-0003-0003",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "Umbraco.Cms" },
                        vulnerable_version_range = "< 13.0.0",
                    },
                },
            },
        };

        var client = CreateClient(JsonResponse(body));

        var results = await client.GetUmbracoAdvisoriesAsync(CancellationToken.None);

        Assert.Equal("Critical", results[0].Severity);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_RespectsLinkHeaderPagination()
    {
        // First page with Link: next header
        var page1 = new[]
        {
            new
            {
                ghsa_id = "GHSA-page1-0001",
                summary = "Page 1 advisory",
                severity = "high",
                published_at = "2024-01-15T00:00:00Z",
                html_url = "https://github.com/advisories/GHSA-page1-0001",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "Umbraco.Cms" },
                        vulnerable_version_range = "< 10.6.1",
                    },
                },
            },
        };

        // Second page, no Link header
        var page2 = new[]
        {
            new
            {
                ghsa_id = "GHSA-page2-0001",
                summary = "Page 2 advisory",
                severity = "moderate",
                published_at = "2024-02-15T00:00:00Z",
                html_url = "https://github.com/advisories/GHSA-page2-0001",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "Umbraco.Forms" },
                        vulnerable_version_range = "< 13.0.0",
                    },
                },
            },
        };

        var page2Url = "https://api.github.com/advisories?ecosystem=nuget&per_page=100&after=cursor2";
        var linkHeader = $"<{page2Url}>; rel=\"next\"";

        var callCount = 0;
        var handler = new DelegatingMockHandler(request =>
        {
            callCount++;
            return callCount == 1
                ? JsonResponse(page1, linkHeader)
                : JsonResponse(page2);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = null };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var client = new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);
        var results = await client.GetUmbracoAdvisoriesAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("GHSA-page1-0001", results[0].GhsaId);
        Assert.Equal("GHSA-page2-0001", results[1].GhsaId);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(
            new HttpClient(new ThrowingCancelHandler()) { BaseAddress = null });

        var client = new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetUmbracoAdvisoriesAsync(cts.Token));
    }

    // Helpers
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public MockHttpMessageHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_response);
    }

    private sealed class DelegatingMockHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public DelegatingMockHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) =>
            _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }

    private sealed class ThrowingCancelHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
