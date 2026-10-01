using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class PrestamoRepositorio : IPrestamoRepositorio
{
    private const string DetalleColumnas = @"
        SELECT d.PrestamoId, d.LibroId, d.FechaDevolucion, l.Titulo, p.FechaPrestamo, p.FechaLimite
        FROM DetallePrestamo d
        INNER JOIN Prestamos p ON p.PrestamoId = d.PrestamoId
        INNER JOIN Libros l ON l.LibroId = d.LibroId";

    public async Task<int> RegistrarAsync(Prestamo prestamo)
    {
        await using var cn = ConexionBD.Crear();
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();

        try
        {
            const string sqlCabecera = @"
                INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
                OUTPUT INSERTED.PrestamoId
                VALUES (@SocioId, @FechaPrestamo, @FechaLimite, @Estado)";

            int prestamoId;
            await using (var cmd = new SqlCommand(sqlCabecera, cn, tx))
            {
                cmd.Parameters.AddWithValue("@SocioId", prestamo.SocioId);
                cmd.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
                cmd.Parameters.AddWithValue("@FechaLimite", prestamo.FechaLimite);
                cmd.Parameters.AddWithValue("@Estado", prestamo.Estado);
                prestamoId = (int)await cmd.ExecuteScalarAsync();
            }

            foreach (var detalle in prestamo.Detalles)
            {
                const string sqlDetalle =
                    "INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (@PrestamoId, @LibroId)";

                await using (var cmd = new SqlCommand(sqlDetalle, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
                    cmd.Parameters.AddWithValue("@LibroId", detalle.LibroId);
                    await cmd.ExecuteNonQueryAsync();
                }

                const string sqlStock = @"
                    UPDATE Libros SET Ejemplares = Ejemplares - 1
                    WHERE LibroId = @LibroId AND Ejemplares > 0 AND Activo = 1";

                await using (var cmd = new SqlCommand(sqlStock, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@LibroId", detalle.LibroId);
                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new ConflictoDeDatosException(
                            $"No se pudo descontar el ejemplar del libro {detalle.LibroId}.");
                }
            }

            await tx.CommitAsync();
            return prestamoId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion, string estadoSiCompleto)
    {
        await using var cn = ConexionBD.Crear();
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();

        try
        {
            const string sqlFecha = @"
                UPDATE DetallePrestamo SET FechaDevolucion = @Fecha
                WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId AND FechaDevolucion IS NULL";

            await using (var cmd = new SqlCommand(sqlFecha, cn, tx))
            {
                cmd.Parameters.AddWithValue("@Fecha", fechaDevolucion);
                cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
                cmd.Parameters.AddWithValue("@LibroId", libroId);
                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new ConflictoDeDatosException("Ese libro ya figura como devuelto.");
            }

            const string sqlStock = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LibroId";

            await using (var cmd = new SqlCommand(sqlStock, cn, tx))
            {
                cmd.Parameters.AddWithValue("@LibroId", libroId);
                await cmd.ExecuteNonQueryAsync();
            }

            const string sqlEstado = @"
                UPDATE Prestamos SET Estado = @Estado
                WHERE PrestamoId = @PrestamoId
                  AND NOT EXISTS (SELECT 1 FROM DetallePrestamo
                                  WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL)";

            await using (var cmd = new SqlCommand(sqlEstado, cn, tx))
            {
                cmd.Parameters.AddWithValue("@Estado", estadoSiCompleto);
                cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<DetallePrestamo> ObtenerDetalleAsync(int prestamoId, int libroId)
    {
        const string sql = DetalleColumnas + @"
            WHERE d.PrestamoId = @PrestamoId AND d.LibroId = @LibroId";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        await cn.OpenAsync();
        return (await LeerDetallesAsync(cmd)).FirstOrDefault();
    }

    public async Task<List<DetallePrestamo>> ListarPendientesPorSocioAsync(int socioId)
    {
        const string sql = DetalleColumnas + @"
            WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL
            ORDER BY p.FechaLimite, l.Titulo";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        await cn.OpenAsync();
        return await LeerDetallesAsync(cmd);
    }

    public async Task<int> ContarLibrosPendientesPorSocioAsync(int socioId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Prestamos p
            INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
            WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<int> ContarPendientesPorLibroAsync(int libroId)
    {
        const string sql =
            "SELECT COUNT(1) FROM DetallePrestamo WHERE LibroId = @LibroId AND FechaDevolucion IS NULL";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<List<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta)
    {
        const string sql = @"
            SELECT p.PrestamoId, s.Nombre, l.Titulo, p.FechaPrestamo, p.FechaLimite,
                   d.FechaDevolucion, p.Estado
            FROM Prestamos p
            INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
            INNER JOIN Libros l ON l.LibroId = d.LibroId
            INNER JOIN Socios s ON s.SocioId = p.SocioId
            WHERE p.FechaPrestamo >= @Desde AND p.FechaPrestamo < @HastaExclusivo
            ORDER BY p.FechaPrestamo DESC, p.PrestamoId DESC, l.Titulo";

        await using var cn = ConexionBD.Crear();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Desde", desde.Date);
        cmd.Parameters.AddWithValue("@HastaExclusivo", hasta.Date.AddDays(1));
        await cn.OpenAsync();

        var lista = new List<PrestamoReporte>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new PrestamoReporte
            {
                PrestamoId = r.GetInt32(0),
                SocioNombre = r.GetString(1),
                LibroTitulo = r.GetString(2),
                FechaPrestamo = r.GetDateTime(3),
                FechaLimite = r.GetDateTime(4),
                FechaDevolucion = r.FechaONula(5),
                Estado = r.GetString(6)
            });
        }
        return lista;
    }

    private static async Task<List<DetallePrestamo>> LeerDetallesAsync(SqlCommand cmd)
    {
        var lista = new List<DetallePrestamo>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new DetallePrestamo
            {
                PrestamoId = r.GetInt32(0),
                LibroId = r.GetInt32(1),
                FechaDevolucion = r.FechaONula(2),
                LibroTitulo = r.GetString(3),
                FechaPrestamo = r.GetDateTime(4),
                FechaLimite = r.GetDateTime(5)
            });
        }
        return lista;
    }
}
