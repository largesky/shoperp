using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.App.ViewModels;
using ShopErp.App.Views.Print;


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
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderReturnOutUserControl.xaml
    /// </summary>
    public partial class OrderReturnOutUserControl : UserControl
    {
        private bool myLoad = false;

        private System.Collections.ObjectModel.ObservableCollection<OrderReturnViewModel> OrderReturns =
            new System.Collections.ObjectModel.ObservableCollection<OrderReturnViewModel>();

        private OrderReturnService OrderReturnService = ServiceContainer.GetService<OrderReturnService>();

        public OrderReturnOutUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DateTime.Now.Hour > 12)
            {
                //下午的话，时间默认为当天
                this.dpStart.Value = DateTime.Now.Date;
            }
            else
            {
                //上午的话，默认读取昨天
                this.dpStart.Value = DateTime.Now.Date.AddDays(-1);
            }
            if (this.myLoad)
            {
                return;
            }
            this.dgvItems.ItemsSource = OrderReturns;
            this.OrderReturns.CollectionChanged += OrderReturns_CollectionChanged;
        }

        void OrderReturns_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.tbCount.Text = string.Format("当前共:{0}条退货记录,共:{1}双鞋子", this.OrderReturns.Count,
                this.OrderReturns.Select(obj => obj.Source.Count).Sum());
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OrderReturnViewModel order = (e.Source as Hyperlink).DataContext as OrderReturnViewModel;
                if (order == null)
                {
                    return;
                }
                this.OrderReturns.Remove(order);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbOrderReturnId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            try
            {
                int id = int.Parse(this.tbId.Text.Trim().ToUpper());
                if (this.OrderReturns.FirstOrDefault(obj => obj.Source.Id == id) != null)
                {
                    Speaker.Speak("已存在");
                    return;
                }
                var oc = this.OrderReturnService.GetById(id);
                if (oc == null)
                {
                    Speaker.Speak("未找到");
                    return;
                }

                this.OrderReturns.Add(new OrderReturnViewModel(oc));
                Speaker.Speak("已接受");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.tbId.Text = "";
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (this.OrderReturns.Count < 1)
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

                if (MessageBox.Show("是否使用打印机:" + printer + Environment.NewLine + "打印?", "提示", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var pd = PrintUtil.GetPrinter(printer);
                VendorService vs = ServiceContainer.GetService<VendorService>();
                var goodsCountDoc = new OrderReturnOutPrintDocument();

                List<GoodsCount> counts = new List<GoodsCount>();
                foreach (var item in this.OrderReturns)
                {
                    string[] infos =
                        item.Source.GoodsInfo.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (infos.Length < 4)
                    {
                        MessageBox.Show("退货信息不正确，请检查:" + item.Source.Id);
                        continue;
                    }
                    var vendor = vs.GetByAll(infos[0], "", "", "", 0, 0).First;
                    if (vendor == null)
                    {
                        vendor = vs.GetByAll(infos[0] + infos[1], "", "", "", 0, 0).First;
                    }

                    if (vendor == null)
                    {
                        MessageBox.Show("退货信息厂家找不到，请检查:" + item.Source.Id);
                        continue;
                    }

                    GoodsCount count = null;
                    if (infos.Length >= 5)
                    {
                        count = counts.FirstOrDefault(
                            obj => obj.Vendor == VendorService.FormatVendorName(infos[0]) && obj.Number == infos[1] &&
                                   obj.Edtion == infos[2] && obj.Color == infos[3] && obj.Size == infos[4]);
                    }
                    else
                    {
                        count = counts.FirstOrDefault(
                            obj => obj.Vendor == VendorService.FormatVendorName(infos[0]) && obj.Number == infos[1] &&
                                   obj.Color == infos[2] && obj.Size == infos[3]);
                    }

                    if (count == null)
                    {
                        count = new GoodsCount
                        {
                            Vendor = infos[0].Trim(),
                            Number = infos[1].Trim(),
                            Money = (int)(item.Source.GoodsMoney / item.Source.Count),
                            Count = 0,
                            FirstPayTime = item.Source.ProcessTime,
                        };

                        if (infos.Length >= 5)
                        {
                            count.Edtion = infos[2].Trim();
                            count.Color = infos[3].Trim();
                            count.Size = infos[4].Trim();
                        }
                        else
                        {
                            count.Edtion = "";
                            count.Color = infos[2].Trim();
                            count.Size = infos[3].Trim();
                        }

                        //解析门牌，街道
                        string address = vendor.MarketAddress;
                        string door = VendorService.FindDoor(address);
                        string area = VendorService.FindAreaOrStreet(address, "区");
                        string street = VendorService.FindAreaOrStreet(address, "街");
                        count.Address = string.Format("{0}-{1}-{2}", area, door, street);
                        count.Vendor = VendorService.FormatVendorName(count.Vendor);

                        int iArea = 0, iDoor = 0, iStreet = 0;

                        int.TryParse(area, out iArea);
                        int.TryParse(door, out iDoor);
                        int.TryParse(street, out iStreet);

                        count.Area = iArea;
                        count.Door = iDoor;
                        count.Street = iStreet;
                        counts.Add(count);
                    }
                    foreach (var c in counts.Where(obj => obj.Vendor == count.Vendor && obj.Number == count.Number &&
                                                          obj.Edtion == count.Edtion))
                    {
                        //取消最大金额值
                        if (c.Money < count.Money)
                        {
                            c.Money = count.Money;
                        }
                        else
                        {
                            count.Money = c.Money;
                        }
                    }

                    if (count.FirstPayTime >= item.Source.ProcessTime)
                    {
                        count.FirstPayTime = item.Source.ProcessTime;
                    }

                    count.Count += item.Source.Count;
                }
                IComparer<GoodsCount> comparer = new GoodsCountSortByDoor();
                counts.Sort(comparer); //区
                counts.Sort(comparer); //连
                counts.Sort(comparer); //门
                counts.Sort(comparer); //街
                counts.Sort(comparer); //货号
                counts.Sort(comparer); //版本
                counts.Sort(comparer); //颜色
                counts.Sort(comparer); //尺码

                goodsCountDoc.PageSize = new Size(793, 1122.24);
                goodsCountDoc.SetGoodsCount(counts.ToArray());
                pd.PrintDocument(goodsCountDoc, "退货统计");
                foreach (var item in this.OrderReturns)
                {
                    this.OrderReturnService.Update(item.Source);
                }
                MessageBox.Show("打印完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "打印出错");
            }
        }

        private void btnMark_Click(object sender, RoutedEventArgs e)
        {
            if (this.OrderReturns.Count < 1)
            {
                MessageBox.Show("没有需要打印的数据");
                return;
            }

            try
            {
                foreach (var item in this.OrderReturns)
                {
                    item.ProcessState = "处理中";
                    WPFHelper.DoEvents();
                    this.OrderReturnService.Update(item.Source);
                    item.ProcessState = "退货中";
                    WPFHelper.DoEvents();
                }
                MessageBox.Show("已完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("清空所有", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            this.OrderReturns.Clear();
        }

        private void btnGetDay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dpStart.Value == null)
                {
                    MessageBox.Show("请选择时间");
                    return;
                }

                var items = ServiceContainer.GetService<OrderReturnService>().GetByAll(0, 0, "", "", "",
                    OrderReturnState.NONE, OrderReturnType.NONE, 0, this.dpStart.Value.Value, DateTime.Now.AddHours(20),
                    0, 0);
                if (items.Datas.Count < 1)
                {
                    MessageBox.Show("没有退货记录");
                    return;
                }

                foreach (var item in items.Datas)
                {
                    if (this.OrderReturns.FirstOrDefault(obj => obj.Source.Id == item.Id) != null)
                    {
                        Speaker.Speak("已存在");
                        return;
                    }
                    var oc = item;
                    if (oc == null)
                    {
                        Speaker.Speak("未找到");
                        return;
                    }
                    this.OrderReturns.Add(new OrderReturnViewModel(oc));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}