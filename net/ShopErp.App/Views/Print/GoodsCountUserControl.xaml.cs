using ShopErp.App.Domain;
using ShopErp.App.Utils;
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
using System.Drawing.Printing;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System.ComponentModel;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// CountUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class GoodsCountUserControl : UserControl
    {
        private OrderGoodsService goodsCountRepertory = ServiceContainer.GetService<OrderGoodsService>();

        private System.Collections.ObjectModel.ObservableCollection<GoodsCount> gcs =
            new System.Collections.ObjectModel.ObservableCollection<GoodsCount>();

        private bool myLoaded = false;

        public GoodsCountUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tbLastInfo.Text = "最后统计信息:" + ServiceContainer.GetService<SystemConfigService>().Get(-1, "GoodsCountLastOrder", "");
                if (this.myLoaded)
                {
                    return;
                }
                //旗帜
                var flags = new ColorFlag[] { ColorFlag.UN_LABEL, ColorFlag.RED, ColorFlag.YELLOW, ColorFlag.GREEN, ColorFlag.BLUE, ColorFlag.PINK };
                var flagVms = flags.Select(obj => new OrderFlagViewModel(false, obj)).ToArray();
                flagVms.Where(obj => obj.Flag == ColorFlag.GREEN || obj.Flag == ColorFlag.BLUE).ToList().ForEach(obj => obj.IsChecked = true);
                this.cbbFlags.ItemsSource = flagVms;
                this.dgvGoodsCount.ItemsSource = this.gcs;
                this.dpStart.Value = DateTime.Now.AddDays(-45);
                this.myLoaded = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private List<ColorFlag> GetSelectedOrderFlags()
        {
            var ovms = (this.cbbFlags.ItemsSource as OrderFlagViewModel[]).Where(obj => obj.IsChecked)
                .Select(obj => obj.Flag).ToList();
            return ovms;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flags = this.GetSelectedOrderFlags().ToArray();
                if (flags.Count() < 1)
                {
                    MessageBox.Show("必须选择颜色");
                    return;
                }

                var end = this.dpEnd.Value == null ? DateTime.Now.AddDays(1) : this.dpEnd.Value.Value;

                List<GoodsCount> counts = goodsCountRepertory.GetGoodsCount(flags, this.dpStart.Value.Value, end, 0, 0).Datas;
                IComparer<GoodsCount> comparer = this.cbbSortType.SelectedIndex == 0 ? new GoodsCountSortByDoor() as IComparer<GoodsCount> : new GoodsCountSortByStreet() as IComparer<GoodsCount>;
                counts.Sort(comparer); //区
                counts.Sort(comparer); //连
                counts.Sort(comparer); //门
                counts.Sort(comparer); //街
                counts.Sort(comparer); //货号
                counts.Sort(comparer); //版本
                counts.Sort(comparer); //颜色
                counts.Sort(comparer); //尺码
                this.gcs.Clear();

                foreach (var col in this.dgvGoodsCount.Columns)
                {
                    col.SortDirection = null;
                }
                foreach (var item in counts)
                {
                    this.gcs.Add(item);
                }
                this.tbTotalCount.Text = string.Format("共下载:{0}个厂家,{1}条记录,{2}件商品，商品金额:{3:F2}", this.gcs.Select(obj => obj.Vendor).Distinct().Count(), this.gcs.Count, this.gcs.Select(obj => obj.Count).Sum(), this.gcs.Select(obj => obj.Count * obj.Money).Sum());
                this.dgvGoodsCount.Items.SortDescriptions.Clear();
                MessageBox.Show("下载成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("下载出错:" + ex.Message);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var items = this.gcs.ToArray();
            if (items == null || items.Length < 1)
            {
                MessageBox.Show("没有需要打印的数据");
                return;
            }

            try
            {
                string printer = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_A4, "");
                if (string.IsNullOrWhiteSpace(printer))
                {
                    throw new Exception("请在系统配置里面，配置要使用的打印机");
                }
                string message = string.Format("是否打印:\n打印机:{0}\n打印数量:{1}", printer,
                    items.Select(obj => obj.Count).Sum());
                if (MessageBox.Show(message, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                var pd = PrintUtil.GetPrinter(printer);
                GoodsCountPrintDocument goodsCountDoc = new GoodsCountPrintDocument();
                goodsCountDoc.PageSize = new Size(796.8, 1123.2);
                goodsCountDoc.SetGoodsCount(items);
                pd.PrintDocument(goodsCountDoc, "拿货统计");
                SaveLastOrderInfo();
                MessageBox.Show("打印完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "打印出错");
            }
        }

        private void btnCreateReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvGoodsCount.SelectedCells.Count < 1)
                {
                    MessageBox.Show("请选择商品");
                    return;
                }

                var item = this.dgvGoodsCount.SelectedCells[0].Item as GoodsCount;
                var s = ServiceContainer.GetService<OrderReturnService>();
                var gu = ServiceContainer.GetService<GoodsService>().GetById(item.NumberId);
                if (gu == null || item.NumberId < 1)
                {
                    MessageBox.Show("该商品不能正确解析");
                    return;
                }

                var vendor = ServiceContainer.GetService<VendorService>().GetById(gu.VendorId);

                if (vendor == null)
                {
                    MessageBox.Show("没有找到厂家信息");
                    return;
                }

                OrderReturn or = new OrderReturn
                {
                    Comment = "次品退货",
                    Count = 1,
                    CreateOperator = OperatorService.LoginOperator.Number,
                    CreateTime = DateTime.Now,
                    DeliveryCompany = "次品退货",
                    DeliveryNumber = "次品退货",
                    GoodsInfo = string.Join(" ", vendor.Name + "," + item.Number, item.Edtion, item.Color, item.Size),
                    GoodsMoney = (float)item.Money,
                    Id = 0,
                    NewOrderId = 0,
                    OrderGoodsId = 0,
                    OrderId = 0,
                    ProcessOperator = OperatorService.LoginOperator.Number,
                    ProcessTime = DateTime.Now,
                    Reason = OrderReturnReason.DAY7,
                    State = OrderReturnState.PROCESSED,
                    Type = OrderReturnType.NONEORDER,
                };
                s.Save(or);
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPrintNew_Click(object sender, RoutedEventArgs e)
        {
            var items = this.gcs.ToArray();
            if (items == null || items.Length < 1)
            {
                MessageBox.Show("没有需要打印的数据");
                return;
            }

            try
            {
                string printer = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_A4, "");
                if (string.IsNullOrWhiteSpace(printer))
                {
                    throw new Exception("请在系统配置里面，配置要使用的打印机");
                }

                //数据数量过滤
                int minCount = 1;
                int.TryParse(this.tbMinCount.Text.Trim(), out minCount);
                Dictionary<string, List<GoodsCount>> gcs = new Dictionary<string, List<GoodsCount>>();
                foreach (var gc in items)
                {
                    if (items.Where(obj => obj.Address == gc.Address).Select(o => o.Count).Sum() >= minCount)
                    {
                        if (gcs.ContainsKey(gc.Vendor) == false)
                        {
                            gcs[gc.Vendor] = new List<GoodsCount>();
                        }
                        gcs[gc.Vendor].Add(gc);
                    }
                }

                string message = string.Format("是否打印:\n打印机:{0}\n打印数量:{1}", printer, gcs.Select(obj => obj.Value.Count).Sum());
                if (MessageBox.Show(message, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var pd = PrintUtil.GetPrinter(printer);
                GoodsCountPrintDocument2 goodsCountDoc = new GoodsCountPrintDocument2();
                goodsCountDoc.PageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                goodsCountDoc.SetGoodsCount(gcs, LocalConfigService.GetValue("GOODS_NAME", ""), LocalConfigService.GetValue("GOODS_PHONE", ""));
                pd.PrintDocument(goodsCountDoc, "拿货统计");
                MessageBox.Show("打印完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "打印出错");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvGoodsCount.SelectedCells.Count < 1)
                {
                    return;
                }
                if (MessageBox.Show("是否删除选中的商品", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var items = this.dgvGoodsCount.SelectedCells.Select(obj => obj.Item as GoodsCount).Distinct();
                foreach (var i in items)
                {
                    if (this.gcs.Contains(i))
                    {
                        this.gcs.Remove(i);
                    }
                }
                this.tbTotalCount.Text = string.Format("共下载:{0}个厂家,{1}条记录,{2}件商品，商品金额:{3:F2}", this.gcs.Select(obj => obj.Vendor).Distinct().Count(), this.gcs.Count, this.gcs.Select(obj => obj.Count).Sum(), this.gcs.Select(obj => obj.Count * obj.Money).Sum());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveLastOrderInfo()
        {
            if (this.gcs == null || this.gcs.Count < 1)
            {
                return;
            }
            var lastOrder = this.gcs.OrderByDescending(obj => obj.LastPayTime).First();
            string info = lastOrder.LastPayTime.ToString("yyyy-MM-dd HH:mm:ss") + "," + string.Join(" ", lastOrder.Vendor, lastOrder.Number, lastOrder.Edtion, lastOrder.Color, lastOrder.Size);
            ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, "GoodsCountLastOrder", info);
            this.tbLastInfo.Text = "最后统计信息:" + info;
        }
    }
}