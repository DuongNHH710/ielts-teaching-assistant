using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;

namespace IeltsTeachingAssistant.Views;

public sealed partial class WritingEvaluationPage : Page
{
    public WritingEvaluationViewModel ViewModel { get; }

    public WritingEvaluationPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<WritingEvaluationViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }

    private void Slider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        ViewModel.UpdateAverages();
    }

    public static Visibility Task1Visibility(int taskNumber) => taskNumber == 1 ? Visibility.Visible : Visibility.Collapsed;
    public static string FormatTaskTitle(int taskNumber) => $"TASK {taskNumber}";
}
