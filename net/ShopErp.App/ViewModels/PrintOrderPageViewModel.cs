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

namespace ShopErp.App.ViewModels
{
    class PrintOrderPageViewModel : DependencyObject
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty CanStartPrintProperty = DependencyProperty.Register("CanStartPrint", typeof(bool), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty CanStopPrintProperty = DependencyProperty.Register("CanStopPrint", typeof(bool), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty SelectedCountProperty = DependencyProperty.Register("SelectedCount", typeof(int), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty SelectTopCountProperty = DependencyProperty.Register("SelectTopCount", typeof(int), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool), typeof(PrintOrderPageViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty StartDeliveryNumberProperty = DependencyProperty.Register("StartDeliveryNumber", typeof(string), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PrintTemplateProperty = DependencyProperty.Register("PrintTemplate", typeof(PrintTemplate), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty AutoDeleteSucessOrderProperty = DependencyProperty.Register("AutoDeleteSucessOrder", typeof(bool), typeof(PrintOrderPageViewModel));

        public static readonly DependencyProperty PackageIdProperty = DependencyProperty.Register("PackageId", typeof(int), typeof(PrintOrderPageViewModel), new PropertyMetadata(0));

        private OrderPrintDocument printDoc = null;

        private PrintHistoryService printHistoryService = ServiceContainer.GetService<PrintHistoryService>();

        private OrderService orderService = ServiceContainer.GetService<OrderService>();

        public System.Collections.ObjectModel.ObservableCollection<PrintOrderViewModel> OrderViewModels { get; set; }

        private Dictionary<Order, List<PrintOrderViewModel>> orderVmToOrder = null;

        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public bool IsStop { get; set; }

        public string State
        {
            get { return (string)this.GetValue(StateProperty); }
            private set { this.SetValue(StateProperty, value); }
        }

        public bool CanStartPrint
        {
            get { return (bool)this.GetValue(CanStartPrintProperty); }
            set { this.SetValue(CanStartPrintProperty, value); }
        }

        public bool CanStopPrint
        {
            get { return (bool)this.GetValue(CanStopPrintProperty); }
            set { this.SetValue(CanStopPrintProperty, value); }
        }

        public int SelectedCount
        {
            get { return (int)this.GetValue(SelectedCountProperty); }
            set { this.SetValue(SelectedCountProperty, value); }
        }

        public int SelectTopCount
        {
            get { return (int)this.GetValue(SelectTopCountProperty); }
            set { this.SetValue(SelectTopCountProperty, value); }
        }

        public bool Checked
        {
            get { return (bool)this.GetValue(CheckedProperty); }
            set { this.SetValue(CheckedProperty, value); }
        }

        public string StartDeliveryNumber
        {
            get { return (string)this.GetValue(StartDeliveryNumberProperty); }
            set { this.SetValue(StartDeliveryNumberProperty, value); }
        }

        public PrintTemplate PrintTemplate
        {
            get { return (PrintTemplate)this.GetValue(PrintTemplateProperty); }
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
            this.SelectTopCount = 0;
            this.AutoDeleteSucessOrder = true;
            this.CanStartPrint = true;
            this.CanStopPrint = false;
            this.StartDeliveryNumber = "";
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
                int inputCount = this.SelectTopCount;
                for (int i = 0; i < this.OrderViewModels.Count; i++)
                {
                    this.OrderViewModels[i].IsChecked = inputCount < 1
                        ? this.Checked
                        : ((i + 1) <= inputCount ? this.Checked : false);
                }
            }
            if (e.Property == PrintOrderPageViewModel.PrintTemplateProperty)
            {
                try
                {
                    var printTemplate = this.PrintTemplate;
                    if (printTemplate == null || printTemplate.PaperType == PaperType.HOT)
                    {
                        this.StartDeliveryNumber = "";
                    }
                    else
                    {
                        this.StartDeliveryNumber = LocalConfigService.GetValue(printTemplate.Name, "");
                    }
                }
                catch (Exception ex)
                {
                    this.StartDeliveryNumber = "";
                    MessageBox.Show(ex.Message, "获取历史单号失败");
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
        public string Print(PrintDialog pd)
        {
            try
            {
                PrintTemplate template = this.PrintTemplate;
                this.CanStartPrint = false;
                this.CanStopPrint = true;
                this.IsStop = false;
                this.orderVmToOrder = new Dictionary<Order, List<PrintOrderViewModel>>();
                this.State = "第一步：正在检查与合并订单...";

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
                WuliuNumber current = new WuliuNumber
                {
                    DeliveryNumber = StartDeliveryNumber,
                    DeliveryCompany = template.DeliveryCompany
                };
                for (int i = 0; i < wuliuNumbers.Length; i++)
                {
                    if (this.IsStop)
                    {
                        throw new Exception("用户已停止打印");
                    }

                    try
                    {
                        this.State = string.Format("第二步：正在生成单号{0}/{1}...", i + 1, wuliuNumbers.Length);
                        if (template.PaperType != PaperType.HOT)
                        {
                            wuliuNumbers[i] = current;
                            if (ServiceContainer.GetService<PrintHistoryService>().GetByAll(0, template.DeliveryCompany, wuliuNumbers[i].DeliveryNumber, 0, DateTime.Now.AddDays(-60), DateTime.Now, 0, 0).Total > 0)
                            {
                                throw new Exception("订单编号:" + mergedOrders[i].Id + ", 快递单号:" + wuliuNumbers[i].DeliveryNumber + "已经在2个月内使用");
                            }
                            current = ServiceContainer.GetService<WuliuNumberService>().GenNormalWuliuNumber(template.DeliveryCompany, current.DeliveryNumber, mergedOrders[i].ReceiverAddress).First;
                        }
                        else
                        {
                            wuliuNumbers[i] = ServiceContainer.GetService<WuliuNumberService>().GenCainiaoWuliuNumber(template.DeliveryCompany, mergedOrders[i], GetMatchOrderViewModelsWuliuId(mergedOrders[i]), this.PackageId > 0 ? this.PackageId.ToString() : "").First;
                        }
                        //复制快递面单信息到订单信息与打印订单信息中以便打印与显示
                        foreach (var ov in GetMatchOrderViewModels(mergedOrders[i]))
                        {
                            ov.WuliuNumber = wuliuNumbers[i];
                            ov.DeliveryCompany = PrintTemplate.DeliveryCompany;
                            ov.DeliveryNumber = wuliuNumbers[i].DeliveryNumber;
                            ov.Source.DeliveryCompany = PrintTemplate.DeliveryCompany;
                            ov.Source.DeliveryNumber = wuliuNumbers[i].DeliveryNumber;
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
                    }
                }
                //生成打印数据
                pd.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(PrintTemplate.Width, PrintTemplate.Height);
                this.printDoc = new OrderPrintDocument();
                this.printDoc.PagePrintStarting += new Action<OrderPrintDocument, Order>(printDoc_PrintStarting);
                this.printDoc.PageGenStarting += PrintDoc_PageGenStarting;
                this.printDoc.GenPages(mergedOrders.ToArray(), wuliuNumbers, template);

                foreach (var preOne in selectedOrderVMs.Where(obj => obj.State == "正在生成"))
                {
                    preOne.State = "生成完成";
                    preOne.Background = null;
                }

                pd.PrintDocument(printDoc, PrintTemplate.DeliveryCompany);
                //储存最后一个元素
                foreach (var preOne in selectedOrderVMs.Where(obj => obj.State == "正在打印"))
                {
                    this.UpdateDelivery(preOne);
                }

                //删除所有成功的打印
                if (this.AutoDeleteSucessOrder)
                {
                    var suOrders = selectedOrderVMs.Where(obj => obj.State == "打印成功").ToArray();
                    foreach (var v in suOrders)
                    {
                        this.OrderViewModels.Remove(v);
                    }
                }
                return current.DeliveryNumber;
            }
            catch (TypeInitializationException te)
            {
                MessageBox.Show(te.InnerException.Message);
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.PackageId = 0;
                this.IsStop = false;
                this.printDoc = null;
                this.CanStartPrint = true;
                this.CanStopPrint = false;
                this.IsStop = true;
                this.State = "已完成输出";
            }
        }

        public void Stop()
        {
            if (this.IsStop && this.printDoc != null)
            {
                this.IsStop = false;
            }
        }

        private void PrintDoc_PageGenStarting(OrderPrintDocument arg1, Order arg2)
        {
            //用户取消了打印
            if (this.IsStop)
            {
                throw new Exception("打印已经终止");
            }
            //上传前一个
            foreach (var preOne in this.OrderViewModels.Where(obj => obj.State == "正在生成" && obj.Source.Id != arg2.Id))
            {
                preOne.State = "生成完成";
                preOne.Background = null;
            }
            //打印当前的
            var vms = GetMatchOrderViewModels(arg2);
            foreach (var vm in vms)
            {
                vm.State = "正在生成";
                vm.Background = Brushes.Yellow;
                vm.Source.PrintOperator = OperatorService.LoginOperator.Number;
                vm.Source.PrintTime = DateTime.Now;
                WPFHelper.DoEvents();
            }
            this.State = string.Format("第三步：正在生成{0}/{1}...",
                this.OrderViewModels.Count(obj => obj.State == "生成完成" && obj.IsChecked),
                this.OrderViewModels.Count(obj => obj.IsChecked));
        }

        private void printDoc_PrintStarting(OrderPrintDocument sender, Order order)
        {
            //用户取消了打印
            if (this.IsStop)
            {
                throw new Exception("打印已经终止");
            }
            //上传前一个
            foreach (var preOne in this.OrderViewModels.Where(obj => obj.State == "正在打印" && obj.Source.Id != order.Id))
            {
                this.UpdateDelivery(preOne);
            }
            //打印当前的
            var vms = GetMatchOrderViewModels(order);
            foreach (var vm in vms)
            {
                vm.State = "正在打印";
                vm.Background = Brushes.Yellow;
                vm.Source.PrintOperator = OperatorService.LoginOperator.Number;
                vm.Source.PrintTime = DateTime.Now;
                WPFHelper.DoEvents();
            }
            this.State = string.Format("第四步：正在打印{0}/{1}...", this.OrderViewModels.Count(obj => obj.State == "打印成功"),
                this.OrderViewModels.Count(obj => obj.IsChecked));
        }

        private void UpdateDelivery(PrintOrderViewModel vm)
        {
            PrintHistory ph = null;
            try
            {
                WPFHelper.DoEvents();
                if (string.IsNullOrWhiteSpace(vm.DeliveryCompany) || string.IsNullOrWhiteSpace(vm.DeliveryNumber))
                {
                    throw new Exception("上传打印信息失败：物流公司和编号为空");
                }
                ph = new PrintHistory
                {
                    UploadTime = this.orderService.GetDBMinTime(),
                    DeliveryCompany = vm.DeliveryCompany,
                    DeliveryNumber = vm.DeliveryNumber,
                    DeliveryTemplate = this.PrintTemplate.Name,
                    Operator = "",
                    OrderId = vm.Source.Id,
                    ReceiverAddress = vm.Source.ReceiverAddress,
                    ReceiverMobile = vm.Source.ReceiverMobile,
                    ReceiverName = vm.Source.ReceiverName,
                    ReceiverPhone = vm.Source.ReceiverPhone,
                    CreateTime = DateTime.Now,
                    GoodsInfo = vm.Goods,
                    PopOrderId = vm.Source.PopOrderId,
                    ShopId = vm.Source.ShopId,
                    PaperType = this.PrintTemplate.PaperType,
                    Id = 0,
                    PageNumber = vm.PageNumber,
                };

                //相同订单会被合并，所以主订单会有其它订单数据，这里需要提取自身的
                if (vm.Source.PopOrderId.Contains(","))
                {
                    ph.PopOrderId =
                        vm.Source.PopOrderId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }
                else
                {
                    ph.PopOrderId = vm.Source.PopOrderId;
                }

                this.printHistoryService.Save(ph);
                vm.State = "打印成功";
                WPFHelper.DoEvents();
                vm.Background = null;
            }
            catch (Exception ex)
            {
                vm.State = ex.Message;
                vm.Background = Brushes.Red;
                Logger.Log("上传打印历史出错,订单编号:" + vm.Source.Id, ex);
                if (ex.InnerException != null)
                {
                    Logger.Log("上传打印历史出错,订单编号:" + vm.Source.Id, ex.InnerException);
                }
                if (ph == null)
                {
                    Logger.Log("上传打印历史出错，历史信息为空");
                }
                else
                {
                    string content = Newtonsoft.Json.JsonConvert.SerializeObject(ph);
                    Logger.Log("上传打印历史出错，历史信息:" + content);
                }
            }
        }

        #endregion
    }
}