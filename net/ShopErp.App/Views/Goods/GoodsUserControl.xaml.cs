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
using System.Threading.Tasks;
using WebSocketSharp.Frame;
using ShopErp.App.Views.AttachUI;
using ShopErp.App.Domain.TaobaoHtml.Image;
using NPOI.SS.Formula.Functions;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodsProcess.xaml
    /// </summary>
    public partial class GoodsUserControl : UserControl
    {
        private bool myLoaded = false;
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        private List<GoodsViewModel> goodsViewModels = new List<GoodsViewModel>();
        private Task uploadImageTask = null;
        private bool isStop;

        public GoodsUserControl()
        {
            InitializeComponent();
        }
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
                var shippers = ServiceContainer.GetService<GoodsService>().GetAllShippers().Datas;
                shippers.Insert(0, "");
                this.cbbShippers.ItemsSource = shippers;
                cbbDisplayType_SelectionChanged(null, null);
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string[] GetImageFile(string dir)
        {
            var files = System.IO.Directory.GetFiles(dir);
            return files.Where(obj => obj.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || obj.EndsWith("bmp", StringComparison.OrdinalIgnoreCase) || obj.EndsWith("png", StringComparison.OrdinalIgnoreCase) || obj.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        private void CheckPt(GoodsViewModel goodsViewModel)
        {
            string webDir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
            string pt = webDir + "\\" + goodsViewModel.Source.ImageDir + "\\PT";
            if (System.IO.Directory.Exists(pt) == false)
            {
                throw new Exception(goodsViewModel.Source.Number + " PT 文件夹不存在");
            }
            string zt = pt + "\\ZT";
            if (System.IO.Directory.Exists(zt) == false)
            {
                throw new Exception(goodsViewModel.Source.Number + " ZT 文件夹不存在");
            }
            var files = GetImageFile(zt);
            if (files.Length < 1)
            {
                throw new Exception(goodsViewModel.Source.Number + " ZT 下面没有图片文件");
            }
            foreach (var file in files)
            {
                int lastSlashIndex = file.LastIndexOf('\\');
                int lastDotIndex = file.LastIndexOf('.');
                string name = file.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);
                if (name.All(c => Char.IsDigit(c)))
                {
                    Size size = GetImageFileSize(file);
                    if (size.Width != size.Height || size.Width > 800 || (new FileInfo(file)).Length > 500 * 1024)
                    {
                        throw new Exception(file + "长宽不相等，或者长宽超过800，或者大小超过500KB是否继续");
                    }
                }
            }

            string yst = pt + "\\YST";
            if (System.IO.Directory.Exists(yst) == false)
            {
                throw new Exception(goodsViewModel.Source.Number + " YST 文件夹不存在");
            }
            files = GetImageFile(yst);
            if (files.Length < 1)
            {
                throw new Exception(goodsViewModel.Source.Number + " YST下面没有颜色图");
            }
            foreach (var file in files)
            {
                Size size = GetImageFileSize(file);
                if (size.Width != size.Height || size.Width > 800 || (new FileInfo(file)).Length > 500 * 1024)
                {
                    throw new Exception(file + "长宽不相等，或者长宽超过800，或者大小超过500KB");
                }
            }

            string xqt = pt + "\\XQT";
            if (System.IO.Directory.Exists(yst) == false)
            {
                throw new Exception(goodsViewModel.Source.Number + " XQT 文件夹不存在");
            }
            files = GetImageFile(xqt);
            if (files.Length < 1)
            {
                throw new Exception(goodsViewModel.Source.Number + "XQT下面没有详情图");
            }
            foreach (var file in files)
            {
                Size size = GetImageFileSize(file);
                if (size.Width > 790 || size.Height > 1500)
                {
                    throw new Exception(file + "宽超过790 或者高超过1500");
                }
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
                this.pb1.Parameters.Add("Start", this.dpStart.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpStart.Value.Value);
                this.pb1.Parameters.Add("End", this.dpEnd.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpEnd.Value.Value);
                this.pb1.Parameters.Add("Vendor", this.tbVendor.Text.Trim());
                this.pb1.Parameters.Add("Number", this.tbNumber.Text.Trim());
                this.pb1.Parameters.Add("Type", this.cbbTypes.GetSelectedEnum<GoodsType>());
                this.pb1.Parameters.Add("Comment", this.tbComment.Text.Trim());
                this.pb1.Parameters.Add("Order", (this.cbbSortType.SelectedItem as ComboBoxItem).Tag.ToString());
                this.pb1.Parameters.Add("Flag", this.cbbFlags.GetSelectedEnum<ColorFlag>());
                this.pb1.Parameters.Add("VideoType", this.cbbVideoTypes.GetSelectedEnum<GoodsVideoType>());
                this.pb1.Parameters.Add("VendorAdd", this.cbbVendorAdds.Text.Trim());
                this.pb1.Parameters.Add("Shipper", this.cbbShippers.Text.Trim());
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
                    e.GetParameter<string>("VendorAdd"),
                    e.GetParameter<string>("Shipper"),
                    e.CurrentPage - 1,
                    e.PageSize);
                this.pb1.Total = data.Total;
                this.pb1.CurrentCount = data.Datas.Count;
                this.goodsViewModels.Clear();
                this.goodsViewModels.AddRange(data.Datas.Select(obj => new GoodsViewModel(obj)));
                this.dgvGoods.ItemsSource = null;
                this.dgvGoods.ItemsSource = this.goodsViewModels;
                this.lstGoods.ItemsSource = null;
                this.lstGoods.ItemsSource = this.goodsViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private Size GetImageFileSize(string file)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(File.ReadAllBytes(file));
            image.EndInit();
            return new Size(image.PixelWidth, image.PixelHeight);
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

                string webDir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");

                //检查图片状态
                if (from == GoodsState.WAITPROCESSIMAGE && to == GoodsState.WAITREVIEW)
                {
                    //YT
                    string yt = webDir + "\\" + gu.Source.ImageDir + "\\YT";
                    if (System.IO.Directory.Exists(yt) == false)
                    {
                        if (MessageBox.Show("YT 文件夹不存，是否继续", "错误", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (System.IO.Directory.GetFiles(yt).Length == 0 && System.IO.Directory.GetDirectories(yt).Length == 0)
                        {
                            if (MessageBox.Show("YT 文件夹下面为空，是否继续", "错误", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }
                    }
                    CheckPt(gu);
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
                            v.UploadTime = Utils.DateTimeUtil.DbMinTime;
                            v.UploadOperator = "";
                        }
                        if (v.State == GoodsState.WAITPROCESSIMAGE)
                        {
                            v.ProcessImageTime = Utils.DateTimeUtil.DbMinTime;
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

        private void btnCreatePrivate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var w = new GoodsCreateWindow();
                w.Show();
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
                GoodsPatchEditWindow win = new GoodsPatchEditWindow { Goods = this.goodsViewModels.ToArray(), Shop = this.cbbShops.SelectedItem as Shop };
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
                var w = new GoodsMapWindow { GoodsId = gu.Source.Id };
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

        private void MiCopyVendorNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var goods = this.GetSelctedItem().Source;
                var vendor = ServiceContainer.GetService<VendorService>().GetById(goods.VendorId);
                if (string.IsNullOrWhiteSpace(vendor.PingyingName))
                {
                    throw new Exception("厂家没有配置拼音");
                }
                System.Windows.Forms.Clipboard.SetText(vendor.PingyingName + "&" + goods.Number);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MiCopyPopNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var goods = this.GetSelctedItem().Source;
                var vendor = ServiceContainer.GetService<VendorService>().GetById(goods.VendorId);
                System.Windows.Forms.Clipboard.SetText(vendor.Id.ToString("D4") + (goods.Number.Length < 3 ? goods.Number.PadRight(3, '0') : goods.Number.Substring(goods.Number.Length - 3)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MiCopyYstPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var goods = this.GetSelctedItem().Source;
                var vendor = ServiceContainer.GetService<VendorService>().GetById(goods.VendorId);
                string dir = goods.ImageDir;
                string webdir = LocalConfigService.GetValue(ShopErp.Domain.SystemNames.CONFIG_WEB_IMAGE_DIR);
                if (string.IsNullOrWhiteSpace(webdir))
                {
                    throw new Exception("没有配置网络图片路径，请在系统中配置");
                }
                string ystDir = System.IO.Path.Combine(webdir, dir) + "\\PT\\YST";
                System.Windows.Forms.Clipboard.SetText(ystDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.uploadImageTask != null)
                {
                    this.isStop = true;
                    return;
                }
                if (this.goodsViewModels.Count < 1)
                {
                    MessageBox.Show("没有需要上传图片的商品");
                    return;
                }

                foreach (var v in this.goodsViewModels)
                {
                    CheckPt(v);
                }
                var tu = MainWindow.ProgramMainWindow.QueryUserControlInstance<AttachUI.Taobao.TaobaoUserControl>();
                var shop = tu.GetLoginShop();
                if (shop == null)
                {
                    MessageBox.Show("未登录店铺，请打开-外接窗口-淘宝登录，进行登录");
                    return;
                }
                if (MessageBox.Show("是否要上传到店铺：" + shop.PopShopName, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                this.uploadImageTask = Task.Factory.StartNew(new Action(() => UploadImage(shop)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UploadImage(Shop shop)
        {
            try
            {
                var tu = MainWindow.ProgramMainWindow.QueryUserControlInstance<AttachUI.Taobao.TaobaoUserControl>();
                string webDir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR);
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUploadImage.Content = "停止传图"));
                this.isStop = false;

                //获取商品图片文件夹信息
                var goodsImageDir = tu.GetImageDirRsp().module.dirs.children.FirstOrDefault(obj => obj.name == "商品图片");
                if (goodsImageDir == null)
                {
                    throw new Exception("没有找到名称为 商品图片 的文件夹，请检查创建");
                }

                foreach (var gv in this.goodsViewModels)
                {
                    if (this.isStop)
                    {
                        break;
                    }
                    string dirId = "";
                    string dirName = ServiceContainer.GetService<VendorService>().GetVendorPingyingName(gv.Source.VendorId) + "&" + gv.Source.Number;
                    var dir = goodsImageDir.children.FirstOrDefault(obj => obj.name.Equals(dirName, StringComparison.OrdinalIgnoreCase));
                    ImageFileRspModuleFile[] existFiles = null;
                    if (dir != null)
                    {
                        //检查下面是否有文件
                        var filesRsp = tu.GetImageFileRsp(dir.id.ToString());
                        existFiles = filesRsp.module.file_module;
                        dirId = dir.id;
                    }
                    else
                    {
                        var rsp = tu.AddDir(goodsImageDir.id.ToString(), dirName);
                        if (rsp.jsonData == null || string.IsNullOrWhiteSpace(rsp.jsonData.id))
                        {
                            this.Dispatcher.BeginInvoke(new Action(() => gv.UploadState = "创建文件夹失败:" + rsp.message));
                            continue;
                        }
                        dirId = rsp.jsonData.id;
                    }
                    List<string> files = new List<string>();
                    List<FileInfo> fileToUpload = new List<FileInfo>();
                    string pt = webDir + "\\" + gv.Source.ImageDir + "\\PT";
                    files.AddRange(GetImageFile(pt + "\\zt"));
                    if (shop.PopType != PopType.TMALL)
                        files.AddRange(GetImageFile(pt + "\\yst"));
                    files.AddRange(GetImageFile(pt + "\\xqt"));

                    string matchInfo = "";
                    foreach (var f in files)
                    {
                        FileInfo fi = new FileInfo(f);
                        var ef = existFiles == null ? null : existFiles.FirstOrDefault(obj => obj.name.Equals(fi.Name, StringComparison.OrdinalIgnoreCase));
                        if (ef != null)
                        {
                            if (fi.Length != ef.sizes)
                                matchInfo += fi.Name + "本地图片与空间图片大小不匹配，未上传，请手动处理";
                        }
                        else
                        {
                            fileToUpload.Add(fi);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(matchInfo) == false)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => gv.UploadState = "创建文件夹失败:" + matchInfo));
                        continue;
                    }
                    if (fileToUpload.Count < 1)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => gv.UploadState = "所有文件与网站上大小一致没有上传"));
                        continue;
                    }

                    foreach (var fi in fileToUpload)
                    {
                        var rsp = tu.AddFile(dirId, fi);
                        if (rsp.success == false)
                        {
                            throw new Exception("上传图片失败:" + fi.FullName + rsp.message);
                        }
                        this.Dispatcher.BeginInvoke(new Action(() => gv.UploadState = "已上传:" + fi.Name));
                    }
                    this.Dispatcher.BeginInvoke(new Action(() => gv.UploadState = "上传成功"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.uploadImageTask = null;
                this.isStop = true;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUploadImage.Content = "批量传图"));
            }
        }



    }
}