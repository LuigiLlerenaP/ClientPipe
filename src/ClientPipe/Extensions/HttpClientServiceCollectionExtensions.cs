using ClientPipe.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClientPipe.Extensions;

/// <summary>
/// Extensiones para registrar opciones validadas y clientes HTTP tipados con gestión óptima de conexiones.
/// </summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// Registra y valida opciones fuertemente tipadas con DataAnnotations al iniciar la aplicación.
    /// </summary>
    public static IServiceCollection AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Registra un cliente HTTP tipado enlazando una sección de configuración (si existe) o manteniendo los valores por defecto de <typeparamref name="TOptions"/>.
    /// </summary>
    public static IHttpClientBuilder AddTypedHttpClient<TInterface, TImplementation, TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TInterface : class
        where TImplementation : class, TInterface
        where TOptions : HttpEndpointOptions, new()
    {
        services.AddValidatedOptions<TOptions>(configuration, sectionName);
        return services.RegisterClientInternal<TInterface, TImplementation, TOptions>();
    }

    /// <summary>
    /// Registra un cliente HTTP tipado usando los valores por defecto de <typeparamref name="TOptions"/> o sobreescribiéndolos en código sin depender de appsettings.json.
    /// </summary>
    public static IHttpClientBuilder AddTypedHttpClient<TInterface, TImplementation, TOptions>(
        this IServiceCollection services,
        Action<TOptions>? configureOptions = null)
        where TInterface : class
        where TImplementation : class, TInterface
        where TOptions : HttpEndpointOptions, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<TOptions>();

        if (configureOptions is not null)
        {
            optionsBuilder.Configure(configureOptions);
        }

        optionsBuilder
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.RegisterClientInternal<TInterface, TImplementation, TOptions>();
    }

    private static IHttpClientBuilder RegisterClientInternal<TInterface, TImplementation, TOptions>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TOptions : HttpEndpointOptions
    {
        return services.AddHttpClient<TInterface, TImplementation>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
            return new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(options.PooledConnectionLifetimeMinutes)
            };
        })
        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
    }
}
