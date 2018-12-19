using System.Windows.Media;
using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    class OtherLine : IOtherFormatter
    {

        public string AcceptType
        {
            get { return PrintTemplateItemType.OTHER_LINE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Format))
            {
                item.Format = Brushes.Black.ToString();
            }
            int argb = int.Parse(item.Format.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
            System.Drawing.Color c = System.Drawing.Color.FromArgb((int)(item.Opacity * 255), System.Drawing.Color.FromArgb(argb));
            System.Drawing.Pen pen = new System.Drawing.Pen(c, (float)(item.Height > item.Width ? item.Width : item.Height));
            pen.DashStyle = item.Value == "是" ? System.Drawing.Drawing2D.DashStyle.Dot : System.Drawing.Drawing2D.DashStyle.Solid;
            pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
            return pen;
        }
    }
}
