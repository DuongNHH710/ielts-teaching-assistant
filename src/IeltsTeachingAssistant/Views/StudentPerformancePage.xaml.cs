using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using System;

namespace IeltsTeachingAssistant.Views;

public sealed partial class StudentPerformancePage : Page
{
    public StudentPerformanceViewModel ViewModel { get; }

    public StudentPerformancePage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StudentPerformanceViewModel>();
        DataContext = ViewModel;
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
}
