using System;

namespace Orders.API.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando un recurso no existe.
    /// </summary>
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        /// <summary>Código de error específico.</summary>
        public string ErrorCode { get; } = errorCode;
    }
}
