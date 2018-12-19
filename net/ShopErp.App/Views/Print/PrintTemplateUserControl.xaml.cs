using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Win32;
using System.Drawing.Text;
using ShopErp.App.Domain;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// Interaction logic for DeliveryTemplateConfig.xaml
    /// </summary>
    public partial class PrintTemplateUserControl : UserControl
    {
        private FilePrintTemplateRepertory deliveryTemplateRepertory = new FilePrintTemplateRepertory();
        private ObservableCollection<Service.Print.PrintTemplate> deliveryTemplates = new ObservableCollection<Service.Print.PrintTemplate>();
        private bool loaded = false;

        System.Windows.Controls.PrintDialog pd = new System.Windows.Controls.PrintDialog();

        public PrintTemplateUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                return;
            }
            this.cbbTemplateTypes.ItemsSource = new string[] { "快递", "退货", "商品" };
            this.cbbDeliverCompanies.ItemsSource = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToArray();
            this.lstDeliveryPrintTemplateTypes.ItemsSource = ViewModels.PrintTemplateItemTypeViewModel.GetAllTypes();
            this.cbbDeliveryPrintTemplateItemFontName.ItemsSource = new InstalledFontCollection().Families.Select(obj => obj.Name).ToArray();
            this.cbbDeliveryPrintTemplateItemFontName.SelectedIndex = 0;
            this.cbbDeliveryPrintTemplateItemTextAlignment.ItemsSource = Enum.GetValues(typeof(TextAlignment));
            this.cbbDeliveryPrintTemplateItemScaleFormat.ItemsSource = new string[] { "否", "是" };
            this.lstDeliveryPrintTemplates.ItemsSource = deliveryTemplates;
            this.btnRefreshAllDeliveryPrintTemplate_Click(null, null);
            loaded = true;
        }

        private void AttachThumbEvents(PrintTemplateItemViewModelCommon thumb)
        {
            thumb.PreviewMouseLeftButtonDown += thumb_MouseEnter;
        }

        private void DeattachThumbEvents(PrintTemplateItemViewModelCommon thumb)
        {
            thumb.PreviewMouseLeftButtonDown -= thumb_MouseEnter;
        }

        void thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            this.SetDeliveryPrintTemplateItemProperties(sender as PrintTemplateItemViewModelCommon);
            (sender as PrintTemplateItemViewModelCommon).UI.Focus();
        }

        private void SetDeliveryPrintTemplateItemProperties(PrintTemplateItemViewModelCommon itemBrige)
        {
            this.dpExtendUI.Children.Clear();
            foreach (FrameworkElement item in this.dgDelivertyTemplateItemProperties.Children)
            {
                item.DataContext = itemBrige;
            }
            if (itemBrige != null)
            {
                this.cbbDeliveryPrintTemplateItemFontName.SelectedItem = itemBrige.FontName;
                this.tbDeliveryPrintTemplateItemFontSize.Text = itemBrige.FontSize.ToString();
                if (itemBrige.PropertyUI != null)
                    this.dpExtendUI.Children.Add(itemBrige.PropertyUI);
            }
        }

        private void tbNewDeliveryPrintTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = this.tbNewDeliverPrintTemplateName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("模板名称不能为空");
                    return;
                }

                if (this.deliveryTemplates.Any(obj => obj.Name == name))
                {
                    MessageBox.Show("已存在相同的模板名称");
                    return;
                }

                Service.Print.PrintTemplate deliveryTemplate = new Service.Print.PrintTemplate { Name = name };
                FilePrintTemplateRepertory.InsertN(deliveryTemplate);
                this.deliveryTemplates.Add(deliveryTemplate);
                this.lstDeliveryPrintTemplates.SelectedItem = deliveryTemplate;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "添加失败");
            }
        }

        private void btnDeleteDeliveryPrintTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;

                if (deliveryTemplate == null)
                {
                    MessageBox.Show("请先选择一个模板");
                    return;
                }

                if (MessageBox.Show("是否要删除：" + deliveryTemplate.Name + "　？", "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                FilePrintTemplateRepertory.DeleteN(deliveryTemplate.Name);
                this.deliveryTemplates.Remove(deliveryTemplate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "删除失败");
            }
        }

        private void btnRefreshAllDeliveryPrintTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = FilePrintTemplateRepertory.GetAllN().OrderBy(obj => obj.Type).ToArray();
                this.deliveryTemplates.Clear();
                foreach (var item in items)
                {
                    this.deliveryTemplates.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "获取所有模板失败");
            }
        }

        private void btnSaveDeliveryPrintTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                if (deliveryTemplate == null)
                {
                    MessageBox.Show("请选择相应模板");
                    return;
                }

                string delivertyTemplateName = this.tbDeliveryTemplateName.Text.Trim();
                deliveryTemplate.DeliveryCompany = this.cbbDeliverCompanies.SelectedItem.ToString();
                deliveryTemplate.Width = this.imgDelivery.Width;
                deliveryTemplate.Height = this.imgDelivery.Height;
                if (deliveryTemplate.DeliveryCompany == null)
                {
                    throw new Exception("必须选择快递公司");
                }
                if (deliveryTemplate.Width <= 0 || deliveryTemplate.Height <= 0)
                {
                    throw new Exception("长与宽必须大于0");
                }
                FilePrintTemplateRepertory.UpdateN(deliveryTemplate, this.tbDeliveryTemplateName.Text.Trim());
                this.lstDeliveryPrintTemplates.ItemsSource = null;
                this.lstDeliveryPrintTemplates.ItemsSource = this.deliveryTemplates;
                this.lstDeliveryPrintTemplates.SelectedItem = deliveryTemplate;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void btnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            if (ofd.ShowDialog().Value == false)
            {
                return;
            }
            try
            {
                var item = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                if (item == null)
                {
                    return;
                }
                item.BackgroundImage = System.IO.File.ReadAllBytes(ofd.FileName);
                this.SetDeliverPrintTemplateBackgroundImage(item.BackgroundImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置背景图片
        /// </summary>
        /// <param name="imageBytes">图片数据</param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        private void SetDeliverPrintTemplateBackgroundImage(byte[] imageBytes, double targetWidth = 0,
            double targetHeight = 0)
        {
            //如果没有图片则设置为空
            if (imageBytes == null || imageBytes.Length == 0)
            {
                this.imgDelivery.Source = null;
                this.cDeliveryHost.LayoutTransform = null;
                this.imgDelivery.Height = targetHeight;
                this.imgDelivery.Width = targetWidth;
                return;
            }
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(imageBytes, false);
            image.EndInit();
            this.imgDelivery.Source = image;
            this.imgDelivery.Height = targetHeight > 0 ? targetHeight : image.Height;
            this.imgDelivery.Width = targetWidth > 0 ? targetWidth : image.Width;

            //计算缩放
            int physicalWidth = 0, physicalHeight = 0, pixelWidth = 0, pixelHeight = 0;
            if (Win32.Monitor.GetMonitorInfo(this, ref physicalWidth, ref physicalHeight, ref pixelWidth, ref pixelHeight) == false)
            {
                MessageBox.Show("无法获取显示器信息,你看到的图片可能与实际大小不一致,但不影响打印");
                this.cDeliveryHost.LayoutTransform = null;
                return;
            }
            PresentationSource source = PresentationSource.FromVisual(this);
            double realDpiX = pixelWidth / (physicalWidth / 2.54);
            double scaleX = realDpiX / (source.CompositionTarget.TransformToDevice.M11 * 96);

            double realDpiY = pixelHeight / (physicalHeight / 2.54);
            double scaleY = realDpiY / (source.CompositionTarget.TransformToDevice.M22 * 96);

            this.cDeliveryHost.LayoutTransform = new ScaleTransform(scaleX, scaleY);
        }

        /// <summary>
        /// 选择模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstDeliveryTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //清空界面
                this.SetDeliveryPrintTemplateItemProperties(null);
                this.imgDelivery.Source = null;
                foreach (var thumb in this.cDeliveryHost.Children.OfType<Thumb>().ToArray())
                {
                    this.cDeliveryHost.Children.Remove(thumb);
                }

                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                this.dgDeliverPrintTemplateHolder.IsEnabled = deliveryTemplate != null;
                if (deliveryTemplate == null)
                {
                    return;
                }

                this.SetDeliverPrintTemplateBackgroundImage(deliveryTemplate.BackgroundImage, deliveryTemplate.Width,
                    deliveryTemplate.Height);

                //生成视图对象
                PrintTemplateItemViewModelCommon[] briges = deliveryTemplate.Items.Select(obj => PrintTemplateItemViewModelFactory.Create(deliveryTemplate, obj.Type, obj.Type)).ToArray();
                for (int i = 0; i < briges.Length; i++)
                {
                    briges[i].ApplayStyleAndData(this.FindResource("ThumbStyle") as Style, deliveryTemplate.Items[i]);
                    briges[i].Data.RunTimeTag = briges[i];
                    AttachThumbEvents(briges[i]);
                }
                foreach (var thumb in briges)
                {
                    this.cDeliveryHost.Children.Add(thumb.UI);
                }

                var layer = AdornerLayer.GetAdornerLayer(this.cDeliveryHost);
                foreach (UIElement ui in cDeliveryHost.Children.OfType<Thumb>())
                    layer.Add(new PrintTemplateItemAdorner(ui));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "错误");
            }
        }

        /// <summary>
        /// 点击增加模板中的打印项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeliveryPrintTemplateItemType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement cb = sender as FrameworkElement;
                if (cb == null)
                {
                    return;
                }
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                PrintTemplateItemTypeViewModel vmItem = cb.DataContext as PrintTemplateItemTypeViewModel;
                PrintTemplateItemViewModelCommon itemViewModel = PrintTemplateItemViewModelFactory.Create(deliveryTemplate, vmItem.Type, vmItem.Type);
                Service.Print.PrintTemplateItem item = new Service.Print.PrintTemplateItem();
                item.RunTimeTag = itemViewModel;
                item.Id = Guid.NewGuid();
                item.Type = vmItem.Type;
                item.X = (int)this.imgDelivery.Width / 2;
                item.Y = (int)this.imgDelivery.Height / 2;
                item.Width = 100;
                item.Height = 30;
                item.FontSize = 12;
                item.FontName = "黑体";
                item.Format = itemViewModel.Format;
                item.Value = itemViewModel.Value;
                item.Value1 = itemViewModel.Value1;
                itemViewModel.ApplayStyleAndData(this.FindResource("ThumbStyle") as Style, item);
                AttachThumbEvents(itemViewModel);
                //添加到界面
                this.cDeliveryHost.Children.Add(itemViewModel.UI);
                var layer = AdornerLayer.GetAdornerLayer(this.cDeliveryHost);
                layer.Add(new PrintTemplateItemAdorner(itemViewModel.UI));

                deliveryTemplate.Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 删除模板中的打印项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveDeliveryPrintTemplateItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement cb = sender as FrameworkElement;
                if (cb == null)
                {
                    return;
                }
                var brige = cb.DataContext as PrintTemplateItemViewModelCommon;
                DeattachThumbEvents(brige);
                this.cDeliveryHost.Children.Remove(brige.UI);
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                deliveryTemplate.Items.Remove(brige.Data);
                if (deliveryTemplate.AttachFiles.ContainsKey(brige.Data.Id.ToString()))
                {
                    deliveryTemplate.AttachFiles.Remove(brige.Data.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 打印测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrintTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printTemplate = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                if (printTemplate == null)
                {
                    MessageBox.Show("请选择一个模板");
                    return;
                }

                int count = 0;
                if (int.TryParse(this.tbTestCount.Text.Trim(), out count) == false)
                {
                    MessageBox.Show("请输入要打印测试的数量");
                    return;
                }

              
                if (printTemplate.Type == Service.Print.PrintTemplate.TYPE_DELIVER)
                {
                    Order[] orders = new Order[count];
                    WuliuNumber[] wns = new WuliuNumber[count];
                    for (int i = 0; i < count; i++)
                    {
                        orders[i] = this.CreateTestOrder();
                        wns[i] = new WuliuNumber
                        {
                            SortationName = "齐齐哈尔",
                            DeliveryCompany = printTemplate.DeliveryCompany,
                            DeliveryNumber = "80600156789001",
                            ConsolidationCode = "20789",
                            RouteCode = "021D-456-789-540",
                            SortationNameAndRouteCode = "021D-456-789",
                        };
                    }
                    DeliveryPrintDocument doc = new GDIDeliveryPrintDocument() { WuliuNumbers = wns };
                    doc.StartPrint(orders, "", true, printTemplate);
                    return;
                }
                var ret = pd.ShowDialog();
                if (ret.Value == false)
                {
                    return;
                }
                pd.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(printTemplate.Width, printTemplate.Height);
                if (printTemplate.Type == Service.Print.PrintTemplate.TYPE_GOODS)
                {
                    OrderGoods[] orderGoodss = new OrderGoods[count];
                    for (int i = 0; i < count; i++)
                    {
                        orderGoodss[i] = CreateTestOrderGoods();
                    }
                    GoodsPrintDocument doc = new GoodsPrintDocument();
                    doc.GenPages(orderGoodss, printTemplate);
                    pd.PrintDocument(doc, "打印测试");
                }
                else if (printTemplate.Type == Service.Print.PrintTemplate.TYPE_RETURN)
                {
                    OrderReturn[] or = new OrderReturn[count];
                    for (int i = 0; i < count; i++)
                    {
                        or[i] = CreateTestOrderReturn();
                    }
                    OrderReturnPrintDocument od = new OrderReturnPrintDocument();
                    od.GenPages(or, printTemplate);
                    pd.PrintDocument(od, "打印测试");
                }
                MessageBox.Show("打印完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 创建测试订单
        /// </summary>
        /// <returns></returns>
        private Order CreateTestOrder()
        {
            Order order = new Order
            {
                PopBuyerComment = "请仔细检查质量,质量不过关不要",
                CreateTime = DateTime.Now,
                DeliveryCompany = "圆通速递",
                DeliveryNumber = "D00099991111",
                DeliveryOperator = OperatorService.LoginOperator.Number,
                DeliveryTime = DateTime.Now,
                Id = 201590890,
                PopDeliveryTime = DateTime.Now,
                OrderGoodss = null,
                PopPayTime = DateTime.Now,
                PopBuyerId = "821234536",
                PopOrderId = "543543543-20140908-1234568908",
                PopOrderTotalMoney = 123,
                PopPayType = PopPayType.COD,
                ShopId = 1,
                PopType = PopType.TAOBAO,
                PrintOperator = OperatorService.LoginOperator.Number,
                PrintTime = DateTime.Now,
                ReceiverAddress = "四川省 成都市 金牛区 成都大道34号金家花园 这是一个长地址 成都大道34号金家花园 这是一个长地址",
                ReceiverMobile = "15882415366",
                ReceiverName = "张三",
                ReceiverPhone = "028-88452365",
                PopSellerComment = "发顺丰",
                State = OrderState.PAYED,
                Weight = 5,
                CloseOperator = "",
                CloseTime = DateTime.Now,
                CreateOperator = "",
                CreateType = OrderCreateType.DOWNLOAD,
                DeliveryMoney = 0,
                ParseResult = true,
                PopCodNumber = "LC02314596987787",
                Type = OrderType.NORMAL,
            };
            var shop = ServiceContainer.GetService<ShopService>().GetByAll().Datas.FirstOrDefault(obj => obj.Enabled);
            if (shop != null)
            {
                order.ShopId = shop.Id;
            }
            order.OrderGoodss = new List<OrderGoods>();

            for (int i = 0; i < 2; i++)
            {
                order.OrderGoodss.Add(CreateTestOrderGoods());
            }
            return order;
        }

        private OrderGoods CreateTestOrderGoods()
        {
            //生成商品
            OrderGoods og = new OrderGoods
            {
                Color = "白色",
                Count = 2,
                Edtion = "升级版",
                NumberId = 1076,
                Id = 2076969,
                Image = "",
                Number = "D34",
                OrderId = 20160121456,
                PopInfo = "",
                PopPrice = 34,
                PopUrl = "http://www.baidu.com",
                Price = 45,
                Size = "37",
                State = OrderState.PAYED,
                Vendor = "彩蝴蝶",
                Weight = 0.6F,
            };
            return og;
        }

        private OrderReturn CreateTestOrderReturn()
        {
            var or = new OrderReturn
            {
                Comment = "注意",
                Count = 2,
                CreateOperator = "",
                CreateTime = DateTime.Now,
                DeliveryCompany = "圆通速递",
                DeliveryNumber = "1111111111",
                GoodsInfo = "彩蝴蝶,D34 黑色 38",
                GoodsMoney = 35,
                Id = 500121,
                NewOrderId = 23232232,
                OrderGoodsId = 0,
                OrderId = 0,
                ProcessOperator = "",
                ProcessTime = DateTime.Now,
                Reason = OrderReturnReason.DAY7,
                State = OrderReturnState.WAITPROCESS,
                Type = OrderReturnType.RETURN,
            };
            return or;
        }


        private void btnCopyDeliveryPrintTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var template = this.lstDeliveryPrintTemplates.SelectedItem as Service.Print.PrintTemplate;
                if (template == null)
                {
                    throw new Exception("请选择模板");
                }

                template.Name = template.Name + DateTime.Now.ToString("MM_dd_HH");
                FilePrintTemplateRepertory.InsertN(template);
                MessageBox.Show("已复制成功，请修改复制成功的名称");
                this.btnRefreshAllDeliveryPrintTemplate_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception("未实现");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}