namespace Orders.API.Exceptions
{
    /// <summary>
    /// Contiene los códigos de error tipificados obligatorios para la API de Órdenes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>Orden no encontrada.</summary>
        public const string ORD_001 = "ORD-001";

        /// <summary>Los datos de la orden son inválidos.</summary>
        public const string ORD_002 = "ORD-002";

        /// <summary>Usuario no encontrado al crear la orden.</summary>
        public const string ORD_003 = "ORD-003";

        /// <summary>Producto no encontrado al crear la orden.</summary>
        public const string ORD_004 = "ORD-004";

        /// <summary>Stock insuficiente para uno o más productos.</summary>
        public const string ORD_005 = "ORD-005";

        /// <summary>El estado de la orden no puede ser modificado.</summary>
        public const string ORD_006 = "ORD-006";

        /// <summary>Error interno al procesar la orden.</summary>
        public const string ORD_007 = "ORD-007";
    }
}
