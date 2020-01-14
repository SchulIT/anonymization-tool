using AnonymizationTool.Data.Persistence;
using System.Windows;
using System.Windows.Controls;

namespace AnonymizationTool.Selectors
{
    public class BooleanStateDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TrueTemplate { get; set; }
        public DataTemplate FalseTemplate { get; set; }
        public DataTemplate NullTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var student = item as AnonymousStudent;

            if (student == null)
            {
                return NullTemplate;
            }

            if (student.IsMissingInSchILD == true)
            {
                return TrueTemplate;
            }
            else if (student.IsMissingInSchILD == false)
            {
                return FalseTemplate;
            }

            return NullTemplate;
        }
    }
}
