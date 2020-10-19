using CefSharp;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Domain.TaobaoHtml.Image;
using ShopErp.App.Domain.TaobaoHtml.Order;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopErp.App.Views.AttachUI.Taobao
{
    /// <summary>
    /// TaobaoUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoUserControl : UserControl, IAttachUIOrder
    {
        string jsgetua = "window.uabModule && window.uabModule.getUA({Token: window.UA_TOKEN})";

        public event EventHandler<AttachUiOrderDownloadEventArgs> OrderDownload;

        public event EventHandler<AttachUIOrderPreviewDownloadEventArgs> OrderPreviewDownload;

        public event EventHandler Start;

        public event EventHandler End;

        public TaobaoUserControl()
        {
            InitializeComponent();
        }

        private void OnStart()
        {
            if (this.Start != null)
            {
                this.Start(this, new EventArgs());
            }
        }

        private void OnOrderDownload(OrderDownload orderDownload)
        {
            if (this.OrderDownload != null)
            {
                this.OrderDownload(this, new AttachUiOrderDownloadEventArgs { OrderDownload = orderDownload });
            }
        }

        private void OnOrderPreviewDownload(AttachUIOrderPreviewDownloadEventArgs e)
        {
            if (this.OrderPreviewDownload != null)
            {
                this.OrderPreviewDownload(this, e);
            }
        }

        private void OnEnd()
        {
            if (this.End != null)
            {
                this.End(this, new EventArgs());
            }
        }

        private void cbbUrls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count < 1)
                {
                    return;
                }
                string text = e.AddedItems[0].ToString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }
                int index = text.IndexOf("https://");
                if (index < 0)
                {
                    throw new Exception("选择内容中不包含https://，无法分析网址");
                }
                string url = text.Substring(index).Trim();
                this.wb1.Load(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var str = EvaluateScript("window.uabModule && window.uabModule.getUA({Token: window.UA_TOKEN})");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGetUserNumberId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string uid = GetUid();
                if (string.IsNullOrWhiteSpace(uid))
                {
                    throw new Exception("网页中未找到 userid= 请检查是否登录");
                }
                MessageBox.Show(uid, "用户数字编号");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.wb1.IsBrowserInitialized == false)
                {
                    throw new Exception("浏览器还没有初始化，请先登录");
                }
                this.wb1.Reload(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetUid()
        {
            if (this.wb1.IsBrowserInitialized == false)
            {
                throw new Exception("浏览器还没有初始化，请先登录");
            }
            string sourceHtml = this.wb1.GetSourceAsync().Result;
            string uid = "";
            int index = 0;
            do
            {
                index = sourceHtml.IndexOf("userid=", index);
                if (index < 0)
                {
                    throw new Exception("网页中未找到 userid= 请检查是否登录");
                }
                int start = index + "userid=".Length;
                int end = index + "userid=".Length;
                for (; end < sourceHtml.Length && Char.IsDigit(sourceHtml[end]); end++)
                {
                }
                uid = sourceHtml.Substring(start, end - start);
                if (string.IsNullOrWhiteSpace(uid) == false)
                {
                    break;
                }
                index = start;
            } while (index < sourceHtml.Length);
            return uid;
        }

        /// <summary>
        /// 执行js脚本
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string EvaluateScript(string script)
        {
            if (this.wb1.IsBrowserInitialized == false)
            {
                throw new Exception("浏览器还没有初始化，请先登录");
            }
            var task = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(script, "", 1, new TimeSpan(0, 0, 30));
            var ret = task.Result;
            if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
            {
                throw new Exception("执行操作失败：" + ret.Message);
            }
            return ret.Result.ToString();
        }

        public Shop GetLoginShop()
        {
            string uid = GetUid();
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new Exception("网页中未找到 userid= 请检查是否登录");
            }
            var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
            var shop = shops.FirstOrDefault(obj => obj.PopSellerNumberId == uid);
            if (shop == null)
            {
                throw new Exception("UID " + uid + " 未在本地找到匹配店铺，请检查登录或者本地店铺是否配置");
            }
            return shop;
        }

        #region 图片空间

        public ImageDirRsp GetImageDirRsp()
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            string tbToken = CefCookieVisitor.GetSignleCookieValue("tadget.taobao.com", "_tb_token_");
            if (string.IsNullOrWhiteSpace(tbToken))
            {
                throw new Exception("获取_tb_token cookie 为空");
            }
            var url = new Uri("https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_dirTree_query&count=true&_input_charset=utf-8&_tb_token_=" + tbToken);
            string json = MsHttpRestful.GetReturnString(url.OriginalString, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageDirRsp>(json);
            return rsp;
        }

        public ImageFileRsp GetImageFileRsp(string catId)
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            string tbToken = CefCookieVisitor.GetSignleCookieValue("tadget.taobao.com", "_tb_token_");
            if (string.IsNullOrWhiteSpace(tbToken))
            {
                throw new Exception("获取_tb_token cookie 为空");
            }
            var url = new Uri("https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_batch_query&_input_charset=utf-8&cat_id=" + catId + "&ignore_cat=0&order_by=0&page=1&client_type=0&deleted=0&status=0&_tb_token_=" + tbToken);
            string json = MsHttpRestful.GetReturnString(url.OriginalString, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageFileRsp>(json);
            return rsp;
        }

        public ImageAddDirRsp AddDir(string catId, string name)
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            string tbToken = CefCookieVisitor.GetSignleCookieValue("tadget.taobao.com", "_tb_token_");
            if (string.IsNullOrWhiteSpace(tbToken))
            {
                throw new Exception("获取_tb_token cookie 为空");
            }
            var url = new Uri(" https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_add_dir&_input_charset=utf-8&dir_id=" + catId + "&name=" + MsHttpRestful.UrlEncode(name, Encoding.UTF8) + "&_tb_token_=" + tbToken);
            string json = MsHttpRestful.GetReturnString(url.OriginalString, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageAddDirRsp>(json);
            return rsp;
        }
        public ImageAddFileRsp AddFile(string catId, FileInfo fi)
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            Dictionary<string, string> param = new Dictionary<string, string>();
            Dictionary<string, FileInfo> files = new Dictionary<string, FileInfo>();
            param["name"] = fi.Name;
            param["ua"] = EvaluateScript(jsgetua);
            Debug.WriteLine("File:" + fi.FullName + ", UA:" + param["ua"]);
            files["file"] = fi;
            var url = new Uri("https://stream-upload.taobao.com/api/upload.api?appkey=tu&folderId=" + catId + "&watermark=false&autoCompress=false&_input_charset=utf-8");
            string json = MsHttpRestful.PostMultipartFormDataBodyReturnString(url.OriginalString, param, files, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageAddFileRsp>(json);
            return rsp;
        }


        #endregion

        #region 订单及发货

        private ColorFlag ConvertFlag(int flag)
        {
            if (flag == 0)
            {
                return ColorFlag.UN_LABEL;
            }
            if (flag == 1)
            {
                return ColorFlag.RED;
            }
            if (flag == 2)
            {
                return ColorFlag.YELLOW;
            }
            if (flag == 3)
            {
                return ColorFlag.GREEN;
            }
            if (flag == 4)
            {
                return ColorFlag.BLUE;
            }
            if (flag == 5)
            {
                return ColorFlag.PINK;
            }
            return ColorFlag.UN_LABEL;
        }

        private OrderState ConvertState(string state)
        {
            if (state == "买家已付款" || state.Contains("买家已付款，等待商家发货"))
            {
                return OrderState.PAYED;
            }
            if (state == "卖家已发货" || state.Contains("商家已发货，等待买家确认"))
            {
                return OrderState.SHIPPED;
            }
            if (state.Contains("订单部分退款中") || state.Contains("待退货") || state.Contains("请退款"))
            {
                return OrderState.RETURNING;
            }
            if (state.Contains("交易关闭") || state.Contains("退款成功"))
            {
                return OrderState.CLOSED;
            }
            if (state.Contains("交易成功"))
            {
                return OrderState.SUCCESS;
            }
            throw new Exception("订单状态不正确：" + state);
        }

        private ShopErp.Domain.Order ParseOrder(TaobaoQueryOrderListResponseOrder orderShort, Shop shop)
        {
            var dbMineTime = Utils.DateTimeUtil.DbMinTime;
            string url = shop.PopType == PopType.TMALL ? "https://trade.tmall.com/detail/orderDetail.htm?bizOrderId=" : "https://trade.taobao.com/detail/orderDetail.htm?bizOrderId=";
            //订单信息
            var content = MsHttpRestful.GetReturnString(url + orderShort.id, CefCookieVisitor.GetCookieValue(shop.PopType == PopType.TMALL ? "trade.tmall.com" : "trade.taobao.com"));
            string title = shop.PopType == PopType.TMALL ? "var detailData" : "var data = JSON";

            int si = content.IndexOf(title);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title + " 请点击订单详情进行验证");
            }
            si = content.IndexOf('{', si);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title);
            }
            int ei = content.IndexOf("</script>", si);
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }
            while (ei >= 0 && content[ei] != '}') ei--;
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }

            String orderInfo = content.Substring(si, ei - si + 1).Trim();
            DateTime popPayTime = dbMineTime, popDeliveryTime = dbMineTime;
            string buyerComment = "", sellerComment = "", reciverInfo = "", popOrderState = "";
            float goodsPrice = 0, deliveryPrice = 0, sellerGetMoney = 0;
            Dictionary<string, float> namePrice = new Dictionary<string, float>();

            if (shop.PopType == PopType.TMALL)
            {
                var orderDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<TmallQueryOrderDetailResponse>(orderInfo, new Newtonsoft.Json.JsonSerializerSettings { StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeHtml });

                string payTime = orderDetail.stepbar.options.First(obj => obj.content == "买家付款").time;
                string deliveryTime = orderDetail.stepbar.options.First(obj => obj.content == "发货").time;

                popOrderState = orderDetail.overStatus.status.content[0].text;
                popPayTime = string.IsNullOrWhiteSpace(payTime) ? dbMineTime : DateTime.Parse(payTime);
                popDeliveryTime = string.IsNullOrWhiteSpace(deliveryTime) ? dbMineTime : DateTime.Parse(deliveryTime);
                buyerComment = orderDetail.basic.lists.First(obj => obj.key == "买家留言").content[0].text;
                if (buyerComment == "-")
                {
                    buyerComment = "";
                }
                var addN = orderDetail.basic.lists.First(obj => obj.key == "收货地址").content[0];
                //html 表示地址是要经过转运的地址，label是不需要经过转运的大陆地址
                if (addN.type.Equals("html", StringComparison.OrdinalIgnoreCase))
                {
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(addN.text);
                    string hh = document.DocumentNode.InnerText;
                    string nhh = hh.Substring(0, hh.IndexOf("]转&nbsp;") + 1);// 
                    string read = nhh.Trim().TrimEnd(']');
                    string mark = "[转运仓转运仓库：";
                    if (read.IndexOf(mark) > 0)
                    {
                        read = read.Replace(mark, "");
                    }
                    reciverInfo = read;
                }
                else if (addN.type.Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    reciverInfo = addN.text;
                }
                else
                {
                    throw new Exception("无法识别的地址格式");
                }

                //订单金额
                var contents = new List<TmallQueryOrderDetailResponseAmountCountContent>();
                foreach (var c in orderDetail.amount.count)
                {
                    foreach (var cc in c)
                    {
                        contents.AddRange(cc.content);
                    }
                }

                string strGoodsPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text == "商品总价").data.money.text.Replace("￥", "").Trim();
                string strDeliveryPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("运费")).data.money.text.Replace("￥", "").Trim();
                string strBuyerPayPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("订单总价")).data.money.text.Replace("￥", "").Trim();
                var strSellerGetMoney = contents.FirstOrDefault(obj => obj.data.titleLink != null && (obj.data.titleLink.text.Contains("应收款") || obj.data.titleLink.text.Contains("实收款")));

                goodsPrice = float.Parse(strGoodsPrice);
                deliveryPrice = float.Parse(strDeliveryPrice);
                sellerGetMoney = float.Parse(strSellerGetMoney.data.dotPrefixMoney.text + strSellerGetMoney.data.dotSufixMoney.text);

                //商家备注
                if (orderDetail.overStatus.operate.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.key)) != null)
                {
                    string comment = orderDetail.overStatus.operate.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.key)).content[0].text;
                    si = comment.IndexOf("备忘：</span><span>");
                    ei = comment.IndexOf("</span>", si + "备忘：</span><span>".Length);
                    sellerComment = comment.Substring(si + "备忘：</span><span>".Length, ei - si - "备忘：</span><span>".Length);
                }
                //运费显示有三种：运费(快递) 运费(平邮) 运费(EMS)
                if (contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("运费(快递)")) == null)
                {
                    sellerComment += "[系统自动识别：此件发邮政]";
                }

                foreach (var vv in orderDetail.orders.list)
                {
                    foreach (var vvv in vv.status)
                    {
                        foreach (var vvvv in vvv.subOrders)
                        {
                            namePrice[vvvv.itemInfo.title] = float.Parse(vvvv.priceInfo[0].text.Trim());
                        }
                    }
                }
            }
            else
            {
                orderInfo = Regex.Unescape(orderInfo);
                var orderDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryOrderDetailResponse>(orderInfo);
                List<TaobaoQueryOrderDetailResponseOrderInfoLineContentNameValue> infoLines = new List<TaobaoQueryOrderDetailResponseOrderInfoLineContentNameValue>();
                foreach (var v in orderDetail.mainOrder.orderInfo.lines)
                {
                    foreach (var vv in v.content)
                    {
                        infoLines.Add(vv.value);
                    }
                }

                var infoLinePayTime = infoLines.FirstOrDefault(obj => obj.name.Contains("付款时间"));
                var infoLineDeliveryTime = infoLines.FirstOrDefault(obj => obj.name.Contains("发货时间"));
                popPayTime = infoLinePayTime != null ? DateTime.Parse(infoLinePayTime.value) : dbMineTime;
                popDeliveryTime = infoLineDeliveryTime != null ? DateTime.Parse(infoLineDeliveryTime.value) : dbMineTime;
                popOrderState = orderDetail.mainOrder.statusInfo.text;
                buyerComment = orderDetail.buyMessage;
                foreach (var v in orderDetail.operationsGuide)
                {
                    foreach (var vv in v.lines)
                    {
                        if (vv.content.Any(obj => obj.value == "标记："))
                        {
                            sellerComment = vv.content[1].value;
                            break;
                        }
                    }
                }
                goodsPrice = float.Parse(orderDetail.mainOrder.totalPrice[0].content[0].value);
                deliveryPrice = float.Parse(orderDetail.mainOrder.totalPrice.FirstOrDefault(obj => obj.content[0].value.Contains("快递")).content[0].value.Replace("(快递:", ""));
                sellerGetMoney = orderDetail.mainOrder.payInfo.actualFee.value;
                foreach (var v in orderDetail.mainOrder.subOrders)
                {
                    namePrice[v.itemInfo.title] = v.priceInfo;
                }
                var addN = orderDetail.tabs.FirstOrDefault(obj => obj.id == "logistics");
                if (addN == null)
                {
                    throw new Exception("未找到地址结点");
                }
                reciverInfo = addN.content.address;
            }

            var order = new Order
            {
                CloseOperator = "",
                CloseTime = dbMineTime,
                CreateOperator = "",
                CreateTime = DateTime.Now,
                CreateType = OrderCreateType.DOWNLOAD,
                DeliveryCompany = "",
                DeliveryNumber = "",
                DeliveryOperator = "",
                DeliveryTime = dbMineTime,
                DeliveryMoney = 0,
                Id = 0,
                PopDeliveryTime = popDeliveryTime,
                OrderGoodss = new List<OrderGoods>(),
                ParseResult = true,
                PopBuyerComment = buyerComment,
                PopBuyerId = orderShort.buyer.nick,
                PopBuyerPayMoney = orderShort.payInfo.actualFee,
                PopCodNumber = "",
                PopCodSevFee = 0,
                PopFlag = ConvertFlag(orderShort.extra.sellerFlag),
                PopOrderId = orderShort.id,
                PopOrderTotalMoney = goodsPrice + deliveryPrice,
                PopPayTime = popPayTime,
                PopPayType = PopPayType.ONLINE,
                PopSellerComment = sellerComment,
                PopSellerGetMoney = sellerGetMoney,
                PopState = popOrderState,
                PopType = shop.PopType,
                PrintOperator = "",
                PrintTime = dbMineTime,
                ReceiverAddress = "",
                ReceiverMobile = "",
                ReceiverName = "",
                ReceiverPhone = "",
                ShopId = shop.Id,
                State = ConvertState(orderShort.statusInfo.text.Trim()),
                Type = OrderType.NORMAL,
                Weight = 0,
                DeliveryTemplateId = 0,
                Refused = false,
            };
            foreach (var so in orderShort.subOrders)
            {
                var og = new OrderGoods
                {
                    CloseOperator = "",
                    CloseTime = dbMineTime,
                    Color = so.itemInfo.skuText.FirstOrDefault(obj => obj.name.Contains("颜色")).value,
                    Comment = "",
                    Count = so.quantity,
                    Edtion = "",
                    GetedCount = 0,
                    Id = 0,
                    Image = so.itemInfo.pic,
                    Number = so.itemInfo.extra[0].value,
                    GoodsId = 0,
                    OrderId = 0,
                    PopOrderSubId = "",
                    PopPrice = namePrice[so.itemInfo.title],
                    PopUrl = "",
                    Price = 0,
                    Size = so.itemInfo.skuText.FirstOrDefault(obj => obj.name.Contains("尺码")).value,
                    State = OrderState.PAYED,
                    Weight = 0,
                    StockOperator = "",
                    StockTime = dbMineTime,
                    Vendor = "",
                    IsPeijian = false,
                };
                og.PopInfo = og.Number + " " + og.Color + " " + og.Size;

                if (so.operations != null && so.operations.FirstOrDefault(obj => obj.text.Trim() == "退款成功") != null)
                {
                    og.State = OrderState.CLOSED;
                }
                else if (so.operations != null && so.operations.FirstOrDefault(obj => obj.text.Trim() == "请卖家处理" || obj.text.Trim() == "请退款") != null)
                {
                    og.State = OrderState.RETURNING;
                }
                else
                {
                    og.State = OrderState.PAYED;
                }
                order.OrderGoodss.Add(og);
            }

            if (order.OrderGoodss.Select(obj => obj.State).Distinct().Count() == 1)
            {
                order.State = order.OrderGoodss[0].State;
            }

            string add = "";
            string[] reinfos = reciverInfo.Split(new char[] { '，', ',' }, StringSplitOptions.RemoveEmptyEntries);
            order.ReceiverName = HttpUtility.HtmlDecode(reinfos[0].Trim());
            order.ReceiverMobile = HttpUtility.HtmlDecode(reinfos[1].Replace("86-", ""));
            int start = 2;
            if (reinfos[2].All(c => Char.IsDigit(c) || c == '-'))
            {
                order.ReceiverPhone = reinfos[2];
                start = 3;
            }
            for (; start < reinfos.Length; start++)
            {
                if (start == reinfos.Length - 1 && reinfos[start].All(c => Char.IsDigit(c)))
                {
                    break;
                }
                add += reinfos[start] + ",";
            }
            order.ReceiverAddress = HttpUtility.HtmlDecode(add.Trim(','));
            return order;
        }

        public void DownloadOrders()
        {
            this.OnStart();
            int totalCount = 0, currentCount = 0;
            int totalPage = 0, currentPage = 1;
            var allShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
            var shop = GetLoginShop();
            if (shop == null)
            {
                throw new Exception("系统中没有找到相应店铺");
            }
            Dictionary<string, string> param = new Dictionary<string, string>();
            param["action"] = "itemlist/SoldQueryAction";
            param["auctionType"] = "0";
            param["orderStatus"] = "PAID";
            param["tabCode"] = "waitSend";
            param["pageSize"] = "15";
            while (true)
            {
                param["prePageNo"] = (currentPage - 1 >= 0 ? currentPage - 1 : 1).ToString();
                param["pageNum"] = currentPage.ToString();
                var header = CefCookieVisitor.GetCookieValue("trade.taobao.com");
                string ret = MsHttpRestful.PostUrlEncodeBodyReturnString("https://trade.taobao.com/trade/itemlist/asyncSold.htm?event_submit_do_query=1&_input_charset=utf8", param, header, null, "https://trade.taobao.com/trade/itemlist/list_sold_items.htm");
                var or = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryOrderListResponse>(ret);
                if (or.page == null)
                {
                    throw new Exception("执行操作失败：返回数据格式无法识别,请点击订单下一页或者上一页进行验证");
                }
                if (or.mainOrders == null || or.mainOrders.Length < 1)
                {
                    break;
                }
                totalCount = or.page.totalNumber;
                totalPage = or.page.totalPage;

                foreach (var v in or.mainOrders)
                {
                    currentCount++;
                    //先检查是否需要下载
                    var state = ConvertState(v.statusInfo.text);
                    if (state == OrderState.PAYED && v.subOrders.All(obj => obj.operations != null && (obj.operations.FirstOrDefault(op => op.text.Trim() == "退款成功" || op.text.Trim() == "请卖家处理" || op.text.Trim() == "请退款") != null)))
                    {
                        state = OrderState.RETURNING;
                    }
                    AttachUIOrderPreviewDownloadEventArgs orderPreviewDownloadEventArgs = new AttachUIOrderPreviewDownloadEventArgs { Current = currentCount, PopOrderId = v.id, Skip = false, Stop = false, Total = totalCount, State = state, PopFlag = ConvertFlag(v.extra.sellerFlag), Shop = shop };
                    this.OnOrderPreviewDownload(orderPreviewDownloadEventArgs);
                    if (orderPreviewDownloadEventArgs.Stop)
                    {
                        return;
                    }
                    if (orderPreviewDownloadEventArgs.Skip)
                    {
                        continue;
                    }
                    OrderDownload od = new OrderDownload();
                    try
                    {
                        var order = ParseOrder(v, shop);
                        od.Order = order;
                    }
                    catch (Exception ex)
                    {
                        od.Error = new OrderDownloadError(shop.Id, v.id, "", ex.Message, ex.StackTrace);
                    }
                    finally
                    {
                        this.OnOrderDownload(od);
                    }
                }
                currentPage++;
            }
            this.OnEnd();
        }

        public void MarkPopDelivery(string popOrderId, string deliveryCompany, string deliveryNumber)
        {
            string formId = "//form[@id = 'orderForm']";
            string inputMark = "input";
            //第一步读取页面信息,需要从该页面中提取要发送的数据
            var uri = new Uri("https://wuliu.taobao.com/user/consign.htm?trade_id=" + popOrderId);
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue(uri.Host);
            string html = MsHttpRestful.GetReturnString(uri.OriginalString, headers);

            //第二步 提取数据
            Dictionary<string, string> paras = new Dictionary<string, string>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var form = doc.DocumentNode.SelectSingleNode(formId);
            if (form == null)
            {
                Debug.WriteLine(form);
                throw new Exception("未在页面中找到：" + formId);
            }

            var inputNodes = doc.DocumentNode.SelectNodes("//input");
            if (inputNodes == null || inputNodes.Count < 1)
            {
                throw new Exception("未找到任何：" + inputMark + " 元素");
            }

            foreach (var v in inputNodes)
            {
                if (string.IsNullOrWhiteSpace(v.GetAttributeValue("name", "")))
                {
                    continue;
                }
                paras[v.GetAttributeValue("name", "")] = v.GetAttributeValue("value", "");
            }
            paras.Remove("q");
            paras.Remove("type");
            paras.Remove("cat");
            paras["companyName"] = "请输入物流公司名称";
            paras["event_submit_do_offline_consign"] = "1";
            paras["offlineNewFlag"] = "1";
            paras["_fmw.r._0.c"] = paras["receiverContactName"];
            paras["_fmw.r._0.adr"] = paras["receiverDetail"];
            paras["_fmw.f._0.fe"] = "";
            paras["_fmw.f._0.fet"] = "";
            paras["_fmw.n._0.g"] = "您可以在此输入备忘信息（仅卖家自己可见）。";
            paras["initialWeightOld"] = "all";
            //进行几种常见错误的检测
            if (paras.ContainsKey("receiverZipCode") == false || paras["receiverZipCode"].Length != 6)
            {
                throw new Exception("邮政编码不为6位,当前值：" + paras["receiverZipCode"]);
            }
            if (paras.ContainsKey("receiverDetail") == false || paras["receiverDetail"].Length < 4)
            {
                throw new Exception("详细地址必须大于等于4个字，当前值：" + paras["receiverDetail"]);
            }

            paras["mailNo"] = deliveryNumber;
            paras["companyCode"] = deliveryCompany;
            //页面使用的什么编码，发送数据就要使用什么编码，否则对面无法接收
            string ret = MsHttpRestful.PostUrlEncodeBodyReturnString(uri.OriginalString, paras, headers, doc.DeclaredEncoding ?? doc.Encoding);
            if (ret.Contains("恭喜您，操作成功"))
            {
                return;
            }
            if (ret.Contains("运单号不符合规则或已经被使用"))
            {
                throw new Exception("运单号不符合规则或已经被使用");
            }
            throw new Exception("返回页面数据无法识别，请查看相应结果");
        }

        public string GetSellerComment(string popOrderId)
        {
            string url = "https://trade.taobao.com/trade/json/memoInfo.htm?user_type=seller&_input_charset=utf-8&orderid=" + popOrderId;
            string ret = MsHttpRestful.GetReturnString(url, CefCookieVisitor.GetCookieValue("trade.taobao.com"));
            if (ret.Contains("{\"tip\":\"") == false)
            {
                throw new Exception("获取订单备注失败:" + popOrderId + ",请检查是否登录");
            }
            TaobaoOrderSellerCommentResponse resp = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoOrderSellerCommentResponse>(ret);
            return resp.tip;
        }

        public PopOrderState GetOrderState(string popOrderId)
        {
            var pos = new PopOrderState()
            {
                PopOrderId = popOrderId,
                PopOrderStateValue = "",
                State = OrderState.NONE
            };
            var shop = GetLoginShop();
            //订单信息
            string url = shop.PopType == PopType.TMALL ? "https://trade.tmall.com/detail/orderDetail.htm?bizOrderId=" : "https://trade.taobao.com/detail/orderDetail.htm?bizOrderId=";
            var content = MsHttpRestful.GetReturnString(url + popOrderId, CefCookieVisitor.GetCookieValue(shop.PopType == PopType.TMALL ? "trade.tmall.com" : "trade.taobao.com"));
            string title = shop.PopType == PopType.TMALL ? "var detailData" : "var data = JSON";

            int si = content.IndexOf(title);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title);
            }
            si = content.IndexOf('{', si);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title);
            }
            int ei = content.IndexOf("</script>", si);
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }
            while (ei >= 0 && content[ei] != '}') ei--;
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }
            String orderInfo = content.Substring(si, ei - si + 1).Trim();

            if (shop.PopType == PopType.TMALL)
            {
                var oi = Newtonsoft.Json.JsonConvert.DeserializeObject<TmallQueryOrderDetailResponse>(orderInfo);
                pos.PopOrderStateValue = oi.overStatus.status.content[0].text;
            }
            else
            {
                orderInfo = Regex.Unescape(orderInfo);
                var oi = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryOrderDetailResponse>(orderInfo);
                pos.PopOrderStateValue = oi.mainOrder.statusInfo.text;
            }
            pos.State = ConvertState(pos.PopOrderStateValue);

            return pos;
        }

        #endregion

    }
}
