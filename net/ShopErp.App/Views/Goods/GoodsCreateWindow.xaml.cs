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
using ShopErp.App.Converters;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.App.Views.Extenstions;
using ShopErp.App.Service.Spider;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// GoodsEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GoodsCreateWindow : Window
    {
        public static readonly DependencyProperty GoodsProperty = DependencyProperty.Register("Goods", typeof(ShopErp.Domain.Goods), typeof(GoodsCreateWindow));

        private GoodsService goodsService = ServiceContainer.GetService<GoodsService>();
        private VendorService vendorService = ServiceContainer.GetService<VendorService>();
        private string imagePath = null;
        private bool hasImageSet = false;

        string oldNumber;
        long oldVendorId;

        public ShopErp.Domain.Goods Goods
        {
            get { return (ShopErp.Domain.Goods)this.GetValue(GoodsProperty); }
            set { this.SetValue(GoodsProperty, value); }
        }

        public GoodsCreateWindow()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != GoodsProperty)
            {
                return;
            }
            this.Title = this.Goods == null || this.Goods.Id < 1 ? "添加商品-粘贴网址后输入回车" : "编辑商品-" + this.Goods.Number;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.cbbTypes.Bind<GoodsType>();
                this.cbbVideoType.Bind<GoodsVideoType>();
                this.cbbShops.ItemsSource = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).Select(obj => new ShopCheckViewModel(obj) { IsChecked = false }).ToArray();
                if (this.Goods != null)
                {
                    this.imagePath = this.Goods.Image;
                    foreach (var shop in this.cbbShops.ItemsSource as ShopCheckViewModel[])
                    {
                        shop.IsChecked = this.Goods.Shops.FirstOrDefault(obj => obj.ShopId == shop.Source.Id) != null;
                    }
                    oldNumber = this.Goods.Number;
                    oldVendorId = this.Goods.VendorId;
                }
                else
                {
                    this.Goods = new ShopErp.Domain.Goods { Colors = "", Comment = "", CreateOperator = OperatorService.LoginOperator.Number, CreateTime = DateTime.Now, Flag = ColorFlag.UN_LABEL, Id = 0, IgnoreEdtion = false, Image = "", ImageDir = "", LastSellTime = DateTime.Now, Material = "", Number = "", Price = 0, Shops = new List<GoodsShop>(), Star = 0, Type = GoodsType.GOODS_SHOES_NONE, UpdateEnabled = true, UpdateTime = DateTime.Now, Url = "", VendorId = 0, VideoType = GoodsVideoType.NONE, Weight = 0 };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                bool? ret = ofd.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(File.ReadAllBytes(ofd.FileName));
                image.EndInit();
                this.img.Source = image;
                this.imagePath = ofd.FileName;
                this.hasImageSet = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TbUrl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                {
                    return;
                }

                if (this.Goods != null && this.Goods.Id > 0)
                {
                    MessageBox.Show("当前处于编辑商品模式，不能自动读取新数据");
                    return;
                }
                string vendorHomePage = "", goodsVideoUrl = "";
                string url = this.tbUrl.Text.Trim();

                url = url.Substring(0, url.IndexOf('?') < 0 ? url.Length : url.IndexOf('?'));
                var goods = SpiderBase.CreateSpider(url, 80, 0).GetGoodsInfoByUrl(url, ref vendorHomePage, ref goodsVideoUrl, false, true);

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
                goods.VendorId = vendor.Id;
                this.imagePath = goods.Image;
                this.hasImageSet = true;
                this.Goods = goods;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var minTime = this.goodsService.GetDBMinTime();
                var newVendor = this.tbVendor.Text.Trim();
                string newNumber = this.tbNumber.Text.Trim();

                if (string.IsNullOrWhiteSpace(newVendor))
                {
                    MessageBox.Show("请输入厂家全名称");
                    return;
                }

                if (this.Goods.VendorId == 0)
                {
                    MessageBox.Show("厂家名称不存在");
                    return;
                }

                if (this.Goods.VendorId < 0)
                {
                    MessageBox.Show("厂家名称找到多个厂家，请输入厂家全名称");
                    return;
                }

                if (this.cbbTypes.SelectedIndex < 0 || this.cbbTypes.GetSelectedEnum<GoodsType>() == GoodsType.GOODS_SHOES_NONE)
                {
                    MessageBox.Show("选择类型");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newNumber))
                {
                    MessageBox.Show("请输入货号");
                    return;
                }

                if (string.IsNullOrWhiteSpace(this.imagePath))
                {
                    MessageBox.Show("请选择商品图片");
                    return;
                }

                //查找该厂家是其它厂家的别名
                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
                var vendor = vendors.FirstOrDefault(obj => obj.Id == this.Goods.VendorId);
                var v = vendors.FirstOrDefault(obj => obj.Alias.IndexOf(vendor.HomePage.Trim('/'), StringComparison.OrdinalIgnoreCase) >= 0);
                if (v != null)
                {
                    vendor = v;
                }
                if (vendor == null)
                {
                    throw new Exception("厂家不存在");
                }
                this.Goods.VendorId = vendor.Id;
                var selectedShops = this.cbbShops.ItemsSource.OfType<ShopCheckViewModel>().Where(obj => obj.IsChecked).ToArray();
                var existGoods = ServiceContainer.GetService<GoodsService>().GetByNumberAndVendorNameLike(newNumber, vendor.Name, 0, 0).Datas.FirstOrDefault(obj => obj.VendorId == vendor.Id);
                if (this.Goods.Id < 1)
                {
                    if (existGoods != null)
                    {
                        throw new Exception("已存在相同厂家货号");
                    }
                    //新建
                    GoodsService.SaveImage(this.Goods, this.imagePath);
                    this.Goods.Id = this.goodsService.Save(this.Goods);
                }
                else
                {
                    if (existGoods != null && existGoods.Id != this.Goods.Id)
                    {
                        throw new Exception("已存在相同厂家货号");
                    }

                    //编辑
                    string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        throw new Exception(SystemNames.CONFIG_WEB_IMAGE_DIR + "不能为空");
                    }

                    //改变了厂家，或者货号则需要，移动图片文件夹
                    if ((oldVendorId > 0 && oldVendorId != vendor.Id) || (string.IsNullOrWhiteSpace(oldNumber) == false && oldNumber.Equals(newNumber, StringComparison.OrdinalIgnoreCase) == false))
                    {
                        //移动图片
                        string oldDir = dir + "\\" + this.Goods.ImageDir;
                        string newDir = dir + "\\goods\\" + vendor.Id.ToString() + "\\" + this.Goods.Number;
                        FileUtil.EnsureExits(new FileInfo(newDir));
                        Directory.Move(oldDir, newDir);
                        this.Goods.ImageDir = "goods\\" + vendor.Id.ToString() + "\\" + this.Goods.Number;
                        this.Goods.Image = this.Goods.ImageDir + "\\index.jpg";
                    }
                    //更新图片
                    if (this.hasImageSet)
                    {
                        GoodsService.SaveImage(this.Goods, this.imagePath);
                    }

                    this.goodsService.Update(this.Goods);
                }

                //上货店铺
                //已有的，没有在选择中的，需要删除
                var gus = this.Goods.Shops == null ? new GoodsShop[0] : this.Goods.Shops.ToArray();
                foreach (var s in gus)
                {
                    if (selectedShops.FirstOrDefault(obj => obj.Source.Id == s.ShopId) == null)
                    {
                        ServiceContainer.GetService<GoodsShopService>().Delete(s.Id);
                        this.Goods.Shops.Remove(s);
                    }
                }

                //没有的则需要增加
                foreach (var ss in selectedShops)
                {
                    if (gus.FirstOrDefault(obj => obj.ShopId == ss.Source.Id) == null)
                    {
                        var gu = new GoodsShop
                        {
                            GoodsId = this.Goods.Id,
                            ShopId = ss.Source.Id,
                            State = GoodsState.WAITPROCESSIMAGE,
                            SalePrice = 0,
                            PopGoodsId = "",
                            ProcessImageOperator = "",
                            UploadOperator = "",
                            ProcessImageTime = minTime,
                            UploadTime = minTime
                        };
                        gu.Id = ServiceContainer.GetService<GoodsShopService>().Save(gu);
                        this.Goods.Shops.Add(gu);
                    }
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}