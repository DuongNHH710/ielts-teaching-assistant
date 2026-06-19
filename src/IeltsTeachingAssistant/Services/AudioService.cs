using NAudio.Wave;

namespace IeltsTeachingAssistant.Services;

public class AudioService : IAudioService
{
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _waveWriter;
    private AudioFileReader? _audioReader;
    private WaveOutEvent? _waveOut;

    private string _currentRecordingPath = string.Empty;

    public event EventHandler? PlaybackStopped;

    public bool IsRecording { get; private set; }
    public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;

    public TimeSpan CurrentPosition => _audioReader?.CurrentTime ?? TimeSpan.Zero;

    public async Task StartRecordingAsync(string outputPath)
    {
        if (IsRecording) return;

        _currentRecordingPath = outputPath;
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _waveIn = new WaveInEvent();
        _waveIn.WaveFormat = new WaveFormat(44100, 1);
        _waveWriter = new WaveFileWriter(outputPath, _waveIn.WaveFormat);

        _waveIn.DataAvailable += (s, a) =>
        {
            _waveWriter.Write(a.Buffer, 0, a.BytesRecorded);
        };

        _waveIn.StartRecording();
        IsRecording = true;

        await Task.CompletedTask;
    }

    public string StopRecording()
    {
        if (!IsRecording) return string.Empty;

        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        _waveIn = null;

        _waveWriter?.Dispose();
        _waveWriter = null;

        IsRecording = false;
        return _currentRecordingPath;
    }

    public async Task PlayAudioAsync(string filePath)
    {
        if (!File.Exists(filePath)) return;

        StopAudio();

        _audioReader = new AudioFileReader(filePath);
        _waveOut = new WaveOutEvent();
        _waveOut.PlaybackStopped += OnPlaybackStopped;
        _waveOut.Init(_audioReader);
        _waveOut.Play();

        await Task.CompletedTask;
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        PlaybackStopped?.Invoke(this, EventArgs.Empty);
    }

    public void PauseAudio()
    {
        if (_waveOut?.PlaybackState == PlaybackState.Playing)
        {
            _waveOut.Pause();
        }
    }

    public void StopAudio()
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        _audioReader?.Dispose();
        _audioReader = null;
    }

    public void SeekTo(TimeSpan position)
    {
        if (_audioReader != null)
        {
            _audioReader.CurrentTime = position;
        }
    }

    public TimeSpan GetDuration(string filePath)
    {
        if (!File.Exists(filePath)) return TimeSpan.Zero;

        using var reader = new AudioFileReader(filePath);
        return reader.TotalTime;
    }

    public int EstimateLongPauses(string filePath, double silenceThresholdDb = -40, double minimumSilenceDurationSeconds = 2.0)
    {
        if (!File.Exists(filePath)) return 0;
        try
        {
            using var reader = new AudioFileReader(filePath);
            var samples = new float[4096];
            int bytesRead;
            double silenceThresholdLinear = Math.Pow(10, silenceThresholdDb / 20.0);

            int consecutiveSilentSamples = 0;
            // Target samples is sampleRate * channels * seconds
            int targetSilentSamples = (int)(reader.WaveFormat.SampleRate * reader.WaveFormat.Channels * minimumSilenceDurationSeconds);

            int pauseCount = 0;
            bool inSilence = false;

            while ((bytesRead = reader.Read(samples, 0, samples.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    var sampleValue = Math.Abs(samples[i]);
                    if (sampleValue < silenceThresholdLinear)
                    {
                        consecutiveSilentSamples++;
                        if (consecutiveSilentSamples >= targetSilentSamples && !inSilence)
                        {
                            inSilence = true;
                            pauseCount++;
                        }
                    }
                    else
                    {
                        consecutiveSilentSamples = 0;
                        inSilence = false;
                    }
                }
            }
            return pauseCount;
        }
        catch
        {
            return 0;
        }
    }
}
