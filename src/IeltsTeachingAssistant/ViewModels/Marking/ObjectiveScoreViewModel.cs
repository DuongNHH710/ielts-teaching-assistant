using CommunityToolkit.Mvvm.ComponentModel;

namespace IeltsTeachingAssistant.ViewModels.Marking
{
    public partial class ObjectiveScoreViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _rawScore;

        [ObservableProperty]
        private string _trainingType = "Academic"; // "Academic" or "General"

        public string TrainingTypeDisplayText => $"Based on {TrainingType} conversion table";

        [ObservableProperty]
        private double _calculatedBand;

        partial void OnRawScoreChanged(int value)
        {
            CalculateBandScore();
        }

        partial void OnTrainingTypeChanged(string value)
        {
            OnPropertyChanged(nameof(TrainingTypeDisplayText));
            CalculateBandScore();
        }

        private void CalculateBandScore()
        {
            if (RawScore < 0 || RawScore > 40)
            {
                CalculatedBand = 0;
                return;
            }

            // Simplified hardcoded lookup tables based on standard IELTS conversion
            if (TrainingType == "Academic")
            {
                // Example Academic Reading / Listening (often similar)
                if (RawScore >= 39) CalculatedBand = 9.0;
                else if (RawScore >= 37) CalculatedBand = 8.5;
                else if (RawScore >= 35) CalculatedBand = 8.0;
                else if (RawScore >= 33) CalculatedBand = 7.5;
                else if (RawScore >= 30) CalculatedBand = 7.0;
                else if (RawScore >= 27) CalculatedBand = 6.5;
                else if (RawScore >= 23) CalculatedBand = 6.0;
                else if (RawScore >= 19) CalculatedBand = 5.5;
                else if (RawScore >= 15) CalculatedBand = 5.0;
                else if (RawScore >= 13) CalculatedBand = 4.5;
                else if (RawScore >= 10) CalculatedBand = 4.0;
                else CalculatedBand = 0.0; // Simplify lower bands
            }
            else // General Training
            {
                // General Training Reading (usually requires more correct answers for same band)
                if (RawScore >= 40) CalculatedBand = 9.0;
                else if (RawScore >= 39) CalculatedBand = 8.5;
                else if (RawScore >= 37) CalculatedBand = 8.0;
                else if (RawScore >= 36) CalculatedBand = 7.5;
                else if (RawScore >= 34) CalculatedBand = 7.0;
                else if (RawScore >= 32) CalculatedBand = 6.5;
                else if (RawScore >= 30) CalculatedBand = 6.0;
                else if (RawScore >= 27) CalculatedBand = 5.5;
                else if (RawScore >= 23) CalculatedBand = 5.0;
                else if (RawScore >= 19) CalculatedBand = 4.5;
                else if (RawScore >= 15) CalculatedBand = 4.0;
                else CalculatedBand = 0.0;
            }
        }
    }
}
