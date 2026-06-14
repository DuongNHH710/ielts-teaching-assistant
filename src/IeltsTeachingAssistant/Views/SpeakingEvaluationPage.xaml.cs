using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;

namespace IeltsTeachingAssistant.Views;

public sealed partial class SpeakingEvaluationPage : Page
{
    public SpeakingEvaluationViewModel ViewModel { get; }

    public SpeakingEvaluationPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SpeakingEvaluationViewModel>();
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

    public static Visibility Part2Visibility(int partNumber) => partNumber == 2 ? Visibility.Visible : Visibility.Collapsed;
    public static string FormatPartTitle(int partNumber) => $"PART {partNumber}";
}
