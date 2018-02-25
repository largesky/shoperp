

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
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Users
{
    /// <summary>
    /// UsersUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class UsersUserControl : UserControl
    {
        public UsersUserControl()
        {
            InitializeComponent();
        }

        private bool CanMan()
        {
            return OperatorService.LoginOperator.Rights.Contains("用户管理") ||
                   OperatorService.LoginOperator.Number == "1001" || OperatorService.LoginOperator.Number == "1002";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                btnRefresh_Click(null, null);
                if (CanMan())
                {
                    this.IsEnabled = true;
                }
                else
                {
                    this.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                this.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = ServiceContainer.GetService<OperatorService>().GetByAll().Datas;
                if (CanMan() == false)
                {
                    foreach (var v in data)
                    {
                        v.Rights = "";
                    }
                }
                this.dgvOperators.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new UserEditWindow();
                var ret = win.ShowDialog();
                if (ret.Value)
                {
                    btnRefresh_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvOperators.SelectedCells.Count < 1)
                {
                    throw new Exception("未选择用户");
                }
                var op = this.dgvOperators.SelectedCells[0].Item as Operator;
                var win = new UserEditWindow { Operator = op };
                var ret = win.ShowDialog();
                if (ret.Value)
                {
                    btnRefresh_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvOperators.SelectedCells.Count < 1)
                {
                    throw new Exception("未选择用户");
                }
                var op = this.dgvOperators.SelectedCells[0].Item as Operator;
                if (MessageBox.Show("是否删除用户：" + op.Number, "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<OperatorService>().Delete(op.Id);
                btnRefresh_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}