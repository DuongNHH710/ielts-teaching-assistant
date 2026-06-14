using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using System;

namespace IeltsTeachingAssistant.Views;

public sealed partial class ClassPerformancePage : Page
{
    public ClassPerformanceViewModel ViewModel { get; }

    public ClassPerformancePage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ClassPerformanceViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int classId)
        {
            await ViewModel.InitializeAsync(classId);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void ViewStudentStats_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int studentId)
        {
            Frame.Navigate(typeof(StudentPerformancePage), studentId);
        }
    }

    public static Visibility ActivityEmptyVisibility(int count)
    {
        return count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public static Visibility ActivityListVisibility(int count)
    {
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
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
}
