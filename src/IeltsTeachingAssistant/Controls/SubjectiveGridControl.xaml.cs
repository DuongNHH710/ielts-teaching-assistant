using System.Collections.Specialized;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using IeltsTeachingAssistant.ViewModels.Marking;

namespace IeltsTeachingAssistant.Controls
{
    public sealed partial class SubjectiveGridControl : UserControl
    {
        public SubjectiveGridControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += SubjectiveGridControl_DataContextChanged;
        }

        private void SubjectiveGridControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is SubjectiveGridViewModel vm)
            {
                vm.Cells.CollectionChanged += Cells_CollectionChanged;
                GenerateGrid(vm);
            }
        }

        private void Cells_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataContext is SubjectiveGridViewModel vm)
            {
                GenerateGrid(vm);
            }
        }

        private void GenerateGrid(SubjectiveGridViewModel vm)
        {
            DynamicGrid.Children.Clear();
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();

            if (vm.CriteriaHeaders.Count == 0 || vm.BandRows.Count == 0 || vm.Cells.Count == 0)
                return;

            // Column 0 is for Band labels. Columns 1..N are for Criteria.
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            for (int i = 0; i < vm.CriteriaHeaders.Count; i++)
            {
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Row 0 is for Criteria headers. Rows 1..M are for Bands.
            DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < vm.BandRows.Count; i++)
            {
                DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Add Top-Left empty cell (or label "BAND")
            var tlLabel = new TextBlock { Text = "BAND", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8) };
            Grid.SetRow(tlLabel, 0);
            Grid.SetColumn(tlLabel, 0);
            DynamicGrid.Children.Add(tlLabel);

            // Add Criteria Headers (Row 0)
            for (int i = 0; i < vm.CriteriaHeaders.Count; i++)
            {
                var headerBorder = new Border 
                { 
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 242, 241)), // Light gray
                    Margin = new Thickness(4)
                };
                var headerLabel = new TextBlock 
                { 
                    Text = vm.CriteriaHeaders[i], 
                    FontWeight = FontWeights.SemiBold, 
                    VerticalAlignment = VerticalAlignment.Center, 
                    Margin = new Thickness(8) 
                };
                headerBorder.Child = headerLabel;
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i + 1);
                DynamicGrid.Children.Add(headerBorder);
            }

            // Add Band Labels (Column 0) and Marking Cells (Columns 1..N)
            for (int r = 0; r < vm.BandRows.Count; r++)
            {
                int band = vm.BandRows[r];
                
                // Band label
                var bandLabel = new TextBlock 
                { 
                    Text = band.ToString(), 
                    FontSize = 24, 
                    FontWeight = FontWeights.Bold, 
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 17, 35)), // Red
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(8) 
                };
                Grid.SetRow(bandLabel, r + 1);
                Grid.SetColumn(bandLabel, 0);
                DynamicGrid.Children.Add(bandLabel);

                // Marking Cells
                for (int c = 0; c < vm.CriteriaHeaders.Count; c++)
                {
                    string criteria = vm.CriteriaHeaders[c];
                    var cellVm = vm.Cells.FirstOrDefault(cell => cell.BandScore == band && cell.CriteriaType == criteria);
                    
                    if (cellVm != null)
                    {
                        var cellControl = new MarkingGridCell { DataContext = cellVm };
                        Grid.SetRow(cellControl, r + 1);
                        Grid.SetColumn(cellControl, c + 1);
                        DynamicGrid.Children.Add(cellControl);
                    }
                }
            }
        }
    }
}
