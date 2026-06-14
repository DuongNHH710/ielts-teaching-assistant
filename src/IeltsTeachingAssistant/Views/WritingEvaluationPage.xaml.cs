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
        ViewModel.HasUnsavedChanges = true;
    }

    private void TeacherComment_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.HasUnsavedChanges = true;
    }

    protected override async void OnNavigatingFrom(Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
    {
        if (ViewModel.HasUnsavedChanges)
        {
            e.Cancel = true;
            
            var dialog = new ContentDialog
            {
                Title = "Unsaved Changes",
                Content = "You have unsaved teacher comments or score changes. Do you want to save them before leaving?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Discard",
                CloseButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot
            };
            
            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await ViewModel.SaveSessionAsync();
                    if (!ViewModel.HasUnsavedChanges)
                    {
                        var frame = Frame;
                        var targetPage = e.SourcePageType;
                        var parameter = e.Parameter;
                        // Force navigation after cleanup
                        frame.Navigate(targetPage, parameter);
                    }
                }
                catch (System.Exception ex)
                {
                    var msg = ex.Message;
                    if (ex.InnerException != null) msg += "\nInner: " + ex.InnerException.Message;
                    ViewModel.ErrorMessage = $"Failed to save: {msg}";
                    ViewModel.IsErrorVisible = true;
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                ViewModel.HasUnsavedChanges = false;
                Frame.Navigate(e.SourcePageType, e.Parameter);
            }
        }
        base.OnNavigatingFrom(e);
    }

    public static Visibility Task1Visibility(int taskNumber) => taskNumber == 1 ? Visibility.Visible : Visibility.Collapsed;
    public static string FormatTaskTitle(int taskNumber) => $"TASK {taskNumber}";
}
