using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.Service.Delivery;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Service.Excel;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryCheckUserControl.xaml
    /// </summary>
    public partial class DeliveryCheckUserControl : UserControl
    {
        private System.Collections.ObjectModel.ObservableCollection<DeliveryCheckViewModel> orders = new ObservableCollection<DeliveryCheckViewModel>();

        private int current = 0;
        private bool isRunning = false;
        private bool isStop = false;
        private Shop[] shops = null;

        public DeliveryCheckUserControl()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.isRunning)
                {
                    this.isStop = true;
                }
                else
                {
                    this.orders.Clear();
                    this.shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.ToArray();
                    var os = ServiceContainer.GetService<OrderService>().GetByAll("", "", "", "", "", 3,
                        DateTime.Now.AddDays(-60), DateTime.MinValue, "", "", OrderState.PRINTED, PopPayType.None, "",
                        "", null, -1, "", 0, OrderCreateType.NONE, OrderType.NORMAL, 0, 0).Datas.ToArray();
                    var orders = os.Select(obj => new DeliveryCheckViewModel(obj) { State = "" })
                        .OrderBy(obj => obj.Source.PopPayTime).ToArray();
                    if (orders.Length < 1)
                    {
                        MessageBox.Show("没有任何订单");
                        return;
                    }
                    foreach (var o in orders)
                    {
                        this.orders.Add(o);
                    }
                    this.dgvOrders.ItemsSource = this.orders;
                    this.tbTotal.Text = "当前共 : " + orders.Length + " 条记录";
                    if (this.chkAutoLoadDelivery.IsChecked.Value)
                    {
                        Task.Factory.StartNew(GetDeliveryInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var seOrder = this.orders.Where(obj => obj.IsChecked).ToArray();
                if (seOrder.Length < 1)
                {
                    MessageBox.Show("没有选择订单");
                    return;
                }
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "xlsx";
                sfd.Filter = "*.xlsx|Office 2007 文件";
                sfd.FileName = "胡平物流录入" + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss");

                var ret = sfd.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }
                var deliveryCompanyGroup = seOrder.GroupBy(obj => obj.Source.DeliveryCompany).ToArray();
                string dir = new FileInfo(sfd.FileName).DirectoryName;
                foreach (var g in deliveryCompanyGroup)
                {
                    var gs = g.Distinct(new DeliveryCheckViewModelComparer()).ToArray();
                    var contents = gs.Select(obj => new string[]
                    {
                        obj.Source.DeliveryNumber, obj.Source.ReceiverName, obj.Source.ReceiverPhone,
                        obj.Source.ReceiverMobile, obj.Source.ReceiverAddress
                    }).ToList();
                    contents.Insert(0, new string[] { "快递单号", "姓名", "座机", "手机", "地址" });
                    ExcelFile.WriteXlsx(dir + "\\" + DateTime.Now.ToString("MM_dd") + g.Key + ".xlsx", contents.ToArray());
                }
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GetDeliveryInfo()
        {
            this.isRunning = true;
            this.isStop = false;

            try
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.btnRefresh.Content = "停止"));
                int threadCount = this.orders.Count >= 10 ? 10 : this.orders.Count;
                int eachThreadCount = this.orders.Count / threadCount;
                List<DeliveryCheckViewModel>[] gvms = new List<DeliveryCheckViewModel>[threadCount];

                for (int i = 0; i < gvms.Count(); i++)
                {
                    gvms[i] = new List<DeliveryCheckViewModel>();
                    for (int j = 0; j < eachThreadCount; j++)
                    {
                        gvms[i].Add(this.orders[i * eachThreadCount + j]);
                    }
                }

                if (this.orders.Count % threadCount != 0)
                {
                    for (int j = 0; j < threadCount && eachThreadCount * threadCount + j < this.orders.Count; j++)
                    {
                        gvms[j].Add(this.orders[eachThreadCount * threadCount + j]);
                    }
                }
                this.isStop = false;
                this.isRunning = true;
                this.current = 0;
                Task.WaitAll(gvms.Select(obj => Task.Factory.StartNew(new Action(() => GetDeliveryInfo(obj.ToArray()))))
                    .ToArray());
                MessageBox.Show("已完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isStop = true;
                this.isRunning = false;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnRefresh.Content = "刷新"));
            }
        }

        private DeliveryTransationItem[] Query(string company, string deliveryNumber)
        {
            try
            {
                var dd = DeliveryService.Query(company, deliveryNumber);
                if (dd != null)
                {
                    return dd.Items.ToArray();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void GetDeliveryInfo(DeliveryCheckViewModel[] dvms)
        {
            foreach (var dvm in dvms)
            {
                string message = "";
                Brush br = null;
                bool isChecked = false;
                if (this.isStop)
                {
                    break;
                }
                try
                {
                    var shop = this.shops.FirstOrDefault(obj => obj.Id == dvm.Source.ShopId);
                    if (shop == null)
                    {
                        message = "店铺不存在";
                        br = Brushes.Red;
                        continue;
                    }
                    var dd = Query(dvm.Source.DeliveryCompany, dvm.Source.DeliveryNumber);
                    //没有物流，则检查发货后，第一条物流时间
                    if (dd == null || dd.Length < 1)
                    {
                        var time = DateTime.Now.Subtract(dvm.Source.PopDeliveryTime).TotalHours;
                        var sTime = shops.FirstOrDefault(obj => obj.Id == dvm.Source.ShopId).FirstDeliveryHours;
                        if (time >= sTime || time - sTime >= -1)
                        {
                            isChecked = true;
                            message = "没有第一条物流";
                            br = Brushes.Red;
                        }
                        else
                        {
                            message = "正常";
                        }
                        continue;
                    }

                    //只有一条物流，则检测第二条物流是否到期
                    if (dd.Length == 1)
                    {
                        var time = DateTime.Now.Subtract(dd[0].Time).TotalHours;
                        var sTime = shops.FirstOrDefault(obj => obj.Id == dvm.Source.ShopId).SecondDeliveryHours;
                        if (time >= sTime || time - sTime >= -1)
                        {
                            isChecked = true;
                            message = "第二条物流超时";
                            br = Brushes.Red;
                        }
                        else
                        {
                            message = "正常";
                        }


                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            dvm.FirstDeliveryInfo = dd[0].Time.ToString("yyyy-MM-dd HH:mm:ss") + ":" +
                                                    dd[0].Description;
                        }));

                        continue;
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dvm.FirstDeliveryInfo = dd[0].Time.ToString("yyyy-MM-dd HH:mm:ss") + ":" + dd[0].Description;
                        dvm.SecondDeliveryInfo = dd[1].Time.ToString("yyyy-MM-dd HH:mm:ss") + ":" + dd[1].Description;
                    }));
                    //其它情况不用管，因为如果超时就已经超了
                    message = "读取成功";
                    br = null;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    br = Brushes.Red;
                }
                finally
                {
                    lock (this.orders)
                    {
                        current++;
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dvm.State = message;
                        dvm.Background = br;
                        dvm.IsChecked = isChecked;
                        this.tbProgress.Text = "正在读取:" + current + "/" + this.orders.Count;
                    }));
                }
            }
        }

        #region 前选 后选 编辑地址

        private DeliveryCheckViewModel GetMIOrder(object sender)
        {
            MenuItem mi = sender as MenuItem;
            var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
            var cells = dg.SelectedCells;
            if (cells.Count < 1)
            {
                throw new Exception("未选择数据");
            }

            var item = cells[0].Item as DeliveryCheckViewModel;
            if (item == null)
            {
                throw new Exception("数据对象不正确");
            }
            return item;
        }

        private void miSelectPre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);
                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = i <= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void miSelectForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);

                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = i >= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }

    class DeliveryCheckViewModelComparer : IEqualityComparer<DeliveryCheckViewModel>
    {
        public bool Equals(DeliveryCheckViewModel x, DeliveryCheckViewModel y)
        {
            if (x != null && y != null)
            {
                return x.Source.DeliveryNumber.Equals(y.Source.DeliveryNumber);
            }
            return false;
        }

        public int GetHashCode(DeliveryCheckViewModel obj)
        {
            return base.GetHashCode();
        }
    }
}