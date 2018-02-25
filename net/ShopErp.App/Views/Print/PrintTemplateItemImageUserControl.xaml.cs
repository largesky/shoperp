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

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// Interaction logic for PrintTemplateItemImageUserControl.xaml
    /// </summary>
    public partial class PrintTemplateItemImageUserControl : UserControl
    {
        public PrintTemplateItemImageUserControl()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            try
            {
                bool? ret = ofd.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }
                ((Button) sender).Tag = ofd.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}