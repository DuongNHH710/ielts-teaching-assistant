using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using IeltsTeachingAssistant.ViewModels.Marking;

namespace IeltsTeachingAssistant.Controls
{
    public sealed partial class MarkingGridCell : UserControl
    {
        public MarkingGridCell()
        {
            this.InitializeComponent();
            this.DataContextChanged += MarkingGridCell_DataContextChanged;
        }

        private void MarkingGridCell_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            UpdateVisualState();
            if (args.NewValue is CriteriaCellViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CriteriaCellViewModel.IsSelected))
                    {
                        UpdateVisualState();
                    }
                };
            }
        }

        private void UpdateVisualState()
        {
            if (DataContext is CriteriaCellViewModel vm)
            {
                VisualStateManager.GoToState(this, vm.IsSelected ? "Selected" : "Normal", true);
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CriteriaCellViewModel vm)
            {
                vm.IsSelected = true;
            }
        }
    }
}
