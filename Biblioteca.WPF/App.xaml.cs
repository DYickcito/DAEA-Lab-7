using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Biblioteca.WPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplicationAccentColorManager.Apply(Rgb(0x2F, 0x5D, 0x50),
            Rgb(0x26, 0x4D, 0x42), Rgb(0x1D, 0x3D, 0x34), Rgb(0x14, 0x2D, 0x26));
        ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.None, false);
        ApplicationAccentColorManager.Apply(Rgb(0x2F, 0x5D, 0x50),
            Rgb(0x26, 0x4D, 0x42), Rgb(0x1D, 0x3D, 0x34), Rgb(0x14, 0x2D, 0x26));
    }

    private static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b);
}
