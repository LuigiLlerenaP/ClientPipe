namespace ClientPipe.Extensions;

/// <summary>
/// Métodos de extensión para lectura eficiente de flujos HTTP sin cargar contenido completo en el buffer de memoria.
/// </summary>
public static class HttpClientStreamExtensions
{
    /// <summary>
    /// Descarga un recurso remoto como <see cref="Stream"/> usando <see cref="HttpCompletionOption.ResponseHeadersRead"/>.
    /// Ideal para procesar archivos YAML, JSON, CSV o binarios grandes con bajo consumo de RAM.
    /// </summary>
    public static async Task<Stream> GetResourceStreamAsync(
        this HttpClient httpClient,
        string relativeUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativeUrl);

        var response = await httpClient.GetAsync(
            relativeUrl.TrimStart('/'),
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }
}
