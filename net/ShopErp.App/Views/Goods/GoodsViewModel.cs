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
        public static readonly DependencyProperty FlagProperty = DependencyProperty.Register("Flag", typeof(ColorFlag), typeof(GoodsViewModel));

        public static readonly DependencyProperty CommentProperty = DependencyProperty.Register("Comment", typeof(string), typeof(GoodsViewModel));

        public static readonly DependencyProperty UploadStateProperty = DependencyProperty.Register("UploadState", typeof(string), typeof(GoodsViewModel));


        public ShopErp.Domain.Goods Source { get; set; }

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

        public string UploadState
        {
            get { return (string)this.GetValue(UploadStateProperty); }
            set { this.SetValue(UploadStateProperty, value); }
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

        public GoodsViewModel(ShopErp.Domain.Goods source)
        {
            this.Source = source;
            this.Comment = source.Comment;
            this.Flag = source.Flag;
        }
    }
}