using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface IPrestamoRepositorio
{
    Task<int> RegistrarAsync(Prestamo prestamo);

    Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion, string estadoSiCompleto);

    Task<DetallePrestamo> ObtenerDetalleAsync(int prestamoId, int libroId);
    Task<List<DetallePrestamo>> ListarPendientesPorSocioAsync(int socioId);
    Task<int> ContarLibrosPendientesPorSocioAsync(int socioId);
    Task<int> ContarPendientesPorLibroAsync(int libroId);

    Task<List<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta);
}
