using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Services;
using Wpf.Ui.Controls;

namespace Biblioteca.WPF.Views;

public partial class LibrosPage : Page
{
    private readonly LibroNegocio _negocio = new LibroNegocio();

    private readonly DispatcherTimer _temporizador = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };

    private Libro _editando;

    public LibrosPage()
    {
        InitializeComponent();

        _temporizador.Tick += async (s, e) =>
        {
            _temporizador.Stop();
            await CargarAsync();
        };

        PreviewKeyDown += Page_PreviewKeyDown;
        Loaded += async (s, e) =>
        {
            await CargarAutoresAsync();
            await CargarAsync();
        };
        LimpiarFormulario();
    }

    private async Task CargarAutoresAsync()
    {
        await UiServices.RunAsync(async () =>
        {
            cboAutor.ItemsSource = await _negocio.ListarAutoresAsync();
            if (_editando != null) cboAutor.SelectedValue = _editando.AutorId;
        }, PonerOcupado);
    }

    private async Task CargarAsync()
    {
        string texto = txtBuscar.Text;

        await UiServices.RunAsync(async () =>
        {
            var libros = await _negocio.BuscarAsync(texto);

            dgLibros.ItemsSource = libros;
            txtVacio.Text = string.IsNullOrWhiteSpace(texto)
                ? "Aún no hay libros. Registre el primero con el formulario."
                : $"Ningún libro coincide con \"{texto}\".";
            estadoVacio.Visibility = libros.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (_editando != null)
                dgLibros.SelectedItem = libros.FirstOrDefault(l => l.LibroId == _editando.LibroId);
        }, PonerOcupado);
    }

    private async Task GuardarAsync()
    {
        var libro = new Libro
        {
            LibroId = _editando?.LibroId ?? 0,
            Titulo = txtTitulo.Text,
            ISBN = txtIsbn.Text,
            AutorId = cboAutor.SelectedValue is int id ? id : 0,
            Ejemplares = (int)(nbEjemplares.Value ?? 0)
        };
        bool esNuevo = libro.LibroId == 0;

        bool ok = await UiServices.RunAsync(
            () => esNuevo ? _negocio.InsertarAsync(libro) : _negocio.ActualizarAsync(libro),
            PonerOcupado);
        if (!ok) return;

        UiServices.ShowSuccess(esNuevo
            ? $"Se registró \"{libro.Titulo}\"."
            : $"Se actualizó \"{libro.Titulo}\".");

        if (esNuevo) LimpiarFormulario();
        await CargarAsync();
    }

    private async Task EliminarAsync()
    {
        if (_editando == null) return;

        var libro = _editando;
        bool confirmado = await UiServices.ConfirmAsync(
            "Dar de baja el libro",
            $"¿Desea dar de baja \"{libro.Titulo}\"?\nDejará de aparecer en las búsquedas y en los préstamos.",
            "Dar de baja");
        if (!confirmado) return;

        bool ok = await UiServices.RunAsync(() => _negocio.EliminarAsync(libro.LibroId), PonerOcupado);
        if (!ok) return;

        UiServices.ShowSuccess($"Se dio de baja \"{libro.Titulo}\".");
        LimpiarFormulario();
        await CargarAsync();
    }

    private void Editar(Libro libro)
    {
        _editando = libro;
        txtTitulo.Text = libro.Titulo;
        txtIsbn.Text = libro.ISBN;
        cboAutor.SelectedValue = libro.AutorId;
        nbEjemplares.Value = libro.Ejemplares;

        txtTituloForm.Text = "Editar libro";
        txtAyudaForm.Text = "Modifique los datos y presione Guardar. Esc para cancelar.";
        ActualizarBotones();
    }

    private void LimpiarFormulario()
    {
        _editando = null;
        dgLibros.SelectedItem = null;
        txtTitulo.Clear();
        txtIsbn.Clear();
        cboAutor.SelectedItem = null;
        nbEjemplares.Value = null;

        txtTituloForm.Text = "Nuevo libro";
        txtAyudaForm.Text = "Complete los datos y presione Guardar.";
        ActualizarBotones();
        txtTitulo.Focus();
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

    private void dgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgLibros.SelectedItem is Libro libro)
            Editar(libro);
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
