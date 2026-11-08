namespace Orders.API.DTOs
{
    /// <summary>
    /// Datos del producto obtenidos desde Products.API.
    /// </summary>
    public class ProductApiResponse
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Precio del producto.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Stock disponible.
        /// </summary>
        public int Stock { get; set; }
    }
}

