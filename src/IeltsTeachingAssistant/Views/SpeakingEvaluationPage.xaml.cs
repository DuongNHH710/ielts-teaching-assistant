using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using IeltsTeachingAssistant.Models;

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

    public static Visibility Part2Visibility(int partNumber) => partNumber == 2 ? Visibility.Visible : Visibility.Collapsed;
    public static string FormatPartTitle(int partNumber) => $"PART {partNumber}";

    public static string GetAudioStatusText(string? audioFilePath, bool isRecording, bool isTranscribing, bool isPlaying)
    {
        if (isRecording) return "Recording speaking...";
        if (isTranscribing) return "Transcribing speaking with AI...";
        if (isPlaying) return "Playing speaking audio...";

        return string.IsNullOrEmpty(audioFilePath)
            ? "No audio file loaded. Record speaking or upload an audio file."
            : $"Audio: {System.IO.Path.GetFileName(audioFilePath)}";
    }

    public static Visibility RecordingButtonVisibility(bool isRecording) => !isRecording ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility StopRecordingButtonVisibility(bool isRecording) => isRecording ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility PlaybackControlsVisibility(string? audioFilePath)
    {
        return string.IsNullOrEmpty(audioFilePath) ? Visibility.Collapsed : Visibility.Visible;
    }

    public static Visibility StopPlaybackButtonVisibility(string? audioFilePath, bool isPlaying)
    {
        return (!string.IsNullOrEmpty(audioFilePath) && isPlaying) ? Visibility.Visible : Visibility.Collapsed;
    }

    public static Visibility GradingProgressVisibility(bool isGrading) => isGrading ? Visibility.Visible : Visibility.Collapsed;

    public static bool CanRecord(bool isRecording, bool isTranscribing, bool isPlaying)
    {
        return !isRecording && !isTranscribing && !isPlaying;
    }

    public static bool CanUpload(bool isRecording, bool isTranscribing, bool isPlaying)
    {
        return !isRecording && !isTranscribing && !isPlaying;
    }

    public static bool CanPlay(bool isRecording, bool isTranscribing, bool isPlaying)
    {
        return !isRecording && !isTranscribing && !isPlaying;
    }

    public static bool CanTranscribe(bool isRecording, bool isTranscribing, bool isPlaying)
    {
        return !isRecording && !isTranscribing && !isPlaying;
    }

    public static bool CanGrade(bool isGrading, bool isTranscribing)
    {
        return !isGrading && !isTranscribing;
    }

    private async void UploadAudioButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SpeakingPart part)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
                picker.FileTypeFilter.Add(".wav");
                picker.FileTypeFilter.Add(".mp3");
                picker.FileTypeFilter.Add(".m4a");
                picker.FileTypeFilter.Add(".ogg");
                picker.FileTypeFilter.Add(".flac");

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    await ViewModel.UploadAudioFileAsync(part, file.Path);
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
}
