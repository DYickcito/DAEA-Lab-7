using System.Globalization;

namespace Biblioteca.WPF.Views;

public class PendienteFila
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public string LibroTitulo { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public int DiasRetraso { get; set; }
    public decimal Multa { get; set; }

    public string MultaTexto => "S/ " + Multa.ToString("0.00", CultureInfo.InvariantCulture);
}
