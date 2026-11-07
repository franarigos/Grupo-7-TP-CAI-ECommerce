using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            IConfiguration config,
            ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db;Foreign Keys=True";

            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS carritos (
                    usuario_id TEXT PRIMARY KEY,
                    fecha_actualizacion TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS carrito_items (
                    usuario_id TEXT NOT NULL,
                    producto_id TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    PRIMARY KEY (usuario_id, producto_id),
                    FOREIGN KEY (usuario_id)
                        REFERENCES carritos(usuario_id)
                        ON DELETE CASCADE
                );
            ");

            _logger.LogInformation(
                "SQLite inicializado correctamente: {BaseDatos}",
                connectionString);
        }
    }
}
