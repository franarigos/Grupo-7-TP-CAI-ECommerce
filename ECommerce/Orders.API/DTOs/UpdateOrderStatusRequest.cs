namespace Orders.API.DTOs
{
    /// <summary>
    /// Solicitud para actualizar el estado de una orden existente.
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// Nuevo estado de la orden (ej. Confirmada, Enviada, Entregada, Cancelada).
        /// </summary>
        public string Estado { get; set; }
    }
}
