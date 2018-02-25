
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
                var gu = new ShopErp.Domain.Goods
                {
                    Comment = this.tbComment.Text.Trim(),
                    CreateOperator = OperatorService.LoginOperator.Number,
                    CreateTime = DateTime.Now,
                    VendorId = 0,
                    Image = "",
                    Id = 0,
                    ImageDir = "",
                    Number = "",
                    Colors = "",
                    Url = "",
                    LastSellTime = goodsService.GetDBMinTime(),
                    Material = "",
                    Price = 0,
                    Type = 0,
                    UpdateEnabled = true,
                    Weight = 0,
                    Star = 0,
                    UpdateTime = goodsService.GetDBMinTime(),
                    Flag = ColorFlag.UN_LABEL,
                    IgnoreEdtion = false,
                };

                //根据网址获取
                string url = this.tbUrl.Text.Trim();
                string v = "";
                var goods = SpiderBase.CreateSpider(url, 80, 0).GetGoodsInfoByUrl(this.tbUrl.Text.Trim(), ref v, true);
                gu.Image = goods.Image;
                gu.Url = url.Substring(0, url.IndexOf('?') < 0 ? url.Length : url.IndexOf('?'));
                gu.Type = goods.Type;
                gu.Price = goods.Price;
                gu.Colors = goods.Colors;

                //查询厂家
                var ven = SpiderBase.CreateSpider(url, 80, 0).GetVendorInfoByUrl(url);
                if (ven == null)
                {
                    throw new Exception("从网页获取厂家信息失败请重试");
                }

                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
                var vendor = vendors.FirstOrDefault(obj => obj.Alias.IndexOf(ven.HomePage, StringComparison.OrdinalIgnoreCase) >= 0);
                if (vendor == null)
                {
                    vendor = vendors.FirstOrDefault(obj => obj.HomePage.Equals(ven.HomePage, StringComparison.OrdinalIgnoreCase));
                }
                if (vendor == null)
                {
                    ven.Id = ServiceContainer.GetService<VendorService>().Save(ven);
                }
                else
                {
                    ven = vendor;
                }
                gu.VendorId = ven.Id;

                //检查重复
                var datas = goodsService.GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, ven.Name, "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, "", 0, 0).Datas;
                if (datas.FirstOrDefault(obj => obj.Number == goods.Number) != null)
                {
                    throw new Exception("已经存在该厂家货号");
                }
                gu.VendorId = ven.Id;
                gu.Material = goods.Material;
                gu.Number = goods.Number;
                GoodsService.SaveImage(gu, gu.Image);
                gu.Id = goodsService.Save(gu);

                //生成店铺
                var gus = (this.cbbShops.ItemsSource as ShopCheckViewModel[]).Where(obj => obj.IsChecked).Select(
                    obj => new GoodsShop
                    {
                        GoodsId = gu.Id,
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