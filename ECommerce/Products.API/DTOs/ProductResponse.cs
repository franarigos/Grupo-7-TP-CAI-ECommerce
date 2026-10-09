namespace Products.API.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}