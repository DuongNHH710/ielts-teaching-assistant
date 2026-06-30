using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Controls;

public sealed partial class InteractiveSpeakingRubricGrid : UserControl
{
    public static readonly DependencyProperty FluencyCoherenceProperty =
        DependencyProperty.Register(nameof(FluencyCoherence), typeof(double), typeof(InteractiveSpeakingRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty LexicalResourceProperty =
        DependencyProperty.Register(nameof(LexicalResource), typeof(double), typeof(InteractiveSpeakingRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty GrammaticalRangeProperty =
        DependencyProperty.Register(nameof(GrammaticalRange), typeof(double), typeof(InteractiveSpeakingRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty PronunciationProperty =
        DependencyProperty.Register(nameof(Pronunciation), typeof(double), typeof(InteractiveSpeakingRubricGrid), new PropertyMetadata(0.0, OnScoreChanged));

    public static readonly DependencyProperty OverallBandProperty =
        DependencyProperty.Register(nameof(OverallBand), typeof(double), typeof(InteractiveSpeakingRubricGrid), new PropertyMetadata(0.0));

    public double FluencyCoherence
    {
        get => (double)GetValue(FluencyCoherenceProperty);
        set => SetValue(FluencyCoherenceProperty, value);
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

    public double Pronunciation
    {
        get => (double)GetValue(PronunciationProperty);
        set => SetValue(PronunciationProperty, value);
    }

    public double OverallBand
    {
        get => (double)GetValue(OverallBandProperty);
        set => SetValue(OverallBandProperty, value);
    }

    public InteractiveSpeakingRubricGrid()
    {
        this.InitializeComponent();
    }

    private static void OnScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InteractiveSpeakingRubricGrid grid)
        {
            grid.OverallBand = Helpers.BandScoreCalculator.RoundToHalfBand((grid.FluencyCoherence + grid.LexicalResource + grid.GrammaticalRange + grid.Pronunciation) / 4.0);
        }
    }
}
