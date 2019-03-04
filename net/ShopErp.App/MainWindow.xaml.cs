using ShopErp.App.Views.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Config;

namespace ShopErp.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserControl lastUserControl = null;
        private Dictionary<Type, UserControl> controls = new Dictionary<Type, UserControl>();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tbOperator.Text = "        " + OperatorService.LoginOperator.Number + " " + OperatorService.LoginOperator.Name;
                this.menu.ItemsSource = MenuConfig.Menus;
                string toolBars = LocalConfigService.GetValue("ToolBarControls", "");
                string[] tbs = toolBars.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<MenuConfig> mcs = new List<MenuConfig>();
                MenuConfig configBar = null;
                foreach (var menu in MenuConfig.Menus)
                {
                    foreach (var v in menu.SubItems)
                    {
                        if (v.Type != null && toolBars.Contains(v.Type.FullName))
                        {
                            mcs.Add(v);
                        }

                        if (v.Type != null && v.Type == typeof(MenuItemConfigUserControl))
                        {
                            configBar = v;
                        }
                    }
                }
                if (toolBars.Contains(typeof(MenuItemConfigUserControl).FullName) == false && mcs.Count < 1)
                {
                    mcs.Add(configBar);
                }
                this.tb.ItemsSource = mcs;
                //this.FontSize = LocalConfigService.GetValueDouble("FontSize", 12);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("对象不为FrameworkElement");
                    return;
                }
                var type = fe.Tag as Type;
                if (type == null)
                {
                    MessageBox.Show("FrameworkElement Tag为空");
                    return;
                }

                UserControl control = null;
                if (this.controls.ContainsKey(type))
                {
                    control = this.controls[type];
                }
                else
                {
                    control = Activator.CreateInstance(type) as UserControl;
                    control.SetValue(Grid.RowProperty, 2);
                    control.SetValue(Grid.ColumnProperty, 0);
                    this.controls.Add(type, control);
                }

                if (control == this.lastUserControl)
                {
                    return;
                }
                this.gHost.Children.Remove(this.lastUserControl);
                this.gHost.Children.Add(control);
                this.lastUserControl = control;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = MessageBox.Show("是否退出？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}