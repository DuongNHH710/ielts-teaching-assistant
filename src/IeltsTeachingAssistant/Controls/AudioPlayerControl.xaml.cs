using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Controls;

public sealed partial class AudioPlayerControl : UserControl
{
    public static readonly DependencyProperty AudioFilePathProperty =
        DependencyProperty.Register(nameof(AudioFilePath), typeof(string), typeof(AudioPlayerControl), new PropertyMetadata(null, OnAudioFilePathChanged));

    public string AudioFilePath
    {
        get => (string)GetValue(AudioFilePathProperty);
        set => SetValue(AudioFilePathProperty, value);
    }

    private bool _isPlaying = false;

    public AudioPlayerControl()
    {
        this.InitializeComponent();
    }

    private static void OnAudioFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AudioPlayerControl control)
        {
            // Reset player when file changes
            control.TotalTimeText.Text = "00:00";
            control.CurrentTimeText.Text = "00:00";
            control.ProgressSlider.Value = 0;
            control._isPlaying = false;
            control.PlayPauseIcon.Glyph = "\uE768"; // Play icon
        }
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        _isPlaying = !_isPlaying;
        if (_isPlaying)
        {
            PlayPauseIcon.Glyph = "\uE769"; // Pause icon
            // TODO: Hook into AudioService to start playback
        }
        else
        {
            PlayPauseIcon.Glyph = "\uE768"; // Play icon
            // TODO: Hook into AudioService to pause playback
        }
    }

    private void ProgressSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        // TODO: Seek audio
    }

    private void Speed_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string speedStr && double.TryParse(speedStr, out double speed))
        {
            // TODO: Change playback speed
        }
    }
}
