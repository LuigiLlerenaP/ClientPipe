using System.ComponentModel.DataAnnotations;

namespace ClientPipe.Options;

/// <summary>
/// Clase base para configurar cualquier cliente HTTP tipado con validación automática al arranque.
/// </summary>
public abstract class HttpEndpointOptions
{
    /// <summary>
    /// URL base del servicio externo.
    /// </summary>
    [Required(ErrorMessage = "La URL base del servicio es obligatoria.")]
    [Url(ErrorMessage = "El formato de la URL base no es válido.")]
    public virtual string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo máximo de espera por petición en segundos (por defecto 30 segundos).
    /// </summary>
    [Range(1, 300, ErrorMessage = "El timeout debe estar entre 1 y 300 segundos.")]
    public virtual int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Tiempo de vida de las conexiones en el pool en minutos para prevenir problemas de DNS caché (por defecto 5 minutos).
    /// </summary>
    [Range(1, 60, ErrorMessage = "El tiempo de vida del pool debe estar entre 1 y 60 minutos.")]
    public virtual int PooledConnectionLifetimeMinutes { get; set; } = 5;
}
