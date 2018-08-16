using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ShopErp.App.Service.Print.DeliveryFormatters;
using ShopErp.App.Service.Print.OrderFormatters;
using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.App.Service.Print.PrintFormatters;
using ShopErp.App.Service.Print.ShopFormatters;
using ShopErp.Domain;
using System.Windows.Controls;
using ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters;
using ShopErp.App.Service.Print;
using ShopErp.App.Utils;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    class OrderPrintDocument : DocumentPaginator, IDocumentPaginatorSource, IDeliveryPrintDocument
    {
        private PrintTemplate template = null;

        public Order[] Orders { get; set; }

        private List<DocumentPage> pages = new List<DocumentPage>();

        public event Action<object, int> PageGening;
        public event Action<object, int> PageGened;
        public event Action<object, int> PagePrinting;
        public event Action<object, int> PagePrinted;

        public override bool IsPageCountValid
        {
            get { return true; }
        }

        public override int PageCount
        {
            get { return this.pages.Count; }
        }

        public override System.Windows.Size PageSize { get; set; }

        public override IDocumentPaginatorSource Source
        {
            get { return this; }
        }

        public DocumentPaginator DocumentPaginator
        {
            get { return this; }
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            if (pageNumber >= this.pages.Count)
            {
                throw new Exception("系统打印调用页已超出最大页");
            }
            this.OnPagePrinting(pageNumber);
            return this.pages[pageNumber];
        }

        private void OnPageGening(int page)
        {
            if (this.PageGening != null)
                this.PageGening(this, page);
        }

        private void OnPageGened(int page)
        {
            if (this.PageGened != null)
                this.PageGened(this, page);
        }

        private void OnPagePrinting(int page)
        {
            if (this.PagePrinting != null)
                this.PagePrinting(this, page);
        }

        private void OnPagePrinted(int page)
        {
            if (this.PagePrinted != null)
                this.PagePrinted(this, page);
        }

        public void GenPages(Order[] orders, WuliuNumber[] wuliuNumbers, PrintTemplate template)
        {
            if (orders.Any(obj => obj == null))
            {
                throw new Exception("参数错误，有订单为空");
            }

            if (wuliuNumbers.Any(obj => obj == null))
            {
                throw new Exception("参数错误，有物流信息为空");
            }

            if (template == null)
            {
                throw new Exception("参数错误，打印模板为空");
            }

            if (orders.Length != wuliuNumbers.Length)
            {
                throw new Exception("订单与物流信息长度不相等");
            }

            this.Orders = new Order[orders.Length];
            Array.Copy(orders, this.Orders, orders.Length);
            this.template = template;
            this.PageSize = new System.Windows.Size(template.Width, template.Height);
            PrintInfo pi = new PrintInfo { PrintTime = DateTime.Now, PageInfo = "" };
            for (int i = 0; i < orders.Length; i++)
            {
                pi.PageInfo = "第" + (i + 1).ToString() + "/" + orders.Length.ToString() + "页";
                this.OnPageGening(i);
                DocumentPage page = DrawingPage(orders[i], wuliuNumbers[i], pi);
                this.pages.Add(page);
            }
        }

        private DocumentPage DrawingPage(Order order, WuliuNumber wuliuNumber, PrintInfo pi)
        {
            DrawingVisual dv = new DrawingVisual() { };
            var rendor = dv.RenderOpen();
            rendor.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(this.PageSize));
            foreach (var printItem in this.template.Items)
            {
                //该用那个格式化程序
                var group = PrintTemplateItemType.GetGroup(printItem.Type);
                object data = null;
                if (group == PrintTemplateItemTypeGroup.PRINT)
                {
                    data = PrintFormatterManager.Format(this.template, printItem, pi);
                }
                else if (group == PrintTemplateItemTypeGroup.OTHER)
                {
                    data = OtherFormatterManager.Format(this.template, printItem);
                }
                else if (group == PrintTemplateItemTypeGroup.ORDER)
                {
                    data = OrderFormatterManager.Format(this.template, printItem, order);
                }
                else if (group == PrintTemplateItemTypeGroup.DELIVERY)
                {
                    data = DeliveryFormatterManager.Format(this.template, printItem, wuliuNumber);
                }
                else if (group == PrintTemplateItemTypeGroup.SHOP)
                {
                    data = ShopFormatterManager.Format(this.template, printItem, order.ShopId);
                }
                else
                {
                    throw new Exception("商品模板不支类型:" + printItem.Type);
                }

                if (data == null || ((data is string) && string.IsNullOrWhiteSpace(data as string)))
                {
                    continue;
                }
                if (Math.Abs(1 - printItem.Opacity) > 0.005)
                {
                    rendor.PushOpacity(printItem.Opacity);
                }
                if (data is string)
                {
                    string text = data as string;
                    FormattedText fText = new FormattedText(text, Thread.CurrentThread.CurrentUICulture, System.Windows.FlowDirection.LeftToRight, new Typeface(printItem.FontName), printItem.FontSize, System.Windows.Media.Brushes.Black);
                    if (printItem.ScaleFormat == "是" && (fText.Width > printItem.Width || fText.Height > printItem.Height))
                    {
                        if (text.Contains(Environment.NewLine))
                        {
                            //多行文本
                        }
                        else
                        {
                            //单行文本
                        }
                        //计算缩放
                        double scale = printItem.Width / fText.Width;
                        ScaleTransform st = new ScaleTransform(scale, scale);
                        rendor.PushTransform(st);
                        rendor.DrawText(fText, new System.Windows.Point((printItem.X + template.XOffset) / scale, (printItem.Y + template.YOffset) / scale + fText.Height * scale / 2));
                        rendor.Pop();
                        continue;
                    }
                    else
                    {
                        fText.TextAlignment = printItem.TextAlignment;
                        //设置最大宽度和高度后，会在区域内打印，否则按行绘制，超出指定区域
                        fText.MaxTextHeight = printItem.Height;
                        fText.MaxTextWidth = printItem.Width;
                        rendor.DrawText(fText, new System.Windows.Point(printItem.X + template.XOffset, printItem.Y + template.YOffset));
                    }

                }
                else if (data is Bitmap)
                {
                    var image = data as Bitmap;
                    var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)printItem.Width, (int)printItem.Height));
                    rendor.DrawImage(bs, new Rect(printItem.X + template.XOffset, printItem.Y + template.YOffset, printItem.Width, printItem.Height));
                }
                else if (data is BitmapSource)
                {
                    var bs = data as BitmapSource;
                    rendor.DrawImage(bs, new Rect(printItem.X + template.XOffset, printItem.Y + template.YOffset, printItem.Width, printItem.Height));
                }
                else if (data is System.Windows.Media.Pen)
                {
                    var pen = data as System.Windows.Media.Pen;
                    if (printItem.Height > printItem.Width)
                    {
                        //竖线
                        var p1 = new System.Windows.Point(printItem.X + printItem.Width / 2, printItem.Y);
                        var p2 = new System.Windows.Point(printItem.X + printItem.Width / 2, printItem.Y + printItem.Height);
                        rendor.DrawLine(pen, p1, p2);
                    }
                    else
                    {
                        //竖线
                        var p1 = new System.Windows.Point(printItem.X, printItem.Y + printItem.Height / 2);
                        var p2 = new System.Windows.Point(printItem.X + printItem.Width, printItem.Y + printItem.Height / 2);
                        rendor.DrawLine(pen, p1, p2);
                    }
                }
                else
                {
                    throw new Exception("无法识别的输出类型:" + data.GetType().FullName);
                }
                if (Math.Abs(1 - printItem.Opacity) > 0.005)
                {
                    rendor.Pop();
                }
            }
            rendor.Close();
            return new DocumentPage(dv, this.PageSize, new Rect(this.PageSize), new Rect(this.PageSize));
        }

        public void Print(string printer)
        {
            PrintUtil.GetPrinter(printer).PrintDocument(this, "快递打印");
        }
    }
}