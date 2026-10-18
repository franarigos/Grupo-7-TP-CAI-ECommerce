using System;
using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs
{
    /// <summary>
    /// Datos requeridos para dar de alta o actualizar un producto.
    /// </summary>
    public class ProductRequest
    {
        /// <summary> Nombre del producto. </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary> Descripción detallada del producto. </summary>
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary> Precio de venta del producto (mayor a 0). </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        /// <summary> Cantidad de unidades disponibles en stock (mayor o igual a 0). </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
        public int Stock { get; set; }

        /// <summary> Categoría a la que pertenece el producto. </summary>
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = string.Empty;
    }
}