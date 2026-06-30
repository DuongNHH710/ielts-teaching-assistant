using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace IeltsTeachingAssistant.ViewModels.Marking
{
    public partial class SubjectiveGridViewModel : ObservableObject
    {
        public ObservableCollection<string> CriteriaHeaders { get; } = new();
        public ObservableCollection<int> BandRows { get; } = new();
        public ObservableCollection<CriteriaCellViewModel> Cells { get; } = new();

        [ObservableProperty]
        private double _overallBandScore;

        // Key: CriteriaType, Value: Score
        [ObservableProperty]
        private ObservableCollection<CriterionScore> _criterionScores = new();

        public void InitializeGrid(IEnumerable<string> criteria, IEnumerable<int> bands, IEnumerable<CriteriaCellViewModel> cells)
        {
            // Clean up old subscriptions
            foreach (var cell in Cells)
            {
                cell.PropertyChanged -= Cell_PropertyChanged;
            }

            CriteriaHeaders.Clear();
            foreach (var c in criteria) CriteriaHeaders.Add(c);

            BandRows.Clear();
            foreach (var b in bands) BandRows.Add(b);

            Cells.Clear();
            foreach (var cell in cells)
            {
                cell.PropertyChanged += Cell_PropertyChanged;
                Cells.Add(cell);
            }

            UpdateScores();
        }

        private void Cell_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CriteriaCellViewModel.IsSelected))
            {
                var changedCell = sender as CriteriaCellViewModel;
                if (changedCell != null && changedCell.IsSelected)
                {
                    // Deselect other cells in the same criteria column
                    var cellsInSameCriteria = Cells.Where(c => c.CriteriaType == changedCell.CriteriaType && c != changedCell);
                    foreach (var otherCell in cellsInSameCriteria)
                    {
                        otherCell.IsSelected = false;
                    }
                }
                UpdateScores();
            }
        }

        private void UpdateScores()
        {
            var newScores = new ObservableCollection<CriterionScore>();
            double totalScore = 0;
            int count = 0;

            foreach (var criteria in CriteriaHeaders)
            {
                var selectedCell = Cells.FirstOrDefault(c => c.CriteriaType == criteria && c.IsSelected);
                int score = selectedCell?.BandScore ?? 0;
                newScores.Add(new CriterionScore { Name = criteria, Score = score });
                
                if (selectedCell != null)
                {
                    totalScore += score;
                    count++;
                }
            }

            CriterionScores = newScores;

            if (count == CriteriaHeaders.Count && count > 0)
            {
                OverallBandScore = totalScore / count;
            }
            else
            {
                OverallBandScore = 0;
            }
        }

        [RelayCommand]
        public void ClearScores()
        {
            foreach (var cell in Cells)
            {
                cell.IsSelected = false;
            }
        }
    }

    public partial class CriterionScore : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _score;
    }
}
