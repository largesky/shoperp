using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShopErp.App.Service.Restful;
using ShopErp.App.Service.Spider;
using ShopErp.App.Utils;
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;
using ShopErp.Domain.Pop;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// ChuchujieGoodsUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PopGoodsUserControl : UserControl
    {
        private Task task = null;
        private bool isStop = false;
        private bool myLoaded = false;
        private Shop lastShop = null;

        public PopGoodsUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                this.cbbShops.ItemsSource = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                this.cbbStatus.Bind<PopGoodsState>();
                this.cbbStatus.SelectedIndex = 1;
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
                if (this.task != null)
                {
                    this.isStop = true;
                    return;
                }
                string title = this.tbTitle.Text.Trim();
                string code = this.tbCode.Text.Trim();
                string stockCode = this.tbStockCode.Text.Trim();
                PopGoodsState state = this.cbbStatus.GetSelectedEnum<PopGoodsState>();
                this.lastShop = this.cbbShops.SelectedItem as Shop;
                if (this.lastShop == null)
                {
                    throw new Exception("未选择店铺");
                }
                this.task = Task.Factory.StartNew(() => QueryTask(this.lastShop, title, code, stockCode, state));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                var goods = tb.DataContext as PopGoodsInfoViewModel;

                if (goods == null)
                {
                    throw new Exception("Edit_Click错误，ChuchujieGoodsResponseGoods");
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (goods.SkuCodes == null || goods.SkuCodes.Length < 1)
                    {
                        throw new Exception("商品没有SKU无法打开Go2连接");
                    }
                    var stocks = goods.SkuCodes[0].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (stocks.Length != 2)
                    {
                        throw new Exception(goods.SkuCodes[0] + "不包含&无法分析");
                    }
                    var og = ServiceContainer.GetService<GoodsService>().ParseGoods(stocks[0], stocks[1]).First;
                    if (og == null)
                    {
                        throw new Exception("解析失败:" + goods.SkuCodes[0]);
                    }
                    Process.Start(og.Url);
                }
                else
                {
                    if (this.lastShop == null)
                    {
                        throw new Exception("未选择店铺");
                    }
                    string url = "";
                    if (this.lastShop.PopType == PopType.CHUCHUJIE)
                    {
                        url = "http://m.chuchujie.com/details/detail.html?id=" + goods.PopGoodsInfo.Id;
                    }
                    else if (this.lastShop.PopType == PopType.TAOBAO)
                    {
                        url = "https://item.taobao.com/item.htm?id=" + goods.PopGoodsInfo.Id;
                    }
                    else if (this.lastShop.PopType == PopType.TMALL)
                    {
                        url = "https://detail.tmall.com/item.htm?id=" + goods.PopGoodsInfo.Id;
                    }
                    else if (this.lastShop.PopType == PopType.PINGDUODUO)
                    {
                        url = "http://mobile.yangkeduo.com/goods.html?goods_id=" + goods.PopGoodsInfo.Id;
                    }
                    else
                    {
                        throw new Exception("不支持的平台");
                    }

                    Process.Start(url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenUrl_Click2(object sender, EventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb == null)
                {
                    throw new Exception("Edit_Click错误，事件源不是TextBlock");
                }
                var goods = tb.DataContext as PopGoodsInfoViewModel;
                string url = "";

                if (this.lastShop == null)
                {
                    throw new Exception("未选择店铺");
                }

                if (this.lastShop.PopType == PopType.CHUCHUJIE)
                {
                    url = string.Format(
                        "http://seller.chuchujie.com/sqe.php?s=/Product/view_edit/product_id/{0}/ctype/online",
                        goods.PopGoodsInfo.Id);
                }
                else if (this.lastShop.PopType == PopType.TAOBAO)
                {
                    url = string.Format("https://upload.taobao.com/auction/container/publish.htm?catId={0}&itemId={1}",
                        goods.PopGoodsInfo.CatId, goods.PopGoodsInfo.Id);
                }
                else if (this.lastShop.PopType == PopType.TMALL)
                {
                    url = "https://upload.tmall.com/auction/publish/edit.htm?auto=fals&itemNumId=" +
                          goods.PopGoodsInfo.Id;
                }
                else
                {
                    throw new Exception("不支持的平台");
                }

                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void IniButtonState(string msg, bool start)
        {
            if (start)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.tbMsg.Text = msg;
                    this.btnQuery.Content = "停止";
                    this.btnCheckNotSale.Content = "停止";
                }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.btnQuery.Content = "查询";
                    this.btnCheckNotSale.Content = "检查下架";
                }));
            }
        }

        private void QueryTask(Shop shop, string title, string code, string stockCode, PopGoodsState state)
        {
            List<PopGoods> goods = new List<PopGoods>();

            var titles = title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                this.IniButtonState("下载线程已开始执行", true);
                this.isStop = false;
                int pageIndex = 0;
                while (this.isStop == false)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => this.tbMsg.Text = "正在下载第:" + (pageIndex + 1) + "页"));
                    var ggs = ServiceContainer.GetService<GoodsService>().SearchPopGoods(shop, state, pageIndex, 50).Datas;
                    if (ggs == null || ggs.Count < 1)
                    {
                        break;
                    }

                    pageIndex++;
                    foreach (var v in ggs)
                    {
                        bool add = true;
                        if (string.IsNullOrWhiteSpace(code) == false)
                        {
                            add = v.Code != null && v.Code.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        if (string.IsNullOrWhiteSpace(stockCode) == false && add)
                        {
                            add = v.Skus == null ? false : v.Skus.FirstOrDefault(obj => obj.Code != null && obj.Code.IndexOf(stockCode, StringComparison.OrdinalIgnoreCase) >= 0) != null;
                        }
                        if (titles != null && titles.Length > 0 && add)
                        {
                            add = titles.All(o => v.Title != null && v.Title.Contains(o));
                        }
                        if (add)
                        {
                            goods.Add(v);
                        }
                    }
                    this.Dispatcher.BeginInvoke(new Action(() => this.tbMsg.Text = "已经下载:" + goods.Count));
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "调用失败:")
                    MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isStop = true;
                this.task = null;
                this.IniButtonState("", false);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var goodsTasks = ServiceContainer.GetService<GoodsTaskService>().GetByAll(shop.Id, 0, 0).Datas;
                    var goodsViewModels = goods.Select(obj => new PopGoodsInfoViewModel(obj)).ToArray();
                    for (int i = 0; i < goodsViewModels.Length; i++)
                    {
                        goodsViewModels[i].UserNumber = i + 1;
                        goodsViewModels[i].GoodsTask = goodsTasks.FirstOrDefault(obj => obj.ShopId == shop.Id && obj.GoodsId == goodsViewModels[i].PopGoodsInfo.Id);
                        if (goodsViewModels[i].GoodsTask == null)
                        {
                            goodsViewModels[i].GoodsTask = new GoodsTask
                            {
                                GoodsId = goods[i].Id,
                                ShopId = shop.Id,
                                Comment = "",
                                Id = 0,
                            };
                        }
                        goodsViewModels[i].SkuCodes = goodsViewModels[i].PopGoodsInfo.Skus.Select(obj => obj.Code).Distinct().ToArray();
                    }
                    var unSaved = goodsViewModels.Where(obj => obj.GoodsTask != null && obj.GoodsTask.Id < 1).Select(obj => obj.GoodsTask).ToArray();

                    foreach (var us in unSaved)
                    {
                        us.Id = ServiceContainer.GetService<GoodsTaskService>().Save(us);
                    }
                    foreach (var good in goodsViewModels)
                    {
                        good.IsDup = goodsViewModels.Any(obj => obj != good && obj.SkuCodes.Intersect(good.SkuCodes).Count() > 0);
                    }
                    this.dgvGoods.ItemsSource = goodsViewModels;
                    this.btnQuery.Content = "查询";
                    if (goodsViewModels == null || goodsViewModels.Length < 1)
                    {
                        MessageBox.Show("没有查询到任何数据");
                    }
                    this.tbMsg.Text = "总共下载到商品:" + (goodsViewModels.Length);
                    var first = goods.FirstOrDefault(obj => obj.Skus != null && obj.Skus.Any(o => string.IsNullOrWhiteSpace(o.Code)));
                    if (first != null)
                    {
                        MessageBox.Show("有商品库存编号为空:" + first.Id);
                    }
                    if (goodsViewModels.Any(obj => obj.IsDup))
                    {
                        MessageBox.Show("有商品重复");
                    }
                }));
            }
        }

        private void dgvGoods_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                var goods = e.Row.DataContext as PopGoodsInfoViewModel;
                goods.GoodsTask.Comment = (e.EditingElement as TextBox).Text.Trim();
                ServiceContainer.GetService<GoodsTaskService>().Update(goods.GoodsTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.Cancel = true;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    string msg = string.Format("您正重置备注，共提醒{0}次，当前第{1}次", 5, i + 1);
                    if (MessageBox.Show(msg, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk,
                            MessageBoxResult.No) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                var goods = this.dgvGoods.ItemsSource as PopGoodsInfoViewModel[];
                if (goods == null || goods.Length < 1)
                {
                    throw new Exception("没有需要重置的数据");
                }
                foreach (var g in goods)
                {
                    g.GoodsTask.Comment = this.tbComment.Text.Trim();
                    ServiceContainer.GetService<GoodsTaskService>().Update(g.GoodsTask);
                }
                this.dgvGoods.ItemsSource = null;
                this.dgvGoods.ItemsSource = goods;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCheckNotSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    if (this.task != null)
                    {
                        this.isStop = true;
                        return;
                    }
                    var popGoodsInfos = this.dgvGoods.ItemsSource as PopGoodsInfoViewModel[];
                    if (popGoodsInfos == null || popGoodsInfos.Length < 1)
                    {
                        MessageBox.Show("没有数据需要检查");
                        return;
                    }
                    foreach (var v in popGoodsInfos)
                    {
                        v.State = "";
                    }
                    this.task = Task.Factory.StartNew(() => CheckNotSaleTask(popGoodsInfos));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckNotSaleTask(PopGoodsInfoViewModel[] goodsInfos)
        {
            try
            {
                var gvms = CollectionsSpilteUtil.Spilte(goodsInfos, 10);
                this.isStop = false;
                this.IniButtonState("正在检查中....", true);
                Task.WaitAll(gvms.Select(obj => Task.Factory.StartNew(new Action(() => CheckNotSale(obj)))).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isStop = true;
                this.IniButtonState("正在检查中....", false);
                this.task = null;
                MessageBox.Show("检查完成");
            }
        }

        private void CheckNotSale(PopGoodsInfoViewModel[] goodsInfos)
        {
            string vendor = "";
            foreach (var goods in goodsInfos)
            {
                try
                {
                    if (goods.SkuCodes == null || goods.SkuCodes.Length < 1)
                    {
                        throw new Exception("商品没有SKU");
                    }
                    var stocks = goods.SkuCodes[0].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (stocks.Length != 2)
                    {
                        throw new Exception(goods.SkuCodes[0] + "不包含&无法分析");
                    }
                    Debug.WriteLine("vendor " + stocks[0] + ", number " + stocks[1]);
                    var og = ServiceContainer.GetService<GoodsService>().ParseGoods(stocks[0], stocks[1]).First;
                    if (og == null)
                    {
                        throw new Exception("解析失败:" + goods.SkuCodes[0]);
                    }
                    if (string.IsNullOrWhiteSpace(og.Url))
                    {
                        throw new Exception("商品网址为空");
                    }
                    var g = SpiderBase.CreateSpider(og.Url, 80, 0).GetGoodsInfoByUrl(og.Url, ref vendor, true);
                    if (g == null)
                    {
                        throw new Exception("商品不存在");
                    }
                    this.Dispatcher.BeginInvoke(new Action(() => goods.State = "正常"));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => goods.State = ex.Message));
                }
            }
        }
    }
}