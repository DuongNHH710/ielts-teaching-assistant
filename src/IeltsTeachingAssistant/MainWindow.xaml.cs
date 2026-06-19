using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using IeltsTeachingAssistant.Views;
using WinRT.Interop;
using Microsoft.Extensions.DependencyInjection;

namespace IeltsTeachingAssistant;

/// <summary>
/// Main application window with NavigationView sidebar,
/// Mica backdrop, and custom title bar.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly AppWindow _appWindow;

    private bool _hasRunStartupCheck;

    public MainWindow()
    {
        this.InitializeComponent();

        // Configure window
        _appWindow = GetAppWindowForCurrentWindow();
        _appWindow.Title = "IELTS Teaching Assistant";
        _appWindow.SetIcon("Assets/app-icon.ico");

        // Set minimum window size
        _appWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));

        // Apply Mica backdrop (follows system theme automatically)
        TrySetMicaBackdrop();

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
        if (pageType == typeof(SpeakingEvaluationPage))
        {
            ContentFrame.Navigate(typeof(EvaluationsPage), "Speaking");
            NavView.SelectedItem = NavEvaluations;
            return;
        }
        if (pageType == typeof(WritingEvaluationPage))
        {
            ContentFrame.Navigate(typeof(EvaluationsPage), "Writing");
            NavView.SelectedItem = NavEvaluations;
            return;
        }

        ContentFrame.Navigate(pageType, parameter);

        // Synchronize the navigation view selection
        if (pageType == typeof(DashboardPage)) NavView.SelectedItem = NavDashboard;
        else if (pageType == typeof(ClassManagementPage)) NavView.SelectedItem = NavClasses;
        else if (pageType == typeof(StudentManagementPage)) NavView.SelectedItem = NavStudents;
        else if (pageType == typeof(EvaluationsPage)) NavView.SelectedItem = NavEvaluations;
    }

    /// <summary>
    /// Applies Mica material backdrop for Windows 11 premium appearance.
    /// Falls back to Acrylic on older Windows 10 versions.
    /// </summary>
    private void TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            SystemBackdrop = new MicaBackdrop
            {
                Kind = MicaKind.Base
            };
        }
        else if (DesktopAcrylicController.IsSupported())
        {
            SystemBackdrop = new DesktopAcrylicBackdrop();
        }
    }

    /// <summary>
    /// Handles navigation selection changes.
    /// Routes to the appropriate page based on the selected tag.
    /// </summary>
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            NavigateToPage(tag);
        }
    }

    /// <summary>
    /// Navigates to a page based on the tag string.
    /// </summary>
    private void NavigateToPage(string? tag)
    {
        Type? pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Classes" => typeof(ClassManagementPage),
            "Evaluations" => typeof(EvaluationsPage),
            "Students" => typeof(StudentManagementPage),
            _ => typeof(DashboardPage)
        };

        if (pageType is not null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    /// <summary>
    /// Called when the NavigationView is loaded.
    /// Sets the initial selected item to Dashboard.
    /// </summary>
    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = NavDashboard;
        ContentFrame.Navigate(typeof(DashboardPage));
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
