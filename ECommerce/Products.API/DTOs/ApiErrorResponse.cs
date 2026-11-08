namespace Products.API.DTOs
{
    /// <summary>
    /// Respuesta de error de Products.API.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>Tipo de error.</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Título del error.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Estado HTTP.</summary>
        public int Status { get; set; }

        /// <summary>Descripción general del error.</summary>
        public string Detail { get; set; } = string.Empty;

        /// <summary>Ruta solicitada.</summary>
        public string Instance { get; set; } = string.Empty;

        /// <summary>Código del error.</summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>Mensaje específico del error.</summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>Identificador de correlación de la solicitud.</summary>
        public string CorrelationId { get; set; } = string.Empty;
    }
}