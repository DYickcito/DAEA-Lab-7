using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class PrestamoNegocio
{
    public const int MaxLibrosPendientes = 3;
    public const int DiasDePrestamo = 7;
    public const decimal MultaPorDia = 1.50m;

    private readonly IPrestamoRepositorio _prestamos;
    private readonly ISocioRepositorio _socios;
    private readonly ILibroRepositorio _libros;

    public PrestamoNegocio()
        : this(new PrestamoRepositorio(), new SocioRepositorio(), new LibroRepositorio()) { }

    public PrestamoNegocio(IPrestamoRepositorio prestamos, ISocioRepositorio socios, ILibroRepositorio libros)
    {
        _prestamos = prestamos;
        _socios = socios;
        _libros = libros;
    }

    public Task<List<Socio>> ListarSociosAsync() => _socios.BuscarAsync("");

    public async Task<List<Libro>> ListarLibrosDisponiblesAsync()
    {
        var libros = await _libros.BuscarAsync("");
        return libros.Where(l => l.Ejemplares > 0).ToList();
    }

    public Task<int> ContarPendientesAsync(int socioId) => _prestamos.ContarLibrosPendientesPorSocioAsync(socioId);

    public Task<List<DetallePrestamo>> ListarPendientesAsync(int socioId) =>
        _prestamos.ListarPendientesPorSocioAsync(socioId);

    public DateTime CalcularFechaLimite(DateTime fechaPrestamo) => fechaPrestamo.Date.AddDays(DiasDePrestamo);

    public async Task<int> RegistrarPrestamoAsync(int socioId, IReadOnlyList<int> libroIds)
    {
        if (socioId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un socio.");
        if (libroIds == null || libroIds.Count == 0)
            throw new ReglaNegocioException("Debe agregar al menos un libro al préstamo.");
        if (libroIds.Distinct().Count() != libroIds.Count)
            throw new ReglaNegocioException("No se puede prestar el mismo libro dos veces en un préstamo.");

        var socio = await _socios.ObtenerPorIdAsync(socioId);
        if (socio == null || !socio.Activo)
            throw new ReglaNegocioException("El socio no existe o está dado de baja.");

        int pendientes = await _prestamos.ContarLibrosPendientesPorSocioAsync(socioId);
        if (pendientes + libroIds.Count > MaxLibrosPendientes)
            throw new ReglaNegocioException(
                $"{socio.Nombre} tiene {pendientes} libro(s) pendiente(s); solo puede tener {MaxLibrosPendientes} " +
                $"a la vez, por lo que puede llevarse {Math.Max(0, MaxLibrosPendientes - pendientes)} más.");

        var ahora = DateTime.Now;
        var prestamo = new Prestamo
        {
            SocioId = socioId,
            FechaPrestamo = ahora,
            FechaLimite = CalcularFechaLimite(ahora),
            Estado = EstadoPrestamo.Pendiente
        };

        foreach (int libroId in libroIds)
        {
            var libro = await _libros.ObtenerPorIdAsync(libroId);
            if (libro == null || !libro.Activo)
                throw new ReglaNegocioException("Uno de los libros no existe o está dado de baja.");
            if (libro.Ejemplares <= 0)
                throw new ReglaNegocioException($"No quedan ejemplares disponibles de \"{libro.Titulo}\".");

            prestamo.Detalles.Add(new DetallePrestamo { LibroId = libroId });
        }

        try
        {
            return await _prestamos.RegistrarAsync(prestamo);
        }
        catch (ConflictoDeDatosException)
        {
            throw new ReglaNegocioException(
                "Mientras se registraba el préstamo, otro usuario se llevó el último ejemplar de un libro. No se guardó nada; revise la lista e intente de nuevo.");
        }
    }

    public decimal CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion) =>
        DiasDeRetraso(fechaLimite, fechaDevolucion) * MultaPorDia;

    public int DiasDeRetraso(DateTime fechaLimite, DateTime fechaDevolucion) =>
        Math.Max(0, (fechaDevolucion.Date - fechaLimite.Date).Days);

    public async Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId)
    {
        var detalle = await _prestamos.ObtenerDetalleAsync(prestamoId, libroId);
        if (detalle == null)
            throw new ReglaNegocioException("No se encontró ese libro en el préstamo indicado.");
        if (detalle.FechaDevolucion != null)
            throw new ReglaNegocioException($"\"{detalle.LibroTitulo}\" ya fue devuelto el {detalle.FechaDevolucion:dd/MM/yyyy}.");

        var fechaDevolucion = DateTime.Now;
        decimal multa = CalcularMulta(detalle.FechaLimite, fechaDevolucion);

        try
        {
            await _prestamos.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion, EstadoPrestamo.Devuelto);
        }
        catch (ConflictoDeDatosException)
        {
            throw new ReglaNegocioException("Ese libro ya fue devuelto por otro usuario. Actualice la lista.");
        }

        return multa;
    }

    public Task<List<PrestamoReporte>> ReporteAsync(DateTime? desde, DateTime? hasta)
    {
        if (desde == null || hasta == null)
            throw new ReglaNegocioException("Indique la fecha inicial y la fecha final del reporte.");
        if (desde.Value.Date > hasta.Value.Date)
            throw new ReglaNegocioException("La fecha inicial no puede ser posterior a la fecha final.");

        return _prestamos.ReporteAsync(desde.Value, hasta.Value);
    }
}
