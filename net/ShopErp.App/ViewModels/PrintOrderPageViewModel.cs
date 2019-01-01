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

namespace ShopErp.App.ViewModels
{
    class PrintOrderPageViewModel : DependencyObject
    {
        public static readonly DependencyProperty WorkStateMessageProperty = DependencyProperty.Register("WorkStateMessage", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PrintButtonStringProperty = DependencyProperty.Register("PrintButtonString", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty SelectedCountProperty = DependencyProperty.Register("SelectedCount", typeof(int), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool), typeof(PrintOrderPageViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PrintTemplateProperty = DependencyProperty.Register("PrintTemplate", typeof(Service.Print.PrintTemplate), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty AutoDeleteSucessOrderProperty = DependencyProperty.Register("AutoDeleteSucessOrder", typeof(bool), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PackageIdProperty = DependencyProperty.Register("PackageId", typeof(int), typeof(PrintOrderPageViewModel), new PropertyMetadata(0));

        private DeliveryPrintDocument printDoc = null;

        private PrintHistoryService printHistoryService = ServiceContainer.GetService<PrintHistoryService>();

        private OrderService orderService = ServiceContainer.GetService<OrderService>();

        public System.Collections.ObjectModel.ObservableCollection<PrintOrderViewModel> OrderViewModels { get; set; }

        private Dictionary<Order, List<PrintOrderViewModel>> orderVmToOrder = null;

        private bool lastOpError = false;

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

        public Service.Print.PrintTemplate PrintTemplate
        {
            get { return (Service.Print.PrintTemplate)this.GetValue(PrintTemplateProperty); }
            set { this.SetValue(PrintTemplateProperty, value); }
        }

        public bool AutoDeleteSucessOrder
        {
            get { return (bool)this.GetValue(AutoDeleteSucessOrderProperty); }
            set { this.SetValue(AutoDeleteSucessOrderProperty, value); }
        }

        public int PackageId
        {
            get { return (int)this.GetValue(PackageIdProperty); }
            set { this.SetValue(PackageIdProperty, value); }
        }


        public PrintOrderPageViewModel(PrintOrderViewModel[] orders)
        {
            this.OrderViewModels = new ObservableCollection<PrintOrderViewModel>();
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
            this.AutoDeleteSucessOrder = true;
            this.PrintButtonString = "打印";
        }

        private void OrderViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var first = this.OrderViewModels.FirstOrDefault();
            string title = first == null ? "未分配" : first.DeliveryCompany;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "未分配";
            }
            this.Title = title + "(" + this.OrderViewModels.Count + ")";
        }

        private bool HasSameReceiverInfo(Order o1, Order o2)
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

        private PrintOrderViewModel[] GetMatchOrderViewModels(Order order)
        {
            return this.orderVmToOrder[order].ToArray();
        }

        private string[] GetMatchOrderViewModelsWuliuId(Order order)
        {
            var orders = GetMatchOrderViewModels(order).OrderBy(obj => obj.Source.Id).ToArray();
            return orders.Select(obj => string.IsNullOrWhiteSpace(obj.Source.PopOrderId) ? obj.Source.Id.ToString() : obj.Source.PopOrderId).ToArray();
        }

        private void PrintOrderViewModelCheckedHandler(object sender, EventArgs e)
        {
            this.SelectedCount = this.OrderViewModels.Count(obj => obj.IsChecked);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PrintOrderPageViewModel.CheckedProperty)
            {
                for (int i = 0; i < this.OrderViewModels.Count; i++)
                {
                    this.OrderViewModels[i].IsChecked = this.Checked;
                }
            }
        }


        #region 打印控制流程


        /// <summary>
        /// 打印数据并返回下一个起始单号
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="template"></param>
        /// <param name="startDeliveryNumber"></param>
        /// <returns></returns>
        public void Print(string printer)
        {
            try
            {
                this.orderVmToOrder = new Dictionary<Order, List<PrintOrderViewModel>>();
                this.lastOpError = false;
                this.IsUserStop = false;
                this.IsRunning = true;
                this.PrintButtonString = "停止";

                this.WorkStateMessage = "第一步：正在检查与合并订单...";

                WPFHelper.DoEvents();

                var selectedOrderVMs = this.OrderViewModels.Where(obj => obj.IsChecked).ToArray();
                var selectedOrders = selectedOrderVMs.Select(obj => obj.Source).ToArray();
                if (selectedOrderVMs.Length < 1)
                {
                    throw new Exception("没有需要打印的订单");
                }
                //初始化待打印订单的状态，物流信息，单号
                foreach (var v in selectedOrderVMs)
                {
                    v.WuliuNumber = null;
                    v.DeliveryNumber = "";
                    v.State = "";
                    v.Background = null;
                    WPFHelper.DoEvents();
                }
                //检查是否打印过
                foreach (var o in selectedOrderVMs)
                {
                    if (printHistoryService.GetByAll(o.Source.Id, "", "", 0, DateTime.Now.AddDays(-60), DateTime.Now, 0, 0).Total > 0)
                    {
                        o.State = "已经打印过，请先删除打印历史";
                        throw new Exception("订单编号:" + o.Source.Id + " 已经打印过，请先删除打印历史");
                    }
                }

                var mergedOrders = new List<Order>();
                //在线支付，需要合并订单
                if (selectedOrders[0].PopPayType == PopPayType.ONLINE)
                {
                    //合并相同订单
                    foreach (var or in selectedOrders)
                    {
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
                        this.WorkStateMessage = string.Format("第二步：正在生成单号{0}/{1}...", i + 1, wuliuNumbers.Length);
                        wuliuNumbers[i] = ServiceContainer.GetService<WuliuNumberService>().GenCainiaoWuliuNumber(this.PrintTemplate.DeliveryCompany, mergedOrders[i], GetMatchOrderViewModelsWuliuId(mergedOrders[i]), this.PackageId > 0 ? this.PackageId.ToString() : "").First;
                        foreach (var ov in GetMatchOrderViewModels(mergedOrders[i]))
                        {
                            ov.WuliuNumber = wuliuNumbers[i];
                            ov.DeliveryCompany = PrintTemplate.DeliveryCompany;
                            ov.DeliveryNumber = wuliuNumbers[i].DeliveryNumber;
                            ov.State = "";
                            ov.PageNumber = i + 1;
                        }
                        WPFHelper.DoEvents();
                    }
                    catch (Exception ex)
                    {
                        var vms = GetMatchOrderViewModels(mergedOrders[i]);
                        foreach (var v in vms)
                        {
                            v.State = ex.Message;
                            v.Background = Brushes.Red;
                        }
                        throw;
                    }
                }
                this.printDoc = new GDIDeliveryPrintDocument { WuliuNumbers = wuliuNumbers };
                this.printDoc.PagePrintStarting += PrintDoc_PagePrintStarting;
                this.printDoc.PagePrintEnded += PrintDoc_PagePrintEnded;
                this.printDoc.PrintEnded += PrintDoc_PrintEnded;
                this.printDoc.PrintStarting += PrintDoc_PrintStarting;
                printDoc.StartPrint(mergedOrders.ToArray(), printer, false, this.PrintTemplate);
            }
            catch
            {
                this.IsRunning = false;
                this.IsUserStop = true;
                this.lastOpError = false;
                this.PrintButtonString = "打印";
                this.WorkStateMessage = "出现错误打印终止";
                throw ;
            }
            finally
            {
            }
        }

        private void PrintDoc_PrintStarting(object sender, EventArgs e)
        {
            //do nothing
        }

        private void PrintDoc_PrintEnded(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() => HandelPrintEnded()));
            }
            catch (Exception ex)
            {
                Log.Logger.Log(this.GetType() + ": PrintDoc_PrintEnded", ex);
            }
        }

        private bool PrintDoc_PagePrintEnded(object arg1, int arg2)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() => HandelPagePrintEnded(arg1 as DeliveryPrintDocument, arg2)));
                return this.lastOpError || this.IsUserStop;
            }
            catch (Exception ex)
            {
                Log.Logger.Log(this.GetType() + ": PrintDoc_PagePrintEnded", ex);
                return false;
            }

        }

        private bool PrintDoc_PagePrintStarting(object arg1, int arg2)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() => HandelPagePrintStarting(arg1 as DeliveryPrintDocument, arg2)));
                return this.lastOpError || this.IsUserStop;
            }
            catch (Exception ex)
            {
                Log.Logger.Log(this.GetType() + ": PrintDoc_PagePrintStarting", ex);
                return false;
            }
        }

        private void HandelPrintEnded()
        {
            //删除所有成功的打印
            if (this.AutoDeleteSucessOrder)
            {
                var suOrders = this.OrderViewModels.Where(obj => obj.State == "打印成功").ToArray();
                foreach (var v in suOrders)
                {
                    this.OrderViewModels.Remove(v);
                }
            }
            this.PackageId = 0;
            this.IsUserStop = false;
            this.printDoc = null;
            this.IsUserStop = true;
            this.IsRunning = false;
            this.PrintButtonString = "打印";
            this.WorkStateMessage = "已完成输出";
        }

        private void HandelPagePrintEnded(DeliveryPrintDocument pd, int arg2)
        {
            try
            {
                PrintOrderViewModel[] vms = this.GetMatchOrderViewModels(pd.Values[arg2]);
                foreach (var vm in vms)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(vm.DeliveryCompany) || string.IsNullOrWhiteSpace(vm.DeliveryNumber))
                        {
                            throw new Exception("上传打印信息失败：物流公司和编号为空");
                        }
                        vm.State = "上传记录";
                        PrintHistory ph = new PrintHistory
                        {
                            UploadTime = this.orderService.GetDBMinTime(),
                            DeliveryCompany = vm.DeliveryCompany,
                            DeliveryNumber = vm.DeliveryNumber,
                            DeliveryTemplate = this.PrintTemplate.Name,
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
                        vm.State = ee.Message;
                        vm.Background = Brushes.Red;
                        vm.State = ee.Message;
                        vm.Background = Brushes.Red;
                        this.lastOpError = true;
                    }
                }
                this.WorkStateMessage = string.Format("第三步：正在打印{0}/{1}...", this.OrderViewModels.Count(obj => obj.State == "打印成功"), this.OrderViewModels.Count(obj => obj.IsChecked));
            }
            catch (Exception ex)
            {
                this.lastOpError = true;
                MessageBox.Show(ex.Message);
            }
        }

        private void HandelPagePrintStarting(DeliveryPrintDocument pd, int arg2)
        {
            try
            {
                var order = pd.Values[arg2];
                var orders = this.GetMatchOrderViewModels(order);
                foreach (var v in orders)
                {
                    v.Background = Brushes.Yellow;
                    v.State = "正在打印";
                }
            }
            catch (Exception ex)
            {
                this.lastOpError = true;
                MessageBox.Show(ex.Message);
            }
        }

        public void Stop()
        {
            if (this.IsUserStop && this.printDoc != null)
            {
                this.IsUserStop = false;
            }
        }

        #endregion
    }
}