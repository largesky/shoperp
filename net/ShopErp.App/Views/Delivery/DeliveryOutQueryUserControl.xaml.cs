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
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryOutQueryUserControl.xaml
    /// </summary>
    public partial class DeliveryOutQueryUserControl : UserControl
    {
        private DeliveryOut[] outs = null;

        public DeliveryOutQueryUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> com = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToList();
                com.Insert(0, "");
                List<Shop> shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.ToList();
                shops.Insert(0, new Shop { });
                var shippers = ServiceContainer.GetService<GoodsService>().GetAllShippers().Datas;
                shippers.Insert(0, "");

                this.cbbDeliveryCompany.ItemsSource = com;
                this.cbbShops.ItemsSource = shops;
                this.cbbShops.SelectedIndex = 0;
                this.cbbPayTypes.Bind<PopPayType>();
                this.cbbPayTypes.SelectedIndex = 0;
                this.cbbShippers.ItemsSource = shippers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string deliveryCompany = this.cbbDeliveryCompany.Text.Trim();
                string deliveryNumber = this.tbDeliveryNumber.Text.Trim();
                DateTime start = this.dpStart.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpStart.Value.Value;
                DateTime end = this.dpEnd.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpEnd.Value.Value;
                string vendor = this.tbVendor.Text.Trim();
                string number = this.tbNumber.Text.Trim();
                var data = ServiceContainer.GetService<DeliveryOutService>().GetByAll(this.cbbPayTypes.GetSelectedEnum<PopPayType>(), (this.cbbShops.SelectedItem as Shop).Id, deliveryCompany, deliveryNumber, vendor, number, this.cbbShippers.Text.Trim(), start, end, 0, 0);
                var sortDatas = data.Datas.OrderBy(obj => obj.DeliveryCompany).ToArray();
                this.dgvItems.ItemsSource = sortDatas;
                this.outs = sortDatas;
                //生成统计信息
                string message = string.Format("当前共:{0}条发货记录,  {1}条快递记录,  成本运费金额:{2},  平台运费金额:{3},  货到付款服务费用:{4},  成本商品金额:{5},  平台商品金额:{6}",
                    this.outs.Length,
                    this.outs.Select(obj => obj.DeliveryNumber).Distinct().Count(),
                    this.outs.Select(obj => obj.ERPDeliveryMoney).Sum(),
                    this.outs.Select(obj => obj.PopDeliveryMoney).Sum(),
                    this.outs.Select(obj => obj.PopCodSevFee).Sum(),
                    this.outs.Select(obj => obj.ERPGoodsMoney).Sum(),
                    this.outs.Select(obj => obj.PopGoodsMoney).Sum());
                this.tbTotal.Text = message;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvItems.SelectedCells.Count < 1)
                {
                    return;
                }

                if (OperatorService.LoginOperator.Rights.Contains("删除发货记录") == false)
                {
                    throw new Exception("你没有权限删除");
                }

                if (MessageBox.Show("是否删除?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var item = this.dgvItems.SelectedCells[0].Item as DeliveryOut;
                ServiceContainer.GetService<DeliveryOutService>().Delete(item.Id);
                MessageBox.Show("删除成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnMatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.outs == null)
                {
                    this.outs = new DeliveryOut[0];
                }

                if (this.outs.Select(obj => obj.DeliveryCompany).Distinct().Count() != 1)
                {
                    throw new Exception("一次只能比较一家快递公司的数据");
                }
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog() { Multiselect = true };
                var ret = ofd.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }
                new DeliveryMatchWindow { DeliveryOuts = this.outs, Files = ofd.FileNames }.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}