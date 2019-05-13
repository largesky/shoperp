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
using ShopErp.App.Service.Print.PrintDocument;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// Interaction logic for DeliveryTemplateConfig.xaml
    /// </summary>
    public partial class PrintTemplateUserControl : UserControl
    {
        private ObservableCollection<PrintTemplate> deliveryTemplates = new ObservableCollection<PrintTemplate>();
        private bool loaded = false;

        System.Windows.Controls.PrintDialog pd = new System.Windows.Controls.PrintDialog();

        public PrintTemplateUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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

                PrintTemplate deliveryTemplate = new PrintTemplate { Name = name };
                PrintTemplateService.InsertLocal(deliveryTemplate);
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
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;

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

                PrintTemplateService.DeleteLocal(deliveryTemplate.Name);
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
                var items = PrintTemplateService.GetAllLocal().OrderBy(obj => obj.Type).ToArray();
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
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;
                if (deliveryTemplate == null)
                {
                    MessageBox.Show("请选择相应模板");
                    return;
                }

                string delivertyTemplateName = this.tbDeliveryTemplateName.Text.Trim();
                deliveryTemplate.SourceType = PrintTemplateSourceType.SELF;
                if (deliveryTemplate.DeliveryCompany == null)
                {
                    throw new Exception("必须选择快递公司");
                }
                if (deliveryTemplate.Width <= 0 || deliveryTemplate.Height <= 0)
                {
                    throw new Exception("长与宽必须大于0");
                }
                PrintTemplateService.UpdateLocal(deliveryTemplate, this.tbDeliveryTemplateName.Text.Trim());
                this.lstDeliveryPrintTemplates.ItemsSource = null;
                this.lstDeliveryPrintTemplates.ItemsSource = this.deliveryTemplates;
                this.lstDeliveryPrintTemplates.SelectedItem = deliveryTemplate;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
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

                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;
                this.dgDeliverPrintTemplateHolder.IsEnabled = deliveryTemplate != null;
                if (deliveryTemplate == null)
                {
                    return;
                }

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
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;
                PrintTemplateItemTypeViewModel vmItem = cb.DataContext as PrintTemplateItemTypeViewModel;
                PrintTemplateItemViewModelCommon itemViewModel = PrintTemplateItemViewModelFactory.Create(deliveryTemplate, vmItem.Type, vmItem.Type);
                PrintTemplateItem item = new PrintTemplateItem();
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
                var deliveryTemplate = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;
                deliveryTemplate.Items.Remove(brige.Data);
                if (deliveryTemplate.AttachFiles.Any(obj => obj.Name == brige.Data.Id.ToString()))
                {
                    var af = deliveryTemplate.AttachFiles.FirstOrDefault(obj => obj.Name == brige.Data.Id.ToString());
                    deliveryTemplate.AttachFiles.Remove(af);
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
                var printTemplate = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;
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


                if (printTemplate.Type == PrintTemplate.TYPE_DELIVER)
                {
                    throw new Exception("电子面单已不支持测试打印");
                }
                var ret = pd.ShowDialog();
                if (ret.Value == false)
                {
                    return;
                }
                pd.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(printTemplate.Width, printTemplate.Height);
                if (printTemplate.Type == PrintTemplate.TYPE_GOODS)
                {
                    OrderGoods[] orderGoodss = new OrderGoods[count];
                    for (int i = 0; i < count; i++)
                    {
                        orderGoodss[i] = CreateTestOrderGoods();
                    }
                    GoodsPrintDocument doc = new GoodsPrintDocument();
                    doc.StartPrint(orderGoodss, "", true, printTemplate);
                }
                else if (printTemplate.Type == PrintTemplate.TYPE_RETURN)
                {
                    OrderReturn[] or = new OrderReturn[count];
                    for (int i = 0; i < count; i++)
                    {
                        or[i] = CreateTestOrderReturn();
                    }
                    OrderReturnPrintDocument od = new OrderReturnPrintDocument();
                    od.StartPrint(or, "", true, printTemplate);
                }
                MessageBox.Show("打印完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                var template = this.lstDeliveryPrintTemplates.SelectedItem as PrintTemplate;
                if (template == null)
                {
                    throw new Exception("请选择模板");
                }

                template.Name = template.Name + DateTime.Now.ToString("MM_dd_HH");
                PrintTemplateService.InsertLocal(template);
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