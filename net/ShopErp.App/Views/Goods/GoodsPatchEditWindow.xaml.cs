
using System;
using System.Collections.Generic;
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
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// GoodsPatchEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GoodsPatchEditWindow : Window
    {
        public GoodsViewModel[] Goods { get; set; }

        public Shop Shop { get; set; }

        public GoodsPatchEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tbMode.Text = "当前设置模式:" + ((this.Goods != null && this.Goods.Length > 0) ? "部分" : "所有");
                this.cbbEditFlag.Bind<ColorFlag>();
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                this.cbbShop.ItemsSource = shops;
                if (shops != null && shops.Count > 0)
                {
                    this.cbbShop.SelectedIndex = 0;
                }
                this.cbbState.Bind<GoodsState>();
                this.cbbShippers.ItemsSource = ServiceContainer.GetService<GoodsService>().GetAllShippers().Datas;
                if (OperatorService.LoginOperator.Rights.Contains("批量管理商品") == false)
                {
                    this.IsEnabled = false;
                    this.DialogResult = false;
                    MessageBox.Show("你没有权限");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEditFlag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorFlag flag = this.cbbEditFlag.GetSelectedEnum<ColorFlag>();
                if (MessageBox.Show("是否将旗帜更新为:" + this.cbbEditFlag.Text, "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                var goods = this.GetGoodss();
                foreach (var v in goods)
                {
                    if (v.Source.Flag != flag)
                    {
                        v.Source.Flag = flag;
                        v.Flag = flag;
                        ServiceContainer.GetService<GoodsService>().Update(v.Source);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEditStar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int star = int.Parse(this.tbStar.Text.Trim());
                if (MessageBox.Show("是否将星级更新为:" + star, "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                var goods = this.GetGoodss();
                foreach (var v in goods)
                {
                    if (v.Source.Star != star)
                    {
                        v.Source.Star = star;
                        ServiceContainer.GetService<GoodsService>().Update(v.Source);
                        v.UpdateStarViewModel(star);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private GoodsViewModel[] GetGoodss()
        {
            if (this.Goods != null && this.Goods.Length > 0)
            {
                return this.Goods;
            }

            var data = ServiceContainer.GetService<GoodsService>().GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", "", "", 0, 0).Datas;
            return data.Select(obj => new GoodsViewModel(obj)).ToArray();
        }

        private void btnDelShop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shop = this.cbbShop.SelectedItem as Shop;
                if (shop == null)
                {
                    throw new Exception("请选择店铺");
                }

                if (OperatorService.LoginOperator.Rights.Contains("批量管理商品") == false)
                {
                    throw new Exception("你没有权限");
                }

                if (MessageBox.Show("是否为店铺:" + shop.Mark + " 删除所有商品?", "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                if (MessageBox.Show("是否为店铺:" + shop.Mark + " 删除所有商品?", "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var goods = this.GetGoodss().Select(obj => obj.Source).ToArray();
                var ss = ServiceContainer.GetService<GoodsShopService>();
                foreach (var g in goods)
                {
                    if (g.Shops != null && g.Shops.FirstOrDefault(obj => obj.ShopId == shop.Id) != null)
                    {
                        ss.Delete(g.Shops.FirstOrDefault(obj => obj.ShopId == shop.Id).Id);
                    }
                }
                MessageBox.Show("删除完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddShop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shop = this.cbbShop.SelectedItem as Shop;
                if (shop == null)
                {
                    throw new Exception("请选择店铺");
                }
                if (OperatorService.LoginOperator.Rights.Contains("批量管理商品") == false)
                {
                    throw new Exception("你没有权限");
                }

                if (MessageBox.Show("是否为店铺:" + shop.Mark + " 添加所有商品?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                if (MessageBox.Show("是否为店铺:" + shop.Mark + " 添加所有商品?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var dbMinTime = ServiceContainer.GetService<GoodsShopService>().GetDBMinTime();
                var goods = this.GetGoodss().Select(obj => obj.Source).ToArray();
                var ss = ServiceContainer.GetService<GoodsShopService>();
                foreach (var g in goods)
                {
                    if (g.Shops != null && g.Shops.FirstOrDefault(obj => obj.ShopId == shop.Id) == null)
                    {
                        var gus = new GoodsShop
                        {
                            GoodsId = g.Id,
                            ShopId = shop.Id,
                            SalePrice = 0,
                            State = GoodsState.WAITPROCESSIMAGE,
                            PopGoodsId = "",
                            ProcessImageOperator = "",
                            ProcessImageTime = dbMinTime,
                            UploadOperator = "",
                            UploadTime = dbMinTime
                        };
                        g.Shops.Add(gus);
                        ss.Save(gus);
                    }
                }
                MessageBox.Show("添加完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnSetState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Shop == null)
                {
                    throw new Exception("批量设置状态必须设置店铺");
                }
                if (this.tbMode.Text.Contains("所有"))
                {
                    throw new Exception("批量设置状态不能是所有");
                }

                var state = this.cbbState.GetSelectedEnum<GoodsState>();
                if (state == GoodsState.NONE)
                {
                    throw new Exception("批量设置状态不能为 NONE");
                }

                if (MessageBox.Show("是否为店铺:" + Shop.Mark + " 批量设置状态?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                if (MessageBox.Show("是否为店铺:" + Shop.Mark + " 批量设置状态?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var g in this.Goods)
                {
                    var gs = g.Source.Shops.FirstOrDefault(obj => obj.ShopId == this.Shop.Id);
                    if (gs == null)
                    {
                        continue;
                    }
                    gs.State = state;
                    ServiceContainer.GetService<GoodsShopService>().Update(gs);
                }
                MessageBox.Show("更新完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnSetShipper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string shipper = this.cbbShippers.Text.Trim();
                if (MessageBox.Show("是否将发货仓库设置成：" + shipper, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                foreach (var g in this.GetGoodss())
                {
                    g.Source.Shipper = shipper;
                    ServiceContainer.GetService<GoodsService>().Update(g.Source);
                }
                MessageBox.Show("更新完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}