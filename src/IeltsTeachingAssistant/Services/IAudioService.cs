namespace IeltsTeachingAssistant.Services;

public interface IAudioService
{
    event EventHandler? PlaybackStopped;

    Task StartRecordingAsync(string outputPath);
    string StopRecording();
    Task PlayAudioAsync(string filePath);
    void PauseAudio();
    void StopAudio();
    void SeekTo(TimeSpan position);
    TimeSpan GetDuration(string filePath);
    int EstimateLongPauses(string filePath, double silenceThresholdDb = -40, double minimumSilenceDurationSeconds = 2.0);

    bool IsRecording { get; }
    bool IsPlaying { get; }
    TimeSpan CurrentPosition { get; }
}
