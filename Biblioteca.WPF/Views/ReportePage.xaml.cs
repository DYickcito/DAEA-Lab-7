using System.Windows;
using System.Windows.Controls;
using Biblioteca.Negocio;
using Biblioteca.WPF.Services;

namespace Biblioteca.WPF.Views;

public partial class ReportePage : Page
{
    private readonly PrestamoNegocio _negocio = new PrestamoNegocio();

    public ReportePage()
    {
        InitializeComponent();

        dpHasta.SelectedDate = DateTime.Today;
        dpDesde.SelectedDate = DateTime.Today.AddDays(-60);

        Loaded += async (s, e) => await GenerarAsync();
    }

    private async Task GenerarAsync()
    {
        var desde = dpDesde.SelectedDate;
        var hasta = dpHasta.SelectedDate;

        await UiServices.RunAsync(async () =>
        {
            var filas = await _negocio.ReporteAsync(desde, hasta);

            dgReporte.ItemsSource = filas;
            estadoVacio.Visibility = filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            int prestamos = filas.Select(f => f.PrestamoId).Distinct().Count();
            txtResumen.Text = $"{prestamos} préstamo(s) · {filas.Count} libro(s)";
        }, PonerOcupado);
    }

    private void PonerOcupado(bool ocupado)
    {
        anilloCarga.Visibility = ocupado ? Visibility.Visible : Visibility.Collapsed;
        btnBuscar.IsEnabled = !ocupado;
    }

    private async void btnBuscar_Click(object sender, RoutedEventArgs e) => await GenerarAsync();
}
