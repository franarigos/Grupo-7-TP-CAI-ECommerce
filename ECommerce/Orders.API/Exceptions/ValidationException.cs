using System;

namespace Orders.API.Exceptions
{
    /// <summary>
    /// Excepción lanzada por datos de entrada incorrectos.
    /// </summary>
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        /// <summary>Código de error específico.</summary>
        public string ErrorCode { get; } = errorCode;
    }
}
