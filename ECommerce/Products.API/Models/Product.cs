using System;

namespace Products.API.Models
{
    /// <summary>
    /// Entidad de dominio que representa un producto.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre obligatorio del producto, de hasta 100 caracteres.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional del producto, de hasta 500 caracteres.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Precio del producto, mayor a cero.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Stock disponible, mayor o igual a cero.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Categoría del producto.
        /// </summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Fecha UTC asignada automáticamente al crear el producto.
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
}