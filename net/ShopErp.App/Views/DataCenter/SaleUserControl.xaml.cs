using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.Utils;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Views.Extenstions;

namespace ShopErp.App.Views.DataCenter
{
    /// <summary>
    /// Interaction logic for SaleUserControl.xaml
    /// </summary>
    public partial class SaleUserControl : UserControl
    {
        private bool myLoaded = false;

        public SaleUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                shops.Insert(0, new Shop { Mark = "" });
                this.cbbShops.ItemsSource = shops;
                this.cbbShops.SelectedIndex = 0;
                this.dpStart.Value = DateTime.Now.Date.AddDays(-30);
                this.dpEnd.Value = DateTime.Now.Date;
                this.cbbCharType.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
                this.cbbOrderTypes.Bind<OrderType>();
                this.cbbOrderTypes.SelectedIndex = 1;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private float GetCommissionPer(long shopId)
        {
            var shops = this.cbbShops.ItemsSource as IList<Shop>;
            if (shops == null || shops.Count < 2)
            {
                return 0;
            }
            var shop = shops.FirstOrDefault(obj => obj.Id == shopId);
            return shop == null ? 0 : shop.CommissionPer;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                long shopId = this.cbbShops.SelectedItem == null ? 0 : (this.cbbShops.SelectedItem as Shop).Id;
                var scs = ServiceContainer.GetService<OrderGoodsService>().GetSaleCount(shopId, this.cbbOrderTypes.GetSelectedEnum<OrderType>(), this.cbbTimeType.SelectedIndex, this.dpStart.Value.Value, this.dpEnd.Value.Value, 0, 0).Datas.ToArray();
                //订单信息金额汇总
                List<SaleInfo> sis = new List<SaleInfo>();
                //根据一个订单包含多个子订单重新计算每一双鞋子应该的价格，根据比例
                var ordersGroupById = scs.GroupBy(obj => obj.OrderId).Where(obj => obj.Count() > 1).ToArray();
                foreach (var or in ordersGroupById)
                {
                    float total = or.Select(obj => obj.Count * obj.PopPrice).Sum();
                    foreach (var o in or)
                    {
                        o.PopSellerGetMoney = o.PopSellerGetMoney * o.Count * o.PopPrice / total;
                    }
                }

                SaleCount[] targetScs = null;
                //左下分析，有货号，则分析商品SKU卖出情况
                string vendor = this.tbVendorName.Text.Trim();
                string goodsId = this.tbGoodsId.Text.Trim();

                //根据商品编号查询
                if (string.IsNullOrWhiteSpace(goodsId) == false)
                {
                    targetScs = scs.Where(obj => obj.GoodsId.ToString() == goodsId).ToArray();
                    float total = targetScs.Select(obj => obj.PopSellerGetMoney).Sum();
                    int count = targetScs.Length;
                    this.FillSaleMoneyInfo(targetScs);

                    //左下统计SKU详情
                    var skuGroup = targetScs.GroupBy(obj => obj.Color + ":" + obj.Size + "," + obj.Edtion);
                    this.dgvCountInfo2.ItemsSource = skuGroup
                        .Select(obj => new SaleCountInfo
                        {
                            VendorName = obj.Key,
                            Count = obj.Count(),
                            SaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum(),
                            PerSaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum() * 100F / total
                        }).OrderByDescending(obj => obj.PerSaleMoney).ToArray();

                    //右下统计每日销售占比
                    List<SaleCountInfo> scis = new List<SaleCountInfo>();
                    var dateGroup = targetScs
                        .GroupBy(obj => this.cbbTimeType.SelectedIndex == 0
                            ? obj.PopPayTime.Date
                            : obj.DeliveryTime.Date).ToArray();
                    foreach (var dg in dateGroup)
                    {
                        SaleCountInfo sc =
                            new SaleCountInfo
                            {
                                VendorName = dg.Key.ToString("yyyy-MM-dd"),
                                Number = string.Join(",", dg.Select(obj => obj.Number).Distinct()),
                                Count = dg.Count(),
                                SaleMoney = dg.Select(obj => obj.PopSellerGetMoney).Sum()
                            };
                        var matchScs = scs.Where(obj => (this.cbbTimeType.SelectedIndex == 0
                                                            ? obj.PopPayTime.Date
                                                            : obj.DeliveryTime.Date) == dg.Key);
                        sc.PerCount = sc.Count * 100F / matchScs.Count();
                        sc.PerSaleMoney = sc.SaleMoney * 100F / matchScs.Select(obj => obj.PopSellerGetMoney).Sum();
                        scis.Add(sc);
                    }
                    this.dgvGoodsInfo.ItemsSource = scis.OrderByDescending(obj => obj.VendorName).ToArray();
                }
                else if (string.IsNullOrWhiteSpace(vendor) == false)
                {
                    targetScs = scs.Where(obj => VendorService.FormatVendorName(obj.Vendor) ==
                                                 VendorService.FormatVendorName(vendor)).ToArray();
                    float total = targetScs.Select(obj => obj.PopSellerGetMoney).Sum();
                    int count = targetScs.Length;
                    //厂家查询
                    this.FillSaleMoneyInfo(targetScs);
                    //左下统计货号卖出详情
                    var numberGroup = targetScs.GroupBy(obj => obj.Number).ToArray();
                    this.dgvCountInfo2.ItemsSource = numberGroup
                        .Select(obj => new SaleCountInfo
                        {
                            VendorName = obj.Key,
                            Number = obj.Key,
                            Count = obj.Count(),
                            SaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum(),
                            PerCount = obj.Count() * 100F / count,
                            PerSaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum() * 100F / total
                        }).OrderByDescending(obj => obj.PerSaleMoney).ToArray();

                    //右下统计每日销售占比
                    List<SaleCountInfo> scis = new List<SaleCountInfo>();
                    var dateGroup = targetScs
                        .GroupBy(obj => this.cbbTimeType.SelectedIndex == 0
                            ? obj.PopPayTime.Date
                            : obj.DeliveryTime.Date).ToArray();
                    foreach (var dg in dateGroup)
                    {
                        SaleCountInfo sc =
                            new SaleCountInfo
                            {
                                VendorName = dg.Key.ToString("yyyy-MM-dd"),
                                Number = string.Join(",", dg.Select(obj => obj.Number).Distinct()),
                                Count = dg.Count(),
                                SaleMoney = dg.Select(obj => obj.PopSellerGetMoney).Sum()
                            };
                        var matchScs = scs.Where(obj => (this.cbbTimeType.SelectedIndex == 0
                                                            ? obj.PopPayTime.Date
                                                            : obj.DeliveryTime.Date) == dg.Key);
                        sc.PerCount = sc.Count * 100F / matchScs.Count();
                        sc.PerSaleMoney = sc.SaleMoney * 100F / matchScs.Select(obj => obj.PopSellerGetMoney).Sum();
                        scis.Add(sc);
                    }
                    this.dgvGoodsInfo.ItemsSource = scis.OrderByDescending(obj => obj.VendorName).ToArray();
                }
                else
                {
                    targetScs = scs;
                    //所有
                    this.FillSaleMoneyInfo(targetScs.ToArray());
                    float total = targetScs.Select(obj => obj.PopSellerGetMoney).Sum();
                    int count = targetScs.Length;
                    //左下统计厂家占比
                    var vGroup = targetScs.GroupBy(obj => VendorService.FormatVendorName(obj.Vendor)).ToArray();
                    this.dgvCountInfo2.ItemsSource = vGroup
                        .Select(obj => new SaleCountInfo
                        {
                            VendorName = obj.Key,
                            SaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum(),
                            Count = obj.Count(),
                            Number = "",
                            PerCount = obj.Count() * 100.0F / count,
                            PerSaleMoney = (100.0F * obj.Select(o => o.PopSellerGetMoney).Sum() / total)
                        }).OrderByDescending(obj => obj.PerSaleMoney).ToArray();
                    //右下统计商品销售占比
                    var gGroup = targetScs.GroupBy(obj => obj.GoodsId).ToArray();
                    this.dgvGoodsInfo.ItemsSource = gGroup
                        .Select(obj => new SaleCountInfo
                        {
                            VendorName = obj.First().Vendor,
                            Number = string.Join(",", obj.Select(o => o.Number).Distinct()),
                            Count = obj.Count(),
                            SaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum(),
                            PerCount = obj.Count() * 100F / count,
                            PerSaleMoney = obj.Select(o => o.PopSellerGetMoney).Sum() * 100F / total
                        }).OrderByDescending(obj => obj.PerSaleMoney).ToArray();
                }
                var ordersGroupByState = targetScs.GroupBy(obj => obj.State).OrderBy(obj => obj.Key).ToArray();
                foreach (var ogroup in ordersGroupByState)
                {
                    sis.Add(new SaleInfo
                    {
                        State = EnumUtil.GetEnumValueDescription(ogroup.Key),
                        Count = ogroup.Select(obj => obj.OrderId).Distinct().Count(),
                        SaleMoney = ogroup.Select(obj => obj.PopSellerGetMoney).Sum(),
                        CostMoney = ogroup.Select(obj => obj.ERPOrderGoodsMoney * obj.Count +
                                                         obj.ERPOrderDeliveryMoney +
                                                         obj.PopSellerGetMoney * GetCommissionPer(obj.ShopId)).Sum()
                    });
                }
                this.dgvCountInfo.ItemsSource = sis;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillSaleMoneyInfo(SaleCount[] scs)
        {
            //销量金额利润图标
            this.chart1.Series[0].Points.Clear();
            var dateGroups = scs
                .GroupBy(obj => this.cbbTimeType.SelectedIndex == 0 ? obj.PopPayTime.Date : obj.DeliveryTime.Date)
                .OrderBy(obj => obj.Key.Date).ToArray();
            int i = 1;
            foreach (var dg in dateGroups)
            {
                var os = dg.Where(obj => (int)obj.State <= (int)OrderState.SUCCESS).ToArray();
                var orderMoney = os.Select(obj => obj.PopSellerGetMoney).Sum();
                var costMoney = os.Select(obj => obj.ERPOrderDeliveryMoney + obj.ERPOrderGoodsMoney * obj.Count +
                                                 obj.PopSellerGetMoney * GetCommissionPer(obj.ShopId)).Sum();
                this.chart1.Series[0].Points.Add(new DataPoint
                {
                    XValue = i++,
                    YValues = new double[] { orderMoney },
                    AxisLabel = dg.Key.ToString("MM-dd"),
                    Label = orderMoney.ToString("F0") + "(" + (orderMoney - costMoney).ToString("F0") + ")"
                });
            }
        }


        private void cbbCharType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.chart1.Series[0].ChartType = (SeriesChartType)this.cbbCharType.SelectedItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}