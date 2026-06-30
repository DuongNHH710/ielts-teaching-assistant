using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IeltsTeachingAssistant.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly DataImportService _dataImportService;
    private readonly IVertexAIService _vertexAIService;

    // ─── GCP Settings ───
    [ObservableProperty] private string _gcpProjectId = string.Empty;
    [ObservableProperty] private string _gcpRegion = string.Empty;
    [ObservableProperty] private string _gcpCredentialsPath = string.Empty;
    [ObservableProperty] private int _preferredModelIndex = 0;

    // ─── Test Connection state ───
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotTesting))]
    [NotifyPropertyChangedFor(nameof(TestingVisibility))]
    private bool _isTesting;

    public bool IsNotTesting => !IsTesting;
    public Visibility TestingVisibility => IsTesting ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty] private bool _connectionResultVisible;
    [ObservableProperty] private InfoBarSeverity _connectionResultSeverity = InfoBarSeverity.Informational;
    [ObservableProperty] private string _connectionResultTitle = string.Empty;
    [ObservableProperty] private string _connectionResultMessage = string.Empty;

    // ─── Password state ───
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetPasswordVisibility))]
    [NotifyPropertyChangedFor(nameof(ChangePasswordVisibility))]
    private bool _hasPassword;

    public Visibility SetPasswordVisibility => HasPassword ? Visibility.Collapsed : Visibility.Visible;
    public Visibility ChangePasswordVisibility => HasPassword ? Visibility.Visible : Visibility.Collapsed;

    public SettingsViewModel(AppDbContext context, DataImportService dataImportService, IVertexAIService vertexAIService)
    {
        _context = context;
        _dataImportService = dataImportService;
        _vertexAIService = vertexAIService;
    }

    public async Task LoadSettingsAsync()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync();
        if (settings == null) return;

        GcpProjectId = settings.GcpProjectId ?? string.Empty;
        GcpRegion = settings.GcpRegion ?? string.Empty;
        GcpCredentialsPath = settings.GcpCredentialsPath ?? string.Empty;
        PreferredModelIndex = settings.PreferredModel == "gemini-2.5-pro" ? 1 : 0;

        var storedPw = settings.PasswordHash ?? settings.AdminPassword;
        HasPassword = !string.IsNullOrWhiteSpace(storedPw);
    }

    public async Task SaveGcpSettingsAsync()
    {
        try
        {
            var settings = await _context.Settings.FirstOrDefaultAsync() ?? new Models.AppSettings();
            settings.GcpProjectId = GcpProjectId;
            settings.GcpRegion = GcpRegion;
            settings.GcpCredentialsPath = GcpCredentialsPath;
            settings.PreferredModel = PreferredModelIndex == 1 ? "gemini-2.5-pro" : "gemini-2.5-flash";

            if (settings.Id == 0) _context.Settings.Add(settings);
            else _context.Settings.Update(settings);

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Error saving settings", ex.Message);
        }
    }

    public async Task TestConnectionAsync()
    {
        IsTesting = true;
        ConnectionResultVisible = false;
        try
        {
            // Save current settings first so the service picks them up
            await SaveGcpSettingsAsync();

            // Attempt a minimal Vertex AI call — list models or ping endpoint
            // We use the existing service's credential resolution path
            await _vertexAIService.AnalyzeReadingAsync("Connection test", "OK");

            ConnectionResultSeverity = InfoBarSeverity.Success;
            ConnectionResultTitle = "Connection Successful";
            ConnectionResultMessage = $"Connected to project '{GcpProjectId}' ({GcpRegion}) successfully.";
        }
        catch (Exception ex)
        {
            ConnectionResultSeverity = InfoBarSeverity.Error;
            ConnectionResultTitle = "Connection Failed";
            ConnectionResultMessage = ex.Message;
        }
        finally
        {
            IsTesting = false;
            ConnectionResultVisible = true;
        }
    }

    public async Task SetPasswordAsync(string newPassword)
    {
        try
        {
            var settings = await _context.Settings.FirstOrDefaultAsync() ?? new Models.AppSettings();
            settings.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            settings.AdminPassword = null; // clear plain-text field

            if (settings.Id == 0) _context.Settings.Add(settings);
            else _context.Settings.Update(settings);

            await _context.SaveChangesAsync();
            HasPassword = true;

            await ShowDialogAsync("Password Saved", "Your admin password has been updated.");
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Error saving password", ex.Message);
        }
    }

    private async Task ShowDialogAsync(string title, string content)
    {
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindowInstance.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        var savePicker = new Windows.Storage.Pickers.FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Excel Workbook", new List<string>() { ".xlsx" });
        savePicker.SuggestedFileName = "DataImportTemplate";

        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            try
            {
                await _dataImportService.GenerateTemplateAsync(file.Path);
                await ShowDialogAsync("Success", "Template downloaded successfully.");
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Error saving template", ex.Message);
            }
        }
    }

    [RelayCommand]
    private async Task ImportDataAsync()
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

        openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add(".xlsx");

        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            var dialog = new IeltsTeachingAssistant.Views.DataImportDialog
            {
                XamlRoot = App.MainWindowInstance.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var strategy = dialog.SelectedStrategy;
                try
                {
                    var (classes, students) = await _dataImportService.ImportDataAsync(file.Path, strategy);
                    await ShowDialogAsync("Import Complete", $"Successfully imported/updated {classes} classes and {students} students.");
                }
                catch (Exception ex)
                {
                    await ShowDialogAsync("Error importing data", ex.Message);
                }
            }
        }
    }
}
