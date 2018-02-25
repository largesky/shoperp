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

namespace ShopErp.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserControl lastUserControl = null;
        private Dictionary<string, UserControl> controls = new Dictionary<string, UserControl>();

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
                this.FontSize = LocalConfigService.GetValueDouble("FontSize", 12);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe == null)
            {
                MessageBox.Show("对象不为FrameworkElement");
                return;
            }
            string tag = fe.Tag.ToString().Trim();
            if (string.IsNullOrWhiteSpace(tag))
            {
                MessageBox.Show("FrameworkElement Tag为空");
                return;
            }

            try
            {
                UserControl control = null;
                if (this.controls.ContainsKey(tag))
                {
                    control = this.controls[tag];
                }
                else
                {
                    var ass = Assembly.GetAssembly(this.GetType());
                    var ts = ass.GetTypes().Where(t => t.BaseType == typeof(UserControl) && t.Name == tag)
                        .FirstOrDefault();
                    if (ts == null)
                    {
                        throw new Exception("没有找到界面对象:" + tag);
                    }
                    control = Activator.CreateInstance(ts) as UserControl;
                    control.SetValue(Grid.RowProperty, 2);
                    control.SetValue(Grid.ColumnProperty, 0);
                    this.controls.Add(tag, control);
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
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}