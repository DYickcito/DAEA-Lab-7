using System.Windows;
using System.Windows.Controls;
using Biblioteca.WPF.Services;
using Biblioteca.WPF.Views;

namespace Biblioteca.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        UiServices.Initialize(SnackbarPresenter, RootDialogHost);
        Menu.SelectedIndex = 0;
    }

    private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Menu.SelectedItem is not ListBoxItem item) return;

        string destino = item.Tag as string;

        Page pagina = destino switch
        {
            "Libros" => new LibrosPage(),
            "Socios" => new SociosPage(),
            "Prestamo" => new PrestamoPage(),
            "Devolucion" => new DevolucionPage(),
            _ => new ReportePage()
        };

        Contenido.Navigate(pagina);
    }
}
