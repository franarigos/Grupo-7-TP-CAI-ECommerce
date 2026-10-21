using System;

namespace Orders.API.Exceptions
{
    /// <summary>
    /// Excepción lanzada por violaciones a las reglas del negocio.
    /// </summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        /// <summary>Código de error específico.</summary>
        public string ErrorCode { get; } = errorCode;
    }
}
