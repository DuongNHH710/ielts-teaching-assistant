using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using IeltsTeachingAssistant.ViewModels.Marking;

namespace IeltsTeachingAssistant.Views
{
    public sealed partial class MarkingPage : Page
    {
        public MarkingPage()
        {
            this.InitializeComponent();
        }
    }

    public class MarkingContentTemplateSelector : DataTemplateSelector
    {
        // These will be instantiated and assigned in a ResourceDictionary or code
        private DataTemplate? _subjectiveTemplate;
        private DataTemplate? _objectiveTemplate;

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is SubjectiveGridViewModel)
            {
                if (_subjectiveTemplate == null)
                {
                    _subjectiveTemplate = CreateTemplate("IeltsTeachingAssistant.Controls.SubjectiveGridControl");
                }
                return _subjectiveTemplate;
            }
            else if (item is ObjectiveScoreViewModel)
            {
                if (_objectiveTemplate == null)
                {
                    _objectiveTemplate = CreateTemplate("IeltsTeachingAssistant.Controls.ObjectiveScoreControl");
                }
                return _objectiveTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }

        private DataTemplate CreateTemplate(string controlTypeName)
        {
            string xaml = $@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:controls=""using:IeltsTeachingAssistant.Controls""><{controlTypeName.Replace("IeltsTeachingAssistant.Controls.", "controls:")} /></DataTemplate>";
            return (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(xaml);
        }
    }
}
