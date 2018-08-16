using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using ShopErp.App.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using ShopErp.App.Domain;
using System.Windows.Media;
using ShopErp.App.Service.Print;

namespace ShopErp.App.ViewModels
{
    public class PrintTemplateItemViewModelCommon : DependencyObject
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty FontNameProperty =
            DependencyProperty.Register("FontName", typeof(string), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty ScaleFormatProperty = DependencyProperty.Register("ScaleFormat", typeof(string), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty OpacityProperty =
          DependencyProperty.Register("Opacity", typeof(double), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty Value1Property =
            DependencyProperty.Register("Value1", typeof(string), typeof(PrintTemplateItemViewModelCommon));

        public static readonly DependencyProperty PreviewValueProperty =
            DependencyProperty.Register("PreviewValue", typeof(object), typeof(PrintTemplateItemViewModelCommon));

        /// <summary>
        /// 所属的模板
        /// </summary>
        public Service.Print.PrintTemplate Template { get; set; }

        public double X
        {
            get { return (double)this.GetValue(XProperty); }
            set { this.SetValue(XProperty, value); }
        }

        public double Y
        {
            get { return (double)this.GetValue(YProperty); }
            set { this.SetValue(YProperty, value); }
        }

        public double Width
        {
            get { return (double)this.GetValue(WidthProperty); }
            set { this.SetValue(WidthProperty, value); }
        }

        public double Height
        {
            get { return (double)this.GetValue(HeightProperty); }
            set { this.SetValue(HeightProperty, value); }
        }

        public string FontName
        {
            get { return (string)this.GetValue(FontNameProperty); }
            set { this.SetValue(FontNameProperty, value); }
        }

        public double FontSize
        {
            get { return (double)this.GetValue(FontSizeProperty); }
            set { this.SetValue(FontSizeProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)this.GetValue(TextAlignmentProperty); }
            set { this.SetValue(TextAlignmentProperty, value); }
        }

        public string ScaleFormat
        {
            get { return (string)this.GetValue(ScaleFormatProperty); }
            set { this.SetValue(ScaleFormatProperty, value); }
        }


        public double Opacity
        {
            get { return (double)this.GetValue(OpacityProperty); }
            set { this.SetValue(OpacityProperty, value); }
        }

        public string Format
        {
            get { return (string)this.GetValue(FormatProperty); }
            set { this.SetValue(FormatProperty, value); }
        }

        public string Type
        {
            get { return (string)this.GetValue(TypeProperty); }
            set { this.SetValue(TypeProperty, value); }
        }

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public string Value1
        {
            get { return (string)this.GetValue(Value1Property); }
            set { this.SetValue(Value1Property, value); }
        }

        public object PreviewValue
        {
            get { return (object)this.GetValue(PreviewValueProperty); }
            set { this.SetValue(PreviewValueProperty, value); }
        }

        public Thumb UI { get; protected set; }

        public Service.Print.PrintTemplateItem Data { get; protected set; }

        public FrameworkElement PropertyUI { get; protected set; }

        public event MouseEventHandler PreviewMouseLeftButtonDown;

        public PrintTemplateItemViewModelCommon(Service.Print.PrintTemplate template)
        {
            this.Format = "";
            this.Template = template;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == XProperty)
            {
                if (double.IsNaN(this.X))
                {
                    this.X = 0;
                    return;
                }
                Canvas.SetLeft(this.UI, double.Parse(e.NewValue.ToString()));
                if (this.Data != null)
                    this.Data.X = double.Parse(e.NewValue.ToString());
            }
            else if (e.Property == YProperty)
            {
                if (double.IsNaN(this.Y))
                {
                    this.Y = 0;
                    return;
                }
                Canvas.SetTop(this.UI, double.Parse(e.NewValue.ToString()));
                if (this.Data != null)
                    this.Data.Y = double.Parse(e.NewValue.ToString());
            }
            else if (e.Property == WidthProperty)
            {
                this.UI.Width = double.Parse(e.NewValue.ToString());
                if (this.Data != null)
                    this.Data.Width = double.Parse(e.NewValue.ToString());
            }
            else if (e.Property == HeightProperty)
            {
                this.UI.Height = double.Parse(e.NewValue.ToString());
                if (this.Data != null)
                    this.Data.Height = double.Parse(e.NewValue.ToString());
            }
            else if (e.Property == FontSizeProperty)
            {
                this.UI.FontSize = double.Parse(e.NewValue.ToString());
                if (this.Data != null)
                    this.Data.FontSize = double.Parse(e.NewValue.ToString());
            }
            else if (e.Property == FontNameProperty)
            {
                this.UI.FontFamily = new System.Windows.Media.FontFamily((string)e.NewValue);
                if (this.Data != null)
                    this.Data.FontName = (string)e.NewValue;
            }
            else if (e.Property == OpacityProperty)
            {
                this.UI.Opacity = (double)e.NewValue;
                if (this.Data != null)
                    this.Data.Opacity = (double)e.NewValue;
            }
            else if (e.Property == TextAlignmentProperty)
            {
                if (this.UI != null)
                {
                    var ta = (TextAlignment)e.NewValue;
                    HorizontalAlignment ha = HorizontalAlignment.Left;
                    if (ta == TextAlignment.Left)
                    {
                        ha = HorizontalAlignment.Left;
                    }
                    else if (ta == TextAlignment.Right)
                    {
                        ha = HorizontalAlignment.Right;
                    }
                    else if (ta == TextAlignment.Center)
                    {
                        ha = HorizontalAlignment.Center;
                    }
                    else
                    {
                        ha = HorizontalAlignment.Stretch;
                    }
                    var ccc = FindSubControl<ContentControl>(this.UI);
                    if (ccc != null)
                        ccc.HorizontalAlignment = ha;
                }
                if (this.Data != null)
                    this.Data.TextAlignment = (TextAlignment)e.NewValue;
            }
            else if (e.Property == ScaleFormatProperty)
            {
                if (this.UI != null)
                {
                }
                if (this.Data != null)
                    this.Data.ScaleFormat = (string)e.NewValue;
            }
            else if (e.Property == FormatProperty)
            {
                if (this.Data != null)
                {
                    this.Data.Format = e.NewValue.ToString();
                }
            }
            else if (e.Property == TypeProperty)
            {
                if (this.Data != null)
                {
                    this.Data.Type = e.NewValue.ToString();
                }
            }
            else if (e.Property == PreviewValueProperty)
            {
                //do noting
            }
            else if (e.Property == ValueProperty)
            {
                if (this.Data != null && e.NewValue != null)
                    this.Data.Value = e.NewValue.ToString();
            }
            else if (e.Property == Value1Property)
            {
                if (this.Data != null && e.NewValue != null)
                    this.Data.Value1 = e.NewValue.ToString();
            }
            else
            {
                MessageBox.Show("未知的属性:" + e.Property.Name);
            }
        }

        protected virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (this.PreviewMouseLeftButtonDown != null)
            {
                this.PreviewMouseLeftButtonDown(this, e);
            }
        }

        private void AttachThumbEvents(Thumb thumb)
        {
            thumb.DragDelta += thumb_DragDelta;
            thumb.SizeChanged += thumb_SizeChanged;
            thumb.PreviewMouseLeftButtonDown += thumb_PreviewMouseLeftButtonDown;
            thumb.PreviewKeyUp += new KeyEventHandler(thumb_PreviewKeyUp);
        }

        void thumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.OnMouseEnter(null);
        }

        private void DeattachThumbEvents(Thumb thumb)
        {
            thumb.DragDelta -= thumb_DragDelta;
            thumb.SizeChanged -= thumb_SizeChanged;
            thumb.PreviewMouseLeftButtonDown -= thumb_PreviewMouseLeftButtonDown;
            thumb.PreviewKeyUp -= new KeyEventHandler(thumb_PreviewKeyUp);
        }

        void thumb_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            var item = thumb.DataContext as PrintTemplateItemViewModelCommon;
            item.Width = (int)thumb.Width;
            item.Height = (int)thumb.Height;
        }

        void thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            var item = thumb.DataContext as PrintTemplateItemViewModelCommon;
            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
            item.X = Canvas.GetLeft(thumb);
            item.Y = Canvas.GetTop(thumb);
        }

        void thumb_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Thumb thumb = sender as Thumb;
            if (e.Key == Key.Up)
            {
                Canvas.SetTop(thumb, Canvas.GetTop(thumb) - 1);
            }
            else if (e.Key == Key.Down)
            {
                Canvas.SetTop(thumb, Canvas.GetTop(thumb) + 1);
            }
            else if (e.Key == Key.Left)
            {
                Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) - 1);
            }
            else if (e.Key == Key.Right)
            {
                Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + 1);
            }
            else
            {
                e.Handled = false;
            }
        }

        public void ApplayStyleAndData(System.Windows.Style uiStyle, Service.Print.PrintTemplateItem item)
        {
            //创建UI
            this.UI = new Thumb { DataContext = this };
            this.UI.Style = uiStyle;
            this.UI.ApplyTemplate();
            Data = item;
            if (double.IsNaN(this.Data.X) || this.Data.X < 0)
            {
                this.Data.X = 0;
            }
            this.X = this.Data.X;
            if (double.IsNaN(this.Data.Y) || this.Data.Y < 0)
            {
                this.Data.Y = 0;
            }
            this.Y = this.Data.Y;
            this.Width = this.Data.Width;
            this.Height = this.Data.Height;
            this.FontName = this.Data.FontName;
            this.FontSize = this.Data.FontSize;
            this.Format = this.Data.Format;
            this.Type = this.Data.Type;
            this.Value = item.Value;
            this.Value1 = item.Value1;
            this.TextAlignment = item.TextAlignment;
            this.Opacity = item.Opacity;
            this.ScaleFormat = item.ScaleFormat;
            item.RunTimeTag = this;
            AttachThumbEvents(this.UI);
        }


        private T FindSubControl<T>(DependencyObject doo) where T : DependencyObject
        {
            if (VisualTreeHelper.GetChildrenCount(doo) < 1)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(doo); i++)
            {
                var cc = VisualTreeHelper.GetChild(doo, i);
                if (cc.GetType() == typeof(T))
                {
                    return cc as T;
                }
                var subCC = FindSubControl<T>(cc);
                if (subCC != null)
                {
                    return subCC;
                }
            }
            return null;
        }
    }


}