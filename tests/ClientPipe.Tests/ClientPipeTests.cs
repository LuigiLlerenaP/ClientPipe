using ClientPipe.Extensions;
using ClientPipe.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClientPipe.Tests;

public sealed class ClientPipeTests
{
    public sealed class FakeGitHubOptions : HttpEndpointOptions
    {
        public const string SectionName = "GitHub";
        public override string BaseUrl { get; set; } = "https://raw.githubusercontent.com/default-org/repo/main/";
    }

    public interface IFakeCatalogClient
    {
        Uri? BaseAddress { get; }
        TimeSpan Timeout { get; }
    }

    public sealed class FakeCatalogClient(HttpClient httpClient) : IFakeCatalogClient
    {
        public Uri? BaseAddress => httpClient.BaseAddress;
        public TimeSpan Timeout => httpClient.Timeout;
    }

    [Fact]
    public void AddTypedHttpClient_WithoutConfiguration_UsesDefaultOptionsValues()
    {
        var services = new ServiceCollection();

        services.AddTypedHttpClient<IFakeCatalogClient, FakeCatalogClient, FakeGitHubOptions>();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IFakeCatalogClient>();

        Assert.Equal(new Uri("https://raw.githubusercontent.com/default-org/repo/main/"), client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);
    }

    [Fact]
    public void AddTypedHttpClient_WithConfiguration_OverridesDefaultValues()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["GitHub:BaseUrl"] = "https://api.customdomain.com/v1",
            ["GitHub:TimeoutSeconds"] = "45"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddTypedHttpClient<IFakeCatalogClient, FakeCatalogClient, FakeGitHubOptions>(
            configuration,
            FakeGitHubOptions.SectionName);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IFakeCatalogClient>();

        // Verifica que aseguró la barra diagonal final (/) y aplicó el nuevo timeout
        Assert.Equal(new Uri("https://api.customdomain.com/v1/"), client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(45), client.Timeout);
    }

    [Fact]
    public void AddTypedHttpClient_WithInvalidUrl_ThrowsOptionsValidationException()
    {
        var services = new ServiceCollection();

        services.AddTypedHttpClient<IFakeCatalogClient, FakeCatalogClient, FakeGitHubOptions>(opt =>
        {
            opt.BaseUrl = "no-es-una-url-valida";
        });

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IFakeCatalogClient>());
    }
}
