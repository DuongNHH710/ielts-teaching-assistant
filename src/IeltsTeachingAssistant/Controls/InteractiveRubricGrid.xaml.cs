using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Controls;

public sealed partial class InteractiveRubricGrid : UserControl
{
    // Mock properties for the UI. In a real app we would calculate these 
    // by handling the ToggleButton Checked/Unchecked events and finding the highest active band.
    
    public static readonly DependencyProperty TaskAchievementProperty =
        DependencyProperty.Register(nameof(TaskAchievement), typeof(double), typeof(InteractiveRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty CoherenceCohesionProperty =
        DependencyProperty.Register(nameof(CoherenceCohesion), typeof(double), typeof(InteractiveRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty LexicalResourceProperty =
        DependencyProperty.Register(nameof(LexicalResource), typeof(double), typeof(InteractiveRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty GrammaticalRangeProperty =
        DependencyProperty.Register(nameof(GrammaticalRange), typeof(double), typeof(InteractiveRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty OverallBandProperty =
        DependencyProperty.Register(nameof(OverallBand), typeof(double), typeof(InteractiveRubricGrid), new PropertyMetadata(0.0));

    public double TaskAchievement
    {
        get => (double)GetValue(TaskAchievementProperty);
        set => SetValue(TaskAchievementProperty, value);
    }

    public double CoherenceCohesion
    {
        get => (double)GetValue(CoherenceCohesionProperty);
        set => SetValue(CoherenceCohesionProperty, value);
    }

    public double LexicalResource
    {
        get => (double)GetValue(LexicalResourceProperty);
        set => SetValue(LexicalResourceProperty, value);
    }

    public double GrammaticalRange
    {
        get => (double)GetValue(GrammaticalRangeProperty);
        set => SetValue(GrammaticalRangeProperty, value);
    }

    public double OverallBand
    {
        get => (double)GetValue(OverallBandProperty);
        set => SetValue(OverallBandProperty, value);
    }

    public InteractiveRubricGrid()
    {
        this.InitializeComponent();
    }

    private static void OnScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InteractiveRubricGrid grid)
        {
            grid.OverallBand = Helpers.BandScoreCalculator.RoundToHalfBand((grid.TaskAchievement + grid.CoherenceCohesion + grid.LexicalResource + grid.GrammaticalRange) / 4.0);
        }
    }
}
