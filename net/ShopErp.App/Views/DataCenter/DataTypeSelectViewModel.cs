using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ShopErp.App.Views.DataCenter
{
    public class DataTypeSelectViewModel : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(DataTypeSelectViewModel));

        public bool IsChecked { get { return (bool)this.GetValue(IsCheckedProperty); } set { this.SetValue(IsCheckedProperty, value); } }

        public string Name { get; set; }

        public System.Drawing.Color Color { get; set; }

        public Brush BrushColor { get; set; }

        public DataTypeSelectViewModel(bool isCheck, string name, System.Drawing.Color color)
        {
            this.IsChecked = isCheck;
            this.Name = name;
            this.Color = color;
            var st = "#" + color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            this.BrushColor = (Brush)new BrushConverter().ConvertFromString(st);
        }
    }
}
