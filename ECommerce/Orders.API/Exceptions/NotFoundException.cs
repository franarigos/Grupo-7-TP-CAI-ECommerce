using System;

namespace Orders.API.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no se encuentra un recurso solicitado (ej. Orden, Usuario o Producto).
    /// </summary>
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        /// <summary>
        /// Código de error tipificado según el catálogo.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}
