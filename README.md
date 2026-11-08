# ECommerce — Grupo 7

Trabajo práctico de microservicios desarrollado en C# con .NET 10, ASP.NET Core, SQLite y Dapper.

## Servicios

| Microservicio | Descripción |
|---|---|
| Products.API | Administra productos, precios, categorías y stock. Consulta Orders antes de eliminar un producto para comprobar que no tenga órdenes activas. |
| Users.API | Registra usuarios, valida credenciales y controla el bloqueo por intentos fallidos. Las contraseñas se almacenan mediante hashing. |
| Cart.API | Administra carritos y consulta Products para validar productos y stock. |
| Orders.API | Administra órdenes y sus estados. Consulta Users y Products para validar los datos al crear una orden. |
| Notifications.API | Registra notificaciones y simula su envío. Consulta Users para validar al destinatario. |

## Arquitectura

Cada servicio tiene su propia base SQLite. La comunicación entre servicios se realiza mediante HTTP.

```text
Cart.API ----------> Products.API
                         |
                         v
                     Orders.API
                         |
                         +----------> Products.API
                         |
                         +----------> Users.API
                                          ^
                                          |
Notifications.API ------------------------+
```

Las llamadas HTTP entre servicios propagan el encabezado `X-Correlation-Id`.

## Requisitos

- SDK de .NET 10.
- Visual Studio con soporte para .NET 10, o una terminal.
- Conexión a Internet para restaurar los paquetes NuGet.

SQLite no requiere instalar un servidor de base de datos. Cada servicio inicializa su base y sus tablas al arrancar.

## Puertos

| Servicio | HTTP | HTTPS | Perfil utilizado |
|---|---:|---:|---|
| Products.API | 5191 | 44380 | https |
| Users.API | 5071 | 7283 | https |
| Cart.API | 5092 | 7174 | https |
| Orders.API | 5180 | 7021 | https |
| Notifications.API | 5085 | — | http |

Los puertos están definidos en `Properties/launchSettings.json` de cada proyecto. Las direcciones de los servicios consumidos están configuradas en la sección `Services` de sus archivos `appsettings.json`.

## Ejecución con Visual Studio

1. Abrir `ECommerce/ECommerce.sln`.
2. Restaurar los paquetes NuGet y recompilar la solución.
3. Configurar los cinco proyectos como proyectos de inicio, con la acción **Iniciar**.
4. Seleccionar el perfil `https` para Products, Users, Cart y Orders; seleccionar `http` para Notifications.
5. Iniciar la ejecución.

Para probar las operaciones que consultan otros microservicios, deben estar ejecutándose también sus dependencias. Para la demostración completa, iniciar los cinco servicios.

## Ejecución desde una terminal

Desde la carpeta raíz del repositorio, restaurar y compilar:

```shell
dotnet restore ECommerce/ECommerce.sln
dotnet build ECommerce/ECommerce.sln
```

Para utilizar HTTPS en desarrollo, confiar en el certificado local:

```shell
dotnet dev-certs https --trust
```

Abrir una terminal por servicio y ejecutar cada comando desde la carpeta raíz del repositorio:

```shell
dotnet run --project ECommerce/Products.API/Products.API.csproj --launch-profile https
```

```shell
dotnet run --project ECommerce/Users.API/Users.API.csproj --launch-profile https
```

```shell
dotnet run --project ECommerce/Cart.API/Cart.API.csproj --launch-profile https
```

```shell
dotnet run --project ECommerce/Orders.API/Orders.API.csproj --launch-profile https
```

```shell
dotnet run --project ECommerce/Notifications.API/Notifications.API.csproj --launch-profile http
```

Los perfiles de ejecución utilizan el entorno `Development`.

## Swagger

| Servicio | Dirección |
|---|---|
| Products.API | https://localhost:44380/swagger |
| Users.API | https://localhost:7283/swagger |
| Cart.API | https://localhost:7174/swagger |
| Orders.API | https://localhost:7021/swagger |
| Notifications.API | http://localhost:5085/swagger |

Swagger contiene la descripción de los endpoints, sus parámetros, códigos HTTP y ejemplos de solicitudes y respuestas.

Para probar el flujo básico:

1. Registrar un usuario en Users.
2. Crear un producto en Products.
3. Usar los identificadores devueltos para agregar el producto a un carrito.
4. Crear una orden con el usuario y el producto.
5. Consultar la orden y actualizar su estado.
6. Registrar una notificación para el usuario.

## Persistencia

Cada microservicio utiliza SQLite y Dapper.

| Servicio | Archivo de base de datos |
|---|---|
| Products.API | products.db |
| Users.API | users.db |
| Cart.API | cart.db |
| Orders.API | orders.db |
| Notifications.API | notifications.db |

Las conexiones se configuran mediante `ConnectionStrings:DefaultConnection` en `appsettings.json`.

Los archivos de base de datos locales y sus archivos auxiliares están excluidos del repositorio mediante `.gitignore`.

## Respuestas de error

Las respuestas incluyen los datos de Problem Details y los campos `errorCode`, `errorMessage` y `correlationId`.

Ejemplo de consulta de un producto inexistente:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/products/ffffffff-ffff-ffff-ffff-ffffffffffff",
  "errorCode": "PRD-001",
  "errorMessage": "Producto no encontrado.",
  "correlationId": "11111111-2222-3333-4444-555555555555"
}
```

El encabezado de respuesta `X-Correlation-Id` permite relacionar la solicitud con sus registros en los logs.

## Códigos de error

La descripción resume cada error. El mensaje puede incluir información específica, como el producto o la cantidad solicitada.

| Código | HTTP | Descripción |
|---|---:|---|
| PRD-001 | 404 | Producto no encontrado. |
| PRD-002 | 400 | Datos del producto inválidos. |
| PRD-003 | 409 | Producto duplicado por nombre y categoría. |
| PRD-004 | 409 | El producto tiene órdenes activas y no puede eliminarse. |
| PRD-005 | 500 | Error interno al procesar el producto. |
| USR-001 | 409 | El email ya está registrado. |
| USR-002 | 400 | Datos del usuario inválidos. |
| USR-003 | 401 | Credenciales incorrectas. |
| USR-004 | 403 | Usuario bloqueado por intentos fallidos. |
| USR-005 | 403 | Usuario bloqueado por razones de seguridad o fraude. |
| USR-006 | 500 | Error interno al procesar el usuario. |
| USR-007 | 404 | Usuario no encontrado al consultar por ID. |
| CRT-001 | 404 | Carrito no encontrado. |
| CRT-002 | 404 | Producto no encontrado. |
| CRT-003 | 422 | Stock insuficiente para el carrito. |
| CRT-004 | 400 | Cantidad inválida o datos de entrada inválidos. |
| CRT-005 | 500 | Error interno al procesar el carrito. |
| ORD-001 | 404 | Orden no encontrada. |
| ORD-002 | 400 | Datos de la orden inválidos. |
| ORD-003 | 404 | Usuario no encontrado al crear la orden. |
| ORD-004 | 404 | Producto no encontrado al crear la orden. |
| ORD-005 | 422 | Stock insuficiente para crear la orden. |
| ORD-006 | 409 | Cambio de estado de la orden no permitido. |
| ORD-007 | 500 | Error interno al procesar la orden. |
| NTF-001 | 404 | Usuario destinatario no encontrado. |
| NTF-002 | 400 | Datos de la notificación inválidos. |
| NTF-003 | 404 | No hay notificaciones para el usuario. |
| NTF-004 | 500 | Error interno al procesar la notificación. |

`USR-007` es una extensión para la consulta de usuarios por ID.

Los errores técnicos HTTP utilizan el prefijo del servicio (`PRD`, `USR`, `CRT`, `ORD` o `NTF`) seguido de `-HTTP-` y el estado:

| Formato | HTTP | Descripción |
|---|---:|---|
| PREFIJO-HTTP-404 | 404 | Ruta inexistente. |
| PREFIJO-HTTP-405 | 405 | Método HTTP no permitido para la ruta. |
| PREFIJO-HTTP-415 | 415 | Formato de contenido no admitido. |

Por ejemplo, una ruta inexistente de Cart devuelve `CRT-HTTP-404`. Estos códigos técnicos complementan el catálogo de errores de negocio.

## Health checks

Todos los servicios exponen:

| Ruta | Función |
|---|---|
| `/health` | Comprueba el estado del servicio y su base SQLite. |
| `/health/ready` | Comprueba que el servicio pueda acceder a las tablas necesarias de su base. |
| `/health/live` | Comprueba que la aplicación responda, sin consultar la base. |

Ejemplo:

```text
https://localhost:44380/health/ready
```

Un servicio saludable responde con HTTP 200 y estado `Healthy`. Un fallo en la comprobación de SQLite devuelve HTTP 503 en `/health` y `/health/ready`.

## Logs

Los servicios utilizan Serilog para registrar información en consola y archivos JSON con rotación diaria en la carpeta `logs`.

Los registros permiten identificar el servicio, el endpoint, el identificador de correlación y el código de error cuando corresponde.

## Capturas de Swagger

Las capturas de entrega se guardan en `docs/capturas/`.

Deben mostrar solicitudes ejecutadas desde Swagger y su respuesta de error, con el código HTTP, `errorCode` y `errorMessage` visibles.