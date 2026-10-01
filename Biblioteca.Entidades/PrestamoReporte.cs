namespace Biblioteca.Entidades;

public class PrestamoReporte
{
    public int PrestamoId { get; set; }
    public string SocioNombre { get; set; }
    public string LibroTitulo { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string Estado { get; set; }
}
