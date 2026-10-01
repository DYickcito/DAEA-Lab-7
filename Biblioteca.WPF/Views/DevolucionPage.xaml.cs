using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Services;

namespace Biblioteca.WPF.Views;

public partial class DevolucionPage : Page
{
    private readonly PrestamoNegocio _negocio = new PrestamoNegocio();

    public DevolucionPage()
    {
        InitializeComponent();

        txtSubtitulo.Text = $"Registre la devolución de un libro. Multa por retraso: S/ " +
                            $"{PrestamoNegocio.MultaPorDia.ToString("0.00", CultureInfo.InvariantCulture)} por día.";
        txtTarifa.Text = $"S/ {PrestamoNegocio.MultaPorDia.ToString("0.00", CultureInfo.InvariantCulture)} por cada día de retraso";

        Loaded += async (s, e) => await CargarSociosAsync();
        MostrarDetalle(null);
    }

    private async Task CargarSociosAsync()
    {
        await UiServices.RunAsync(
            async () => cboSocios.ItemsSource = await _negocio.ListarSociosAsync(),
            PonerOcupado);
    }

    private async Task CargarPendientesAsync()
    {
        int socioId = cboSocios.SelectedItem is Socio socio ? socio.SocioId : 0;
        if (socioId == 0)
        {
            dgPendientes.ItemsSource = null;
            txtPendientes.Text = "–";
            return;
        }

        await UiServices.RunAsync(async () =>
        {
            var pendientes = await _negocio.ListarPendientesAsync(socioId);
            var hoy = DateTime.Now;

            var filas = pendientes.Select(d => new PendienteFila
            {
                PrestamoId = d.PrestamoId,
                LibroId = d.LibroId,
                LibroTitulo = d.LibroTitulo,
                FechaPrestamo = d.FechaPrestamo,
                FechaLimite = d.FechaLimite,
                DiasRetraso = _negocio.DiasDeRetraso(d.FechaLimite, hoy),
                Multa = _negocio.CalcularMulta(d.FechaLimite, hoy)
            }).ToList();

            dgPendientes.ItemsSource = filas;
            txtPendientes.Text = filas.Count.ToString();
            txtVacio.Text = "Este socio no tiene libros pendientes de devolución.";
            estadoVacio.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            MostrarDetalle(null);
        }, PonerOcupado);
    }

    private async Task DevolverAsync()
    {
        if (dgPendientes.SelectedItem is not PendienteFila fila)
        {
            UiServices.ShowWarning("Seleccione en la tabla el libro que se devuelve.");
            return;
        }

        decimal multa = 0;
        bool ok = await UiServices.RunAsync(
            async () => multa = await _negocio.RegistrarDevolucionAsync(fila.PrestamoId, fila.LibroId),
            PonerOcupado);
        if (!ok) return;

        string multaTexto = "S/ " + multa.ToString("0.00", CultureInfo.InvariantCulture);
        string mensaje = multa > 0
            ? $"\"{fila.LibroTitulo}\" devuelto con retraso. Multa a cobrar: {multaTexto}."
            : $"\"{fila.LibroTitulo}\" devuelto a tiempo. Sin multa.";

        UiServices.ShowSuccess(mensaje);
        await CargarPendientesAsync();

        txtResultado.Text = mensaje;
        panelResultado.Visibility = Visibility.Visible;
    }

    private void MostrarDetalle(PendienteFila fila)
    {
        panelResultado.Visibility = Visibility.Collapsed;
        btnDevolver.IsEnabled = fila != null;

        txtLibro.Text = fila?.LibroTitulo ?? "–";
        txtLimite.Text = fila?.FechaLimite.ToString("dd/MM/yyyy") ?? "–";
        txtRetraso.Text = fila == null ? "–" : $"{fila.DiasRetraso} día(s)";
        txtMulta.Text = fila?.MultaTexto ?? "S/ 0.00";
    }

    private void PonerOcupado(bool ocupado)
    {
        anilloCarga.Visibility = ocupado ? Visibility.Visible : Visibility.Collapsed;
        btnDevolver.IsEnabled = !ocupado && dgPendientes.SelectedItem != null;
    }

    private async void cboSocios_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        await CargarPendientesAsync();

    private void dgPendientes_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        MostrarDetalle(dgPendientes.SelectedItem as PendienteFila);

    private async void btnDevolver_Click(object sender, RoutedEventArgs e) => await DevolverAsync();
}
