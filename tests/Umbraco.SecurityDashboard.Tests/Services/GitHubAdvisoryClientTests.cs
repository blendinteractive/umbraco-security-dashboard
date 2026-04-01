using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class GitHubAdvisoryClientTests
{
    private static IHttpClientFactory CreateFactory(params HttpResponseMessage[] responses)
    {
        var queue = new Queue<HttpResponseMessage>(responses);
        var handler = new QueuedMessageHandler(queue);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("GitHubAdvisories").Returns(client);
        return factory;
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_DeserializesJson()
    {
        var json = JsonSerializer.Serialize(new[]
        {
            new
            {
                ghsa_id = "GHSA-1234-5678-abcd",
                summary = "Test advisory",
                severity = "moderate",
                published_at = "2024-01-15T00:00:00Z",
                html_url = "https://github.com/advisories/GHSA-1234-5678-abcd",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "Umbraco.Cms" },
                        vulnerable_version_range = "< 10.6.1",
                        first_patched_version = "10.6.1"
                    }
                }
            }
        });

        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });

        var sut = new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);
        var result = await sut.GetUmbracoAdvisoriesAsync();

        Assert.Single(result);
        Assert.Equal("GHSA-1234-5678-abcd", result[0].GhsaId);
        Assert.Equal("moderate", result[0].Severity);
        Assert.Equal("Umbraco.Cms", result[0].Vulnerabilities[0].Package.Name);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_FiltersNonUmbracoPackages()
    {
        var json = JsonSerializer.Serialize(new[]
        {
            new
            {
                ghsa_id = "GHSA-aaaa-bbbb-cccc",
                summary = "Non-Umbraco",
                severity = "low",
                published_at = "2024-01-01T00:00:00Z",
                html_url = "https://example.com",
                vulnerabilities = new[]
                {
                    new
                    {
                        package = new { ecosystem = "nuget", name = "SomeOther.Package" },
                        vulnerable_version_range = "< 1.0.0",
                        first_patched_version = "1.0.0"
                    }
                }
            }
        });

        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });

        var sut = new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);
        var result = await sut.GetUmbracoAdvisoriesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_PaginatesViaLinkHeader()
    {
        var page1Json = JsonSerializer.Serialize(new[]
        {
            new
            {
                ghsa_id = "GHSA-page1",
                summary = "Page 1",
                severity = "low",
                published_at = "2024-01-01T00:00:00Z",
                html_url = "https://example.com",
                vulnerabilities = new[] { new { package = new { ecosystem = "nuget", name = "Umbraco.Cms" }, vulnerable_version_range = "< 1.0.0", first_patched_version = "1.0.0" } }
            }
        });

        var page2Json = JsonSerializer.Serialize(new[]
        {
            new
            {
                ghsa_id = "GHSA-page2",
                summary = "Page 2",
                severity = "high",
                published_at = "2024-01-02T00:00:00Z",
                html_url = "https://example.com",
                vulnerabilities = new[] { new { package = new { ecosystem = "nuget", name = "Umbraco.Cms" }, vulnerable_version_range = "< 2.0.0", first_patched_version = "2.0.0" } }
            }
        });

        var response1 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(page1Json, System.Text.Encoding.UTF8, "application/json")
        };
        response1.Headers.Add("Link", "<https://api.github.com/advisories?page=2>; rel=\"next\"");

        var response2 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(page2Json, System.Text.Encoding.UTF8, "application/json")
        };

        var factory = CreateFactory(response1, response2);
        var sut = new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);
        var result = await sut.GetUmbracoAdvisoriesAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUmbracoAdvisoriesAsync_CancellationPropagates()
    {
        var handler = new DelayingHandler();
        var client = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("GitHubAdvisories").Returns(client);

        var sut = new GitHubAdvisoryClient(factory, NullLogger<GitHubAdvisoryClient>.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.GetUmbracoAdvisoriesAsync(cts.Token));
    }

    private class QueuedMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public QueuedMessageHandler(Queue<HttpResponseMessage> responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private class DelayingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
