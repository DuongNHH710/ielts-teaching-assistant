using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using IeltsTeachingAssistant.Models;

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
        ViewModel.ClearSession();
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

    private async void UploadDocumentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is WritingTask task)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".txt");
                picker.FileTypeFilter.Add(".docx");
                picker.FileTypeFilter.Add(".pdf");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    await ViewModel.LoadDocumentContentAsync(task, file.Path);
                }
            }
            catch (System.Exception ex)
            {
                ViewModel.ErrorMessage = $"Failed to open file picker: {ex.Message}";
                ViewModel.IsErrorVisible = true;
                ViewModel.InfoBarSeverity = InfoBarSeverity.Error;
            }
        }
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
                        ViewModel.ClearSession();
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
                ViewModel.ClearSession();
                Frame.Navigate(e.SourcePageType, e.Parameter);
            }
        }
        else
        {
            ViewModel.ClearSession();
            base.OnNavigatingFrom(e);
        }
    }

    public static Visibility Task1Visibility(int taskNumber) => taskNumber == 1 ? Visibility.Visible : Visibility.Collapsed;
    public static string FormatTaskTitle(int taskNumber) => $"TASK {taskNumber}";
}
