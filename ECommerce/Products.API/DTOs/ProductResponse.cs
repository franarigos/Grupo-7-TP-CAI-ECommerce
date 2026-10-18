using System;

namespace Products.API.DTOs
{
    /// <summary>
    /// Representa la respuesta con los datos detallados de un producto.
    /// </summary>
    public class ProductResponse
    {
        /// <summary> Identificador único del producto. </summary>
        public Guid Id { get; set; }

        /// <summary> Nombre del producto. </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary> Descripción detallada del producto. </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary> Precio de venta del producto. </summary>
        public decimal Precio { get; set; }

        /// <summary> Cantidad de unidades disponibles. </summary>
        public int Stock { get; set; }

        /// <summary> Categoría a la que pertenece el producto. </summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary> Fecha y hora en la que fue creado el producto. </summary>
        public DateTime FechaCreacion { get; set; }
    }
}