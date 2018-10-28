using System.Windows;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// ExtendPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ExtendPanel
    {
        public ExtendPanel()
        {
            InitializeComponent();
        }

        public UIElement Child
        {
            get => child.Child;
            set => child.Child = value;
        }
    }
}
