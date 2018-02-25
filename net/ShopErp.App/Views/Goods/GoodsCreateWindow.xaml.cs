
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

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// GoodsEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GoodsCreateWindow : Window
    {
        private GoodsService goodsService = ServiceContainer.GetService<GoodsService>();
        private VendorService vendorService = ServiceContainer.GetService<VendorService>();
        private string imagePath = null;

        public ShopErp.Domain.Goods Goods { get; set; }

        public GoodsCreateWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "编辑商品";
            if (this.Goods == null)
            {
                this.Title = "新增商品";
            }

            try
            {
                this.cbbTypes.Bind<GoodsType>();
                this.cbbShops.ItemsSource = ServiceContainer.GetService<ShopService>().GetByAll().Datas
                    .Where(obj => obj.Enabled).Select(obj => new ShopCheckViewModel(obj) { IsChecked = false }).ToArray();
                var minTime = this.goodsService.GetDBMinTime();
                if (this.Goods == null)
                {
                    this.Goods = new ShopErp.Domain.Goods();
                    this.Goods.CreateOperator = OperatorService.LoginOperator.Number;
                    this.Goods.CreateTime = DateTime.Now;
                    this.Goods.Image = "";
                    this.Goods.ImageDir = "";
                    this.Goods.Id = 0;
                    this.Goods.LastSellTime = minTime;
                    this.Goods.Star = 0;
                    this.Goods.UpdateTime = minTime;
                    this.Goods.Shops = new List<GoodsShop>();
                    this.Goods.Flag = ColorFlag.UN_LABEL;
                }
                this.imagePath = this.Goods.Image;
                this.img.Source = (new WebUrlImageConverter()).Convert(this.Goods.Image, null, null, null) as ImageSource;
                this.cbbVendors.Text = this.Goods.VendorId < 1 ? "" : vendorService.GetById(this.Goods.VendorId).Name;
                this.cbbTypes.SetSelectedEnum(this.Goods.Type);
                this.tbNumber.Text = this.Goods.Number;
                this.tbPrice.Text = this.Goods.Price.ToString("F0");
                this.tbUrl.Text = this.Goods.Url;
                this.tbWeight.Text = this.Goods.Weight.ToString("F2");
                this.tbMaterial.Text = this.Goods.Material;
                this.chkUpdateEnabled.IsChecked = this.Goods.UpdateEnabled;
                this.tbColors.Text = this.Goods.Colors;
                this.tbStar.Text = this.Goods.Star.ToString();

                foreach (var shop in this.cbbShops.ItemsSource as ShopCheckViewModel[])
                {
                    shop.IsChecked = this.Goods.Shops.FirstOrDefault(obj => obj.ShopId == shop.Source.Id) != null;
                }
                this.tbComment.Text = this.Goods.Comment;
                this.chkIgnoreEdtion.IsChecked = this.Goods.IgnoreEdtion;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            bool? ret = ofd.ShowDialog();
            if (ret == null || ret.Value == false)
            {
                return;
            }

            this.img.Source = new BitmapImage(new Uri(ofd.FileName));
            this.imagePath = ofd.FileName;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var minTime = this.goodsService.GetDBMinTime();
                var vendor = this.cbbVendors.Text.Trim();
                int typeIndex = this.cbbTypes.SelectedIndex;
                string number = this.tbNumber.Text.Trim();
                float price = float.Parse(this.tbPrice.Text.Trim());
                float weight = float.Parse(this.tbWeight.Text.Trim());
                string url = this.tbUrl.Text.Trim();
                bool updateEnabled = this.chkUpdateEnabled == null ? false : this.chkUpdateEnabled.IsChecked.Value;

                if (vendor == null)
                {
                    MessageBox.Show("请选择厂家");
                    return;
                }

                if (typeIndex < 0)
                {
                    MessageBox.Show("选择类型");
                    return;
                }

                if (string.IsNullOrWhiteSpace(number))
                {
                    MessageBox.Show("请输入货号");
                    return;
                }
                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
                var v = vendors.FirstOrDefault(obj => obj.Alias.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) >= 0);
                if (v == null)
                {
                    v = vendors.FirstOrDefault(obj => obj.Name.Equals(vendor, StringComparison.OrdinalIgnoreCase));
                }
                if (v == null)
                {
                    throw new Exception("厂家不存在");
                }
                long oldId = this.Goods.VendorId;

                this.Goods.VendorId = v.Id;
                this.Goods.Type = this.cbbTypes.GetSelectedEnum<GoodsType>();
                this.Goods.Number = number;
                this.Goods.Price = price;
                this.Goods.Url = url;
                this.Goods.Weight = weight;
                this.Goods.Colors = string.Join(",", this.tbColors.Text.Trim().Split(new char[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                this.Goods.Material = this.tbMaterial.Text.Trim();
                this.Goods.Comment = this.tbComment.Text.Trim();
                this.Goods.UpdateEnabled = updateEnabled;
                this.Goods.IgnoreEdtion = this.chkIgnoreEdtion.IsChecked == null ? false : this.chkIgnoreEdtion.IsChecked.Value;
                this.Goods.Star = int.Parse(this.tbStar.Text.Trim());
                var selectedShops = this.cbbShops.ItemsSource.OfType<ShopCheckViewModel>().Where(obj => obj.IsChecked).ToArray();
                this.Goods.Vendor = v;

                if (this.Goods.Id < 1)
                {
                    GoodsService.SaveImage(this.Goods, this.imagePath);
                    this.goodsService.Save(this.Goods);
                }
                else
                {
                    string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        throw new Exception(SystemNames.CONFIG_WEB_IMAGE_DIR + "不能为空");
                    }

                    if (oldId > 0 && oldId != v.Id)
                    {
                        //移动图片
                        string oldDir = dir + "\\" + this.Goods.ImageDir;
                        string newDir = dir + "\\goods\\" + v.Id.ToString() + "\\" + this.Goods.Number;
                        FileUtil.EnsureExits(new FileInfo(newDir));
                        Directory.Move(oldDir, newDir);
                        this.Goods.ImageDir = "\\goods\\" + v.Id.ToString() + "\\" + this.Goods.Number;
                    }
                    if (string.IsNullOrWhiteSpace(imagePath) == false && imagePath != this.Goods.Image)
                    {
                        GoodsService.SaveImage(this.Goods, this.imagePath);
                    }
                    else
                    {
                        this.Goods.Image = this.Goods.ImageDir + "\\index" + this.Goods.Image.Substring(this.Goods.Image.LastIndexOf("."));
                    }
                    this.goodsService.Update(this.Goods);
                }

                //上货店铺
                //已有的，没有在选择中的，需要删除
                var gus = this.Goods.Shops.ToArray();
                foreach (var s in gus)
                {
                    if (selectedShops.FirstOrDefault(obj => obj.Source.Id == s.ShopId) == null)
                    {
                        ServiceContainer.GetService<GoodsShopService>().Delete(s.Id);
                        this.Goods.Shops.Remove(s);
                    }
                }
                //从数据库读取一篇，防止其它人添加了
                gus = ServiceContainer.GetService<GoodsShopService>().GetByAll(this.Goods.Id, 0, 0).Datas.ToArray();
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