using System;

namespace Products.API.Models
{
    /// <summary>
    /// Entidad de dominio que representa un producto.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Identificador único.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Requerido, máx. 100 caracteres.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Opcional, máx. 500 caracteres.
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Requerido, mayor a 0.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Requerido, mayor o igual a 0.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Requerido.
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// Asignado automáticamente al crear.
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
}