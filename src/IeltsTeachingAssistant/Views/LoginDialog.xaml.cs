using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace IeltsTeachingAssistant.Views;

public sealed partial class LoginDialog : ContentDialog
{
    private readonly string _correctPassword;

    public LoginDialog(string correctPassword)
    {
        this.InitializeComponent();
        _correctPassword = correctPassword;
    }

    private void PasswordBox_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(PasswordBox.Password);
        ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (PasswordBox.Password != _correctPassword)
        {
            args.Cancel = true;
            ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }

    private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && IsPrimaryButtonEnabled)
        {
            // Simulate primary button click
            if (PasswordBox.Password != _correctPassword)
            {
                ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.Hide();
            }
        }
    }
}
