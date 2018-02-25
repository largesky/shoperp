using System.Windows.Media;
using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    class OtherLine : IOtherFormatter
    {
        private BrushConverter converter = new BrushConverter();

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
            var brush = this.converter.ConvertFromString(item.Format) as Brush;
            var pen = new System.Windows.Media.Pen(brush, item.Height > item.Width ? item.Width : item.Height);
            pen.DashStyle = item.Value == "是" ? new DashStyle(new double[] { 1, 1 }, 0) : null;
            pen.DashCap = PenLineCap.Flat;
            pen.EndLineCap = PenLineCap.Flat;
            pen.StartLineCap = PenLineCap.Flat;
            return pen;
        }
    }
}
