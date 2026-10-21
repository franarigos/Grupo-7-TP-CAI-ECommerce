namespace Orders.API.Exceptions
{
    /// <summary>
    /// Catálogo de códigos de error para la API de Órdenes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>Orden no encontrada.</summary>
        public const string ORD_001 = "ORD-001";

        /// <summary>Datos de orden inválidos.</summary>
        public const string ORD_002 = "ORD-002";

        /// <summary>Usuario no encontrado.</summary>
        public const string ORD_003 = "ORD-003";

        /// <summary>Producto no encontrado.</summary>
        public const string ORD_004 = "ORD-004";

        /// <summary>Stock insuficiente.</summary>
        public const string ORD_005 = "ORD-005";

        /// <summary>Transición de estado inválida.</summary>
        public const string ORD_006 = "ORD-006";

        /// <summary>Error interno del servidor.</summary>
        public const string ORD_007 = "ORD-007";
    }
}
