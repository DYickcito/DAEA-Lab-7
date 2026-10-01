using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class AutorRepositorio : IAutorRepositorio
{
    private const string Columnas = "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores";

    public async Task<List<Autor>> ListarActivosAsync()
    {
        const string sql = Columnas + " WHERE Activo = 1 ORDER BY Nombre";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        return await LeerAsync(cmd);
    }

    public async Task<Autor> ObtenerPorIdAsync(int autorId)
    {
        const string sql = Columnas + " WHERE AutorId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", autorId);
        await cn.OpenAsync();
        return (await LeerAsync(cmd)).FirstOrDefault();
    }

    private static async Task<List<Autor>> LeerAsync(SqlCommand cmd)
    {
        var lista = new List<Autor>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new Autor
            {
                AutorId = r.GetInt32(0),
                Nombre = r.GetString(1),
                Nacionalidad = r.TextoONulo(2),
                Activo = r.GetBoolean(3)
            });
        }
        return lista;
    }
}
