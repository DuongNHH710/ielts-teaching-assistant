using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Views;

public sealed partial class ClassManagementPage : Page
{
    public ClassManagementViewModel ViewModel { get; }

    public ClassManagementPage()
    {
        ViewModel = App.Services.GetRequiredService<ClassManagementViewModel>();
        this.InitializeComponent();
        this.Loaded += (s, e) => ViewModel.LoadDataCommand.Execute(null);
    }

    private async void AddClass_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new AddClassDialog();
        dialog.XamlRoot = this.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var newClass = dialog.GetNewClass();
            await ViewModel.AddClassAsync(newClass);
        }
    }

    private void ViewPerformance_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedClass != null)
        {
            Frame.Navigate(typeof(ClassPerformancePage), ViewModel.SelectedClass.Id);
        }
    }

    public static Visibility DetailVisibility(ClassEntity? selectedClass) => selectedClass != null ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility GetLinkVisibility(string? link) => !string.IsNullOrEmpty(link) ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Safely converts a nullable string to a Uri for HyperlinkButton.NavigateUri.
    /// Returns a non-null fallback (about:blank) when the string is null, empty, or
    /// not a valid absolute URI — because the WinUI 3 x:Bind engine evaluates NavigateUri
    /// before Visibility collapses the element and does NOT accept null Uri values.
    /// The Visibility binding (GetLinkVisibility) ensures the button is never shown to users
    /// when the link is invalid.
    /// </summary>
    public static Uri ConvertToUri(string? link)
    {
        if (!string.IsNullOrWhiteSpace(link) &&
            Uri.TryCreate(link, UriKind.Absolute, out var uri))
        {
            return uri;
        }
        return new Uri("about:blank");
    }

    public static string GetModeIcon(ClassMode mode)
    {
        return mode == ClassMode.Online ? "\uE12B" : "\uE80F";
    }

    public static Microsoft.UI.Xaml.Media.Brush GetStatusBrush(ClassStatus status)
    {
        var color = status switch
        {
            ClassStatus.Upcoming => Microsoft.UI.Colors.Orange,
            ClassStatus.Active => Microsoft.UI.Colors.MediumSeaGreen,
            ClassStatus.Completed => Microsoft.UI.Colors.DodgerBlue,
            _ => Microsoft.UI.Colors.Gray
        };
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
    }

    public static string GetProgressText(int completed, int total)
    {
        return $"{completed} / {total}";
    }

    private async void SaveClassChanges_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.SaveChangesAsync();
    }

    private async void DeleteClass_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedClass == null) return;

        var confirmDialog = new ContentDialog
        {
            Title = "Confirm deletion",
            Content = $"Are you sure you want to delete the class '{ViewModel.SelectedClass.Name}'? This will also delete all students and evaluations in this class.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var classToDelete = ViewModel.SelectedClass;
            ViewModel.SelectedClass = null; // Clear selection
            await ViewModel.DeleteClassAsync(classToDelete);
        }
    }
}
