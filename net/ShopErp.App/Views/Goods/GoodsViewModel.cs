using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using ShopErp.App.Converters;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    public class GoodsViewModel : DependencyObject
    {
        public static readonly DependencyProperty FlagProperty =
            DependencyProperty.Register("Flag", typeof(ColorFlag), typeof(GoodsViewModel));

        public static readonly DependencyProperty ImageDownloadStateProperty =
            DependencyProperty.Register("ImageDownloadState", typeof(string), typeof(GoodsViewModel));

        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(string), typeof(GoodsViewModel));

        public static readonly DependencyProperty StarStringProperty =
            DependencyProperty.Register("StarString", typeof(string), typeof(GoodsViewModel));

        public static readonly DependencyProperty StarImageProperty =
            DependencyProperty.Register("StarImage", typeof(BitmapSource), typeof(GoodsViewModel));

        public ShopErp.Domain.Goods Source { get; set; }

        public string ImageDownloadState
        {
            get
            {
                if (this.Source == null)
                {
                    return "";
                }

                if (this.Source.Shops != null &&
                    this.Source.Shops.Any(obj => (int)obj.State >= (int)GoodsState.WAITREVIEW))
                {
                    return "";
                }
                return (string)this.GetValue(ImageDownloadStateProperty);
            }
            set
            {
                if (this.Source == null)
                {
                    this.SetValue(ImageDownloadStateProperty, "");
                    return;
                }

                if (this.Source.Shops != null &&
                    this.Source.Shops.Any(obj => (int)obj.State >= (int)GoodsState.WAITREVIEW))
                {
                    this.SetValue(ImageDownloadStateProperty, "");
                }
                this.SetValue(ImageDownloadStateProperty, value);
            }
        }

        public string Comment
        {
            get { return (string)this.GetValue(CommentProperty); }
            set { this.SetValue(CommentProperty, value); }
        }

        public ColorFlag Flag
        {
            get { return (ColorFlag)this.GetValue(FlagProperty); }
            set { this.SetValue(FlagProperty, value); }
        }

        public string StarString
        {
            get { return (string)this.GetValue(StarStringProperty); }
        }

        public BitmapSource StarImage
        {
            get { return (BitmapSource)this.GetValue(StarImageProperty); }
        }

        public string TargetShops
        {
            get
            {
                if (this.Source == null || this.Source.Shops == null || this.Source.Shops.Count < 1)
                {
                    return "";
                }
                var ss = this.Source.Shops.Select(obj => ShopMarkConverter.Convert(obj.ShopId) + GetShortString(obj.State));
                return string.Join(",", ss);
            }
        }

        private string GetShortString(GoodsState state)
        {
            if (state == GoodsState.NONE)
            {
                return "";
            }
            if (state == GoodsState.WAITPROCESSIMAGE)
            {
                return "";
            }
            if (state == GoodsState.WAITREVIEW)
            {
                return "(图)";
            }
            if (state == GoodsState.WAITUPLOADED)
            {
                return "(审)";
            }
            if (state == GoodsState.UPLOADED)
            {
                return "(上)";
            }
            if (state == GoodsState.NOTSALE)
            {
                return "(下)";
            }
            return "(ERR)";
        }

        public void UpdateStarViewModel(int star)
        {
            if (this.Source == null)
            {
                this.SetValue(StarStringProperty, "");
                return;
            }
            this.Source.Star = star;
            this.SetValue(StarStringProperty, star <= 0 ? "" : star.ToString());
        }

        public GoodsViewModel(ShopErp.Domain.Goods source)
        {
            this.Source = source;
            this.Comment = source.Comment;
            this.UpdateStarViewModel(source.Star);
            this.Flag = source.Flag;
        }
    }
}