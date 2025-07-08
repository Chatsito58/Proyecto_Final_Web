using System.Data;
using Microsoft.Data.SqlClient;
using Proyecto_Final_Web.Models;
using System.Threading.Tasks;

namespace Proyecto_Final_Web.Logica
{
    public class Logica_Usuarios
    {
        private readonly string _connectionString;

        public Logica_Usuarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionBD");
        }

        public async Task<Usuario?> AutenticarUsuario(string correo, string clave)
        {
            Usuario? usuario = null;

            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    await conexion.OpenAsync();

                    const string consulta = @"
                        SELECT
                            u.IdUsuario,
                            u.IdRol,
                            u.NombreCompleto,
                            u.Correo,
                            u.Contrasena,
                            u.Telefono,
                            u.FechaRegistro,
                            r.Nombre AS RolNombre,
                            r.Descripcion AS RolDescripcion
                        FROM Usuarios u
                        INNER JOIN Roles r ON u.IdRol = r.IdRol
                        WHERE u.Correo = @correo";

                    using (var comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@correo", correo);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var storedHash = reader.GetString("Contrasena");
                                if (storedHash == PasswordSecurity.HashPassword(clave))
                                {
                                    usuario = new Usuario
                                    {
                                        IdUsuario = reader.GetInt32("IdUsuario"),
                                        IdRol = reader.GetInt32("IdRol"),
                                        NombreCompleto = reader.GetString("NombreCompleto"),
                                        Correo = reader.GetString("Correo"),
                                        Contrasena = storedHash,
                                        Telefono = reader.IsDBNull("Telefono") ? null : reader.GetString("Telefono"),
                                        FechaRegistro = reader.GetDateTime("FechaRegistro"),
                                        IdRolNavigation = new Role
                                        {
                                            IdRol = reader.GetInt32("IdRol"),
                                            Nombre = reader.GetString("RolNombre"),
                                            Descripcion = reader.GetString("RolDescripcion")
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Loggear el error adecuadamente
                Console.WriteLine($"Error al autenticar usuario: {ex.Message}");
                throw; // Opcional: relanzar o manejar según tu arquitectura
            }

            return usuario;
        }
    }
}