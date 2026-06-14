namespace IeltsTeachingAssistant.Services;

public interface IAudioService
{
    Task StartRecordingAsync(string outputPath);
    string StopRecording();
    Task PlayAudioAsync(string filePath);
    void PauseAudio();
    void StopAudio();
    void SeekTo(TimeSpan position);
    TimeSpan GetDuration(string filePath);
    
    bool IsRecording { get; }
    bool IsPlaying { get; }
    TimeSpan CurrentPosition { get; }
}
