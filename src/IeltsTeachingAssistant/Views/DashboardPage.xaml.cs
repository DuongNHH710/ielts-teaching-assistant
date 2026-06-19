using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;

namespace IeltsTeachingAssistant.Views;

/// <summary>
/// Dashboard page showing overview stats, quick actions, and recent activity.
/// </summary>
public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
        this.InitializeComponent();
        DataContext = ViewModel;

        // Set greeting based on time of day
        var hour = DateTime.Now.Hour;
        GreetingText.Text = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };

        DateText.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadDashboardDataAsync();

        // Update stats
        TotalStudentsText.Text = ViewModel.TotalStudents.ToString();
        ActiveClassesText.Text = ViewModel.ActiveClasses.ToString();
        MonthlyEvalsText.Text = ViewModel.MonthlyEvaluations.ToString();
        AvgBandText.Text = ViewModel.AverageBand > 0
            ? ViewModel.AverageBand.ToString("F1")
            : "—";

        // Show/hide empty state
        EmptyStatePanel.Visibility = ViewModel.RecentEvaluations.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        RecentActivityList.Visibility = ViewModel.RecentEvaluations.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void NewSpeakingEval_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(SpeakingEvaluationPage));
    }

    private void NewWritingEval_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(WritingEvaluationPage));
    }

    private void AddClass_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(ClassManagementPage), "add");
    }

    private void AddStudent_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(StudentManagementPage), "add");
    }

    private void ViewAllRecentActivity_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(StudentManagementPage));
    }

    private void EvaluateNewTask_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(EvaluationsPage));
    }

    private void ViewClasses_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(ClassManagementPage));
    }

    private void PendingGrading_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.MainWindowInstance;
        mainWindow.NavigateTo(typeof(StudentManagementPage));
    }

    public static Visibility AlertsListVisibility(int count) => count > 0 ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility AlertsEmptyVisibility(int count) => count == 0 ? Visibility.Visible : Visibility.Collapsed;
}

