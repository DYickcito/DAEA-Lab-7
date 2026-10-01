using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Services;
using Wpf.Ui.Controls;

namespace Biblioteca.WPF.Views;

public partial class SociosPage : Page
{
    private readonly SocioNegocio _negocio = new SocioNegocio();
    private readonly DispatcherTimer _temporizador = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };

    private Socio _editando;

    public SociosPage()
    {
        InitializeComponent();

        _temporizador.Tick += async (s, e) =>
        {
            _temporizador.Stop();
            await CargarAsync();
        };

        PreviewKeyDown += Page_PreviewKeyDown;
        Loaded += async (s, e) => await CargarAsync();
        LimpiarFormulario();
    }

    private async Task CargarAsync()
    {
        string texto = txtBuscar.Text;

        await UiServices.RunAsync(async () =>
        {
            var socios = await _negocio.BuscarAsync(texto);

            dgSocios.ItemsSource = socios;
            txtVacio.Text = string.IsNullOrWhiteSpace(texto)
                ? "Aún no hay socios. Registre el primero con el formulario."
                : $"Ningún socio coincide con \"{texto}\".";
            estadoVacio.Visibility = socios.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (_editando != null)
                dgSocios.SelectedItem = socios.FirstOrDefault(s => s.SocioId == _editando.SocioId);
        }, PonerOcupado);
    }

    private async Task GuardarAsync()
    {
        var socio = new Socio
        {
            SocioId = _editando?.SocioId ?? 0,
            DNI = txtDni.Text,
            Nombre = txtNombre.Text,
            Email = txtEmail.Text
        };
        bool esNuevo = socio.SocioId == 0;

        bool ok = await UiServices.RunAsync(
            () => esNuevo ? _negocio.InsertarAsync(socio) : _negocio.ActualizarAsync(socio),
            PonerOcupado);
        if (!ok) return;

        UiServices.ShowSuccess(esNuevo
            ? $"Se registró a {socio.Nombre}."
            : $"Se actualizó a {socio.Nombre}.");

        if (esNuevo) LimpiarFormulario();
        await CargarAsync();
    }

    private async Task EliminarAsync()
    {
        if (_editando == null) return;

        var socio = _editando;
        bool confirmado = await UiServices.ConfirmAsync(
            "Dar de baja al socio",
            $"¿Desea dar de baja a {socio.Nombre} (DNI {socio.DNI})?\nDejará de aparecer en las búsquedas y no podrá pedir préstamos.",
            "Dar de baja");
        if (!confirmado) return;

        bool ok = await UiServices.RunAsync(() => _negocio.EliminarAsync(socio.SocioId), PonerOcupado);
        if (!ok) return;

        UiServices.ShowSuccess($"Se dio de baja a {socio.Nombre}.");
        LimpiarFormulario();
        await CargarAsync();
    }

    private void Editar(Socio socio)
    {
        _editando = socio;
        txtDni.Text = socio.DNI;
        txtNombre.Text = socio.Nombre;
        txtEmail.Text = socio.Email;

        txtTituloForm.Text = "Editar socio";
        txtAyudaForm.Text = "Modifique los datos y presione Guardar. Esc para cancelar.";
        ActualizarBotones();
    }

    private void LimpiarFormulario()
    {
        _editando = null;
        dgSocios.SelectedItem = null;
        txtDni.Clear();
        txtNombre.Clear();
        txtEmail.Clear();

        txtTituloForm.Text = "Nuevo socio";
        txtAyudaForm.Text = "Complete los datos y presione Guardar.";
        ActualizarBotones();
        txtDni.Focus();
    }

    private void PonerOcupado(bool ocupado)
    {
        anilloCarga.Visibility = ocupado ? Visibility.Visible : Visibility.Collapsed;
        btnGuardar.IsEnabled = !ocupado;
        btnNuevo.IsEnabled = !ocupado;
        btnActualizar.IsEnabled = !ocupado;
        ActualizarBotones(ocupado);
    }

    private void ActualizarBotones(bool ocupado = false) =>
        btnEliminar.IsEnabled = !ocupado && _editando != null;

    private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        _temporizador.Stop();
        _temporizador.Start();
    }

    private async void btnActualizar_Click(object sender, RoutedEventArgs e) => await CargarAsync();

    private void dgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgSocios.SelectedItem is Socio socio)
            Editar(socio);
    }

    private void btnNuevo_Click(object sender, RoutedEventArgs e) => LimpiarFormulario();

    private async void btnGuardar_Click(object sender, RoutedEventArgs e) => await GuardarAsync();

    private async void btnEliminar_Click(object sender, RoutedEventArgs e) => await EliminarAsync();

    private async void Page_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && btnGuardar.IsEnabled)
        {
            e.Handled = true;
            await GuardarAsync();
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
        {
            e.Handled = true;
            LimpiarFormulario();
        }
        else if (e.Key == Key.Escape && _editando != null)
        {
            e.Handled = true;
            LimpiarFormulario();
        }
    }
}
