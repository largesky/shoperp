using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument
{
    public abstract class GdiPrintDocumentBase<T>
    {
        protected int index;

        public PrintTemplate Template { get; private set; }

        public T[] Values { get; private set; }

        /// <summary>
        /// 整个文档开始打印
        /// </summary>
        public event EventHandler PrintStarting;

        /// <summary>
        /// 开始打印一页
        /// </summary>
        public event Func<object, int, bool> PagePrintStarting;

        /// <summary>
        /// 结束打印一页
        /// </summary>
        public event Func<object, int, bool> PagePrintEnded;

        /// <summary>
        /// 整个文档结束
        /// </summary>
        public event EventHandler PrintEnded;

        protected abstract object FormatData(PrintTemplateItem printTemplateItem, PrintInfo pi);

        protected virtual void BenginPrint() { }

        protected virtual void EndPrint() { }

        protected void OnPrintStarting()
        {
            if (this.PrintStarting != null)
            {
                this.PrintStarting(this, new EventArgs());
            }
        }

        protected void OnPrintEnded()
        {
            if (this.PrintEnded != null)
            {
                this.PrintEnded(this, new EventArgs());
            }
        }

        protected bool OnPagePrintStarting(int i)
        {
            if (PagePrintStarting != null)
            {
                return this.PagePrintStarting(this, i);
            }
            return false;
        }

        protected bool OnPagePagePrintEnded(int i)
        {
            if (PagePrintEnded != null)
            {
                return this.PagePrintEnded(this, i);
            }
            return false;
        }

        protected void PrintValue(PrintPageEventArgs e)
        {
            var rendor = e.Graphics;
            PrintInfo pi = new PrintInfo { PageInfo = string.Format("第{0}/{1}页", this.index + 1, this.Values.Length), PrintTime = DateTime.Now };
            foreach (var printItem in this.Template.Items)
            {
                object data = FormatData(printItem, pi);
                if (data == null || ((data is string) && string.IsNullOrWhiteSpace(data as string)))
                {
                    continue;
                }
                if (data is string)
                {
                    int al = 255;
                    if (Math.Abs(1 - printItem.Opacity) > 0.005)
                    {
                        al = (int)(255 * printItem.Opacity);
                    }
                    string text = data as string;
                    System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(al, System.Drawing.Color.Black));
                    var font = new System.Drawing.Font(printItem.FontName, (float)(printItem.FontSize * 72.0F / 96F));
                    var rect = new System.Drawing.RectangleF(MapToPrinterPix(printItem.X + Template.XOffset), MapToPrinterPix(printItem.Y + Template.YOffset), MapToPrinterPix(printItem.Width), MapToPrinterPix(printItem.Height));
                    var stringFormat = new System.Drawing.StringFormat();
                    if (printItem.TextAlignment == System.Windows.TextAlignment.Center)
                    {
                        stringFormat.Alignment = System.Drawing.StringAlignment.Center;
                        stringFormat.LineAlignment = System.Drawing.StringAlignment.Center;
                    }
                    else if (printItem.TextAlignment == System.Windows.TextAlignment.Justify)
                    {
                        stringFormat.Alignment = System.Drawing.StringAlignment.Near;
                        stringFormat.LineAlignment = System.Drawing.StringAlignment.Near;
                    }
                    else if (printItem.TextAlignment == System.Windows.TextAlignment.Left)
                    {
                        stringFormat.Alignment = System.Drawing.StringAlignment.Near;
                        stringFormat.LineAlignment = System.Drawing.StringAlignment.Near;
                    }
                    else
                    {
                        stringFormat.Alignment = System.Drawing.StringAlignment.Far;
                        stringFormat.LineAlignment = System.Drawing.StringAlignment.Near;
                    }
                    rendor.DrawString(text, font, solidBrush, rect, stringFormat);
                }
                else if (data is System.Drawing.Image)
                {
                    var image = data as System.Drawing.Image;
                    rendor.DrawImage(image, new System.Drawing.PointF(MapToPrinterPix(printItem.X + Template.XOffset), MapToPrinterPix(printItem.Y + Template.YOffset)));
                }
                else if (data is System.Drawing.Pen)
                {
                    var pen = data as System.Drawing.Pen;
                    if (printItem.Height > printItem.Width)
                    {
                        //竖线
                        var p1 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + printItem.Width / 2 + Template.XOffset), MapToPrinterPix(printItem.Y + Template.YOffset));
                        var p2 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + printItem.Width / 2 + Template.XOffset), MapToPrinterPix(printItem.Y + printItem.Height + Template.YOffset));
                        rendor.DrawLine(pen, p1, p2);
                    }
                    else
                    {
                        //横线
                        var p1 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + Template.XOffset), MapToPrinterPix(printItem.Y + printItem.Height / 2 + Template.YOffset));
                        var p2 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + printItem.Width + Template.XOffset), MapToPrinterPix(printItem.Y + printItem.Height / 2 + Template.YOffset));
                        rendor.DrawLine(pen, p1, p2);
                    }
                }
                else
                {
                    throw new Exception("无法识别的输出类型:" + data.GetType().FullName);
                }
            }
        }

        private void Document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            try
            {
                if (this.OnPagePrintStarting(index))
                {
                    e.Cancel = true;
                    return;
                }
                this.PrintValue(e);
                if (this.OnPagePagePrintEnded(index))
                {
                    e.Cancel = true;
                    return;
                }
                this.index++;
                e.HasMorePages = this.index < this.Values.Length;
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                Log.Logger.Log(this.GetType().FullName + ": Document_PrintPage", ex);
                App.Current.Dispatcher.BeginInvoke(new Action(() => System.Windows.MessageBox.Show("打印错误：" + ex.Message)));
            }
        }

        public void StartPrint(T[] values, string printer, bool showPrinterDialog, PrintTemplate template)
        {
            if (template == null)
            {
                throw new Exception("打印失败参数：template 为空");
            }

            if (values == null)
            {
                throw new Exception("打印失败参数：vlaues 为空");
            }

            if (values.Length == 0)
            {
                throw new Exception("打印失败参数：vlaues 没有数据");
            }

            if (values.Any(obj => obj == null))
            {
                throw new Exception("打印失败参数：vlaues 中含有空对象");
            }
            System.Drawing.Printing.PrintDocument document = new System.Drawing.Printing.PrintDocument();
            if (showPrinterDialog == false)
            {
                if (string.IsNullOrWhiteSpace(printer))
                {
                    throw new Exception("打印机名称为空，请设置打印机");
                }
                if (System.Drawing.Printing.PrinterSettings.InstalledPrinters.OfType<string>().Contains(printer) == false)
                {
                    throw new Exception("打印机不存在此电脑上:" + printer);
                }
                document.PrinterSettings.PrinterName = printer;
            }
            else
            {
                var pd = new System.Windows.Forms.PrintDialog() { UseEXDialog = true };
                var ret = pd.ShowDialog();
                if (ret != System.Windows.Forms.DialogResult.OK && ret != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
                document.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
            }
            document.PrintController = new System.Drawing.Printing.StandardPrintController();
            document.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("shoperp_print_size", (int)MapToPrinterPix(template.Width), (int)MapToPrinterPix(template.Height));
            document.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            document.DocumentName = template.Name + DateTime.Now;
            document.PrintPage += Document_PrintPage;
            document.BeginPrint += Document_BeginPrint;
            document.EndPrint += Document_EndPrint;
            this.index = 0;
            this.Values = values;
            this.Template = template;
            document.Print();
        }

        private void Document_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            this.OnPrintEnded();
        }

        private void Document_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            this.OnPrintStarting();
        }

        protected static float MapToPrinterPix(double value)
        {
            return (float)(100.0 * value / 96.0);
        }
    }
}
