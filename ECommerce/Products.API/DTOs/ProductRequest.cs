using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Products.API.DTOs
{
    /// <summary>
    /// Datos para crear o actualizar un producto.
    /// </summary>
    public class ProductRequest
    {
        /// <summary>
        /// Nombre obligatorio, de hasta 100 caracteres.
        /// </summary>
        [JsonRequired]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(
            100,
            ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional, de hasta 500 caracteres.
        /// </summary>
        [MaxLength(
            500,
            ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Precio obligatorio, mayor a cero.
        /// </summary>
        [JsonRequired]
        public decimal Precio { get; set; }

        /// <summary>
        /// Stock obligatorio, mayor o igual a cero.
        /// </summary>
        [JsonRequired]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "El stock debe ser mayor o igual a 0.")]
        public int Stock { get; set; }

        /// <summary>
        /// Categoría obligatoria del producto.
        /// </summary>
        [JsonRequired]
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = string.Empty;
    }
}