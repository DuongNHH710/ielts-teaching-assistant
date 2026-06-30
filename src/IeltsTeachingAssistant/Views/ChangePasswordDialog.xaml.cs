using IeltsTeachingAssistant.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Views;

public sealed partial class ChangePasswordDialog : ContentDialog
{
    public string NewPassword => NewPasswordBox.Password;

    public ChangePasswordDialog()
    {
        this.InitializeComponent();
    }

    private void Fields_Changed(object sender, RoutedEventArgs e)
    {
        bool newMatch = NewPasswordBox.Password == ConfirmPasswordBox.Password;
        bool newNotEmpty = !string.IsNullOrWhiteSpace(NewPasswordBox.Password);
        bool currentNotEmpty = !string.IsNullOrWhiteSpace(CurrentPasswordBox.Password);

        MismatchText.Visibility = (!newMatch && !string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            ? Visibility.Visible : Visibility.Collapsed;

        IsPrimaryButtonEnabled = currentNotEmpty && newNotEmpty && newMatch;
    }

    private async void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            // Verify current password
            using var scope = App.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settings = await context.Settings.FirstOrDefaultAsync();

            var currentStored = settings?.PasswordHash ?? settings?.AdminPassword;
            var entered = CurrentPasswordBox.Password;

            bool valid = false;
            if (!string.IsNullOrEmpty(currentStored))
            {
                // Try BCrypt first, fall back to plain text comparison
                try { valid = BCrypt.Net.BCrypt.Verify(entered, currentStored); }
                catch { valid = entered == currentStored; }
            }

            if (!valid)
            {
                args.Cancel = true;
                ErrorText.Text = "Current password is incorrect.";
                ErrorText.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            args.Cancel = true;
            ErrorText.Text = $"Error: {ex.Message}";
            ErrorText.Visibility = Visibility.Visible;
        }
        finally
        {
            deferral.Complete();
        }
    }
}
