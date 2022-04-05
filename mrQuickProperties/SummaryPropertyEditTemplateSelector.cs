namespace mrQuickProperties;

using System.Windows;
using System.Windows.Controls;
using Models;

public class SummaryPropertyEditTemplateSelector : DataTemplateSelector
{
    /// <inheritdoc/>
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is SummaryProperty summaryProperty && container is FrameworkElement frameworkElement)
        {
            return frameworkElement.FindResource($"For{summaryProperty.DisplayValueControlType}") as DataTemplate;
        }

        return null;
    }
}