
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Service.Spider;
using ShopErp.App.Utils;
using ShopErp.App.Service.Net;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodsCreateWindow.xaml
    /// </summary>
    public partial class GoodsCreateSampleWindow : Window
    {
        public GoodsCreateSampleWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var goodsService = ServiceContainer.GetService<GoodsService>();
                string vendorHomePage = "", goodsVideoUrl = "";
                string url = this.tbUrl.Text.Trim();

                url = url.Substring(0, url.IndexOf('?') < 0 ? url.Length : url.IndexOf('?'));
                var goods = SpiderBase.CreateSpider(url, 80, 0).GetGoodsInfoByUrl(url, ref vendorHomePage, ref goodsVideoUrl, true);

                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
                var vendor = vendors.FirstOrDefault(obj => obj.Alias.IndexOf(vendorHomePage, StringComparison.OrdinalIgnoreCase) >= 0);
                if (vendor == null)
                {
                    vendor = vendors.FirstOrDefault(obj => obj.HomePage.Equals(vendorHomePage, StringComparison.OrdinalIgnoreCase));
                }
                if (vendor == null)
                {
                    vendor = SpiderBase.CreateSpider(url, 80, 0).GetVendorInfoByUrl(url);
                    vendor.Id = ServiceContainer.GetService<VendorService>().Save(vendor);
                }
                else
                {
                    //检查重复
                    var datas = goodsService.GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, "", goods.Number, GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", 0, 0).Datas;
                    if (datas.FirstOrDefault(obj => obj.VendorId == vendor.Id) != null)
                    {
                        throw new Exception("已经存在该厂家货号");
                    }
                }
                goods.VendorId = vendor.Id;
                goods.CreateOperator = OperatorService.LoginOperator.Number;
                goods.CreateTime = DateTime.Now;
                goods.UpdateTime = DateTime.Now;
                goods.Flag = ColorFlag.UN_LABEL;
                GoodsService.SaveImage(goods, goods.Image);

                if (this.rbAutoCheck.IsChecked.Value)
                {
                    GoodsService.SaveVideo(goods, goodsVideoUrl);
                }
                else if (this.rbNo.IsChecked.Value)
                {
                    goods.VideoType = GoodsVideoType.NOT;
                }
                else if (this.rbYes.IsChecked.Value)
                {
                    goods.VideoType = GoodsVideoType.VIDEO;
                }


                goods.Id = goodsService.Save(goods);

                //生成店铺
                var gus = (this.cbbShops.ItemsSource as ShopCheckViewModel[]).Where(obj => obj.IsChecked).Select(
                    obj => new GoodsShop
                    {
                        GoodsId = goods.Id,
                        SalePrice = 0,
                        ShopId = obj.Source.Id,
                        State = GoodsState.WAITPROCESSIMAGE,
                        PopGoodsId = "",
                        ProcessImageOperator = "",
                        ProcessImageTime = goodsService.GetDBMinTime(),
                        UploadOperator = "",
                        UploadTime = goodsService.GetDBMinTime()
                    }).ToArray();
                foreach (var o in gus)
                {
                    ServiceContainer.GetService<GoodsShopService>().Save(o);
                }

                MessageBox.Show("保存成功");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.cbbShops.ItemsSource = ServiceContainer.GetService<ShopService>().GetByAll().Datas
                    .Where(obj => obj.Enabled).Select(obj => new ShopCheckViewModel(obj) { IsChecked = false }).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}