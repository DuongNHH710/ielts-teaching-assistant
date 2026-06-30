using IeltsTeachingAssistant.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace IeltsTeachingAssistant.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        DataContext = ViewModel;
        this.Loaded += async (s, e) => await ViewModel.LoadSettingsAsync();
    }

    private async void BrowseCredentials_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ViewModel.GcpCredentialsPath = file.Path;
        }
    }

    private async void SaveGcpSettings_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveGcpSettingsAsync();
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.TestConnectionAsync();
    }

    private void ModelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ViewModel.PreferredModelIndex TwoWay binding handles it — just trigger save
        _ = ViewModel.SaveGcpSettingsAsync();
    }

    private void Password_Changed(object sender, RoutedEventArgs e)
    {
        bool match = NewPasswordBox.Password == ConfirmPasswordBox.Password;
        bool notEmpty = !string.IsNullOrWhiteSpace(NewPasswordBox.Password);
        PasswordMismatchText.Visibility = (!match && !string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            ? Visibility.Visible : Visibility.Collapsed;
        SetPasswordBtn.IsEnabled = match && notEmpty;
    }

    private async void SetPassword_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SetPasswordAsync(NewPasswordBox.Password);
        NewPasswordBox.Password = string.Empty;
        ConfirmPasswordBox.Password = string.Empty;
    }

    private async void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ChangePasswordDialog { XamlRoot = this.XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.SetPasswordAsync(dialog.NewPassword);
        }
    }
}
