using Biblioteca.Negocio;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Biblioteca.WPF.Services;

public static class UiServices
{
    private static readonly SnackbarService Snackbar = new SnackbarService();
    private static readonly ContentDialogService Dialogs = new ContentDialogService();

    public static void Initialize(SnackbarPresenter snackbarPresenter, ContentDialogHost dialogHost)
    {
        Snackbar.SetSnackbarPresenter(snackbarPresenter);
        Dialogs.SetDialogHost(dialogHost);
    }

    public static void ShowSuccess(string message) =>
        Snackbar.Show("Listo", message, ControlAppearance.Success,
            new SymbolIcon(SymbolRegular.CheckmarkCircle24), TimeSpan.FromSeconds(3));

    public static void ShowWarning(string message) =>
        Snackbar.Show("Revise los datos", message, ControlAppearance.Caution,
            new SymbolIcon(SymbolRegular.Warning24), TimeSpan.FromSeconds(6));

    public static void ShowError(string message) =>
        Snackbar.Show("Algo salió mal", message, ControlAppearance.Danger,
            new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(8));

    public static async Task<bool> ConfirmAsync(string title, string message, string confirmText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmText,
            PrimaryButtonAppearance = ControlAppearance.Danger,
            CloseButtonText = "Cancelar"
        };

        var result = await Dialogs.ShowAsync(dialog, CancellationToken.None);
        return result == ContentDialogResult.Primary;
    }

    public static async Task<bool> RunAsync(Func<Task> action, Action<bool> setBusy)
    {
        try
        {
            setBusy(true);
            await action();
            return true;
        }
        catch (ReglaNegocioException ex)
        {
            ShowWarning(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            setBusy(false);
        }
        return false;
    }
}
