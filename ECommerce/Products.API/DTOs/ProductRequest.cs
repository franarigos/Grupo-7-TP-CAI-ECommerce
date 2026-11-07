using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Products.API.DTOs
{
    /// <summary>
    /// Datos para crear o actualizar un producto.
    /// </summary>
    public class ProductRequest
    {
        /// <summary>Nombre del producto.</summary>
        [JsonRequired]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripción del producto.</summary>
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Precio del producto.</summary>
        [JsonRequired]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        /// <summary>Stock disponible.</summary>
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
        public int Stock { get; set; }

        /// <summary>Categoría del producto.</summary>
        [JsonRequired]
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = string.Empty;
    }
}