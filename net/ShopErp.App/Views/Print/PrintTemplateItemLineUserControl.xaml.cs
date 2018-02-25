using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.Domain;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// Interaction logic for PrintTemplateItemLineUserControl.xaml
    /// </summary>
    public partial class PrintTemplateItemLineUserControl : UserControl
    {
        public PrintTemplateItemLineUserControl()
        {
            InitializeComponent();
        }

        private void tbLineWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            int wid = 1;
            int.TryParse(this.tbLineWidth.Text.Trim(), out wid);
            if (wid <= 0)
            {
                return;
            }
            var pvm = this.DataContext as PrintTemplateItemViewModelForLine;
            foreach (var p in pvm.Template.Items.Where(obj => obj.Type == PrintTemplateItemType.OTHER_LINE))
            {
                if (p.Width < p.Height)
                {
                    (p.RunTimeTag as PrintTemplateItemViewModelCommon).Width = wid;
                }
                else
                {
                    (p.RunTimeTag as PrintTemplateItemViewModelCommon).Height = wid;
                }
            }
        }

        private void cbbColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var pvm = this.DataContext as PrintTemplateItemViewModelForLine;
            pvm.Format = e.NewValue.ToString();
        }

        private void cbbColorAll_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var pvm = this.DataContext as PrintTemplateItemViewModelForLine;
            foreach (var p in pvm.Template.Items.Where(obj => obj.Type == PrintTemplateItemType.OTHER_LINE))
            {
                (p.RunTimeTag as PrintTemplateItemViewModelCommon).Format = e.NewValue.ToString();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbVirutalLine.ItemsSource = new string[] { "是", "否" };
        }
    }
}