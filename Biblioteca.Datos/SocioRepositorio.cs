using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class SocioRepositorio : ISocioRepositorio
{
    private const string Columnas = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios";

    public async Task<List<Socio>> BuscarAsync(string texto)
    {
        const string sql = Columnas + @"
            WHERE Activo = 1
              AND (Nombre LIKE @Texto OR DNI LIKE @Texto)
            ORDER BY Nombre";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Texto", $"%{texto}%");
        await cn.OpenAsync();
        return await LeerAsync(cmd);
    }

    public async Task<Socio> ObtenerPorIdAsync(int socioId)
    {
        const string sql = Columnas + " WHERE SocioId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", socioId);
        await cn.OpenAsync();
        return (await LeerAsync(cmd)).FirstOrDefault();
    }

    public async Task<bool> ExisteDniAsync(string dni, int excluirSocioId)
    {
        const string sql = "SELECT COUNT(1) FROM Socios WHERE DNI = @Dni AND SocioId <> @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Dni", dni);
        cmd.Parameters.AddWithValue("@Id", excluirSocioId);
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }

    public async Task InsertarAsync(Socio socio)
    {
        const string sql = "INSERT INTO Socios (DNI, Nombre, Email) VALUES (@Dni, @Nombre, @Email)";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        AgregarParametros(cmd, socio);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ActualizarAsync(Socio socio)
    {
        const string sql = "UPDATE Socios SET DNI = @Dni, Nombre = @Nombre, Email = @Email WHERE SocioId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        AgregarParametros(cmd, socio);
        cmd.Parameters.AddWithValue("@Id", socio.SocioId);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DarDeBajaAsync(int socioId)
    {
        const string sql = "UPDATE Socios SET Activo = 0 WHERE SocioId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", socioId);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AgregarParametros(SqlCommand cmd, Socio socio)
    {
        cmd.Parameters.AddWithValue("@Dni", socio.DNI);
        cmd.Parameters.AddWithValue("@Nombre", socio.Nombre);
        cmd.Parameters.AddWithValue("@Email", (object)socio.Email ?? DBNull.Value);
    }

    private static async Task<List<Socio>> LeerAsync(SqlCommand cmd)
    {
        var lista = new List<Socio>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new Socio
            {
                SocioId = r.GetInt32(0),
                DNI = r.GetString(1),
                Nombre = r.GetString(2),
                Email = r.TextoONulo(3),
                Activo = r.GetBoolean(4)
            });
        }
        return lista;
    }
}
