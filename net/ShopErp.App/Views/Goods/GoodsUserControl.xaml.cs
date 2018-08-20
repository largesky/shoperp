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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.Views.Extenstions;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodsProcess.xaml
    /// </summary>
    public partial class GoodsUserControl : UserControl
    {
        private bool myLoaded = false;
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        private List<GoodsViewModel> models = new List<GoodsViewModel>();

        public GoodsUserControl()
        {
            InitializeComponent();
        }

        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myLoaded)
                {
                    return;
                }
                this.cbbStates.Bind<GoodsState>();
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).ToList();
                shops.Insert(0, new Shop { Mark = "", Id = 0 });
                shops.Insert(1, new Shop { Mark = "无", Id = -1 });
                this.cbbShops.ItemsSource = shops;
                this.cbbShops.SelectedIndex = 0;
                this.cbbTypes.Bind<GoodsType>();
                this.cbbFlags.Bind<ColorFlag>();
                this.cbbVideoTypes.Bind<GoodsVideoType>();
                cbbDisplayType_SelectionChanged(null, null);
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.pb1.Parameters.Clear();
                this.pb1.Parameters.Add("ShopId", (this.cbbShops.SelectedItem as Shop).Id);
                this.pb1.Parameters.Add("State", this.cbbStates.GetSelectedEnum<GoodsState>());
                this.pb1.Parameters.Add("TimeType", this.cbbTimeType.SelectedIndex);
                this.pb1.Parameters.Add("Start", this.dpStart.Value == null ? DateTime.MinValue : this.dpStart.Value.Value);
                this.pb1.Parameters.Add("End", this.dpEnd.Value == null ? DateTime.MinValue : this.dpEnd.Value.Value);
                this.pb1.Parameters.Add("Vendor", this.tbVendor.Text.Trim());
                this.pb1.Parameters.Add("Number", this.tbNumber.Text.Trim());
                this.pb1.Parameters.Add("Type", this.cbbTypes.GetSelectedEnum<GoodsType>());
                this.pb1.Parameters.Add("Comment", this.tbComment.Text.Trim());
                this.pb1.Parameters.Add("Order", (this.cbbSortType.SelectedItem as ComboBoxItem).Tag.ToString());
                this.pb1.Parameters.Add("Flag", this.cbbFlags.GetSelectedEnum<ColorFlag>());
                this.pb1.Parameters.Add("VideoType", this.cbbVideoTypes.GetSelectedEnum<GoodsVideoType>());
                this.pb1.StartPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void pb1_PageChanging(object sender, PageBar.PageChangeEventArgs e)
        {
            try
            {
                var data = ServiceContainer.GetService<GoodsService>().GetByAll(e.GetParameter<long>("ShopId"),
                    e.GetParameter<GoodsState>("State"),
                    e.GetParameter<int>("TimeType"),
                    e.GetParameter<DateTime>("Start"),
                    e.GetParameter<DateTime>("End"),
                    e.GetParameter<string>("Vendor"),
                    e.GetParameter<string>("Number"),
                    e.GetParameter<GoodsType>("Type"),
                    e.GetParameter<string>("Comment"),
                    e.GetParameter<ColorFlag>("Flag"),
                    e.GetParameter<GoodsVideoType>("VideoType"),
                    e.GetParameter<string>("Order"),
                    e.CurrentPage - 1,
                    e.PageSize);
                this.pb1.Total = data.Total;
                this.pb1.CurrentCount = data.Datas.Count;
                this.models.Clear();
                this.models.AddRange(data.Datas.Select(obj => new GoodsViewModel(obj)));
                this.SortData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SortData()
        {
            var sc = this.cbbSortType.SelectedItem as ComboBoxItem;
            if (sc == null)
            {
                this.dgvGoods.ItemsSource = null;
                this.dgvGoods.ItemsSource = this.models;
                this.lstGoods.ItemsSource = null;
                this.lstGoods.ItemsSource = this.models;
                return;
            }

            string sortType = sc.Tag.ToString();
            string[] sts = sortType.Split(new char[] { '-', ' ', }, StringSplitOptions.RemoveEmptyEntries);
            if (sts.Length != 2)
            {
                throw new Exception("排序类型:" + sc.Content.ToString() + "不正确");
            }

            List<GoodsViewModel> nms = null;

            if (sts[1].Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                if (sts[0].Equals("CreateTime", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderBy(obj => obj.Source.CreateTime).ToList();
                }
                else if (sts[0].Equals("Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderBy(obj => ServiceContainer.GetService<VendorService>()
                        .GetVendorPingYingFirstChar(obj.Source.VendorId)).ToList();
                }
                else if (sts[0].Equals("Type", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderBy(obj => obj.Source.Type).ToList();
                }
                else if (sts[0].Equals("Price", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderBy(obj => obj.Source.Price).ToList();
                }
                else if (sts[0].Equals("State", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderBy(obj => obj.Source.Shops.Count > 0
                        ? obj.Source.Shops[0].State
                        : GoodsState.NONE).ToList();
                }
                else if (sts[0].Equals("Star", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderBy(obj => obj.Source.Star).ToList();
                }
                else
                {
                    throw new Exception("无法支持的排序类型:" + sortType);
                }
            }
            else
            {
                if (sts[0].Equals("CreateTime", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderByDescending(obj => obj.Source.CreateTime).ToList();
                }
                else if (sts[0].Equals("Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderByDescending(obj => ServiceContainer.GetService<VendorService>()
                        .GetVendorPingYingFirstChar(obj.Source.VendorId)).ToList();
                }
                else if (sts[0].Equals("Type", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderByDescending(obj => obj.Source.Type).ToList();
                }
                else if (sts[0].Equals("Price", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderByDescending(obj => obj.Source.Price).ToList();
                }
                else if (sts[0].Equals("State", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models
                        .OrderByDescending(obj => obj.Source.Shops.Count > 0
                            ? obj.Source.Shops[0].State
                            : GoodsState.NONE).ToList();
                }
                else if (sts[0].Equals("Star", StringComparison.OrdinalIgnoreCase))
                {
                    nms = this.models.OrderByDescending(obj => obj.Source.Star).ToList();
                }
                else
                {
                    throw new Exception("无法支持的排序类型:" + sortType);
                }
            }
            this.models = nms;
            this.dgvGoods.ItemsSource = null;
            this.dgvGoods.ItemsSource = this.models;
            this.lstGoods.ItemsSource = null;
            this.lstGoods.ItemsSource = this.models;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new GoodsCreateSampleWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private GoodsViewModel GetSelctedItem()
        {
            if (this.cbbDisplayType.SelectedIndex == 0)
            {
                if (this.dgvGoods.SelectedCells.Count < 1)
                {
                    return null;
                }
                return this.dgvGoods.SelectedCells[0].Item as GoodsViewModel;
            }
            else
            {
                if (this.lstGoods.SelectedItem == null)
                {
                    return null;
                }
                return this.lstGoods.SelectedItem as GoodsViewModel;
            }
        }

        private void OpenUrl_Click(object sender, EventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb == null)
                {
                    throw new Exception("Edit_Click错误，事件源不是TextBlock");
                }
                var goods = tb.DataContext as GoodsViewModel;
                Process.Start(goods.Source.Url.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenDir_Click(object sender, EventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb == null)
                {
                    throw new Exception("Edit_Click错误，事件源不是TextBlock");
                }
                OpenImageDir(tb.DataContext as GoodsViewModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var w = new GoodUpdateWindow();
                w.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gu = this.GetSelctedItem();
                if (gu == null)
                {
                    return;
                }
                if (MessageBox.Show("是否删除:" + gu.Source.Number, "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                {
                    return;
                }
                var tmp = gu.Source.Shops.ToArray();
                foreach (var v in tmp)
                {
                    ServiceContainer.GetService<GoodsShopService>().Delete(v.Id);
                }
                gu.Source.Shops = null;
                ServiceContainer.GetService<GoodsService>().Delete(gu.Source.Id);
                Directory.Delete(System.IO.Path.Combine(LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR), gu.Source.ImageDir), true);
                MessageBox.Show("删除成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void miProcesscedImg_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeState(GoodsState.WAITPROCESSIMAGE, GoodsState.WAITREVIEW);
        }

        private void miReviewed_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeState(GoodsState.WAITREVIEW, GoodsState.WAITUPLOADED);
        }

        private void miUploadedToShop_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeState(GoodsState.WAITUPLOADED, GoodsState.UPLOADED);
        }

        private void miUnuploadedToShop_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeState(GoodsState.UPLOADED, GoodsState.NOTSALE);
        }

        private void ChangeState(GoodsState from, GoodsState to)
        {
            try
            {
                var gu = this.GetSelctedItem();
                if (gu == null)
                {
                    return;
                }

                if (gu.Source.Shops == null || gu.Source.Shops.Count < 1)
                {
                    throw new Exception("没有相关的上货店铺");
                }

                var win = new GoodsShopSelectWindow
                {
                    Goods = gu.Source,
                    InitSelectedShopIds = gu.Source.Shops.Where(obj => obj.State == to).Select(obj => obj.ShopId).ToArray()
                };
                var ret = win.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }

                int afCount = 0;

                foreach (var v in gu.Source.Shops)
                {
                    bool isChecked = win.SelectedShops.Contains(v.ShopId);

                    if (v.State == from && isChecked)
                    {
                        v.State = to;
                        if (v.State == GoodsState.UPLOADED)
                        {
                            v.UploadTime = DateTime.Now;
                            v.UploadOperator = OperatorService.LoginOperator.Number;
                        }
                        if (v.State == GoodsState.WAITREVIEW)
                        {
                            v.ProcessImageTime = DateTime.Now;
                            v.ProcessImageOperator = OperatorService.LoginOperator.Number;
                        }
                        afCount++;
                    }

                    //撤消回滚状态
                    if (v.State == to && isChecked == false)
                    {
                        v.State = from;
                        if (v.State == GoodsState.WAITREVIEW)
                        {
                            v.UploadTime = ServiceContainer.GetService<GoodsService>().GetDBMinTime();
                            v.UploadOperator = "";
                        }
                        if (v.State == GoodsState.WAITPROCESSIMAGE)
                        {
                            v.ProcessImageTime = ServiceContainer.GetService<GoodsService>().GetDBMinTime();
                            v.ProcessImageOperator = "";
                        }
                        afCount++;
                    }
                    ServiceContainer.GetService<GoodsShopService>().Update(v);
                }
                if (afCount > 0)
                {
                    MessageBox.Show("已标记成功");
                }
                else
                {
                    MessageBox.Show("未更新任何数据");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbDisplayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.dgvGoods == null || this.lstGoods == null)
            {
                return;
            }
            if (this.cbbDisplayType.SelectedIndex == 0)
            {
                this.gHost.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                this.gHost.RowDefinitions[2].Height = GridLength.Auto;
                this.lstGoods.Visibility = System.Windows.Visibility.Collapsed;
                this.dgvGoods.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.gHost.RowDefinitions[1].Height = GridLength.Auto;
                this.gHost.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                this.lstGoods.Visibility = System.Windows.Visibility.Visible;
                this.dgvGoods.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void imgButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FrameworkElement btn = sender as FrameworkElement;
                if (btn == null)
                {
                    MessageBox.Show("事件源不是FrameworkElement");
                    return;
                }

                var gu = btn.DataContext as GoodsViewModel;
                if (gu == null)
                {
                    MessageBox.Show("事件源DataContext不是GoodsViewModel");
                    return;
                }

                if (string.IsNullOrWhiteSpace(gu.Source.Url))
                {
                    MessageBox.Show("商品外部网址为空");
                    return;
                }
                Process.Start(gu.Source.Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbSortType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.models == null || this.models.Count < 1)
            {
                return;
            }
            this.SortData();
        }

        private void star_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var gu = this.GetSelctedItem();
                var mi = sender as MenuItem;
                int star = int.Parse(mi.Tag.ToString());
                gu.Source.Star = star;
                ServiceContainer.GetService<GoodsService>().Update(gu.Source);
                gu.UpdateStarViewModel(star);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreatePrivate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var w = new GoodsCreateWindow();
                w.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPatchEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GoodsPatchEditWindow win = new GoodsPatchEditWindow { Goods = this.models.ToArray() };
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gu = this.GetSelctedItem();
                if (gu == null)
                {
                    return;
                }
                var w = new GoodsCreateWindow { Goods = gu.Source };
                var ret = w.ShowDialog();
                if (ret != null && ret.Value)
                {
                    gu.Comment = gu.Source.Comment;
                    gu.UpdateStarViewModel(gu.Source.Star);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void miEditFlagAndComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gu = this.GetSelctedItem();
                if (gu == null)
                {
                    return;
                }
                EditGoodsCommentAndFlag(gu);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void miEditGoodsMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gu = this.GetSelctedItem();
                if (gu == null)
                {
                    return;
                }
                var w = new GoodsEditMapWindow { GoodsId = gu.Source.Id };
                w.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void miOpenDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = this.GetSelctedItem();
                if (vm == null)
                {
                    throw new Exception("没有选择相应的商品");
                }
                OpenImageDir(vm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            this.EditGoodsCommentAndFlag(row.DataContext as GoodsViewModel);
        }

        private void EditGoodsCommentAndFlag(GoodsViewModel goods)
        {
            var win = new GoodsCommentEditWindow { Flag = goods.Flag, Comment = goods.Comment };
            if (win.ShowDialog().Value == false)
            {
                return;
            }
            goods.Source.Flag = win.Flag;
            goods.Source.Comment = win.Comment;
            ServiceContainer.GetService<GoodsService>().Update(goods.Source);
            goods.Flag = win.Flag;
            goods.Comment = win.Comment;
        }

        private void Image_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (sender as Image);
            GoodsViewModel gu = img.Tag as GoodsViewModel;
            if (e.LeftButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Left)
            {
                EditGoodsCommentAndFlag(gu);
            }
            e.Handled = true;
        }

        private void dgvGoods_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                var goods = e.Row.DataContext as GoodsViewModel;
                goods.Source.Comment = (e.EditingElement as TextBox).Text.Trim();
                ServiceContainer.GetService<GoodsService>().Update(goods.Source);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.Cancel = true;
            }
        }

        private void miDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = this.GetSelctedItem();
                if (sel == null)
                {
                    return;
                }
                new GoodsDetailWindow { Goods = sel.Source }.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenImageDir(GoodsViewModel goodsViewModel)
        {
            if (string.IsNullOrWhiteSpace(goodsViewModel.Source.ImageDir))
            {
                return;
            }
            string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR) + "\\" + goodsViewModel.Source.ImageDir.Trim();
            if (System.IO.Directory.Exists(dir) == false)
            {
                throw new Exception("文件夹不存在或者被删除:" + dir);
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    var d = System.IO.Path.Combine(dir, "PT");
                    if (System.IO.Directory.Exists(d) == false)
                    {
                        MessageBox.Show("文件夹不存在或者被删除:" + d);
                    }
                    else
                    {
                        Process.Start(d);
                    }
                }
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    var d = System.IO.Path.Combine(dir, "YT");
                    if (System.IO.Directory.Exists(d) == false)
                    {
                        MessageBox.Show("文件夹不存在或者被删除:" + d);
                    }
                    else
                    {
                        Process.Start(d);
                    }
                }
            }
            else
            {
                Process.Start(dir);
            }
        }

        private void miCreateBoxImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new GoodsBoxImageWindow { Goods = this.GetSelctedItem().Source }.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}