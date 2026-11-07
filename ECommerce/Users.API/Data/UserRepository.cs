
using System;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=users.db";

            return new SqliteConnection(connectionString);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using SqliteConnection connection = CreateConnection();

            UserRow? fila =
                await connection.QuerySingleOrDefaultAsync<UserRow>(
                    """
                    SELECT
                        id AS Id,
                        nombre AS Nombre,
                        apellido AS Apellido,
                        email AS Email,
                        password_hash AS PasswordHash,
                        fecha_registro AS FechaRegistro,
                        activo AS Activo,
                        intentos_fallidos AS IntentosFallidos
                    FROM usuarios
                    WHERE email = @Email
                    """,
                    new { Email = email });

            if (fila == null)
            {
                return null;
            }

            return MapearUsuario(fila);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using SqliteConnection connection = CreateConnection();

            UserRow? fila =
                await connection.QuerySingleOrDefaultAsync<UserRow>(
                    """
                    SELECT
                        id AS Id,
                        nombre AS Nombre,
                        apellido AS Apellido,
                        email AS Email,
                        password_hash AS PasswordHash,
                        fecha_registro AS FechaRegistro,
                        activo AS Activo,
                        intentos_fallidos AS IntentosFallidos
                    FROM usuarios
                    WHERE id = @Id
                    """,
                    new { Id = id.ToString() });

            if (fila == null)
            {
                return null;
            }

            return MapearUsuario(fila);
        }

        public async Task CreateAsync(User usuario)
        {
            using SqliteConnection connection = CreateConnection();

            await connection.ExecuteAsync(
                """
                INSERT INTO usuarios (
                    id,
                    nombre,
                    apellido,
                    email,
                    password_hash,
                    fecha_registro,
                    activo,
                    intentos_fallidos
                )
                VALUES (
                    @Id,
                    @Nombre,
                    @Apellido,
                    @Email,
                    @PasswordHash,
                    @FechaRegistro,
                    @Activo,
                    @IntentosFallidos
                )
                """,
                new
                {
                    Id = usuario.Id.ToString(),
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    PasswordHash = usuario.PasswordHash,
                    FechaRegistro =
                        usuario.FechaRegistro.ToString(
                            "O",
                            CultureInfo.InvariantCulture),
                    Activo = usuario.Activo ? 1 : 0,
                    IntentosFallidos = usuario.IntentosFallidos
                });
        }

        public async Task AddFailedAttemptAsync(Guid id)
        {
            using SqliteConnection connection = CreateConnection();

            await connection.ExecuteAsync(
                """
                UPDATE usuarios
                SET
                    intentos_fallidos = intentos_fallidos + 1,
                    activo = CASE
                        WHEN intentos_fallidos + 1 >= 3
                        THEN 0
                        ELSE activo
                    END
                WHERE id = @Id AND activo = 1
                """,
                new { Id = id.ToString() });
        }

        public async Task ResetFailedAttemptsAsync(Guid id)
        {
            using SqliteConnection connection = CreateConnection();

            await connection.ExecuteAsync(
                """
                UPDATE usuarios
                SET intentos_fallidos = 0
                WHERE id = @Id AND activo = 1
                """,
                new { Id = id.ToString() });
        }

        private User MapearUsuario(UserRow fila)
        {
            User usuario = new User(
                fila.Nombre,
                fila.Apellido,
                fila.Email,
                fila.PasswordHash);

            usuario.Id = Guid.Parse(fila.Id);
            usuario.FechaRegistro = DateTime.Parse(
                fila.FechaRegistro,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            usuario.Activo = fila.Activo == 1;
            usuario.IntentosFallidos = fila.IntentosFallidos;

            return usuario;
        }
    }

    public class UserRow
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FechaRegistro { get; set; } = string.Empty;
        public int Activo { get; set; }
        public int IntentosFallidos { get; set; }
    }
}
