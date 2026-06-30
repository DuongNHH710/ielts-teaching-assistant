using CommunityToolkit.Mvvm.ComponentModel;

namespace IeltsTeachingAssistant.ViewModels.Marking
{
    public partial class DescriptorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private bool _isChecked;

        public DescriptorViewModel(string text)
        {
            _text = text;
        }
    }
}
