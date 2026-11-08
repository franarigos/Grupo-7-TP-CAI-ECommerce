namespace Notifications.API.DTOs
{
    /// <summary>
    /// Respuesta de error de Notifications.API.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Referencia al tipo de error.
        /// </summary>
        public string Type { get; set; } = "about:blank";

        /// <summary>
        /// Título del error.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Código HTTP de la respuesta.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Descripción general del error.
        /// </summary>
        public string Detail { get; set; } = string.Empty;

        /// <summary>
        /// Ruta de la solicitud.
        /// </summary>
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// Código de error del microservicio.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje del error.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Identificador de correlación de la solicitud.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;
    }
}