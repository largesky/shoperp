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
using System.Windows.Shapes;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Utils;

namespace ShopErp.App.Views.Users
{
    /// <summary>
    /// UserEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserEditWindow : Window
    {
        public Operator Operator { get; set; }

        public UserEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OperatorService.LoginOperator.Rights.Contains("用户管理") == false)
                {
                    throw new Exception("当前用户没有 用户管理 权限");
                }
                if (this.Operator == null)
                {
                    this.Operator = new Operator
                    {
                        Enabled = true,
                        CreateOperator = OperatorService.LoginOperator.Number,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                    };
                }
                this.tbNumber.Text = this.Operator.Number;
                this.tbName.Text = this.Operator.Name;
                this.tbPhone.Text = this.Operator.Phone;
                this.chkEnabled.IsChecked = this.Operator.Enabled;
                var rights = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_SYSTEM_RIGHTS, "").Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                var rvm = rights.Select(obj => new RightsViewModel() { Name = obj, IsChecked = this.Operator.Rights.Contains(obj) }).ToArray();
                this.lbRights.ItemsSource = rvm;
            }
            catch (Exception ex)
            {
                this.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string p = this.tbPassword.Text.Trim();
                this.Operator.Name = this.tbName.Text.Trim();
                this.Operator.Number = this.tbNumber.Text.Trim();
                this.Operator.Phone = this.tbPhone.Text.Trim();
                this.Operator.Rights = string.Join(",", (this.lbRights.ItemsSource as RightsViewModel[]).Where(obj => obj.IsChecked).Select(obj => obj.Name));
                this.Operator.Enabled = this.chkEnabled.IsChecked.Value;
                if (string.IsNullOrWhiteSpace(this.Operator.Name) || string.IsNullOrWhiteSpace(this.Operator.Number))
                {
                    throw new Exception("姓名，工号不能为空");
                }

                if (this.Operator.Id > 0)
                {
                    ServiceContainer.GetService<OperatorService>().Update(this.Operator);
                    if (string.IsNullOrWhiteSpace(p) == false)
                    {
                        string pp = Md5Util.Md5(p);
                        ServiceContainer.GetService<OperatorService>().ModifyPassword(this.Operator.Id, pp);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(p))
                    {
                        throw new Exception("添加的用户密码不能为空");
                    }
                    this.Operator.Password = Md5Util.Md5(p);
                    ServiceContainer.GetService<OperatorService>().Save(this.Operator);
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                this.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
        }
    }
}