using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using System;
using System.Linq;

namespace IeltsTeachingAssistant.Views;

public sealed partial class StudentPerformancePage : Page
{
    private readonly IServiceScope _scope;
    public StudentPerformanceViewModel ViewModel { get; }

    public StudentPerformancePage()
    {
        _scope = App.Services.CreateScope();
        ViewModel = _scope.ServiceProvider.GetRequiredService<StudentPerformanceViewModel>();
        this.InitializeComponent();
        DataContext = ViewModel;
        this.Unloaded += (s, e) => _scope.Dispose();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int studentId)
        {
            await ViewModel.InitializeAsync(studentId);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    public static Visibility DetailViewEmptyVisibility(EvaluationHistoryItem? item)
    {
        return item == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public static Visibility DetailViewContentVisibility(EvaluationHistoryItem? item)
    {
        return item != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public static Microsoft.UI.Xaml.Media.Brush ConvertHexToBrush(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);

        hex = hex.Replace("#", "");
        try
        {
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
            }
            else if (hex.Length == 8)
            {
                byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            }
        }
        catch
        {
            // fallback
        }

        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    private async void DeleteEvaluation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int evalId) return;

        var item = ViewModel.Evaluations.FirstOrDefault(ev => ev.Id == evalId);
        if (item == null) return;

        var dialog = new ContentDialog
        {
            Title = "Delete Evaluation",
            Content = $"Are you sure you want to permanently delete this {item.Type} evaluation from {item.DateString}? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteEvaluationAsync(item);
        }
    }
}
