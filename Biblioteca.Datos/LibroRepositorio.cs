using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class LibroRepositorio : ILibroRepositorio
{
    private const string Columnas = @"
        SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo, a.Nombre
        FROM Libros l
        INNER JOIN Autores a ON a.AutorId = l.AutorId";

    public async Task<List<Libro>> BuscarAsync(string texto)
    {
        const string sql = Columnas + @"
            WHERE l.Activo = 1
              AND (l.Titulo LIKE @Texto OR a.Nombre LIKE @Texto)
            ORDER BY l.Titulo";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Texto", $"%{texto}%");
        await cn.OpenAsync();
        return await LeerAsync(cmd);
    }

    public async Task<Libro> ObtenerPorIdAsync(int libroId)
    {
        const string sql = Columnas + " WHERE l.LibroId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", libroId);
        await cn.OpenAsync();
        return (await LeerAsync(cmd)).FirstOrDefault();
    }

    public async Task<bool> ExisteIsbnAsync(string isbn, int excluirLibroId)
    {
        const string sql = "SELECT COUNT(1) FROM Libros WHERE ISBN = @Isbn AND LibroId <> @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Isbn", isbn);
        cmd.Parameters.AddWithValue("@Id", excluirLibroId);
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }

    public async Task InsertarAsync(Libro libro)
    {
        const string sql = @"INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares)
                             VALUES (@Titulo, @Isbn, @AutorId, @Ejemplares)";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        AgregarParametros(cmd, libro);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ActualizarAsync(Libro libro)
    {
        const string sql = @"UPDATE Libros
                             SET Titulo = @Titulo, ISBN = @Isbn, AutorId = @AutorId, Ejemplares = @Ejemplares
                             WHERE LibroId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        AgregarParametros(cmd, libro);
        cmd.Parameters.AddWithValue("@Id", libro.LibroId);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DarDeBajaAsync(int libroId)
    {
        const string sql = "UPDATE Libros SET Activo = 0 WHERE LibroId = @Id";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", libroId);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AgregarParametros(SqlCommand cmd, Libro libro)
    {
        cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
        cmd.Parameters.AddWithValue("@Isbn", libro.ISBN);
        cmd.Parameters.AddWithValue("@AutorId", libro.AutorId);
        cmd.Parameters.AddWithValue("@Ejemplares", libro.Ejemplares);
    }

    private static async Task<List<Libro>> LeerAsync(SqlCommand cmd)
    {
        var lista = new List<Libro>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new Libro
            {
                LibroId = r.GetInt32(0),
                Titulo = r.GetString(1),
                ISBN = r.GetString(2),
                AutorId = r.GetInt32(3),
                Ejemplares = r.GetInt32(4),
                Activo = r.GetBoolean(5),
                AutorNombre = r.GetString(6)
            });
        }
        return lista;
    }
}
