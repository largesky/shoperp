using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShopErp.App.ViewModels
{
    [Serializable]
    public class PrintTemplateItemViewModelForLine : PrintTemplateItemViewModelCommon
    {
        private BrushConverter converter = new BrushConverter();

        public PrintTemplateItemViewModelForLine(Service.Print.PrintTemplate template)
            : base(template)
        {
            this.PropertyUI = new PrintTemplateItemLineUserControl();
            this.PropertyUI.DataContext = this;
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var l = this.PreviewValue as Line;

            if (this.PreviewValue == null)
            {
                this.PreviewValue = new Line();
                l = this.PreviewValue as Line;
            }

            base.OnPropertyChanged(e);


            if (e.Property == PrintTemplateItemViewModelCommon.XProperty || e.Property == PrintTemplateItemViewModelCommon.YProperty || e.Property == PrintTemplateItemViewModelCommon.WidthProperty || e.Property == PrintTemplateItemViewModelCommon.HeightProperty)
            {
                if (this.Width > this.Height)
                {
                    l.X1 = 0;
                    l.Y2 = l.Y1 = this.Height / 2;
                    l.X2 = this.Width;
                    l.StrokeThickness = this.Height;
                }
                else
                {
                    l.Y1 = 0;
                    l.Y2 = Height;
                    l.X1 = l.X2 = this.Width / 2;
                    l.StrokeThickness = this.Width;
                }
                return;
            }

            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty || e.Property == PrintTemplateItemViewModelCommon.ValueProperty)
            {
                if (string.IsNullOrWhiteSpace(this.Format))
                {
                    this.Format = Brushes.Black.ToString();
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.Value))
                {
                    this.Value = "否";
                    return;
                }

                Brush b = string.IsNullOrWhiteSpace(this.Format) ? Brushes.Black : this.converter.ConvertFromString(this.Format) as Brush;
                l.Stroke = b;
                l.Fill = Brushes.White;
                l.StrokeDashArray = this.Value == "否" ? null : new DoubleCollection(new double[] { 1, 1 });
                return;
            }

        }
    }
}