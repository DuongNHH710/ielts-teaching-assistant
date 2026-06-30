using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace IeltsTeachingAssistant.Views;

public sealed partial class SetupWizardDialog : ContentDialog
{
    private readonly AppDbContext _context;
    private Window _window;

    public SetupWizardDialog(AppDbContext context, Window window)
    {
        this.InitializeComponent();
        _context = context;
        _window = window;
    }

    private void Field_Changed(object sender, RoutedEventArgs e)
    {
        // GcpCredentialsPath is optional — ADC is used when empty
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(PasswordBox.Password) &&
                                 !string.IsNullOrWhiteSpace(GcpProjectIdBox.Text) &&
                                 !string.IsNullOrWhiteSpace(GcpRegionBox.Text);
    }

    private async void BrowseCredentialsButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            GcpCredentialsPathBox.Text = file.Path;
        }
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            var settings = await _context.Settings.FirstOrDefaultAsync() ?? new AppSettings();

            settings.AdminPassword = PasswordBox.Password;
            settings.GcpProjectId = GcpProjectIdBox.Text;
            settings.GcpRegion = GcpRegionBox.Text;
            settings.GcpCredentialsPath = GcpCredentialsPathBox.Text;

            if (settings.Id == 0)
            {
                _context.Settings.Add(settings);
            }
            else
            {
                _context.Settings.Update(settings);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            args.Cancel = true;
            ErrorText.Text = $"Error saving settings: {ex.Message}";
            ErrorText.Visibility = Visibility.Visible;
        }
        finally
        {
            deferral.Complete();
        }
    }
}
