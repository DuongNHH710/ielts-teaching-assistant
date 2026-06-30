using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IeltsTeachingAssistant.ViewModels;

public partial class MatrixPoint : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}

public class MatrixBand
{
    public int Band { get; set; }
    public List<MatrixPoint> Points { get; set; } = new();
}

public class MatrixCriterion
{
    public string CriterionKey { get; set; } = string.Empty;
    public string CriterionName { get; set; } = string.Empty;
    public List<MatrixBand> Bands { get; set; } = new();
}
