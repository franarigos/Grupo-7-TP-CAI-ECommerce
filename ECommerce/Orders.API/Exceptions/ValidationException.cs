using System;

namespace Orders.API.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando los datos de entrada no cumplen con el formato o requisitos esperados.
    /// </summary>
    public class ValidationException(string errorCode, string message) : Exception(message)
    {
        /// <summary>
        /// Código de error tipificado según el catálogo.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}
