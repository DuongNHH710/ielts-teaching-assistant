using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IeltsTeachingAssistant.ViewModels.Marking
{
    public partial class CriteriaCellViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _bandScore;

        [ObservableProperty]
        private string _criteriaType = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        public ObservableCollection<DescriptorViewModel> Descriptors { get; } = new();

        public CriteriaCellViewModel(int bandScore, string criteriaType)
        {
            BandScore = bandScore;
            CriteriaType = criteriaType;
        }
    }
}
