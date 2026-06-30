using System.Collections.Generic;
using System.Linq;
using IeltsTeachingAssistant.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Controls;

/// <summary>
/// A 4-column grid control that renders one <see cref="RubricCriterionPanel"/> per
/// IELTS criterion. Accepts the full <see cref="MatrixDescriptors"/> collection and
/// per-criterion band scores, AI comments, and highlighted descriptor IDs.
///
/// Designed to be embedded inside the WritingTask / SpeakingPart DataTemplates so it
/// automatically refreshes whenever the task is graded.
/// </summary>
public sealed partial class RubricGridPanel : UserControl
{
    // ── Dependency Properties ─────────────────────────────────────────────────

    public static readonly DependencyProperty MatrixDescriptorsProperty =
        DependencyProperty.Register(nameof(MatrixDescriptors), typeof(IReadOnlyList<MatrixCriterion>),
            typeof(RubricGridPanel), new PropertyMetadata(null, OnAnyChanged));

    // Band scores (one per criterion; order matches MatrixDescriptors)
    public static readonly DependencyProperty Score0Property =
        DependencyProperty.Register(nameof(Score0), typeof(double), typeof(RubricGridPanel), new PropertyMetadata(0.0, OnAnyChanged));
    public static readonly DependencyProperty Score1Property =
        DependencyProperty.Register(nameof(Score1), typeof(double), typeof(RubricGridPanel), new PropertyMetadata(0.0, OnAnyChanged));
    public static readonly DependencyProperty Score2Property =
        DependencyProperty.Register(nameof(Score2), typeof(double), typeof(RubricGridPanel), new PropertyMetadata(0.0, OnAnyChanged));
    public static readonly DependencyProperty Score3Property =
        DependencyProperty.Register(nameof(Score3), typeof(double), typeof(RubricGridPanel), new PropertyMetadata(0.0, OnAnyChanged));

    // AI comments
    public static readonly DependencyProperty Comment0Property =
        DependencyProperty.Register(nameof(Comment0), typeof(string), typeof(RubricGridPanel), new PropertyMetadata(string.Empty, OnAnyChanged));
    public static readonly DependencyProperty Comment1Property =
        DependencyProperty.Register(nameof(Comment1), typeof(string), typeof(RubricGridPanel), new PropertyMetadata(string.Empty, OnAnyChanged));
    public static readonly DependencyProperty Comment2Property =
        DependencyProperty.Register(nameof(Comment2), typeof(string), typeof(RubricGridPanel), new PropertyMetadata(string.Empty, OnAnyChanged));
    public static readonly DependencyProperty Comment3Property =
        DependencyProperty.Register(nameof(Comment3), typeof(string), typeof(RubricGridPanel), new PropertyMetadata(string.Empty, OnAnyChanged));

    // Highlighted descriptor ID lists (JSON-serialised lists from the ViewModel)
    public static readonly DependencyProperty HighlightedIds0Property =
        DependencyProperty.Register(nameof(HighlightedIds0), typeof(IReadOnlyList<string>), typeof(RubricGridPanel), new PropertyMetadata(null, OnAnyChanged));
    public static readonly DependencyProperty HighlightedIds1Property =
        DependencyProperty.Register(nameof(HighlightedIds1), typeof(IReadOnlyList<string>), typeof(RubricGridPanel), new PropertyMetadata(null, OnAnyChanged));
    public static readonly DependencyProperty HighlightedIds2Property =
        DependencyProperty.Register(nameof(HighlightedIds2), typeof(IReadOnlyList<string>), typeof(RubricGridPanel), new PropertyMetadata(null, OnAnyChanged));
    public static readonly DependencyProperty HighlightedIds3Property =
        DependencyProperty.Register(nameof(HighlightedIds3), typeof(IReadOnlyList<string>), typeof(RubricGridPanel), new PropertyMetadata(null, OnAnyChanged));

    // ── CLR Wrappers ──────────────────────────────────────────────────────────

    public IReadOnlyList<MatrixCriterion>? MatrixDescriptors
    {
        get => (IReadOnlyList<MatrixCriterion>?)GetValue(MatrixDescriptorsProperty);
        set => SetValue(MatrixDescriptorsProperty, value);
    }

    public double Score0 { get => (double)GetValue(Score0Property); set => SetValue(Score0Property, value); }
    public double Score1 { get => (double)GetValue(Score1Property); set => SetValue(Score1Property, value); }
    public double Score2 { get => (double)GetValue(Score2Property); set => SetValue(Score2Property, value); }
    public double Score3 { get => (double)GetValue(Score3Property); set => SetValue(Score3Property, value); }

    public string? Comment0 { get => (string?)GetValue(Comment0Property); set => SetValue(Comment0Property, value); }
    public string? Comment1 { get => (string?)GetValue(Comment1Property); set => SetValue(Comment1Property, value); }
    public string? Comment2 { get => (string?)GetValue(Comment2Property); set => SetValue(Comment2Property, value); }
    public string? Comment3 { get => (string?)GetValue(Comment3Property); set => SetValue(Comment3Property, value); }

    public IReadOnlyList<string>? HighlightedIds0 { get => (IReadOnlyList<string>?)GetValue(HighlightedIds0Property); set => SetValue(HighlightedIds0Property, value); }
    public IReadOnlyList<string>? HighlightedIds1 { get => (IReadOnlyList<string>?)GetValue(HighlightedIds1Property); set => SetValue(HighlightedIds1Property, value); }
    public IReadOnlyList<string>? HighlightedIds2 { get => (IReadOnlyList<string>?)GetValue(HighlightedIds2Property); set => SetValue(HighlightedIds2Property, value); }
    public IReadOnlyList<string>? HighlightedIds3 { get => (IReadOnlyList<string>?)GetValue(HighlightedIds3Property); set => SetValue(HighlightedIds3Property, value); }

    // ── Constructor ───────────────────────────────────────────────────────────

    public RubricGridPanel()
    {
        this.InitializeComponent();
    }

    // ── Change handler ────────────────────────────────────────────────────────

    private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RubricGridPanel panel)
            panel.Refresh();
    }

    // ── Refresh UI ────────────────────────────────────────────────────────────

    private void Refresh()
    {
        var descriptors = MatrixDescriptors;
        if (descriptors == null || descriptors.Count == 0) return;

        var panels = new[] { Panel0, Panel1, Panel2, Panel3 };
        double[] scores   = { Score0, Score1, Score2, Score3 };
        string?[] comments = { Comment0, Comment1, Comment2, Comment3 };
        IReadOnlyList<string>?[] ids = { HighlightedIds0, HighlightedIds1, HighlightedIds2, HighlightedIds3 };

        for (int i = 0; i < panels.Length; i++)
        {
            var panel = panels[i];
            if (i < descriptors.Count)
            {
                panel.Criterion             = descriptors[i];
                panel.BandScore             = scores[i];
                panel.AiComment             = comments[i] ?? string.Empty;
                panel.HighlightedDescriptorIds = ids[i];
                panel.Visibility            = Visibility.Visible;
            }
            else
            {
                panel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
