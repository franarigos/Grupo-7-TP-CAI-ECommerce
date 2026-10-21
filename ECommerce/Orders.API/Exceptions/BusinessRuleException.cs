using System;

namespace Orders.API.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se viola una regla de negocio del sistema (ej. stock insuficiente).
    /// </summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        /// <summary>
        /// Código de error tipificado según el catálogo.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}
