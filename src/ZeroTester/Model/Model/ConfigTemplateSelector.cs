using System.Windows;
using System.Windows.Controls;
using Agebull.EntityModel.Config;

namespace Agebull.EntityModel.Designer
{
    public class ConfigTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ConfigTemplate
        {
            get;
            set;
        }
        public DataTemplate EntityTemplate
        {
            get;
            set;
        }

        public DataTemplate SolutionTemplate
        {
            get;
            set;
        }

        public DataTemplate ProjectTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SolutionConfig)
            {
                return SolutionTemplate;
            }
            if (item is EntityConfig)
            {
                return EntityTemplate;
            }
            if (item is ProjectConfig)
            {
                return ProjectTemplate;
            }
            return ConfigTemplate;
        }
    }
}