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
        private Task task = null;
        private bool isStop = false;
        private bool myLoaded = false;
        private Shop lastShop = null;
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
                this.cbbTypes.Bind<GoodsType>();
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

        private void AppendDataToUi(List<PopGoods> goods, Shop shop, string title, string code, string stockCode, PopGoodsState state, GoodsType type)
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
                GoodsType gt = (GoodsType)EnumUtil.GetEnumValueByDesc(typeof(GoodsType), v.Type);
                if (type != GoodsType.GOODS_SHOES_NONE && gt != GoodsType.GOODS_SHOES_NONE && gt != type)
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
                GoodsType type = this.cbbTypes.GetSelectedEnum<GoodsType>();
                if (this.lastShop == null)
                {
                    throw new Exception("未选择店铺");
                }
                this.popGoodsInfoViewModels.Clear();
                this.task = Task.Factory.StartNew(() => QueryTask(this.lastShop, title, code, stockCode, state, type));
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
                    url = "https://ipublish.tmall.com/tmall/manager/render.htm?tab=all";
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

        private void QueryTask(Shop shop, string title, string code, string stockCode, PopGoodsState state, GoodsType type)
        {
            List<PopGoods> goods = new List<PopGoods>();

            var titles = title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                this.SetUiMsg(this.btnQuery, "下载线程已开始执行", true, "停止", "查询");
                this.isStop = false;
                int pageIndex = 0;
                while (this.isStop == false)
                {
                    SetUiMsg(this.btnQuery, "正在下载第:" + (pageIndex + 1) + "页", true, "停止", "查询");
                    List<PopGoods> ggs = null;
                    if (shop.AppEnabled)
                    {
                        ggs = ServiceContainer.GetService<GoodsService>().SearchPopGoods(shop, state, pageIndex, 50).Datas;
                    }
                    else
                    {
                        ggs = this.QueryHtmlGoods(shop, pageIndex + 1, state);
                    }
                    if (ggs == null || ggs.Count < 1)
                    {
                        break;
                    }
                    goods.AddRange(ggs);
                    pageIndex++;
                    SetUiMsg(this.btnQuery, "已经下载:" + goods.Count, true, "停止", "查询");
                    this.Dispatcher.Invoke(new Action(() => AppendDataToUi(ggs, shop, title, code, stockCode, state, type)));
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
                this.SetUiMsg(this.btnQuery, "下载完成", false, "停止", "查询");
                this.Dispatcher.BeginInvoke(new Action(() =>
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
                }));
            }
        }

        private List<PopGoods> QueryHtmlGoods(Shop shop, int pageIndex, PopGoodsState state)
        {
            string htmlRet = this.wb1.GetTextAsync().Result;
            if (htmlRet.Contains(shop.PopSellerId) == false)
            {
                throw new Exception("选择店铺与当前登录店铺不一致");
            }
            var cefCookieVisitor = new CefCookieVisitor(shop.PopType == PopType.TMALL ? "ipublish.tmall.com" : "item.publish.taobao.com", "XSRF-TOKEN");
            Cef.GetGlobalCookieManager().VisitAllCookies(cefCookieVisitor);
            var cookieValue = cefCookieVisitor.WaitValue();
            if (string.IsNullOrWhiteSpace(cookieValue))
            {
                throw new Exception("当前店铺没有登录");
            }
            string goodsState = state == PopGoodsState.NONE ? "all" : (state == PopGoodsState.ONSALE ? "on_sale" : "in_stock");
            string goodsListUrl = shop.PopType == PopType.TMALL ? "https://ipublish.tmall.com/tmall/manager/table.htm" : "https://item.publish.taobao.com/taobao/manager/table.htm";
            string goodsListScript = ScriptManager.GetBody(jspath, "TAOBAO_SEARCH_GOODS").Replace("###url", goodsListUrl).Replace("###pageIndex", pageIndex.ToString()).Replace("###state", goodsState).Replace("###xsrf-token", cookieValue); ;
            string goodsEditDetailUrl = shop.PopType == PopType.TMALL ? "https://ipublish.tmall.com/tmall/publish.htm?id=" : "https://item.publish.taobao.com/sell/publish.htm?itemId=";

            JavascriptResponse ret = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(goodsListScript, "", 1, new TimeSpan(0, 0, 30)).Result;
            if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
            {
                throw new Exception("执行操作失败：" + ret.Message);
            }
            var content = ret.Result.ToString();
            var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryGoodsListResponse>(content);
            List<PopGoods> goods = new List<PopGoods>();
            if (resp.data.table.dataSource == null)
            {
                return goods;
            }
            var last = DateTime.Now;
            foreach (var g in resp.data.table.dataSource)
            {
                if (this.isStop)
                {
                    break;
                }
                long mis = (long)DateTime.Now.Subtract(last).TotalMilliseconds;
                if (mis < 2000)
                {
                    Thread.Sleep(2000 - (int)mis);
                }
                last = DateTime.Now;
                Debug.WriteLine("开始抓取商品：" + g.itemId);

                this.SetUiMsg(this.btnQuery, "正在下载第：" + pageIndex + "/" + ((resp.data.pagination.total / resp.data.pagination.pageSize) + 1) + "页，第" + (goods.Count + 1) + "条商品详情", true, "停止", "查询");
                string url = goodsEditDetailUrl + g.itemId;
                string goodsEditDetailScript = ScriptManager.GetBody(jspath, "TAOBAO_GET_GOODS").Replace("###url", url);
                ret = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(goodsEditDetailScript, "", 1, new TimeSpan(0, 0, 30)).Result;
                if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
                {
                    throw new Exception("执行操作失败：" + ret.Message);
                }
                string htmlContent = ret.Result.ToString();

                //第一步找JS JSON 起始符
                int startIndex = htmlContent.IndexOf("window.Json =");
                if (startIndex < 0)
                {
                    throw new Exception("读取商品编辑详情面失败：" + g.itemId);
                }

                //第二步，找JSON开始符
                startIndex = htmlContent.IndexOf("{\"", startIndex);
                if (startIndex < 0)
                {
                    throw new Exception("读取商品编辑详情面失败：" + g.itemId);
                }
                startIndex = htmlContent.IndexOf("{\"", startIndex);

                //第三步找JSON结尾字符串
                int end = htmlContent.IndexOf("}}};", startIndex);
                if (end <= 0)
                {
                    throw new Exception("读取商品编辑详情面失败：" + g.itemId);
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
                //详情图片
                pg.DescImages = ParseImages(goodsDetail.models.formValues.desc);
                goods.Add(pg);
            }
            return goods;
        }

        private string[] ParseImages(string desc)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(desc);
            var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
            string[] ss = nodes.Select(obj => obj.GetAttributeValue("src", "")).Where(obj => string.IsNullOrWhiteSpace(obj) == false).ToArray();
            return ss;
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
                var allGoods = ServiceContainer.GetService<GoodsService>().GetByAll(this.lastShop.Id, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", 0, 0).Datas.OrderBy(obj => obj.VendorId).ToList();
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
            this.task = null;
            this.isStop = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.SetUiMsg(this.btnExportToPdd, "导入完成", false, "停止", "导入");
                MessageBox.Show("导入完成");
            }));
        }
    }
}