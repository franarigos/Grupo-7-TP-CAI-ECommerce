namespace Orders.API.DTOs
{
    /// <summary>
    /// Solicitud para modificar el estado.
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>Nuevo estado a asignar.</summary>
        public string Estado { get; set; }
    }
}