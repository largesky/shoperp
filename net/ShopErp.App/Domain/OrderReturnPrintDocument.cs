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
using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.App.Service.Print.PrintFormatters;
using ShopErp.App.Service.Print.ReturnFormatters;
using ShopErp.Domain;

namespace ShopErp.App.Domain
{
    class OrderReturnPrintDocument : DocumentPaginator, IDocumentPaginatorSource
    {
        private PrintTemplate template = null;

        private OrderReturn[] orderReturns = null;

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

        private List<DocumentPage> pages = new List<DocumentPage>();

        public override DocumentPage GetPage(int pageNumber)
        {
            if (pageNumber >= this.pages.Count)
            {
                throw new Exception("系统打印调用页已超出最大页");
            }
            return this.pages[pageNumber];
        }

        public void GenPages(OrderReturn[] orderReturns, PrintTemplate template)
        {
            this.template = template;
            this.orderReturns = orderReturns;
            this.PageSize = new System.Windows.Size(template.Width, template.Height);
            PrintInfo pi = new PrintInfo { };
            //生成页
            for (int i = 0; i < this.orderReturns.Length; i++)
            {
                try
                {
                    pi.PrintTime = DateTime.Now;
                    pi.PageInfo = "第" + (i + 1).ToString() + "/" + this.orderReturns.Length + "页";
                    DocumentPage page = DrawingPage(this.orderReturns[i], pi);
                    this.pages.Add(page);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private DocumentPage DrawingPage(OrderReturn or, PrintInfo pi)
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
                else if (group == PrintTemplateItemTypeGroup.RETURN)
                {
                    data = ReturnFormatterManager.Format(this.template, printItem,
                        or);
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