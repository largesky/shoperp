using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShopErp.App.Service.Print.GoodsFormatters;
using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.App.Service.Print.PrintFormatters;
using ShopErp.Domain;

namespace ShopErp.App.Domain
{
    class GoodsPrintDocument : DocumentPaginator, IDocumentPaginatorSource
    {
        private OrderGoods[] orderGoodss = null;

        private int count = 0;

        private PrintTemplate template = null;

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

        /// <summary>
        /// 获取或者设置 打印的订单，如果一个订单被检查不需要打印，则不包含该集合中
        /// </summary>
        public List<Order> Orders { get; private set; }

        public event Func<OrderPrintDocument, Order, Exception, bool> PrintError;

        private List<DocumentPage> pages = new List<DocumentPage>();

        public override DocumentPage GetPage(int pageNumber)
        {
            if (pageNumber >= this.pages.Count)
            {
                throw new Exception("系统打印调用页已超出最大页");
            }

            return this.pages[pageNumber];
        }

        public void GenPages(OrderGoods[] orderGoods, PrintTemplate template)
        {
            this.template = template;
            this.orderGoodss = orderGoods;
            this.PageSize = new System.Windows.Size(template.Width, template.Height);
            PrintInfo pi = new PrintInfo { };
            //生成页
            for (int i = 0; i < this.orderGoodss.Length; i++)
            {
                try
                {
                    pi.PrintTime = DateTime.Now;
                    pi.PageInfo = "第" + (i + 1).ToString() + "/" + count + "页";
                    DocumentPage page = DrawingPage(this.orderGoodss[i], pi);
                    this.pages.Add(page);
                }
                catch (Exception ex)
                {
                    if (this.PrintError == null)
                    {
                        throw ex;
                    }
                }
            }
        }

        private DocumentPage DrawingPage(OrderGoods og, PrintInfo pi)
        {
            DrawingVisual dv = new DrawingVisual() { };
            var rendor = dv.RenderOpen();
            rendor.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(this.PageSize));
            foreach (var printItem in this.template.Items)
            {
                //该用那个格式货程序
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
                else if (group == PrintTemplateItemTypeGroup.GOODS)
                {
                    data = GoodsFormatterManager.Format(this.template, printItem, og);
                }
                else
                {
                    throw new Exception("商品模板不支类型:" + printItem.Type);
                }

                if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
                {
                    continue;
                }

                if (data is string)
                {
                    string text = data as string;
                    FormattedText fText = new FormattedText(text, Thread.CurrentThread.CurrentUICulture,
                        System.Windows.FlowDirection.LeftToRight, new Typeface(printItem.FontName), printItem.FontSize,
                        System.Windows.Media.Brushes.Black);
                    fText.SetFontFamily(printItem.FontName);
                    fText.MaxLineCount = 99;
                    fText.MaxTextHeight = printItem.Height;
                    fText.MaxTextWidth = printItem.Width;
                    fText.SetFontWeight(FontWeight.FromOpenTypeWeight(10));
                    rendor.DrawText(fText,
                        new System.Windows.Point(printItem.X + template.XOffset, printItem.Y + template.YOffset));
                }
                else if (data is Bitmap)
                {
                    var image = data as Bitmap;
                    var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(),
                        IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight((int) printItem.Width, (int) printItem.Height));
                    rendor.DrawImage(bs,
                        new Rect(printItem.X + template.XOffset, printItem.Y + template.YOffset, printItem.Width,
                            printItem.Height));
                }
                else if (data is BitmapSource)
                {
                    var bs = data as BitmapSource;
                    rendor.DrawImage(bs,
                        new Rect(printItem.X + template.XOffset, printItem.Y + template.YOffset, printItem.Width,
                            printItem.Height));
                }
                else if (data is System.Windows.Media.Brush)
                {
                    var brush = data as System.Windows.Media.Brush;
                    var rect = new Rect(printItem.X + template.XOffset, printItem.Y + template.YOffset, printItem.Width,
                        printItem.Height);
                    rendor.DrawRectangle(brush, new System.Windows.Media.Pen(brush, 0), rect);
                }
                else
                {
                    throw new Exception("无法识别的输出类型:" + data.GetType().FullName);
                }
            }
            rendor.Close();
            return new DocumentPage(dv, this.PageSize, new Rect(this.PageSize), new Rect(this.PageSize));
        }
    }
}