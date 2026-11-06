using System.Net;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        private static readonly List<Notification> _notificaciones =
            new List<Notification>();

        private static readonly object _lock = new object();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<NotificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<NotificationResponse> SendAsync(
            SendNotificationRequest request,
            CancellationToken cancellationToken)
        {
            ValidarDatos(request);

            await ValidarUsuarioAsync(
                request.UsuarioId,
                cancellationToken);

            Notification notification = new Notification();

            notification.Id = Guid.NewGuid();
            notification.UsuarioId = request.UsuarioId;
            notification.Mensaje = request.Mensaje;
            notification.Tipo = request.Tipo;
            notification.Estado = "Enviada";
            notification.FechaEnvio = DateTime.UtcNow;

            lock (_lock)
            {
                _notificaciones.Add(notification);
            }

            _logger.LogInformation(
                "Envío SIMULADO registrado. Notificación {NotificationId}, tipo {Tipo}",
                notification.Id,
                notification.Tipo);

            return MapToResponse(notification);
        }

        public List<NotificationResponse> GetByUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ValidationException(
                    "NTF-002",
                    "El identificador del usuario es inválido.");
            }

            List<NotificationResponse> resultado =
                new List<NotificationResponse>();

            lock (_lock)
            {
                foreach (Notification notification in _notificaciones)
                {
                    if (notification.UsuarioId == userId)
                    {
                        resultado.Add(MapToResponse(notification));
                    }
                }
            }

            if (resultado.Count == 0)
            {
                throw new NotFoundException(
                    "NTF-003",
                    "No se encontraron notificaciones para el usuario.");
            }

            return resultado;
        }

        private void ValidarDatos(SendNotificationRequest request)
        {
            if (request.UsuarioId == Guid.Empty)
            {
                throw new ValidationException(
                    "NTF-002",
                    "El identificador del usuario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Mensaje))
            {
                throw new ValidationException(
                    "NTF-002",
                    "El mensaje es obligatorio.");
            }

            if (request.Mensaje.Length > 500)
            {
                throw new ValidationException(
                    "NTF-002",
                    "El mensaje no puede superar 500 caracteres.");
            }

            if (request.Tipo != "Email" &&
                request.Tipo != "Push" &&
                request.Tipo != "SMS")
            {
                throw new ValidationException(
                    "NTF-002",
                    "El tipo debe ser Email, Push o SMS.");
            }
        }

        private async Task ValidarUsuarioAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            HttpClient client =
                _httpClientFactory.CreateClient("UsersAPI");

            using HttpRequestMessage request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/users/" + userId);

            HttpContext? context = _httpContextAccessor.HttpContext;

            if (context != null)
            {
                request.Headers.Add(
                    "X-Correlation-Id",
                    context.TraceIdentifier);
            }

            using HttpResponseMessage response =
                await client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    "NTF-001",
                    "Usuario no encontrado.");
            }

            response.EnsureSuccessStatusCode();
        }

        private NotificationResponse MapToResponse(
            Notification notification)
        {
            NotificationResponse response = new NotificationResponse();

            response.Id = notification.Id;
            response.UsuarioId = notification.UsuarioId;
            response.Mensaje = notification.Mensaje;
            response.Tipo = notification.Tipo;
            response.Estado = notification.Estado;
            response.FechaEnvio = notification.FechaEnvio;

            return response;
        }
    }
}