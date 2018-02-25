using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// PrintTemplateItemViewModelRouteCodeUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PrintTemplateItemViewModelRouteCodeUserControl : UserControl
    {
        public PrintTemplateItemViewModelRouteCodeUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbParts.ItemsSource = new string[] { "全部", "第一段", "第二段", "第三段" };
        }
    }
}
