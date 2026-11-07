namespace Cart.API.DTOs
{
    /// <summary>
    /// Respuesta de error del carrito.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>Tipo de error.</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Título del error.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Estado HTTP.</summary>
        public int Status { get; set; }

        /// <summary>Detalle del error.</summary>
        public string Detail { get; set; } = string.Empty;

        /// <summary>Ruta solicitada.</summary>
        public string Instance { get; set; } = string.Empty;

        /// <summary>Código del catálogo de errores.</summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>Mensaje del error.</summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>Identificador de correlación.</summary>
        public string CorrelationId { get; set; } = string.Empty;
    }
}