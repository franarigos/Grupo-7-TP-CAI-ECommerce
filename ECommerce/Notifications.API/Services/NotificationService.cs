
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationRepository repository,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<NotificationService> logger)
        {
            _repository = repository;
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
            notification.Mensaje = request.Mensaje.Trim();
            notification.Tipo = request.Tipo;
            notification.Estado = "Enviada";
            notification.FechaEnvio = DateTime.UtcNow;

            await _repository.CreateAsync(notification);

            _logger.LogInformation(
                "Envío simulado registrado. Notificación {NotificationId}, tipo {Tipo}",
                notification.Id,
                notification.Tipo);

            return MapToResponse(notification);
        }

        public async Task<List<NotificationResponse>> GetByUserAsync(
            Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ValidationException(
                    "NTF-002",
                    "El identificador del usuario es inválido.");
            }

            List<Notification> notificaciones =
                await _repository.GetByUserAsync(userId);

            if (notificaciones.Count == 0)
            {
                throw new NotFoundException(
                    "NTF-003",
                    "No se encontraron notificaciones para el usuario.");
            }

            List<NotificationResponse> resultado =
                new List<NotificationResponse>();

            foreach (Notification notification in notificaciones)
            {
                resultado.Add(MapToResponse(notification));
            }

            return resultado;
        }

        private void ValidarDatos(SendNotificationRequest request)
        {
            if (request == null ||
                request.UsuarioId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.Mensaje) ||
                request.Mensaje.Length > 500 ||
                string.IsNullOrWhiteSpace(request.Tipo))
            {
                throw new ValidationException(
                    "NTF-002",
                    "Los datos de la notificación son inválidos.");
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

            using HttpRequestMessage solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/users/" + userId);

            HttpContext? context =
                _httpContextAccessor.HttpContext;

            if (context != null)
            {
                solicitud.Headers.TryAddWithoutValidation(
                    "X-Correlation-Id",
                    context.TraceIdentifier);
            }

            using HttpResponseMessage respuesta =
                await client.SendAsync(
                    solicitud,
                    cancellationToken);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    "NTF-001",
                    "El usuario destinatario no fue encontrado.");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "No se pudo consultar Users.API.");
            }
        }

        private NotificationResponse MapToResponse(
            Notification notification)
        {
            NotificationResponse response =
                new NotificationResponse();

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
