using System;
using System.Windows;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Windows.Controls;

namespace ShopErp.App.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        private OperatorService operatorService = ServiceContainer.GetService<OperatorService>();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private List<string> GetUrls()
        {
            string url = LocalConfigService.GetValue(SystemNames.CONFIG_SERVER_ADDRESS, "http://192.168.31.67/shoperp,http://bjcgroup.imwork.net:60014");
            string[] urls = url.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            return urls.ToList();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string number = this.tbNumber.Text.Trim();
            string password = this.pbPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(number) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("用户名或密码不能为空");
                return;
            }

            try
            {
                ServiceContainer.ServerAddress = this.cbbSerer.Text.Trim();
                this.operatorService.Login(number, password);
                var urls = this.cbbSerer.ItemsSource.OfType<string>().ToList();
                urls.Remove(ServiceContainer.ServerAddress);
                urls.Insert(0, ServiceContainer.ServerAddress);
                LocalConfigService.UpdateValue(SystemNames.CONFIG_SERVER_ADDRESS, string.Join(",", urls));
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void LoginWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var urls = GetUrls();
                this.cbbSerer.ItemsSource = urls;
                if (urls.Count > 0)
                    this.cbbSerer.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var str = (sender as Button).Tag as String;
                var urls = this.cbbSerer.ItemsSource.OfType<string>().ToList();
                urls.Remove(str);
                LocalConfigService.UpdateValue(SystemNames.CONFIG_SERVER_ADDRESS, string.Join(",", urls));
                this.cbbSerer.ItemsSource = urls;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
