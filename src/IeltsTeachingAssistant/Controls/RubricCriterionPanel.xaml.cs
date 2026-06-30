using System.Collections.Generic;
using System.Linq;
using IeltsTeachingAssistant.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace IeltsTeachingAssistant.Controls;

/// <summary>
/// UserControl that shows a vertical accordion rubric for ONE IELTS criterion.
/// All band rows are built programmatically so we avoid WinUI 3 compile-time binding
/// restrictions (FontWeight from string, SolidColorBrush colour binding, etc.).
/// </summary>
public sealed partial class RubricCriterionPanel : UserControl
{
    // ── Dependency Properties ─────────────────────────────────────────────────

    public static readonly DependencyProperty CriterionProperty =
        DependencyProperty.Register(nameof(Criterion), typeof(MatrixCriterion),
            typeof(RubricCriterionPanel), new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty BandScoreProperty =
        DependencyProperty.Register(nameof(BandScore), typeof(double),
            typeof(RubricCriterionPanel), new PropertyMetadata(0.0, OnDataChanged));

    public static readonly DependencyProperty AiCommentProperty =
        DependencyProperty.Register(nameof(AiComment), typeof(string),
            typeof(RubricCriterionPanel), new PropertyMetadata(string.Empty, OnDataChanged));

    public static readonly DependencyProperty HighlightedDescriptorIdsProperty =
        DependencyProperty.Register(nameof(HighlightedDescriptorIds), typeof(IReadOnlyList<string>),
            typeof(RubricCriterionPanel), new PropertyMetadata(null, OnDataChanged));

    // ── CLR wrappers ──────────────────────────────────────────────────────────

    public MatrixCriterion? Criterion
    {
        get => (MatrixCriterion?)GetValue(CriterionProperty);
        set => SetValue(CriterionProperty, value);
    }

    public double BandScore
    {
        get => (double)GetValue(BandScoreProperty);
        set => SetValue(BandScoreProperty, value);
    }

    public string AiComment
    {
        get => (string)GetValue(AiCommentProperty);
        set => SetValue(AiCommentProperty, value);
    }

    public IReadOnlyList<string>? HighlightedDescriptorIds
    {
        get => (IReadOnlyList<string>?)GetValue(HighlightedDescriptorIdsProperty);
        set => SetValue(HighlightedDescriptorIdsProperty, value);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    public RubricCriterionPanel()
    {
        this.InitializeComponent();
    }

    // ── Change handler ────────────────────────────────────────────────────────

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RubricCriterionPanel panel)
            panel.Refresh();
    }

    // ── Refresh UI ────────────────────────────────────────────────────────────

    private void Refresh()
    {
        var criterion = Criterion;
        if (criterion == null) return;

        // Criterion name
        CriterionNameText.Text = criterion.CriterionName;

        // Band badge
        int matchedBand = (int)System.Math.Floor(BandScore);
        BandBadgeText.Text = BandScore > 0 ? BandScore.ToString("0.#") : "–";
        BadgeBrush.Color   = GetBadgeColor(BandScore);

        // AI comment
        AiCommentText.Inlines.Clear();
        if (!string.IsNullOrEmpty(AiComment))
        {
            var lines = AiComment.Split('\n');
            for (int k = 0; k < lines.Length; k++)
            {
                var line = lines[k];
                var nextLineSuffix = (k < lines.Length - 1) ? "\n" : string.Empty;

                if (line.StartsWith("JUSTIFICATION:") || 
                    line.StartsWith("EVIDENCE QUOTES:") || 
                    line.StartsWith("LIMITING FACTORS:"))
                {
                    AiCommentText.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                    {
                        Text = line + nextLineSuffix,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold
                    });
                }
                else
                {
                    AiCommentText.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                    {
                        Text = line + nextLineSuffix
                    });
                }
            }
        }

        // Rebuild band rows programmatically
        BandRowsHost.Children.Clear();

        var highlightedIds = HighlightedDescriptorIds ?? new List<string>();

        foreach (var band in criterion.Bands.OrderByDescending(b => b.Band))
        {
            bool isBandHighlighted = band.Band == matchedBand;
            var expander = BuildBandExpander(band, isBandHighlighted, highlightedIds);
            expander.IsExpanded = isBandHighlighted;
            BandRowsHost.Children.Add(expander);
        }
    }

    // ── Build a single band expander ──────────────────────────────────────────

    private static Expander BuildBandExpander(MatrixBand band, bool isBandHighlighted,
        IReadOnlyList<string> highlightedIds)
    {
        // Preview: first descriptor text (truncated)
        string preview = band.Points.FirstOrDefault()?.Text ?? string.Empty;
        if (preview.Length > 50) preview = preview[..50] + "…";

        // Header
        var headerGrid = new Grid { Padding = new Thickness(4, 6, 4, 6) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var bandLabel = new TextBlock
        {
            Text       = band.Band.ToString(),
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            FontSize   = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(
                isBandHighlighted ? GetBadgeColor(band.Band) : Color.FromArgb(180, 160, 160, 160))
        };
        Grid.SetColumn(bandLabel, 0);

        var previewText = new TextBlock
        {
            Text              = preview,
            FontSize          = 11,
            Opacity           = 0.65,
            TextTrimming      = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(previewText, 1);

        headerGrid.Children.Add(bandLabel);
        headerGrid.Children.Add(previewText);

        // Points list
        var pointsPanel = new StackPanel { Padding = new Thickness(12, 4, 12, 8), Spacing = 4 };
        foreach (var point in band.Points)
        {
            bool isHighlighted = highlightedIds.Contains(point.Id);
            pointsPanel.Children.Add(BuildDescriptorRow(point.Text, isHighlighted));
        }

        // Expander
        var expander = new Expander
        {
            HorizontalAlignment        = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0),
            Header  = headerGrid,
            Content = pointsPanel
        };

        // Highlight the matched band row background
        if (isBandHighlighted)
        {
            expander.Background = new SolidColorBrush(Color.FromArgb(20, 34, 197, 94));
        }

        return expander;
    }

    // ── Build one descriptor bullet row ──────────────────────────────────────

    private static Border BuildDescriptorRow(string text, bool isHighlighted)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };

        var bullet = new FontIcon
        {
            Glyph             = isHighlighted ? "\uE73E" : "\uE915", // checkmark : circle
            FontSize          = 9,
            VerticalAlignment = VerticalAlignment.Top,
            Margin            = new Thickness(0, 3, 0, 0),
            Foreground        = new SolidColorBrush(
                isHighlighted
                    ? Color.FromArgb(255, 34, 197, 94)   // green
                    : Color.FromArgb(100, 160, 160, 160)) // muted
        };

        var label = new TextBlock
        {
            Text        = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize    = 12,
            FontWeight  = isHighlighted
                ? Microsoft.UI.Text.FontWeights.SemiBold
                : Microsoft.UI.Text.FontWeights.Normal,
            Foreground = new SolidColorBrush(
                isHighlighted
                    ? Color.FromArgb(255, 220, 255, 220)  // light green tint
                    : Color.FromArgb(200, 200, 200, 200)),
            VerticalAlignment = VerticalAlignment.Center
        };

        row.Children.Add(bullet);
        row.Children.Add(label);

        var border = new Border
        {
            Padding       = new Thickness(4, 3, 4, 3),
            CornerRadius  = new CornerRadius(4),
            Margin        = new Thickness(0, 2, 0, 2),
            Background    = new SolidColorBrush(
                isHighlighted
                    ? Color.FromArgb(40, 34, 197, 94)   // subtle green tint
                    : Colors.Transparent),
            Child = row
        };

        return border;
    }

    // ── Badge colour helper ───────────────────────────────────────────────────

    private static Color GetBadgeColor(double band) => band switch
    {
        >= 7.0 => Color.FromArgb(255, 34, 197, 94),  // green
        >= 6.0 => Color.FromArgb(255, 245, 158, 11), // amber
        > 0    => Color.FromArgb(255, 239, 68, 68),  // red
        _      => Color.FromArgb(255, 120, 120, 120) // grey
    };

    // ── Badge click handler ───────────────────────────────────────────────────

    private void BandBadge_Click(object sender, RoutedEventArgs e)
    {
        AiCommentBorder.Visibility = AiCommentBorder.Visibility == Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
