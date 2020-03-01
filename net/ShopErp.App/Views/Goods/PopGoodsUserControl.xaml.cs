using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Domain.TaobaoHtml.Goods;
using ShopErp.App.Service;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using ShopErp.App.Service.Spider;
using ShopErp.App.Utils;
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;
using ShopErp.Domain.Common;
using ShopErp.Domain.Pop;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// ChuchujieGoodsUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PopGoodsUserControl : UserControl
    {
        string jspath = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA + "\\TAOBAOJS.js");
        private GoodsDownloadWorker goodsDownloadWorker = null;
        private bool myLoaded = false;
        private Shop lastShop = null;
        private bool isStop;
        private Task task;
        private System.Collections.ObjectModel.ObservableCollection<PopGoodsInfoViewModel> popGoodsInfoViewModels = new System.Collections.ObjectModel.ObservableCollection<PopGoodsInfoViewModel>();

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
                this.dgvGoods.ItemsSource = this.popGoodsInfoViewModels;
                this.cbbPddShops.ItemsSource = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled && obj.PopType == PopType.PINGDUODUO).ToArray();
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetUiMsg(Button btn, string msg, bool running, string runningmsg, string stopmsg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                btn.Content = running ? runningmsg : stopmsg;
                this.cbbShops.IsEnabled = !running;
                this.tbMsg.Text = msg;
            }));
        }

        private void AppendDataToUi(List<PopGoods> goods, Shop shop, string title, string code, string stockCode, PopGoodsState state)
        {
            List<PopGoods> toAdd = new List<PopGoods>();
            string[] titles = title.Split(' ');

            //第一步过滤
            foreach (var v in goods)
            {
                if (this.popGoodsInfoViewModels.Any(obj => obj.PopGoodsInfo.Id == v.Id))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(code) == false && v.Code != code)
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(stockCode) == false && v.Skus != null && v.Skus.Any(obj => obj.Code.IndexOf(stockCode, StringComparison.OrdinalIgnoreCase) >= 0) == false)
                {
                    continue;
                }
                if (title != null && title.Length > 0)
                {
                    if (titles.All(o => v.Title != null && v.Title.Contains(o)) == false)
                    {
                        continue;
                    }
                }
                toAdd.Add(v);
            }
            //生成VM
            PopGoodsInfoViewModel[] vms = new PopGoodsInfoViewModel[toAdd.Count];
            var goodsTasks = ServiceContainer.GetService<GoodsTaskService>().GetByAll(shop.Id, 0, 0).Datas;
            for (int i = 0; i < vms.Length; i++)
            {
                vms[i] = new PopGoodsInfoViewModel(toAdd[i]);
                vms[i].UserNumber = this.popGoodsInfoViewModels.Count + i + 1;
                vms[i].GoodsTask = goodsTasks.FirstOrDefault(obj => obj.ShopId == shop.Id && obj.GoodsId == vms[i].PopGoodsInfo.Id);
                vms[i].GoodsTask = vms[i].GoodsTask ?? new GoodsTask
                {
                    GoodsId = goods[i].Id,
                    ShopId = shop.Id,
                    Comment = "",
                    Id = 0,
                };
            }
            var unSaved = vms.Where(obj => obj.GoodsTask != null && obj.GoodsTask.Id < 1).Select(obj => obj.GoodsTask).ToArray();
            if (unSaved.Length > 0)
            {
                var retId = ServiceContainer.GetService<GoodsTaskService>().SaveBatch(unSaved);
                for (int i = 0; i < unSaved.Length; i++)
                {
                    unSaved[i].Id = retId.Datas[i];
                }
            }
            foreach (var v in vms)
            {
                this.popGoodsInfoViewModels.Add(v);
            }
            this.dgvGoods.ItemsSource = popGoodsInfoViewModels;
            for (int i = 0; i < this.popGoodsInfoViewModels.Count; i++)
            {
                var v = this.popGoodsInfoViewModels[i];
                if (v.PopGoodsInfo.Skus.Any(o => string.IsNullOrWhiteSpace(o.Code)))
                {
                    v.State = "有SKU库存编码为空";
                }
                if (this.popGoodsInfoViewModels.Any(obj => obj != v && obj.SkuCodesInfo.Equals(v.SkuCodesInfo, StringComparison.OrdinalIgnoreCase)))
                {
                    v.State += "商品重复";
                }
            }
        }

        private void OverlayTask(GoodsDownloadPauseEventArgs e)
        {
            try
            {
                int time = e.WaitSeconds;
                this.Dispatcher.BeginInvoke(new Action(() => this.overlay.Visibility = Visibility.Visible));
                while (time >= 0)
                {
                    string msg = "检测到：淘宝返回操作过于频繁，将于：" + time + "秒后自动重试";
                    this.Dispatcher.BeginInvoke(new Action(() => this.tbCount.Text = msg));
                    Thread.Sleep(1000);
                    time--;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.overlay.Visibility = Visibility.Collapsed));
                if (this.goodsDownloadWorker != null)
                {
                    this.goodsDownloadWorker.GoOn();
                }
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //运行中或者暂停中
                if (this.goodsDownloadWorker != null)
                {
                    if (goodsDownloadWorker.WorkerState == WorkerState.PAUSE)
                    {
                        goodsDownloadWorker.GoOn();
                    }
                    if (goodsDownloadWorker.WorkerState == WorkerState.WORKING)
                    {
                        goodsDownloadWorker.Pause(false, "用户暂停", 0);
                    }
                }
                else
                {
                    //创建新线程开始
                    string title = this.tbTitle.Text.Trim();
                    string code = this.tbCode.Text.Trim();
                    string stockCode = this.tbStockCode.Text.Trim();
                    PopGoodsState state = this.cbbStatus.GetSelectedEnum<PopGoodsState>();
                    this.lastShop = this.cbbShops.SelectedItem as Shop;
                    if (this.lastShop == null)
                    {
                        throw new Exception("未选择店铺");
                    }
                    if (this.lastShop.AppEnabled == false)
                    {
                        string htmlRet = this.wb1.GetTextAsync().Result;
                        if (htmlRet.Contains(this.lastShop.PopSellerId) == false)
                        {
                            throw new Exception("选择店铺与当前登录店铺不一致");
                        }
                    }
                    goodsDownloadWorker = new GoodsDownloadWorker(this.lastShop, title, code, stockCode, state);
                    this.popGoodsInfoViewModels.Clear();
                    goodsDownloadWorker.Download += GoodsDownloadWorker_Download;
                    goodsDownloadWorker.DownloadData += GoodsDownloadWorker_DownloadData;
                    goodsDownloadWorker.Pausing += GoodsDownloadWorker_Pausing;
                    goodsDownloadWorker.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GoodsDownloadWorker_Pausing(object sender, GoodsDownloadPauseEventArgs e)
        {
            if (e.Msg == "用户暂停")
            {
                return;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.IsVerify)
                {
                    MessageBox.Show("登录超时需要登录验证，进行操作后点击继续");
                }
                else
                {
                    Task.Factory.StartNew(new Action(() => OverlayTask(e)));
                }
            }));
        }

        private void GoodsDownloadWorker_DownloadData(object sender, GoodsDownloadDataEventArgs e)
        {
            GoodsDownloadWorker worker = sender as GoodsDownloadWorker;
            this.Dispatcher.BeginInvoke(new Action(() => AppendDataToUi(e.Goods, worker.shop, worker.title, worker.code, worker.stockCode, worker.state)));
        }

        private void GoodsDownloadWorker_Download(object sender, GoodsDownloadWorkerEventArgs e)
        {
            GoodsDownloadWorkerEventArgs le = e;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.btnQuery.Content = le.BtnQueryTitle;
                this.btnStop.IsEnabled = le.BtnStopEnabled;
                this.tbMsg.Text = le.Msg;
                if (e.IsComppleted)
                {
                    for (int i = 0; i < this.popGoodsInfoViewModels.Count; i++)
                    {
                        var v = this.popGoodsInfoViewModels[i];
                        if (v.PopGoodsInfo.Skus.Any(o => string.IsNullOrWhiteSpace(o.Code)))
                        {
                            v.State = "有SKU库存编码为空";
                        }
                        if (this.popGoodsInfoViewModels.Any(obj => obj != v && obj.SkuCodesInfo.Equals(v.SkuCodesInfo, StringComparison.OrdinalIgnoreCase)))
                        {
                            v.State += "商品重复";
                        }
                    }
                    if (this.popGoodsInfoViewModels.Any(obj => string.IsNullOrWhiteSpace(obj.State) == false))
                    {
                        MessageBox.Show("下载完成，有商品库存编码为空或者商品重复");
                    }
                    else
                    {
                        MessageBox.Show("下载完成");
                    }
                    this.goodsDownloadWorker = null;
                }
            }));
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.goodsDownloadWorker == null)
            {
                return;
            }

            try
            {
                this.goodsDownloadWorker.Stop();
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
                    if (goods.PopGoodsInfo.Skus == null || goods.PopGoodsInfo.Skus.Count < 1)
                    {
                        throw new Exception("商品没有SKU无法打开Go2连接");
                    }
                    var stocks = goods.PopGoodsInfo.Skus[0].Code.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (stocks.Length != 2)
                    {
                        throw new Exception(goods.PopGoodsInfo.Skus[0].Code + "不包含&无法分析");
                    }
                    var og = ServiceContainer.GetService<GoodsService>().ParseGoods(stocks[0], stocks[1]).First;
                    if (og == null)
                    {
                        throw new Exception("解析失败:" + goods.PopGoodsInfo.Skus[0]);
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
                    if (this.lastShop.PopType == PopType.TAOBAO)
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

                if (this.lastShop.PopType == PopType.TAOBAO)
                {
                    url = string.Format("https://upload.taobao.com/auction/container/publish.htm?catId={0}&itemId={1}", goods.PopGoodsInfo.CatId, goods.PopGoodsInfo.Id);
                }
                else if (this.lastShop.PopType == PopType.TMALL)
                {
                    url = "https://upload.tmall.com/auction/publish/edit.htm?auto=fals&itemNumId=" + goods.PopGoodsInfo.Id;
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

        private void dgvGoods_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.Column.Header != null && e.Column.Header.ToString() == "备注")
                {
                    var goods = e.Row.DataContext as PopGoodsInfoViewModel;
                    goods.GoodsTask.Comment = (e.EditingElement as TextBox).Text.Trim();
                    ServiceContainer.GetService<GoodsTaskService>().Update(goods.GoodsTask);
                }
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
                    if (MessageBox.Show(msg, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No) != MessageBoxResult.Yes)
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

        private void CbbShops_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var shop = this.cbbShops.SelectedItem as ShopErp.Domain.Shop;
                if (shop == null)
                {
                    return;
                }
                string url = "";
                if (shop.PopType == PopType.TAOBAO)
                {
                    url = "https://item.publish.taobao.com/taobao/manager/render.htm?tab=all";
                }
                else if (shop.PopType == PopType.TMALL)
                {
                    url = "https://item.manager.tmall.com/tmall/manager/render.htm";
                }
                if (string.IsNullOrWhiteSpace(url) == false)
                {
                    this.wb1.Load(url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb == null)
                {
                    throw new Exception("程序错误需要改程序");
                }
                foreach (var g in this.popGoodsInfoViewModels)
                {
                    g.IsChecked = cb.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnExportToPdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.task != null)
                {
                    this.isStop = true;
                    this.task.Wait();
                    return;
                }
                this.isStop = false;
                var pgs = this.popGoodsInfoViewModels.Where(obj => obj.IsChecked).ToArray();
                if (pgs.Length < 1)
                {
                    MessageBox.Show("没有选择数据");
                    return;
                }
                float[] buyInPrice = new float[pgs.Length];
                for (int i = 0; i < buyInPrice.Length; i++)
                {
                    if (pgs[i].PopGoodsInfo.Skus == null || pgs[i].PopGoodsInfo.Skus.Count < 1)
                    {
                        throw new Exception("商品没有SKU无法解析厂家价格");
                    }
                    var stocks = pgs[i].PopGoodsInfo.Skus[0].Code.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (stocks.Length != 2)
                    {
                        throw new Exception(pgs[i].PopGoodsInfo.Skus[0].Code + "不包含&无法分析");
                    }
                    var og = ServiceContainer.GetService<GoodsService>().ParseGoods(stocks[0], stocks[1]).First;
                    if (og == null)
                    {
                        throw new Exception("解析失败:" + pgs[i].PopGoodsInfo.Skus[0].Code);
                    }
                    buyInPrice[i] = og.Price;
                }
                var targetShop = this.cbbPddShops.SelectedItem as Shop;
                this.task = Task.Factory.StartNew(() => ExportToPddTask(targetShop, pgs, buyInPrice));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allGoods = ServiceContainer.GetService<GoodsService>().GetByAll(this.lastShop.Id, GoodsState.UPLOADED, 0, DateTime.MinValue, DateTime.MinValue, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", 0, 0).Datas.OrderBy(obj => obj.VendorId).ToList();
                var popGoods = this.popGoodsInfoViewModels.ToList();
                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas.ToList();

                List<ShopErp.Domain.Goods> unMatchGoods = new List<ShopErp.Domain.Goods>();
                List<PopGoodsInfoViewModel> unMatchPopGoods = new List<PopGoodsInfoViewModel>();

                foreach (var g in allGoods)
                {
                    var vendor = vendors.FirstOrDefault(obj => obj.Id == g.VendorId);
                    if (vendor == null)
                    {
                        throw new Exception("商品未找到对应厂家：" + g.Number + " 商品ID：" + g.Id);
                    }
                    if (string.IsNullOrWhiteSpace(vendor.PingyingName))
                    {
                        throw new Exception("厂家没有配置拼音：" + vendor.Name);
                    }
                    string nn = vendor.PingyingName.Trim() + "&" + g.Number.Trim();
                    var pg = popGoods.FirstOrDefault(obj => obj.SkuCodesInfo.IndexOf(nn, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (pg == null)
                    {
                        unMatchGoods.Add(g);
                    }
                }

                foreach (var pg in popGoods)
                {
                    string[] skus = pg.SkuCodesInfo.Split(',');
                    foreach (var code in skus)
                    {
                        string[] nn = code.Split('&');
                        var g = allGoods.FirstOrDefault(obj => obj.Number.Equals(nn[1], StringComparison.OrdinalIgnoreCase) && vendors.FirstOrDefault(o => o.Id == obj.VendorId).PingyingName.Equals(nn[0], StringComparison.OrdinalIgnoreCase));
                        if (g == null)
                        {
                            unMatchPopGoods.Add(pg);
                        }
                    }
                }
                if (unMatchGoods.Count > 0 || unMatchPopGoods.Count > 0)
                {
                    string msg1 = string.Join(",", unMatchGoods.Select(obj => vendors.FirstOrDefault(o => o.Id == obj.VendorId).Name + "&" + obj.Number));
                    string msg2 = string.Join(",", unMatchPopGoods.Select(obj => obj.SkuCodesInfo));
                    string msg = string.Format("系统中未在网站上匹配的：{0}\r\n,网站上未在系统中匹配的：{1}", msg1, msg2);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    MessageBox.Show("完全匹配");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportToPddTask(Shop shop, PopGoodsInfoViewModel[] pgs, float[] buyInPrice)
        {
            var popGoods = new PopGoods[1];
            var price = new float[1];
            string exportMsg = "";
            for (int i = 0; i < pgs.Length && this.isStop == false; i++)
            {
                int index = i;
                this.SetUiMsg(this.btnExportToPdd, "正在导入： " + (index + 1) + "/" + pgs.Length + " " + pgs[index].PopGoodsInfo.Id + " " + pgs[index].SkuCodesInfo, true, "停止", "导入");
                try
                {
                    popGoods[0] = pgs[i].PopGoodsInfo;
                    price[0] = buyInPrice[i];
                    var ret = ServiceContainer.GetService<GoodsService>().AddGoods(shop, popGoods, price);
                    exportMsg = ret.Datas[0];
                    if (index < pgs.Length - 1)
                    {
                        this.SetUiMsg(this.btnExportToPdd, "等待2秒后继续导入", true, "停止", "导入");
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ee)
                {
                    exportMsg = ee.Message;
                }
                finally
                {
                    this.Dispatcher.BeginInvoke(new Action(() => pgs[index].State = exportMsg));
                }
            }
            this.goodsDownloadWorker = null;
            this.isStop = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.SetUiMsg(this.btnExportToPdd, "导入完成", false, "停止", "导入");
                MessageBox.Show("导入完成");
            }));
        }

        private void BtnCheckImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shop = this.cbbShops.SelectedItem as ShopErp.Domain.Shop;
                if (shop == null)
                {
                    throw new Exception("只有淘宝天猫可以进行匹配");
                }
                string tadgetDomainCookies = CefCookieVisitor.GetCookieValues("tadget.taobao.com", null);
                string tbToken = CefCookieVisitor.GetCookieValues("tadget.taobao.com", "_tb_token_");
                if (string.IsNullOrWhiteSpace(tbToken))
                {
                    throw new Exception("获取_tb_token cookie 为空");
                }
                var url = new Uri("https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_dirTree_query&count=true&_input_charset=utf-8&_tb_token_=" + tbToken.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                string json = MsHttpRestful.SendTbData(System.Net.Http.HttpMethod.Get, url, null, tadgetDomainCookies, null);
                var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageRsp>(json);
                var imageDirs = rsp.module.dirs.children.FirstOrDefault(obj => obj.name == "商品图片").children.Select(obj => obj.name).ToArray();
                var allGoods = ServiceContainer.GetService<GoodsService>().GetByAll((this.cbbShops.SelectedItem as Shop).Id, GoodsState.UPLOADED, 0, DateTime.MinValue, DateTime.MinValue, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", 0, 0).Datas.OrderBy(obj => obj.VendorId).ToList();
                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas.ToList();

                List<ShopErp.Domain.Goods> unMatchGoods = new List<ShopErp.Domain.Goods>();
                List<string> unMatchDirs = new List<string>();

                foreach (var g in allGoods)
                {
                    var vendor = vendors.FirstOrDefault(obj => obj.Id == g.VendorId);
                    if (vendor == null)
                    {
                        throw new Exception("商品未找到对应厂家：" + g.Number + " 商品ID：" + g.Id);
                    }
                    if (string.IsNullOrWhiteSpace(vendor.PingyingName))
                    {
                        throw new Exception("厂家没有配置拼音：" + vendor.Name);
                    }
                    string nn = vendor.PingyingName.Trim() + "&" + g.Number.Trim();
                    var pg = imageDirs.FirstOrDefault(obj => obj.IndexOf(nn, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (pg == null)
                    {
                        unMatchGoods.Add(g);
                    }
                }

                foreach (var pg in imageDirs)
                {
                    string[] skus = pg.Split(',');
                    foreach (var code in skus)
                    {
                        string[] nn = code.Split('&');
                        var g = allGoods.FirstOrDefault(obj => obj.Number.Equals(nn[1], StringComparison.OrdinalIgnoreCase) && vendors.FirstOrDefault(o => o.Id == obj.VendorId).PingyingName.Equals(nn[0], StringComparison.OrdinalIgnoreCase));
                        if (g == null)
                        {
                            unMatchDirs.Add(pg);
                        }
                    }
                }
                if (unMatchGoods.Count > 0 || unMatchDirs.Count > 0)
                {
                    string msg1 = string.Join(",", unMatchGoods.Select(obj => vendors.FirstOrDefault(o => o.Id == obj.VendorId).Name + "&" + obj.Number));
                    string msg2 = string.Join(",", unMatchDirs);
                    string msg = string.Format("系统中未在网站上匹配的：{0}\r\n,网站上未在系统中匹配的：{1}", msg1, msg2);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    MessageBox.Show("完全匹配");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class ImageRsp
    {
        public ImageRspModule module;
    }

    public class ImageRspModule
    {
        public ImageRspModuleDir dirs;
    }

    public class ImageRspModuleDir
    {
        public ImageRspModuleDirChildren[] children;
    }

    public class ImageRspModuleDirChildren
    {
        public string name;

        public ImageRspModuleDirChildren[] children;
    }


    public enum WorkerState
    {
        INIT = 0,
        WORKING = 1,
        STOPED = 2,
        PAUSE = 3
    }

    public class GoodsDownloadWorkerEventArgs : EventArgs
    {
        public string BtnQueryTitle { get; set; }

        public bool BtnStopEnabled { get; set; }

        public string Msg { get; set; }

        public bool IsComppleted { get; set; }
    }

    public class GoodsDownloadDataEventArgs : EventArgs
    {
        public List<PopGoods> Goods { get; set; }
    }

    public class GoodsDownloadPauseEventArgs : EventArgs
    {
        public bool IsVerify { get; set; }

        public string Msg { get; set; }

        public int WaitSeconds { get; set; }
    }

    public class GoodsDownloadWorker
    {
        public Shop shop;

        public string title;

        public string code;

        public string stockCode;

        public PopGoodsState state;

        private AutoResetEvent waiterLock = new AutoResetEvent(false);

        private Task task = null;

        private bool isStop = false;

        public WorkerState WorkerState { get; private set; }

        public event EventHandler<GoodsDownloadWorkerEventArgs> Download;

        public event EventHandler<GoodsDownloadDataEventArgs> DownloadData;

        public event EventHandler<GoodsDownloadPauseEventArgs> Pausing;

        protected void OnDownload(string btnQueryTitle, bool btnStopEnabled, string msg, bool isCompeleted)
        {
            if (this.Download != null)
            {
                this.Download(this, new GoodsDownloadWorkerEventArgs { BtnQueryTitle = btnQueryTitle, BtnStopEnabled = btnStopEnabled, Msg = msg, IsComppleted = isCompeleted });
            }
        }

        protected void OnDownloadData(List<PopGoods> goods)
        {
            if (this.DownloadData != null)
            {
                this.DownloadData(this, new GoodsDownloadDataEventArgs { Goods = goods });
            }
        }

        protected void OnPausing(bool isVerify, string msg, int waitSeconds)
        {
            if (this.Pausing != null)
            {
                this.Pausing(this, new GoodsDownloadPauseEventArgs { IsVerify = isVerify, Msg = msg, WaitSeconds = waitSeconds });
            }
        }

        public GoodsDownloadWorker(Shop shop, string title, string code, string stockCode, PopGoodsState state)
        {
            this.shop = shop;
            this.title = title;
            this.code = code;
            this.stockCode = stockCode;
            this.state = state;
            this.WorkerState = WorkerState.INIT;
        }

        public void Start()
        {
            if (WorkerState != WorkerState.INIT)
            {
                throw new Exception("线程不是初始状态无法启动");
            }
            this.task = Task.Factory.StartNew(TaskFunc);
        }

        public void Pause(bool isVerify, string msg, int waitSeconds)
        {
            if (WorkerState != WorkerState.WORKING)
            {
                throw new Exception("线程不是运行状态无法暂停");
            }
            this.WorkerState = WorkerState.PAUSE;
            this.Download(this, new GoodsDownloadWorkerEventArgs { BtnQueryTitle = "继续", BtnStopEnabled = true, IsComppleted = false, Msg = msg });
            this.OnPausing(isVerify, msg, waitSeconds);
        }

        public void GoOn()
        {
            if (WorkerState != WorkerState.PAUSE)
            {
                throw new Exception("线程不是暂停状态，无法继续");
            }
            this.WorkerState = WorkerState.WORKING;
            this.waiterLock.Set();
        }

        public bool WaitGoOn()
        {
            if (this.isStop)
            {
                return false;
            }

            if (this.WorkerState != WorkerState.PAUSE)
            {
                return true;
            }
            //无限期等待
            this.waiterLock.WaitOne();
            return !this.isStop;
        }

        public void Stop()
        {
            this.isStop = true;
            this.waiterLock.Set();
            if (this.task == null || task.Status == TaskStatus.Canceled || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.RanToCompletion)
            {
                return;
            }
            if (this.task.Wait(1000 * 60 * 20) == false)
            {
                throw new Exception("等待20分钟，停止任务失败，后台线程还在继续运行，可以关闭程序");
            }
        }

        public void TaskFunc()
        {
            bool hasError = false;
            DateTime start = DateTime.Now;
            List<PopGoods> goods = new List<PopGoods>();
            try
            {
                this.WorkerState = WorkerState.WORKING;
                this.isStop = false;
                this.OnDownload("暂停", true, "下载线程已开始执行", false);
                int pageIndex = 0;
                while (this.WaitGoOn())
                {
                    this.OnDownload("暂停", true, "正在下载第:" + (pageIndex + 1) + "页", false);
                    List<PopGoods> ggs = null;
                    if (shop.AppEnabled)
                    {
                        ggs = ServiceContainer.GetService<GoodsService>().SearchPopGoods(shop, state, pageIndex, 50).Datas;
                    }
                    else
                    {
                        ggs = this.QueryHtmlGoodsPage(shop, pageIndex + 1, state);
                    }
                    if (ggs == null || ggs.Count < 1)
                    {
                        break;
                    }
                    goods.AddRange(ggs);
                    pageIndex++;
                    this.OnDownload("暂停", true, "已经下载:" + goods.Count, false);
                    this.OnDownloadData(ggs);
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                this.OnDownload("开始", false, "错误：" + ex.Message, true);
            }
            finally
            {
                DateTime end = DateTime.Now;
                Debug.WriteLine("用时：" + (end - start).TotalSeconds + ",共下载：" + goods.Count);
                this.isStop = true;
                this.task = null;
                if (hasError == false)
                {
                    this.OnDownload("开始", false, "下载完成", true);
                }
            }
        }

        private List<PopGoods> QueryHtmlGoodsPage(Shop shop, int pageIndex, PopGoodsState state)
        {
            Uri goodsListPageUri = new Uri(shop.PopType == PopType.TMALL ? "https://item.manager.tmall.com/tmall/manager/table.htm" : "https://item.publish.taobao.com/taobao/manager/table.htm");
            string goodsEditPageDomain = shop.PopType == PopType.TMALL ? "item.upload.tmall.com" : "item.publish.taobao.com";
            string goodsEditPageUrlBase = shop.PopType == PopType.TMALL ? "https://item.upload.tmall.com/tmall/publish.htm?id=" : "https://item.publish.taobao.com/sell/publish.htm?itemId=";

            var goodsListPageXsrfCookieValue = CefCookieVisitor.GetCookieValues(goodsListPageUri.Host, "XSRF-TOKEN");
            if (string.IsNullOrWhiteSpace(goodsListPageXsrfCookieValue))
            {
                throw new Exception("没有找到相应XSRF-TOKEN,刷新页面重新登录");
            }
            var goodsListPageDomainCookie = CefCookieVisitor.GetCookieValues(goodsListPageUri.Host, null);
            if (string.IsNullOrWhiteSpace(goodsListPageDomainCookie))
            {
                throw new Exception("未找到域:" + goodsListPageUri.Host + " 的任何COOKIE ,刷新页面重新登录");
            }
            var goodsEditPageCookie = CefCookieVisitor.GetCookieValues(goodsEditPageDomain, null);
            string goodsState = state == PopGoodsState.NONE ? "all" : (state == PopGoodsState.ONSALE ? "on_sale" : "in_stock");
            var xhrdata = "jsonBody=" + MsHttpRestful.UrlEncode("{\"filter\":{},\"pagination\":{\"current\":" + pageIndex + ",\"pageSize\":20},\"table\":{\"sort\":{}},\"tab\":\"" + goodsState + "\"}", Encoding.UTF8);
            var header = new Dictionary<string, string>();
            header.Add("X-XSRF-TOKEN", goodsListPageXsrfCookieValue.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);

            var content = MsHttpRestful.SendTbData(System.Net.Http.HttpMethod.Post, goodsListPageUri, header, goodsListPageDomainCookie, xhrdata);
            var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryGoodsListResponse>(content);
            List<PopGoods> goods = new List<PopGoods>();
            if (resp == null || resp.success == false)
            {
                throw new Exception("返回数据错误，请点击页面分页进行验证后重新开始");
            }
            if (resp.data.table.dataSource == null)
            {
                return goods;
            }
            foreach (var g in resp.data.table.dataSource)
            {
                if (this.WaitGoOn() == false)
                {
                    break;
                }
                //每个商品获取的时候，都有可能，登录超时或者其它问题，需要循环
                PopGoods pg = null;
                string goodsEditPageUrl = goodsEditPageUrlBase + g.itemId;

                for (int i = 0; i < 5; i++)
                {
                    if (this.WaitGoOn() == false)
                    {
                        break;
                    }
                    Debug.WriteLine("开始抓取商品：" + g.itemId);
                    this.OnDownload("暂停", true, "第:" + (i + 1) + " 次下载，第：" + pageIndex + "/" + ((resp.data.pagination.total / resp.data.pagination.pageSize) + 1) + "页，第" + (goods.Count + 1) + "条商品详情", false);
                    bool ret = QueryHtmlGoods(g, goodsEditPageUrl, goodsEditPageCookie, ref pg);
                    if (ret == true)
                    {
                        goods.Add(pg);
                        break;
                    }
                }
                if (this.isStop == false && pg == null)
                {
                    throw new Exception("抓取商品5次都为成功：" + g.itemId);
                }
                if (this.WaitGoOn() == false)
                {
                    break;
                }
                //每次等待3秒，太快淘宝会返回错误
                Thread.Sleep(3000);
            }
            return goods;
        }

        private bool QueryHtmlGoods(TaobaoQueryGoodsListResponseDataTableDataSource g, string url, string cookie, ref PopGoods popGoods)
        {
            string htmlContent = MsHttpRestful.SendTbData(System.Net.Http.HttpMethod.Get, new Uri(url), null, cookie, null);

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                throw new Exception("读取商品详情返回为空：" + g.itemId);
            }

            try
            {
                if (htmlContent[0] == '{' && htmlContent.Last() == '}')
                {
                    var error = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryGoodsDetailErrorResponse>(htmlContent);
                    if (error.success == false)
                    {
                        if ("SYS_REQUEST_TOO_FAST".Equals(error.code, StringComparison.OrdinalIgnoreCase))
                        {
                            int wait = error.data == null ? 70 : error.data.wait + 10;
                            this.Pause(false, "程序读取商品详情过快，等待：" + wait + "秒", wait);
                            return false;
                        }
                        else
                        {
                            Log.Logger.Log("获取淘宝详情失败,错误信息:" + htmlContent);
                            throw new Exception("获取淘宝详情失败,错误信息：" + error.code + "," + error.msg);
                        }
                    }
                }
            }
            catch (Newtonsoft.Json.JsonException)
            {
            }

            if (htmlContent.Contains("客官别急") && htmlContent.Contains("交通拥堵"))
            {
                Debug.WriteLine(DateTime.Now + "检测到返回网页操作过快");
                this.Pause(false, "程序读取商品详情过快，等待：70 秒", 70);
                return false;
            }

            //第一步找JS JSON 起始符
            int startIndex = htmlContent.IndexOf("window.Json =");
            if (startIndex < 0)
            {
                Debug.WriteLine("商品详情：" + g.itemId + ":" + htmlContent);
                throw new Exception("分析商品编辑页失败，未找到window.Json =：" + g.itemId);
            }

            //第二步，找JSON开始符
            startIndex = htmlContent.IndexOf("{\"", startIndex);
            if (startIndex < 0)
            {
                Debug.WriteLine("商品详情：" + g.itemId + ":" + htmlContent);
                throw new Exception("分析商品编辑页失败，未找到window.Json =：之后的{\"" + g.itemId);
            }
            startIndex = htmlContent.IndexOf("{\"", startIndex);

            //第三步找JSON结尾字符串
            int end = htmlContent.IndexOf("}}};", startIndex);
            if (end <= 0)
            {
                Debug.WriteLine("商品详情：" + g.itemId + ":" + htmlContent);
                throw new Exception("分析商品编辑页失败，未找到JSON结束符}}};" + g.itemId);
            }
            string json = htmlContent.Substring(startIndex, end + 3 - startIndex);
            var goodsDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryGoodsDetailResponse>(json);
            var pg = new PopGoods { AddTime = "", CatId = g.catId, Code = goodsDetail.models.formValues.outerId, Id = g.itemId, Images = goodsDetail.models.formValues.images.Select(obj => obj.url.StartsWith("http") ? obj.url : "https:" + obj.url).ToArray(), SaleNum = g.soldQuantity_m, UpdateTime = g.upShelfDate_m.value };

            //状态 
            pg.State = g.upShelfDate_m.status.text == "出售中" ? PopGoodsState.ONSALE : PopGoodsState.NOTSALE;
            pg.Title = g.itemDesc.desc.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.href) == false).text.Trim();

            //SKU
            foreach (var vSku in goodsDetail.models.formValues.sku)
            {
                var pSku = new PopGoodsSku
                {
                    Code = vSku.skuOuterId ?? "",
                    Id = vSku.skuId ?? "",
                    Stock = vSku.skuStock.ToString(),
                    Price = vSku.skuPrice.ToString("F2"),
                    Status = PopGoodsState.ONSALE,
                    Color = vSku.props[0].text.Trim(),
                    Size = vSku.props[1].text.Trim(),
                };
                if (vSku.skuImage != null && vSku.skuImage.Length > 0)
                {
                    pSku.Image = vSku.skuImage[0].url;
                }
                else
                {
                    pSku.Image = goodsDetail.models.formValues.saleProp.Colors.FirstOrDefault(obj => obj.text == pSku.Color).img;
                }

                if (string.IsNullOrWhiteSpace(pSku.Image))
                {
                    throw new Exception("商品SKU图片为空：" + pg.Id);
                }
                if (pSku.Image.StartsWith("http") == false)
                {
                    pSku.Image = "https:" + pSku.Image;
                }
                pg.Skus.Add(pSku);
            }
            //属性 天猫
            //基本属性
            if (goodsDetail.models.formValues.bindProp != null)
            {
                foreach (var v in goodsDetail.models.formValues.bindProp)
                {
                    string key = goodsDetail.models.bindProp.dataSource.First(obj => obj.name == v.name).label;
                    string values = string.Join("@#@", v.values.Select(obj => obj.text));
                    pg.Properties.Add(new KeyValuePairClass<string, string>(key, values));
                }
            }
            if (goodsDetail.models.formValues.itemProp != null)
            {
                foreach (var v in goodsDetail.models.formValues.itemProp)
                {
                    string key = goodsDetail.models.itemProp.dataSource.First(obj => obj.name == v.name).label;
                    string values = string.Join("@#@", v.values.Select(obj => obj.text));
                    pg.Properties.Add(new KeyValuePairClass<string, string>(key, values));
                }
            }

            if (goodsDetail.models.formValues.catProp != null)
            {
                foreach (var v in goodsDetail.models.formValues.catProp)
                {
                    string key = goodsDetail.models.catProp.dataSource.First(obj => obj.name == v.name).label;
                    string values = string.Join("@#@", v.values.Select(obj => obj.text));
                    pg.Properties.Add(new KeyValuePairClass<string, string>(key, values));
                }
            }
            pg.PopType = shop.PopType;
            pg.Type = goodsDetail.models.catpath.value.Split(new string[] { ">>", "》" }, StringSplitOptions.RemoveEmptyEntries)[1];
            pg.ShippingCity = goodsDetail.models.formValues.location != null ? goodsDetail.models.formValues.location.value.text : goodsDetail.models.global.location.city;
            if (shop.PopType == PopType.TMALL && (goodsDetail.models.formValues.modularDesc == null || goodsDetail.models.formValues.modularDesc.Length < 1))
            {
                throw new Exception("商品：" + g.itemId + " 电脑端描述为空");
            }
            if (shop.PopType == PopType.TAOBAO && string.IsNullOrWhiteSpace(goodsDetail.models.formValues.desc))
            {
                throw new Exception("商品：" + g.itemId + " 电脑端描述为空");
            }

            //详情图片
            pg.DescImages = ParseImages(shop.PopType == PopType.TMALL ? goodsDetail.models.formValues.modularDesc.First().content : goodsDetail.models.formValues.desc);
            popGoods = pg;
            return true;
        }

        private string[] ParseImages(string desc)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(desc);
            var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
            string[] ss = nodes.Select(obj => obj.GetAttributeValue("src", "")).Where(obj => string.IsNullOrWhiteSpace(obj) == false).ToArray();
            return ss;
        }

    }


}