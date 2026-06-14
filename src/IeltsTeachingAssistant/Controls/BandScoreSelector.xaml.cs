using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Controls;

public sealed partial class BandScoreSelector : UserControl
{
    public BandScoreSelector()
    {
        this.InitializeComponent();
    }

    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(BandScoreSelector), new PropertyMetadata(string.Empty, OnHeaderChanged));

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BandScoreSelector control && e.NewValue is string header)
        {
            control.HeaderTextBlock.Text = header;
        }
    }

    public double Value
    {
        get { return (double)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(BandScoreSelector), new PropertyMetadata(0.0));
}
