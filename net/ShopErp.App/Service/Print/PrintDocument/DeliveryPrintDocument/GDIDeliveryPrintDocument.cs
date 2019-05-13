using ShopErp.App.Service.Print.DeliveryFormatters;
using ShopErp.App.Service.Print.OrderFormatters;
using ShopErp.App.Service.Print.OtherFormatters;
using ShopErp.App.Service.Print.PrintFormatters.PrintInfoFormatters;
using ShopErp.App.Service.Print.ShopFormatters;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public class GDIDeliveryPrintDocument : DeliveryPrintDocument
    {
        protected int index;

        public GDIDeliveryPrintDocument(Order[] orders, WuliuNumber[] wuliuNumbers, Dictionary<string, string>[] userDatas, PrintTemplate wuliuTemplate) : base(orders, wuliuNumbers, userDatas, wuliuTemplate) { }

        protected static float MapToPrinterPix(double value)
        {
            return (float)(100.0 * value / 96.0);
        }

        protected object FormatData(PrintTemplateItem printTemplateItem)
        {
            //该用那个格式化程序
            var group = PrintTemplateItemType.GetGroup(printTemplateItem.Type);
            object data = null;
            if (group == PrintTemplateItemTypeGroup.PRINT)
            {
                PrintInfo pi = new PrintInfo { PrintTime = DateTime.Now, PageInfo = "第" + (this.index + 1).ToString() + "/" + this.Orders.Length.ToString() + "页" };
                data = PrintFormatterManager.Format(this.WuliuTemplate, printTemplateItem, pi);
            }
            else if (group == PrintTemplateItemTypeGroup.OTHER)
            {
                data = OtherFormatterManager.Format(this.WuliuTemplate, printTemplateItem);
            }
            else if (group == PrintTemplateItemTypeGroup.ORDER)
            {
                data = OrderFormatterManager.Format(this.WuliuTemplate, printTemplateItem, this.Orders[index]);
            }
            else if (group == PrintTemplateItemTypeGroup.DELIVERY)
            {
                data = DeliveryFormatterManager.Format(this.WuliuTemplate, printTemplateItem, this.WuliuNumbers[index]);
            }
            else if (group == PrintTemplateItemTypeGroup.SHOP)
            {
                data = ShopFormatterManager.Format(this.WuliuTemplate, printTemplateItem, this.Orders[index].ShopId);
            }
            else
            {
                throw new Exception("商品模板不支类型:" + printTemplateItem.Type);
            }
            return data;
        }

        protected void PrintValue(PrintPageEventArgs e)
        {
            var rendor = e.Graphics;
            foreach (var printItem in this.WuliuTemplate.Items)
            {
                object data = FormatData(printItem);
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
                    var rect = new System.Drawing.RectangleF(MapToPrinterPix(printItem.X + WuliuTemplate.XOffset), MapToPrinterPix(printItem.Y + WuliuTemplate.YOffset), MapToPrinterPix(printItem.Width), MapToPrinterPix(printItem.Height));
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
                    rendor.DrawImage(image, new System.Drawing.PointF(MapToPrinterPix(printItem.X + WuliuTemplate.XOffset), MapToPrinterPix(printItem.Y + WuliuTemplate.YOffset)));
                }
                else if (data is System.Drawing.Pen)
                {
                    var pen = data as System.Drawing.Pen;
                    if (printItem.Height > printItem.Width)
                    {
                        //竖线
                        var p1 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + printItem.Width / 2 + WuliuTemplate.XOffset), MapToPrinterPix(printItem.Y + WuliuTemplate.YOffset));
                        var p2 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + printItem.Width / 2 + WuliuTemplate.XOffset), MapToPrinterPix(printItem.Y + printItem.Height + WuliuTemplate.YOffset));
                        rendor.DrawLine(pen, p1, p2);
                    }
                    else
                    {
                        //横线
                        var p1 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + WuliuTemplate.XOffset), MapToPrinterPix(printItem.Y + printItem.Height / 2 + WuliuTemplate.YOffset));
                        var p2 = new System.Drawing.PointF(MapToPrinterPix(printItem.X + printItem.Width + WuliuTemplate.XOffset), MapToPrinterPix(printItem.Y + printItem.Height / 2 + WuliuTemplate.YOffset));
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
                if (this.OnPagePrintEnded(index))
                {
                    e.Cancel = true;
                    return;
                }
                this.index++;
                e.HasMorePages = this.index < this.Orders.Length;
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                Log.Logger.Log(this.GetType().FullName + ": Document_PrintPage", ex);
                App.Current.Dispatcher.BeginInvoke(new Action(() => System.Windows.MessageBox.Show("打印错误：" + ex.Message)));
            }
        }

        private void Document_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            this.OnPrintEnded();
        }

        private void Document_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            this.OnPrintStarting();
        }

        public override string StartPrint(string printer, string printServerAdd)
        {
            System.Drawing.Printing.PrintDocument document = new System.Drawing.Printing.PrintDocument();
            if (System.Drawing.Printing.PrinterSettings.InstalledPrinters.OfType<string>().Contains(printer) == false)
            {
                throw new Exception("打印机不存在此电脑上:" + printer);
            }
            document.PrinterSettings.PrinterName = printer;
            document.PrintController = new System.Drawing.Printing.StandardPrintController();
            document.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("shoperp_print_size", (int)MapToPrinterPix(WuliuTemplate.Width), (int)MapToPrinterPix(WuliuTemplate.Height));
            document.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            document.DocumentName = WuliuTemplate.Name + DateTime.Now;
            document.PrintPage += Document_PrintPage;
            document.BeginPrint += Document_BeginPrint;
            document.EndPrint += Document_EndPrint;
            this.index = 0;
            document.Print();
            return "";
        }
    }
}
