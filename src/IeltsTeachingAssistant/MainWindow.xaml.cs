using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using IeltsTeachingAssistant.Views;
using WinRT.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using System;

namespace IeltsTeachingAssistant;

/// <summary>
/// Main application window with Custom Floating Sidebar,
/// and Glassmorphism aesthetics.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly AppWindow _appWindow;
    private bool _hasRunStartupCheck;
    private Button? _activeSidebarButton;

    public MainWindow()
    {
        this.InitializeComponent();

        // Configure window
        _appWindow = GetAppWindowForCurrentWindow();
        _appWindow.Title = "IELTS Teaching Assistant";
        _appWindow.SetIcon("Assets/app-icon.ico");

        // Set minimum window size
        _appWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));

        // Extend content into title bar for premium feel
        ExtendsContentIntoTitleBar = true;

        if (this.Content is FrameworkElement fe)
        {
            fe.Loaded += MainWindow_Loaded;
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_hasRunStartupCheck) return;
            _hasRunStartupCheck = true;
            
            // Set initial state
            SetActiveSidebarButton(NavDashboard);
            ContentFrame.Navigate(typeof(DashboardPage));

            using var scope = App.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IeltsTeachingAssistant.Data.AppDbContext>();
            var settings = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Settings);

            if (settings == null || string.IsNullOrWhiteSpace(settings.AdminPassword) || string.IsNullOrWhiteSpace(settings.GcpProjectId))
            {
                var dialog = new SetupWizardDialog(context, this) { XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
            else
            {
                var dialog = new LoginDialog(settings.AdminPassword) { XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText(@"d:\Project\ielts-teaching-assistant\crash2.txt", ex.ToString());
        }
    }

    /// <summary>
    /// Navigates to a specific page type within the main content frame.
    /// </summary>
    public void NavigateTo(Type pageType, object? parameter = null)
    {
        ContentFrame.Navigate(pageType, parameter);

        // Synchronize the navigation view selection
        if (pageType == typeof(DashboardPage)) SetActiveSidebarButton(NavDashboard);
        else if (pageType == typeof(ClassManagementPage)) SetActiveSidebarButton(NavClasses);
        else if (pageType == typeof(StudentManagementPage)) SetActiveSidebarButton(NavStudents);
        else if (pageType == typeof(EvaluationsPage)) SetActiveSidebarButton(NavMarking);
        else if (pageType == typeof(SettingsPage)) SetActiveSidebarButton(NavSettings);
    }

    /// <summary>
    /// Handles click on the custom sidebar buttons.
    /// </summary>
    private void SidebarButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var tag = button.Tag?.ToString();
            
            Type? pageType = tag switch
            {
                "Dashboard" => typeof(DashboardPage),
                "Classes" => typeof(ClassManagementPage),
                "Students" => typeof(StudentManagementPage),
                "Marking" => typeof(EvaluationsPage),
                "Settings" => typeof(SettingsPage),
                _ => typeof(DashboardPage)
            };

            if (pageType is not null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
                SetActiveSidebarButton(button);
            }
        }
    }

    private void SetActiveSidebarButton(Button button)
    {
        if (_activeSidebarButton != null)
        {
            _activeSidebarButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            _activeSidebarButton.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        }

        _activeSidebarButton = button;
        
        // Use a subtle highlight for the active state
        _activeSidebarButton.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 255, 255, 255));
        _activeSidebarButton.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
    }

    /// <summary>
    /// Gets the AppWindow for the current window handle.
    /// </summary>
    private AppWindow GetAppWindowForCurrentWindow()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }
}
