using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace IeltsTeachingAssistant.Views;

public sealed partial class EvaluationsPage : Page
{
    public EvaluationsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string tabName)
        {
            switch (tabName.ToLower())
            {
                case "reading":
                    EvalPivot.SelectedIndex = 0;
                    break;
                case "listening":
                    EvalPivot.SelectedIndex = 1;
                    break;
                case "writing":
                    EvalPivot.SelectedIndex = 2;
                    break;
                case "speaking":
                    EvalPivot.SelectedIndex = 3;
                    break;
            }
        }

        // Trigger loading the initial selected tab
        LoadTab(EvalPivot.SelectedIndex);
    }

    private void EvalPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadTab(EvalPivot.SelectedIndex);
    }

    private void LoadTab(int index)
    {
        switch (index)
        {
            case 0:
                if (ReadingFrame.Content == null)
                {
                    ReadingFrame.Navigate(typeof(ReadingEvaluationPage));
                }
                break;
            case 1:
                if (ListeningFrame.Content == null)
                {
                    ListeningFrame.Navigate(typeof(ListeningEvaluationPage));
                }
                break;
            case 2:
                if (WritingFrame.Content == null)
                {
                    WritingFrame.Navigate(typeof(WritingEvaluationPage));
                }
                break;
            case 3:
                if (SpeakingFrame.Content == null)
                {
                    SpeakingFrame.Navigate(typeof(SpeakingEvaluationPage));
                }
                break;
        }
    }
}
