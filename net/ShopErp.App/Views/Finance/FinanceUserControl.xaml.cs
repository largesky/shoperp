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

namespace ShopErp.App.Views.Finance
{
    /// <summary>
    /// Interaction logic for FinanceUserControl.xaml
    /// </summary>
    public partial class FinanceUserControl : UserControl
    {
        public FinanceUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OperatorService.LoginOperator.Rights.Contains("财务"))
                {
                    var items = ServiceContainer.GetService<FinanceTypeService>().GetByAll().OrderBy(obj => obj.Mode).Select(obj => obj.Name).ToList();
                    items.Insert(0, "");
                    this.cbbTypes.ItemsSource = items;
                    this.dpStartTime.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                    this.cbbAccounts.ItemsSource = ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas;
                }
                else
                {
                    this.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.pb1.Parameters.Clear();
                this.pb1.Parameters.Add("Type", this.cbbTypes.SelectedIndex >= 0 ? this.cbbTypes.Text.Trim() : "");
                this.pb1.Parameters.Add("StartTime", this.dpStartTime.Value != null ? this.dpStartTime.Value.Value : DateTime.MinValue);
                this.pb1.Parameters.Add("EndTime", this.dpEndTime.Value != null ? this.dpEndTime.Value.Value : DateTime.MinValue);
                this.pb1.Parameters.Add("Comment", this.tbComment.Text.Trim());
                this.pb1.Parameters.Add("AccountId", this.cbbAccounts.SelectedItem is FinanceAccount ? (this.cbbAccounts.SelectedItem as FinanceAccount).Id : 0);
                this.pb1.StartPage();
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
                new FinanceEdit().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PageBarUserControl_PageChanging(object sender, PageBar.PageChangeEventArgs e)
        {
            try
            {
                var data = ServiceContainer.GetService<FinanceService>().GetByAll(e.GetParameter<string>("Type"), e.GetParameter<long>("AccountId"),
                    e.GetParameter<string>("Comment"),
                    e.GetParameter<DateTime>("StartTime"), e.GetParameter<DateTime>("EndTime"), e.CurrentPage - 1,
                    e.PageSize);
                this.dgvFinace.ItemsSource = data.Datas;
                this.pb1.Total = data.Total;
                this.pb1.CurrentCount = data.Datas.Count;
                var ac = ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas;
                var group = data.Datas.GroupBy(obj => obj.FinaceAccountId);
                var moneyChange = group.Select(obj => new KeyValuePair<long, float>(obj.Key, obj.Sum(o => o.Money))).ToArray();
                var msg = "金额有变动:" + string.Join(",", moneyChange.Where(obj => Math.Abs(obj.Value) > 0.001).Select(obj => ac.First(o => o.Id == obj.Key).ShortInfo + ":" + obj.Value));
                msg += "   金额无变动：" + string.Join(",", moneyChange.Where(obj => Math.Abs(obj.Value) < 0.001).Select(obj => ac.First(o => o.Id == obj.Key).ShortInfo));
                this.pb1.TitleMessage = msg;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnTypeConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new FinanceTypeConfigWindow().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvFinace.SelectedCells.Count < 1)
                {
                    throw new Exception("未选择数据");
                }

                var fa = this.dgvFinace.SelectedCells[0].Item as ShopErp.Domain.Finance;
                if (fa == null)
                {
                    throw new Exception("绑定数据类型不为" + typeof(ShopErp.Domain.Finance).FullName);
                }

                if (MessageBox.Show("是否删除：" + fa.Type + "," + fa.Money, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<FinanceService>().Delete(fa.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new FinanceCreateBatchWindow().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}