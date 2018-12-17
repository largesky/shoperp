using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument
{
    public abstract class FixedPrintDocument<T> : PrintDocumentBase<T>
    {
        protected abstract object FormatData(PrintTemplateItem printTemplateItem);

        protected override void PrintValue(PrintPageEventArgs e)
        {
            var rendor = e.Graphics;
            foreach (var printItem in this.Template.Items)
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
    }
}
