using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Services;

namespace Biblioteca.WPF.Views;

public partial class PrestamoPage : Page
{
    private readonly PrestamoNegocio _negocio = new PrestamoNegocio();

    private readonly ObservableCollection<Libro> _seleccion = new();

    public PrestamoPage()
    {
        InitializeComponent();

        dgLibros.ItemsSource = _seleccion;
        _seleccion.CollectionChanged += (s, e) => ActualizarResumen();
        txtFechaLimite.Text = _negocio.CalcularFechaLimite(DateTime.Today).ToString("dd/MM/yyyy");

        Loaded += async (s, e) => await CargarAsync();
        ActualizarResumen();
    }

    private async Task CargarAsync()
    {
        int socioId = SocioSeleccionadoId();

        await UiServices.RunAsync(async () =>
        {
            var socios = await _negocio.ListarSociosAsync();
            var libros = await _negocio.ListarLibrosDisponiblesAsync();

            cboSocios.ItemsSource = socios;
            cboLibros.ItemsSource = libros;

            if (socioId > 0)
                cboSocios.SelectedItem = socios.FirstOrDefault(x => x.SocioId == socioId);
        }, PonerOcupado);
    }

    private async Task ActualizarPendientesAsync()
    {
        int socioId = SocioSeleccionadoId();
        if (socioId == 0)
        {
            txtPendientes.Text = "–";
            return;
        }

        await UiServices.RunAsync(async () =>
        {
            int pendientes = await _negocio.ContarPendientesAsync(socioId);
            txtPendientes.Text = $"{pendientes} / {PrestamoNegocio.MaxLibrosPendientes}";
        }, PonerOcupado);
    }

    private async Task RegistrarAsync()
    {
        int socioId = SocioSeleccionadoId();
        var libroIds = _seleccion.Select(l => l.LibroId).ToList();
        int prestamoId = 0;

        bool ok = await UiServices.RunAsync(
            async () => prestamoId = await _negocio.RegistrarPrestamoAsync(socioId, libroIds),
            PonerOcupado);
        if (!ok) return;

        UiServices.ShowSuccess(
            $"Préstamo N° {prestamoId} registrado ({libroIds.Count} libro(s)). " +
            $"Fecha límite: {_negocio.CalcularFechaLimite(DateTime.Today):dd/MM/yyyy}.");

        _seleccion.Clear();
        cboLibros.SelectedItem = null;
        await CargarAsync();
        await ActualizarPendientesAsync();
    }

    private int SocioSeleccionadoId() => cboSocios.SelectedItem is Socio socio ? socio.SocioId : 0;

    private void ActualizarResumen()
    {
        txtCantidad.Text = _seleccion.Count.ToString();
        estadoVacio.Visibility = _seleccion.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        btnQuitar.IsEnabled = _seleccion.Count > 0;
    }

    private void PonerOcupado(bool ocupado)
    {
        anilloCarga.Visibility = ocupado ? Visibility.Visible : Visibility.Collapsed;
        btnRegistrar.IsEnabled = !ocupado;
        btnAgregar.IsEnabled = !ocupado;
    }

    private async void cboSocios_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        await ActualizarPendientesAsync();

    private void btnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (cboLibros.SelectedItem is not Libro libro)
        {
            UiServices.ShowWarning("Seleccione un libro de la lista para agregarlo.");
            return;
        }
        if (_seleccion.Any(l => l.LibroId == libro.LibroId))
        {
            UiServices.ShowWarning($"\"{libro.Titulo}\" ya está en la lista.");
            return;
        }

        _seleccion.Add(libro);
        cboLibros.SelectedItem = null;
    }

    private void btnQuitar_Click(object sender, RoutedEventArgs e)
    {
        if (dgLibros.SelectedItem is Libro libro)
            _seleccion.Remove(libro);
        else
            UiServices.ShowWarning("Seleccione en la tabla el libro que desea quitar.");
    }

    private async void btnRegistrar_Click(object sender, RoutedEventArgs e) => await RegistrarAsync();
}
