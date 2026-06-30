using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace IeltsTeachingAssistant.Views
{
    public sealed partial class EvaluationsPage : Page
    {
        public EvaluationsPage()
        {
            this.InitializeComponent();
        }

        private void EvalPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EvalPivot == null) return;

            switch (EvalPivot.SelectedIndex)
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            EvalPivot_SelectionChanged(EvalPivot, null!);
        }
    }
}
