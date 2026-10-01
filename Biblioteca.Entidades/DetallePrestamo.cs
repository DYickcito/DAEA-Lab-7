namespace Biblioteca.Entidades;

public class DetallePrestamo
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }

    public DateTime? FechaDevolucion { get; set; }

    public string LibroTitulo { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
}
