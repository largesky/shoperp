using ShopErp.App.Service;
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

namespace ShopErp.App.Views.Config
{
    /// <summary>
    /// MenuItemConfigUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class MenuItemConfigUserControl : UserControl
    {
        public MenuItemConfigUserControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tcs = "";
                foreach (var menu in MenuConfig.Menus)
                {
                    foreach (var v in menu.SubItems)
                    {
                        if (v.IsChecked)
                        {
                            tcs += v.Type.FullName + ",";
                        }
                    }
                }
                LocalConfigService.UpdateValue("ToolBarControls", tcs);
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string toolBars = LocalConfigService.GetValue("ToolBarControls", "");
                foreach (var menu in MenuConfig.Menus)
                {
                    foreach (var v in menu.SubItems)
                    {
                        v.IsChecked = v.Type != null && toolBars.Contains(v.Type.FullName);
                    }
                }
                this.lb.ItemsSource = MenuConfig.Menus;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
