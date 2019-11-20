using ShopErp.App.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using ShopErp.App.Log;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Views;
using ShopErp.App.Service.Print;
using ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument;
using ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument.TaobaoCainiao;
using ShopErp.App.Service.Net;
using Microsoft.Win32;

namespace ShopErp.App.ViewModels
{
    class PrintOrderPageViewModel : DependencyObject
    {
        public static readonly DependencyProperty WorkStateMessageProperty = DependencyProperty.Register("WorkStateMessage", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PrintButtonStringProperty = DependencyProperty.Register("PrintButtonString", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty SelectedCountProperty = DependencyProperty.Register("SelectedCount", typeof(int), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool), typeof(PrintOrderPageViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty WuliuPrintTemplateProperty = DependencyProperty.Register("WuliuPrintTemplate", typeof(WuliuPrintTemplate), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PackageIdProperty = DependencyProperty.Register("PackageId", typeof(int), typeof(PrintOrderPageViewModel), new PropertyMetadata(0));

        public static readonly DependencyProperty WuliuBrachProperty = DependencyProperty.Register("WuliuBrach", typeof(WuliuBranch), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty ShopProperty = DependencyProperty.Register("Shop", typeof(Shop), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PrinterProperty = DependencyProperty.Register("Printer", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PrintServerAddProperty = DependencyProperty.Register("PrintServerAdd", typeof(string), typeof(PrintOrderPageViewModel));

        private PrintHistoryService printHistoryService = ServiceContainer.GetService<PrintHistoryService>();

        private OrderService orderService = ServiceContainer.GetService<OrderService>();

        public System.Collections.ObjectModel.ObservableCollection<PrintOrderViewModel> OrderViewModels { get; set; }

        public ObservableCollection<WuliuBranch> WuliuBranches { get; set; }

        public ObservableCollection<WuliuPrintTemplate> WuliuPrintTemplates { get; set; }

        public ObservableCollection<string> Printers { get; set; }

        public ObservableCollection<Shop> Shops { get; set; }

        private DeliveryPrintDocument printDoc;

        private Dictionary<Order, List<PrintOrderViewModel>> orderVmToOrder;

        public bool IsUserStop { get; set; }

        public bool IsRunning { get; set; }

        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public string WorkStateMessage
        {
            get { return (string)this.GetValue(WorkStateMessageProperty); }
            private set { this.SetValue(WorkStateMessageProperty, value); }
        }

        public string PrintButtonString
        {
            get { return (string)this.GetValue(PrintButtonStringProperty); }
            set { this.SetValue(PrintButtonStringProperty, value); }
        }

        public int SelectedCount
        {
            get { return (int)this.GetValue(SelectedCountProperty); }
            set { this.SetValue(SelectedCountProperty, value); }
        }

        public bool Checked
        {
            get { return (bool)this.GetValue(CheckedProperty); }
            set { this.SetValue(CheckedProperty, value); }
        }

        public int PackageId
        {
            get { return (int)this.GetValue(PackageIdProperty); }
            set { this.SetValue(PackageIdProperty, value); }
        }

        public WuliuPrintTemplate WuliuPrintTemplate
        {
            get { return (WuliuPrintTemplate)this.GetValue(WuliuPrintTemplateProperty); }
            set { this.SetValue(WuliuPrintTemplateProperty, value); }
        }


        public WuliuBranch WuliuBranch
        {
            get { return (WuliuBranch)this.GetValue(WuliuBrachProperty); }
            set { this.SetValue(WuliuBrachProperty, value); }
        }

        public Shop Shop
        {
            get { return (Shop)this.GetValue(ShopProperty); }
            set { this.SetValue(ShopProperty, value); }
        }

        public string PrintServerAdd
        {
            get { return (string)this.GetValue(PrintServerAddProperty); }
            set { this.SetValue(PrintServerAddProperty, value); }
        }

        public string Printer
        {
            get { return (string)this.GetValue(PrinterProperty); }
            set { this.SetValue(PrinterProperty, value); }
        }

        public PrintOrderPageViewModel(PrintOrderViewModel[] orders)
        {
            this.OrderViewModels = new ObservableCollection<PrintOrderViewModel>();
            this.WuliuBranches = new ObservableCollection<WuliuBranch>();
            this.Shops = new ObservableCollection<Shop>();
            this.WuliuPrintTemplates = new ObservableCollection<WuliuPrintTemplate>();
            this.Printers = new ObservableCollection<string>();
            DependencyPropertyDescriptor notiy = DependencyPropertyDescriptor.FromProperty(PrintOrderViewModel.IsCheckedProperty, typeof(PrintOrderViewModel));
            foreach (var v in orders)
            {
                this.OrderViewModels.Add(v);
                notiy.AddValueChanged(v, PrintOrderViewModelCheckedHandler);
            }
            this.OrderViewModels.CollectionChanged += OrderViewModels_CollectionChanged;
            this.Title = (string.IsNullOrWhiteSpace(orders.First().DeliveryCompany) ? "未分配" : orders.First().DeliveryCompany) + "(" + orders.Length + ")";
            this.Checked = true;
            this.SelectedCount = orders.Count(obj => obj.IsChecked);
            this.PrintButtonString = "打印";
        }

        private void OrderViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //订单打印完，被删除后，会为空
            var first = this.OrderViewModels.FirstOrDefault();
            string title = first == null ? "未分配" : first.DeliveryCompany;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "未分配";
            }
            this.Title = title + "(" + this.OrderViewModels.Count + ")";
        }

        private static bool HasSameReceiverInfo(Order o1, Order o2)
        {
            if (o1 == null || o2 == null)
            {
                throw new ArgumentNullException("HasSameReceiverInfo");
            }

            return o1.ShopId == o2.ShopId &&
                   o1.PopBuyerId.Trim() == o2.PopBuyerId.Trim() &&
                   o1.ReceiverName.Trim() == o2.ReceiverName.Trim() &&
                   o1.ReceiverPhone.Trim() == o2.ReceiverPhone.Trim() &&
                   o1.ReceiverMobile.Trim() == o2.ReceiverMobile.Trim() &&
                   o1.ReceiverAddress.Trim() == o2.ReceiverAddress.Trim();
        }

        private string[] GetMatchOrderViewModelsWuliuId(Order order)
        {
            var orders = this.orderVmToOrder[order].OrderBy(obj => obj.Source.Id).ToArray();
            return orders.Select(obj => string.IsNullOrWhiteSpace(obj.Source.PopOrderId) ? obj.Source.Id.ToString() : obj.Source.PopOrderId).ToArray();
        }

        private void PrintOrderViewModelCheckedHandler(object sender, EventArgs e)
        {
            this.SelectedCount = this.OrderViewModels.Count(obj => obj.IsChecked);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            try
            {
                base.OnPropertyChanged(e);
                if (e.Property == PrintOrderPageViewModel.CheckedProperty)
                {
                    for (int i = 0; i < this.OrderViewModels.Count; i++)
                    {
                        this.OrderViewModels[i].IsChecked = this.Checked;
                    }
                }

                //店铺变了
                if (e.Property == PrintOrderPageViewModel.ShopProperty)
                {
                    this.WuliuBranch = null;
                    this.WuliuBranches.Clear();
                    this.PrintServerAdd = "";
                    if (e.NewValue == null)
                    {
                        return;
                    }
                    var wbs = ServiceContainer.GetService<WuliuNumberService>().GetWuliuBrachs(this.Shop);
                    foreach (var v in wbs.Datas)
                    {
                        this.WuliuBranches.Add(v);
                    }
                }
                //物流网点变了
                if (e.Property == PrintOrderPageViewModel.WuliuBrachProperty)
                {
                    this.WuliuPrintTemplate = null;
                    this.WuliuPrintTemplates.Clear();
                    if (e.NewValue == null)
                    {
                        return;
                    }
                    var pts = ServiceContainer.GetService<WuliuPrintTemplateService>().GetWuliuPrintTemplates(this.Shop, this.WuliuBranch.Type).Datas;
                    if (pts.Count < 1)
                    {
                        return;
                    }
                    foreach (var pt in pts)
                    {
                        this.WuliuPrintTemplates.Add(pt);
                    }
                    this.WuliuPrintTemplate = pts.FirstOrDefault();
                    if (WuliuPrintTemplate.SourceType == WuliuPrintTemplateSourceType.CAINIAO)
                    {
                        this.PrintServerAdd = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTSERVERADD_CAINIAO, "ws://localhost:13528");
                    }
                    else if (WuliuPrintTemplate.SourceType == WuliuPrintTemplateSourceType.PINDUODUO)
                    {
                        this.PrintServerAdd = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTSERVERADD_PDD, "wss://127.0.0.1:18653");
                    }
                    else
                    {
                        throw new Exception("暂时不支持的平台");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("变量更新失败，你可以先打开其它界面再返回这个界面，将再次重试：" + ex.Message);
            }
        }

        /// <summary>
        /// 重新读取相关数据，因为有时候配置会变，但读取订单很耗时。
        /// </summary>
        public void LoadBarValue()
        {
            this.WuliuBranches.Clear();
            this.WuliuPrintTemplates.Clear();
            this.Shops.Clear();
            this.Printers.Clear();

            foreach (var s in ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.WuliuEnabled))
            {
                this.Shops.Add(s);
            }

            var printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters.OfType<string>().ToArray();
            foreach (var s in printers)
            {
                this.Printers.Add(s);
            }
            this.Printer = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_DELIVERY_HOT, "");
        }

        #region 打印控制流程

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public void Print()
        {
            try
            {
                this.orderVmToOrder = new Dictionary<Order, List<PrintOrderViewModel>>();
                this.IsUserStop = false;
                this.IsRunning = true;
                this.PrintButtonString = "停止";

                string senderName = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, "");
                string senderPhone = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, "");

                if (string.IsNullOrWhiteSpace(senderName) || string.IsNullOrWhiteSpace(senderPhone))
                {
                    throw new Exception("系统中没有配置发货姓名和电话无法打印");
                }
                var selectedOrderVMs = this.OrderViewModels.Where(obj => obj.IsChecked).ToArray();
                var selectedOrders = selectedOrderVMs.Select(obj => obj.Source).ToArray();
                if (selectedOrderVMs.Length < 1)
                {
                    throw new Exception("没有需要打印的订单");
                }

                this.WorkStateMessage = "第一步：正在检查是否打印过...";
                WPFHelper.DoEvents();
                foreach (var o in selectedOrderVMs)
                {
                    if (printHistoryService.GetByAll(o.Source.Id, "", "", 0, DateTime.Now.AddDays(-30), DateTime.Now, 0, 0).Total > 0)
                    {
                        o.State = "已经打印过，请先删除打印历史";
                        o.Background = Brushes.Red;
                        throw new Exception("订单编号:" + o.Source.Id + " 已经打印过，请先删除打印历史");
                    }
                    WPFHelper.DoEvents();
                }

                this.WorkStateMessage = "第二步：正在重置当前打印数据...";
                WPFHelper.DoEvents();
                foreach (var v in selectedOrderVMs)
                {
                    v.WuliuNumber = null;
                    v.DeliveryNumber = "";
                    v.DeliveryCompany = "";
                    v.State = "";
                    v.Background = null;
                    WPFHelper.DoEvents();
                }

                this.WorkStateMessage = "第三步：正在合并订单数据...";
                WPFHelper.DoEvents();
                //在线支付，需要合并订单
                var mergedOrders = new List<Order>();
                if (selectedOrders[0].PopPayType == PopPayType.ONLINE)
                {
                    //合并相同订单
                    foreach (var or in selectedOrders)
                    {
                        if (this.IsUserStop)
                        {
                            throw new Exception("用户已停止打印");
                        }
                        WPFHelper.DoEvents();
                        var first = mergedOrders.FirstOrDefault(obj => HasSameReceiverInfo(or, obj));
                        if (first == null)
                        {
                            mergedOrders.Add(or);
                            List<PrintOrderViewModel> vms = new List<PrintOrderViewModel>();
                            vms.Add(this.OrderViewModels.First(obj => obj.Source.Id == or.Id));
                            orderVmToOrder.Add(or, vms);
                        }
                        else
                        {
                            //合并商品，订单可能被重复打印，以前合并过的，不再合并
                            foreach (var og in or.OrderGoodss)
                            {
                                if (first.OrderGoodss.Any(obj => obj.Id == og.Id) == false)
                                {
                                    first.OrderGoodss.Add(og);
                                }
                            }
                            orderVmToOrder[first].Add(this.OrderViewModels.First(obj => obj.Source.Id == or.Id));
                        }
                    }
                }
                else
                {
                    mergedOrders.AddRange(selectedOrders);
                    foreach (var mo in mergedOrders)
                    {
                        orderVmToOrder.Add(mo, this.OrderViewModels.Where(obj => obj.Source.Id == mo.Id).ToList());
                    }
                }
                //生成快递单号
                var wuliuNumbers = new WuliuNumber[mergedOrders.Count];
                for (int i = 0; i < wuliuNumbers.Length; i++)
                {
                    if (this.IsUserStop)
                    {
                        throw new Exception("用户已停止打印");
                    }

                    try
                    {
                        this.WorkStateMessage = string.Format("第四步：正在获取快递单号{0}/{1}...", i + 1, wuliuNumbers.Length);
                        WPFHelper.DoEvents();
                        wuliuNumbers[i] = ServiceContainer.GetService<WuliuNumberService>().GenWuliuNumber(this.Shop, this.WuliuPrintTemplate, mergedOrders[i], GetMatchOrderViewModelsWuliuId(mergedOrders[i]), this.PackageId > 0 ? this.PackageId.ToString() : "", senderName, senderPhone, this.WuliuBranch.SenderAddress).First;
                        foreach (var ov in this.orderVmToOrder[mergedOrders[i]])
                        {
                            ov.WuliuNumber = wuliuNumbers[i];
                            ov.DeliveryCompany = wuliuNumbers[i].DeliveryCompany;
                            ov.DeliveryNumber = wuliuNumbers[i].DeliveryNumber;
                            ov.State = "";
                            ov.PageNumber = i + 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        var vms = this.orderVmToOrder[mergedOrders[i]];
                        foreach (var v in vms)
                        {
                            v.State = ex.Message;
                            v.Background = Brushes.Red;
                        }
                        throw;
                    }
                }

                var allShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                var vs = ServiceContainer.GetService<VendorService>();
                //生成自定义打印数据
                var userDatas = new Dictionary<string, string>[mergedOrders.Count];
                for (int i = 0; i < userDatas.Length; i++)
                {
                    if (this.IsUserStop)
                    {
                        throw new Exception("用户已停止打印");
                    }

                    this.WorkStateMessage = string.Format("第五步：正在生成自定义数据{0}/{1}...", i + 1, wuliuNumbers.Length);
                    WPFHelper.DoEvents();
                    userDatas[i] = new Dictionary<string, string>();
                    StringBuilder goods_commment = new StringBuilder();
                    if (mergedOrders[i].Type == OrderType.NORMAL)
                    {
                        if (mergedOrders[i].OrderGoodss != null && mergedOrders[i].OrderGoodss.Count > 0)
                        {
                            foreach (var goods in mergedOrders[i].OrderGoodss.Where(obj => (int)obj.State <= (int)OrderState.SUCCESS))
                            {
                                goods_commment.AppendLine(vs.GetVendorPingyingName(goods.Vendor).ToUpper() + " " + goods.Number + " " + goods.Edtion + " " + goods.Color + " " + goods.Size + " (" + goods.Count + ")");
                            }
                        }
                        if (mergedOrders[i].PopPayType != PopPayType.COD)
                            goods_commment.AppendLine(mergedOrders[i].PopSellerComment);
                    }
                    userDatas[i].Add("payTime", "付款：" + mergedOrders[i].PopPayTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    userDatas[i].Add("shopMark", allShops.FirstOrDefault(obj => obj.Id == mergedOrders[i].ShopId).Mark);
                    userDatas[i].Add("goodsCount", mergedOrders[i].OrderGoodss.Select(obj => obj.Count).Sum().ToString());
                    userDatas[i].Add("goodsInfoSellerComment", goods_commment.ToString());
                    userDatas[i].Add("suminfo", string.Format("店:{0},数:{1},付:{2}", allShops.FirstOrDefault(obj => obj.Id == mergedOrders[i].ShopId).Mark, mergedOrders[i].OrderGoodss.Select(obj => obj.Count).Sum().ToString(), mergedOrders[i].PopPayTime.ToString("yyyy-MM-dd HH:mm:ss")));
                }

                this.WorkStateMessage = string.Format("第六步：输出打印数据...");
                WPFHelper.DoEvents();
                this.printDoc = new CainiaoPrintDocument(mergedOrders.ToArray(), wuliuNumbers, userDatas, this.WuliuPrintTemplate);
                string file = printDoc.StartPrint(this.Printer, this.PrintServerAdd);
                this.WorkStateMessage = string.Format("第七步：保存打印记录...");
                WPFHelper.DoEvents();
                UploadPrintHistory(selectedOrderVMs);
                HandelPrintEnded();
                if (this.WuliuPrintTemplate.SourceType == WuliuPrintTemplateSourceType.CAINIAO)
                {
                    LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTSERVERADD_CAINIAO, this.PrintServerAdd);
                }
                else if (this.WuliuPrintTemplate.SourceType == WuliuPrintTemplateSourceType.PINDUODUO)
                {
                    LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTSERVERADD_PDD, this.PrintServerAdd);
                }
                if (string.IsNullOrWhiteSpace(file) == false && file.StartsWith("http"))
                {
                    //下载文件
                    byte[] content = MsHttpRestful.GetUrlEncodeBodyReturnBytes(file, null);
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.AddExtension = true;
                    sfd.DefaultExt = "pdf";
                    sfd.Filter = "*.pdf|PDF 文件";
                    sfd.FileName = "快递单 " + this.WuliuPrintTemplate.DeliveryCompany + " " + DateTime.Now.ToString("MM-dd") + ".pdf";
                    if (sfd.ShowDialog().Value == false)
                    {
                        return;
                    }
                    File.WriteAllBytes(sfd.FileName, content);
                }
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_DELIVERY_HOT, this.Printer);
            }
            finally
            {
                HandelPrintEnded();
            }
        }

        private void HandelPrintEnded()
        {
            //删除所有成功的打印
            var suOrders = this.OrderViewModels.Where(obj => obj.State == "打印成功").ToArray();
            foreach (var v in suOrders)
            {
                this.OrderViewModels.Remove(v);
            }
            this.PackageId = 0;
            this.IsUserStop = false;
            this.printDoc = null;
            this.IsRunning = false;
            this.PrintButtonString = "打印";
            this.WorkStateMessage = "已完成输出";
        }

        private void UploadPrintHistory(PrintOrderViewModel[] orderViewModels)
        {
            foreach (var vm in orderViewModels)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(vm.DeliveryCompany) || string.IsNullOrWhiteSpace(vm.DeliveryNumber))
                    {
                        throw new Exception("上传打印信息失败：物流公司和编号为空");
                    }
                    PrintHistory ph = new PrintHistory
                    {
                        UploadTime = this.orderService.GetDBMinTime(),
                        DeliveryCompany = vm.DeliveryCompany,
                        DeliveryNumber = vm.DeliveryNumber,
                        DeliveryTemplate = this.WuliuPrintTemplate.Name,
                        Operator = OperatorService.LoginOperator.Number,
                        OrderId = vm.Source.Id,
                        ReceiverAddress = vm.Source.ReceiverAddress,
                        ReceiverMobile = vm.Source.ReceiverMobile,
                        ReceiverName = vm.Source.ReceiverName,
                        ReceiverPhone = vm.Source.ReceiverPhone,
                        CreateTime = DateTime.Now,
                        GoodsInfo = vm.Goods,
                        PopOrderId = vm.Source.PopOrderId,
                        ShopId = vm.Source.ShopId,
                        Id = 0,
                        PageNumber = vm.PageNumber,
                    };
                    this.printHistoryService.Save(ph);
                    vm.State = "打印成功";
                    vm.Background = null;
                    WPFHelper.DoEvents();
                }
                catch (Exception ee)
                {
                    vm.State = "保存打印记录失败：" + ee.Message;
                    vm.Background = Brushes.Red;
                }
            }
        }

        public void Stop()
        {
            this.IsUserStop = true;
        }

        #endregion
    }
}