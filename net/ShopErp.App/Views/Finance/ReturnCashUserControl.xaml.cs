
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
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;


namespace ShopErp.App.Views.Finance
{
    /// <summary>
    /// Interaction logic for ReturnCashUserControl.xaml
    /// </summary>
    public partial class ReturnCashUserControl : UserControl
    {
        public ReturnCashUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).ToList();
            shops.Insert(0, new Shop {Id = 0, Mark = "  "});
            this.cbbShops.ItemsSource = shops;
            this.cbbShops.SelectedIndex = 0;
            this.cbbStates.Bind<ReturnCashState>();
            this.cbbStates.SelectedIndex = 1;
        }

        private void pb1_PageChanging(object sender, PageBar.PageChangeEventArgs e)
        {
            try
            {
                var datas = ServiceContainer.GetService<ReturnCashService>().GetByAll(e.GetParameter<long>("ShopId"),
                    e.GetParameter<string>("PopOrderId"), e.GetParameter<string>("Type"), "",
                    e.GetParameter<int>("TimeType"), e.GetParameter<DateTime>("StartTime"),
                    e.GetParameter<DateTime>("EndTime"),
                    e.GetParameter<ReturnCashState>("State"), e.CurrentPage - 1, e.PageSize);
                this.pb1.Total = datas.Total;
                this.dgvItems.ItemsSource = datas.Datas;
                double m = datas.Datas.Select(obj => obj.Money).Sum();
                this.pb1.TitleMessage = "当前页金额:" + datas.Datas.Select(obj => obj.Money).Sum().ToString("F0");
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
                var minTime = ServiceContainer.GetService<ShopService>().GetDBMinTime();
                this.pb1.Parameters.Clear();
                this.pb1.Parameters.Add("ShopId", (this.cbbShops.SelectedItem as Shop).Id);
                this.pb1.Parameters.Add("State", this.cbbStates.GetSelectedEnum<ReturnCashState>());
                this.pb1.Parameters.Add("TimeType", this.cbbTimeTypes.SelectedIndex);
                this.pb1.Parameters.Add("StartTime",
                    this.dpStartTime.Value == null ? minTime : this.dpStartTime.Value.Value);
                this.pb1.Parameters.Add("EndTime", this.dpEndTime.Value == null ? minTime : this.dpEndTime.Value.Value);
                this.pb1.Parameters.Add("PopOrderId", this.tbPopOrderId.Text.Trim());
                this.pb1.Parameters.Add("Type", this.cbbTypes.Text.Trim());
                this.pb1.StartPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private ReturnCash GetSelectedCach()
        {
            if (this.dgvItems.SelectedCells.Count < 1)
            {
                MessageBox.Show("请选择数据");
                return null;
            }
            return this.dgvItems.SelectedCells[0].Item as ReturnCash;
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            var rc = this.GetSelectedCach();
            if (rc == null)
            {
                return;
            }

            try
            {
                if (rc.State == ReturnCashState.COMPLETED)
                {
                    throw new Exception("好评返现已完成不能再处理");
                }
                ReturnCashCompleteWindow wi = new ReturnCashCompleteWindow {ReturnCash = rc};
                wi.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rc = this.GetSelectedCach();
            if (rc == null)
            {
                return;
            }
            try
            {
                if (rc.State == ReturnCashState.COMPLETED)
                {
                    throw new Exception("已完成的返现不能删除");
                }

                if (MessageBox.Show("是否删除?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Error) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<ReturnCashService>().Delete(rc.Id);
                MessageBox.Show("已删除");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}