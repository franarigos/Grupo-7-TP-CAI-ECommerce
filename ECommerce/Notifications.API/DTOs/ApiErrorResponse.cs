namespace Notifications.API.DTOs
{
    public class ApiErrorResponse
    {
        public string Type { get; set; } = "about:blank";

        public string Title { get; set; } = string.Empty;

        public int Status { get; set; }

        public string Detail { get; set; } = string.Empty;

        public string Instance { get; set; } = string.Empty;

        /// <example>NTF-002</example>
        public string ErrorCode { get; set; } = string.Empty;

        /// <example>Los datos de la notificación son inválidos.</example>
        public string ErrorMessage { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;
    }
}
