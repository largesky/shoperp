using System;
using System.Collections.Generic;
using System.Linq;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Service.Restful;
using ShopErp.Server.Service.Net;
using ShopErp.Server.Utils;
using System.Text;
using System.Diagnostics;
using ShopErp.Domain.RestfulResponse.DomainResponse;
using System.Xml.Linq;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddPop : PopBase
    {
        private static char[] LEFT_S = "(（".ToCharArray();
        private const string SERVER_URL = "http://gw-api.pinduoduo.com/api/router";

        private static string TrimJsonWrapper(string json, int level)
        {
            string bodyContent = json;
            for (int i = 0; i < level; i++)
            {
                //去除内容头
                bodyContent = bodyContent.Substring(bodyContent.IndexOf('{', 1));
                bodyContent = bodyContent.Substring(0, bodyContent.LastIndexOf('}'));
                bodyContent = bodyContent.Substring(0, bodyContent.LastIndexOf('}') + 1);
            }
            return bodyContent;
        }

        private string Sign(string appSecret, SortedDictionary<string, string> param)
        {
            param.Remove("sign");
            string value = appSecret + string.Join("", param.Select(obj => string.IsNullOrWhiteSpace(obj.Value) ? "" : obj.Key + obj.Value)) + appSecret;
            return Md5Util.Md5(value);
        }

        private T Invoke<T>(Domain.Shop shop, string apiName, SortedDictionary<string, string> param, int trimJsonWrapperLevel = 1) where T : PddRspBase
        {
            string timeStamp = ((long)DateTime.UtcNow.Subtract(UNIX_START_TIME).TotalSeconds).ToString();
            param["type"] = apiName;
            param["client_id"] = shop.AppKey;
            param["access_token"] = shop.AppAccessToken;
            param["timestamp"] = timeStamp;
            param["data_type"] = "JSON";
            param["sign"] = Sign(shop.AppSecret, param);
            var content = MsHttpRestful.PostUrlEncodeBodyReturnString(SERVER_URL, param);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("接口调用失败：返回空数据");
            }
            //去除PHP有可能产生的BOM头
            if (content[0] != '{')
            {
                if (content.IndexOf('{') < 0)
                {
                    throw new Exception("拼多多返回错误数据");
                }
                content = content.Substring(content.IndexOf('{'));
            }
            //去除内容头
            string bodyContent = TrimJsonWrapper(content, trimJsonWrapperLevel);
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(bodyContent);
            if (string.IsNullOrWhiteSpace(t.error_code) == false)
            {
                if (t.error_msg.Contains("access_token已过期"))
                {
                    throw new PopAccesstokenTimeOutException();
                }
                if (t.error_msg.Contains("refresh_token已过期"))
                {
                    throw new Exception("拼多多调用失败：授权已到期，请到店铺里面进行授权");
                }
                Debug.WriteLine("请求参数：" + string.Join(Environment.NewLine, param.Select(obj => obj.Key + " " + obj.Value)));
                throw new Exception("拼多多调用失败：接口：" + apiName + " 错误信息" + t.error_code + "," + t.error_msg + "," + t.sub_msg);
            }
            return t;
        }

        private string[] UploadImage(Shop shop, string[] images)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            string[] urls = new string[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                byte[] bytes = MsHttpRestful.GetReturnBytes(images[i], null);
                string base64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
                param["image"] = "data:image/jpeg;base64," + base64;
                PddRspUploadImg ret = Invoke<PddRspUploadImg>(shop, "pdd.goods.image.upload", param);
                urls[i] = ret.image_url;
            }
            return urls;
        }

        private PddRspGetOrderStateOrder GetOrderStatePingduoduo(Domain.Shop shop, string popOrderId)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["order_sns"] = popOrderId;
            var rsp = this.Invoke<PddRspGetOrderState>(shop, "pdd.order.status.get", param);

            if (rsp.order_status_list == null || rsp.order_status_list.Length < 1)
            {
                throw new Exception("拼多多查询状态返回空数据");
            }

            return rsp.order_status_list[0];
        }

        private Domain.OrderState ConvertToOrderState(string popOrderId, PddRspGetOrderStateOrder state)
        {

            Domain.OrderState os = Domain.OrderState.NONE;

            var s = state;

            if (s.refund_status == "0")
            {
                os = Domain.OrderState.NONE;
            }
            else if (s.refund_status == "1")
            {
                if (s.order_status == "0")
                {
                    os = Domain.OrderState.NONE;
                }
                else if (s.order_status == "1")
                {
                    os = Domain.OrderState.PAYED;
                }
                else if (s.order_status == "2")
                {
                    os = Domain.OrderState.SHIPPED;
                }
                else if (s.order_status == "3")
                {
                    os = Domain.OrderState.SUCCESS;
                }
            }
            else if (s.refund_status == "2" || s.refund_status == "3")
            {
                os = Domain.OrderState.RETURNING;
            }
            else if (s.refund_status == "4")
            {
                os = Domain.OrderState.CLOSED;
            }
            else
            {
                throw new Exception(string.Format("无法认识的订单:{0},退款状态:{1}", popOrderId, s.refund_status));
            }

            return os;
        }

        public override bool Accept(Domain.PopType popType)
        {
            return popType == Domain.PopType.PINGDUODUO;
        }

        private string GetDeliveryCompany(string id)
        {
            if (id == "0")
            {
                return "";
            }

            var dc = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.FirstOrDefault(obj => obj.PopMapPingduoduo == id);
            if (dc == null)
            {
                throw new Exception("拼多多快递公司没有相关映射请添加，快递编号：" + id);
            }
            return dc.Name;
        }

        public OrderDownload ConvertOrderDownload(Domain.Shop shop, PddRspOrderListOrder pddRspOrderListOrder)
        {
            OrderDownload od = new OrderDownload();
            try
            {
                var o = pddRspOrderListOrder;
                DateTime minTime = new DateTime(1970, 01, 01);
                var order = new Domain.Order
                {
                    CloseOperator = "",
                    CloseTime = minTime,
                    CreateOperator = "",
                    CreateTime = DateTime.Parse(o.created_time),
                    CreateType = Domain.OrderCreateType.DOWNLOAD,
                    DeliveryCompany = GetDeliveryCompany(o.logistics_id),
                    DeliveryNumber = o.tracking_number,
                    DeliveryOperator = "",
                    DeliveryTime = minTime,
                    DeliveryMoney = 0,
                    Id = 0,
                    PopDeliveryTime = Utils.DateTimeUtil.DbMinTime,
                    OrderGoodss = new List<Domain.OrderGoods>(),
                    ParseResult = false,
                    PopBuyerComment = "",
                    PopBuyerId = "",
                    PopBuyerPayMoney = float.Parse(o.pay_amount),
                    PopCodNumber = "",
                    PopCodSevFee = 0,
                    PopFlag = Domain.ColorFlag.UN_LABEL,
                    PopOrderId = o.order_sn,
                    PopOrderTotalMoney = float.Parse(o.goods_amount) + float.Parse(o.postage ?? "0"),
                    PopPayTime = DateTime.Parse(o.confirm_time ?? "1970-01-01 00:00:01"),
                    PopPayType = Domain.PopPayType.ONLINE,
                    PopSellerComment = o.remark,
                    PopSellerGetMoney = float.Parse(o.goods_amount) + float.Parse(o.postage ?? "") - float.Parse(o.seller_discount ?? "0") - float.Parse(o.capital_free_discount ?? "0"),
                    PopState = "",
                    PopType = Domain.PopType.PINGDUODUO,
                    PrintOperator = "",
                    PrintTime = minTime,
                    ReceiverAddress = o.address,
                    ReceiverMobile = o.receiver_phone,
                    ReceiverName = o.receiver_name,
                    ReceiverPhone = "",
                    ShopId = shop.Id,
                    State = Domain.OrderState.NONE,
                    Type = Domain.OrderType.NORMAL,
                    Weight = 0,
                };

                //解析商品
                if (o.item_list != null)
                {
                    foreach (var goods in o.item_list)
                    {
                        var orderGoods = new Domain.OrderGoods
                        {
                            CloseOperator = "",
                            CloseTime = Utils.DateTimeUtil.DbMinTime,
                            Color = "",
                            Comment = "",
                            Count = goods.goods_count,
                            Edtion = "",
                            GetedCount = 0,
                            Id = 0,
                            Image = goods.goods_img,
                            Number = goods.outer_id,
                            GoodsId = 0,
                            OrderId = 0,
                            PopInfo = goods.outer_id + " " + goods.goods_spec,
                            PopOrderSubId = "",
                            PopPrice = goods.goods_price,
                            PopUrl = goods.goods_id,
                            Price = 0,
                            Size = "",
                            State = Domain.OrderState.NONE,
                            StockOperator = "",
                            StockTime = Utils.DateTimeUtil.DbMinTime,
                            Vendor = "",
                            Weight = 0,
                            Shipper = ""
                        };
                        //拼多以 ‘，’号分开，前面为颜色，后面为尺码
                        string[] stocks = goods.goods_spec.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                        if (stocks.Length == 2)
                        {
                            orderGoods.Color = stocks[0];
                            orderGoods.Size = stocks[1];
                        }
                        order.OrderGoodss.Add(orderGoods);
                    }
                }
                //获取订单状态
                var os = ConvertToOrderState(pddRspOrderListOrder.order_sn, new PddRspGetOrderStateOrder { orderSn = pddRspOrderListOrder.order_sn, order_status = o.order_status, refund_status = o.refund_status });
                order.State = os;
                order.OrderGoodss[0].State = os;
                od.Order = order;
            }
            catch (Exception e)
            {
                od.Error = new OrderDownloadError(shop.Id, pddRspOrderListOrder.order_sn, "", e.Message, e.StackTrace);
            }
            return od;
        }

        public override OrderDownloadCollectionResponse GetOrders(Shop shop, string state, DateTime time, int pageIndex, int pageSize)
        {
            SortedDictionary<string, string> para = new SortedDictionary<string, string>();
            var ret = new OrderDownloadCollectionResponse { IsTotalValid = false };
            if (state == PopService.QUERY_STATE_WAITSHIP_COD)
            {
                return ret;
            }
            long now = (long)time.Subtract(UNIX_START_TIME).TotalSeconds;
            para["order_status"] = "1";
            para["refund_status"] = "1";
            para["page"] = (pageIndex + 1).ToString();
            para["page_size"] = pageSize.ToString();
            para["start_confirm_at"] = now.ToString();
            para["end_confirm_at"] = (now + 86400L).ToString();
            var resp = this.Invoke<PddRspOrderList>(shop, "pdd.order.list.get", para);
            if (resp.order_list != null && resp.order_list.Length > 0)
            {
                ret.Total = resp.total_count;
                foreach (var or in resp.order_list)
                {
                    if (ret.Datas.Any(obj => obj.Order != null && obj.Order.PopOrderId == or.order_sn) == false)
                    {
                        var e = this.ConvertOrderDownload(shop, or);
                        ret.Datas.Add(e);
                    }
                }
            }
            return ret;
        }

        public override PopOrderState GetOrderState(Domain.Shop shop, string popOrderId)
        {
            var orderState = this.GetOrderStatePingduoduo(shop, popOrderId);
            var os = ConvertToOrderState(popOrderId, orderState);

            var popOrderState = new PopOrderState
            {
                PopOrderId = popOrderId,
                PopOrderStateValue = orderState.order_status,
                State = os,
            };
            return popOrderState;
        }

        public override void MarkDelivery(Domain.Shop shop, string popOrderId, Domain.PopPayType payType, string deliveryCompany, string deliveryNumber)
        {
            var orderState = this.GetOrderStatePingduoduo(shop, popOrderId);
            var os = ConvertToOrderState(popOrderId, orderState);

            if (os == Domain.OrderState.SHIPPED || os == Domain.OrderState.SUCCESS)
            {
                return;
            }

            if (os != Domain.OrderState.PAYED)
            {
                throw new Exception("订单状态不正确:" + os);
            }
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["order_sn"] = popOrderId;
            param["logistics_id"] = ServiceContainer.GetService<DeliveryCompanyService>().GetDeliveryCompany(deliveryCompany).First.PopMapPingduoduo;
            param["tracking_number"] = deliveryNumber;
            var rsp = this.Invoke<PddRspShipping>(shop, "pdd.logistics.online.send", param);
            if (rsp.is_success.ToLower() != "true" && rsp.is_success.ToLower() != "1")
            {
                throw new Exception("发货失败:" + rsp.error_msg);
            }
        }

        public override List<PopGoods> SearchPopGoods(Domain.Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["page_size"] = pageSize.ToString();
            param["page"] = (pageIndex + 1).ToString();
            if (state != PopGoodsState.NONE)
                param["is_onsale"] = state == PopGoodsState.ONSALE ? "1" : "0";
            var ret = this.Invoke<PddRspGoods>(shop, "pdd.goods.list.get", param);
            List<PopGoods> goods = new List<PopGoods>();

            if (ret.goods_list == null || ret.goods_list == null)
            {
                return goods;
            }

            foreach (var g in ret.goods_list)
            {
                var pg = new PopGoods
                {
                    Id = g.goods_id,
                    Title = g.goods_name,
                    AddTime = "",
                    UpdateTime = "",
                    SaleNum = 0,
                    Images = new string[] { g.thumb_url },
                    CatId = "",
                    Code = "",
                    State = g.is_onsale == "1" ? PopGoodsState.ONSALE : PopGoodsState.NOTSALE,
                    Type = "所有",
                };
                if (g.sku_list == null || g.sku_list.Length < 1)
                {
                    continue;
                }
                foreach (var sku in g.sku_list)
                {
                    if (sku.is_sku_onsale == 0)
                    {
                        continue;
                    }
                    pg.Code = sku.outer_goods_id;
                    var psku = new PopGoodsSku
                    {
                        Id = sku.sku_id,
                        Code = sku.outer_id,
                        Stock = sku.sku_quantity.ToString(),
                        Status = PopGoodsState.ONSALE,
                        Price = "0",
                    };
                    string[] ss = sku.spec.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    psku.Color = ss[0];
                    psku.Size = ss[1];
                    pg.Skus.Add(psku);
                }
                goods.Add(pg);
            }
            return goods;
        }

        public override PopDeliveryInfo GetDeliveryInfo(Domain.Shop shop, string popOrderId)
        {
            throw new NotImplementedException();
        }

        public override void ModifyComment(Domain.Shop shop, string popOrderId, string comment, Domain.ColorFlag flag)
        {
        }

        public override string GetShopOauthUrl(Shop shop)
        {
            string url = string.Format("https://mms.pinduoduo.com/open.html?response_type=code&client_id={0}&redirect_uri={1}&state={2}", shop.AppKey, shop.AppCallbackUrl, shop.Id + "_" + shop.AppKey + "_" + shop.AppSecret);
            return url;
        }

        public override Shop GetAcessTokenInfo(Shop shop, string code)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["client_id"] = shop.AppKey;
            para["client_secret"] = shop.AppSecret;
            para["grant_type"] = "authorization_code";
            para["code"] = code;
            para["redirect_uri"] = "http://bjcgroup.imwork.net:60014/shoperp/shop/pddoauth.html";
            string url = "http://open-api.pinduoduo.com/oauth/token";
            var content = Net.MsHttpRestful.PostJsonBodyReturnString(url, para);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("接口调用失败：返回空数据");
            }
            //去除PHP有可能产生的BOM头
            if (content[0] != '{')
            {
                if (content.IndexOf('{') < 0)
                {
                    throw new Exception("拼多多返回错误数据");
                }
                content = content.Substring(content.IndexOf('{'));
            }
            //去除内容头
            string bodyContent = TrimJsonWrapper(content, 0);
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<PddRspAccessToken>(bodyContent);
            if (string.IsNullOrWhiteSpace(t.error_code) == false)
            {
                throw new Exception("拼多多调用失败：" + t.error_code + "," + t.error_msg);
            }
            if (t.owner_name.Equals(shop.PopSellerId, StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new Exception("系统店铺:" + shop.PopSellerId + "返回授权店铺:" + t.owner_name + " 不匹配");
            }
            shop.AppAccessToken = t.access_token;
            shop.AppRefreshToken = t.refresh_token;
            shop.PopSellerNumberId = t.owner_id;
            return shop;
        }

        public override Shop GetRefreshTokenInfo(Shop shop)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["client_id"] = shop.AppKey;
            para["client_secret"] = shop.AppSecret;
            para["grant_type"] = "refresh_token";
            para["refresh_token"] = shop.AppRefreshToken;
            string url = "http://open-api.pinduoduo.com/oauth/token";
            var content = Net.MsHttpRestful.PostJsonBodyReturnString(url, para);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("接口调用失败：返回空数据");
            }
            //去除PHP有可能产生的BOM头
            if (content[0] != '{')
            {
                if (content.IndexOf('{') < 0)
                {
                    throw new Exception("拼多多返回错误数据");
                }
                content = content.Substring(content.IndexOf('{'));
            }
            //去除内容头
            string bodyContent = TrimJsonWrapper(content, 0);
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<PddRspAccessToken>(bodyContent);
            if (string.IsNullOrWhiteSpace(t.error_code) == false)
            {
                throw new Exception("拼多多调用失败：" + t.error_code + "," + t.error_msg);
            }
            if (string.IsNullOrWhiteSpace(t.access_token) || string.IsNullOrWhiteSpace(t.refresh_token))
            {
                throw new Exception("拼多多调用失败：刷新AcessToken返回空数据，需要重新授权");
            }
            shop.AppAccessToken = t.access_token;
            shop.AppRefreshToken = t.refresh_token;
            shop.PopSellerNumberId = t.owner_id;
            return shop;
        }

        public override List<WuliuBranch> GetWuliuBranchs(Shop shop)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            var rsp = this.Invoke<PddRspWaybillSearch>(shop, "pdd.waybill.search", param);
            var wbs = new List<WuliuBranch>();

            foreach (var v in rsp.waybill_apply_subscription_cols)
            {
                foreach (var vv in v.branch_account_cols)
                {
                    foreach (var vvv in vv.shipp_address_cols)
                    {
                        var wb = new WuliuBranch { Name = vv.branch_name, Number = vv.branch_code, Quantity = vv.quantity, Type = v.wp_code, SenderName = "", SenderPhone = "", SenderAddress = vvv.province + " " + vvv.city + " " + vvv.district + "  " + vvv.detail };
                        wbs.Add(wb);
                    }
                }
            }
            return wbs;
        }

        public override List<WuliuPrintTemplate> GetWuliuPrintTemplates(Shop shop, string cpCode)
        {
            List<WuliuPrintTemplate> pts = new List<WuliuPrintTemplate>();
            //第一步先查找所有的标准模板
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["wp_code"] = cpCode;
            var std = this.Invoke<PddRspCloudprintStdtemplates>(shop, "pdd.cloudprint.stdtemplates.get", param);
            if (std.result == null || std.result.datas == null || std.result.datas.Length == 0)
            {
                return pts;
            }
            foreach (var v in std.result.datas)
            {
                foreach (var vv in v.standard_templates)
                {
                    if (string.IsNullOrWhiteSpace(vv.standard_template_id) || vv.standard_template_id == "0")
                    {
                        continue;
                    }
                    param.Clear();
                    param["template_id"] = vv.standard_template_id;
                    var cus = this.Invoke<PddRspCloudprintCustomares>(shop, "pdd.cloudprint.customares.get", param);
                    if (cus.result == null || cus.result.datas == null || cus.result.datas.Length == 0)
                        continue;
                    foreach (var av in cus.result.datas)
                    {
                        var pt = new WuliuPrintTemplate { CpCode = cpCode, DeliveryCompany = "", IsIsv = false, Name = av.custom_area_name, PrinterName = "", SourceType = WuliuPrintTemplateSourceType.PINDUODUO, StandTemplateId = vv.standard_template_id, StandTemplateUrl = vv.standard_template_url, UserOrIsvTemplateAreaId = av.custom_area_id, UserOrIsvTemplateAreaUrl = av.custom_area_url };
                        pts.Add(pt);
                    }
                }
            }
            return pts;
        }

        public override WuliuNumber GetWuliuNumber(Shop shop, string popSellerNumberId, WuliuPrintTemplate wuliuTemplate, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress)
        {
            PddReqWaybillGet reqGet = new PddReqWaybillGet();
            reqGet.sender = new PddReqWaybillGetSender { name = senderName, mobile = senderPhone, address = ConvertToAddress(senderAddress, PopType.PINGDUODUO) };
            reqGet.wp_code = wuliuTemplate.CpCode;
            reqGet.need_encrypt = true;
            reqGet.trade_order_info_dtos = new PddReqWaybillGetTradeOrderInfoDto[1] { new PddReqWaybillGetTradeOrderInfoDto() };
            reqGet.trade_order_info_dtos[0].object_id = ((long)DateTime.UtcNow.Subtract(UNIX_START_TIME).TotalSeconds).ToString();
            reqGet.trade_order_info_dtos[0].user_id = popSellerNumberId;
            reqGet.trade_order_info_dtos[0].template_url = wuliuTemplate.StandTemplateUrl;
            reqGet.trade_order_info_dtos[0].recipient = new PddReqWaybillGetTradeOrderInfoDtoRecipient() { name = order.ReceiverName, mobile = order.ReceiverMobile, phone = order.ReceiverPhone, address = ConvertToAddress(order.ReceiverAddress, order.PopType) };
            reqGet.trade_order_info_dtos[0].package_info = new PddReqWaybillGetTradeOrderInfoDtoPackageInfo { id = packageId, items = new PddReqWaybillGetTradeOrderInfoDtoPackageInfoItem[] { new PddReqWaybillGetTradeOrderInfoDtoPackageInfoItem { name = "商品", count = 1 } } };
            reqGet.trade_order_info_dtos[0].order_info = new PddReqWaybillGetTradeOrderInfoDtoOrderInfo { order_channels_type = "OTHER", trade_order_list = wuliuIds };

            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["param_waybill_cloud_print_apply_new_request"] = Newtonsoft.Json.JsonConvert.SerializeObject(reqGet);
            var rspGet = this.Invoke<PddRspWaybillGet>(shop, "pdd.waybill.get", param);
            var wn = new WuliuNumber { CreateTime = DateTime.Now, DeliveryCompany = wuliuTemplate.DeliveryCompany, DeliveryNumber = rspGet.modules[0].waybill_code, PackageId = packageId, PrintData = rspGet.modules[0].print_data, ReceiverAddress = order.ReceiverAddress, ReceiverMobile = order.ReceiverMobile, ReceiverPhone = order.ReceiverPhone, ReceiverName = order.ReceiverName, SourceType = WuliuPrintTemplateSourceType.PINDUODUO, WuliuIds = string.Join(",", wuliuIds) };
            return wn;
        }

        public override void UpdateWuliuNumber(Shop shop, WuliuPrintTemplate wuliuTemplate, Order order, WuliuNumber wuliuNumber)
        {
            PddReqWaybillUpdate reqUpdate = new PddReqWaybillUpdate();
            reqUpdate.waybill_code = wuliuNumber.DeliveryNumber;
            reqUpdate.wp_code = wuliuTemplate.CpCode;
            reqUpdate.recipient = new PddReqWaybillGetTradeOrderInfoDtoRecipient { name = order.ReceiverName, mobile = order.ReceiverMobile, phone = order.ReceiverPhone, address = ConvertToAddress(order.ReceiverAddress, order.PopType) };
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["param_waybill_cloud_print_update_request"] = Newtonsoft.Json.JsonConvert.SerializeObject(reqUpdate);
            var rspUpdate = this.Invoke<PddRspWaybillUpdate>(shop, "pdd.waybill.update", param);
            wuliuNumber.PrintData = rspUpdate.print_data;
        }

        private static string GetNodeName(int type)
        {
            if (type == 1)
            {
                return AddressService.PROVINCE;
            }
            if (type == 2)
            {
                return AddressService.CITY;
            }
            if (type == 3)
            {
                return AddressService.DISTRICT;
            }
            if (type == 4)
            {
                return AddressService.TOWN;
            }
            throw new Exception("拼多多未知的行政区域：" + type);
        }

        private static void FindSub(XElement parent, long parentId, List<PddRspLogisticsAddressGetAddress> areas)
        {
            var aa = areas.Where(obj => parentId == obj.parent_id).ToArray();
            if (aa.Length < 1)
            {
                return;
            }

            foreach (var a in aa)
            {
                string sn = a.region_type == 2 ? AddressService.GetProvinceShortName(a.region_name) : AddressService.GetCityShortName(a.region_name);
                var xe = new XElement(GetNodeName(a.region_type), new XAttribute("Name", a.region_name.Trim()), new XAttribute("ShortName", sn));
                areas.Remove(a);
                parent.Add(xe);
                FindSub(xe, a.id, areas);
            }
        }

        public override XDocument GetAddress(Shop shop)
        {
            var rsp = this.Invoke<PddRspLogisticsAddressGet>(shop, "pdd.logistics.address.get", new SortedDictionary<string, string>());
            XDocument xDoc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf - 8\"?><Address/>");
            var newList = rsp.logistics_address_list.ToList();
            var dd = newList.Where(obj => obj.region_type == 4).ToArray();
            foreach (var d in dd)
            {
                Debug.WriteLine(d.parent_id + " " + d.id + " " + d.region_name + " " + d.region_type);
            }
            FindSub(xDoc.Root, 1, newList);
            if (newList.Count == rsp.logistics_address_list.Length)
            {
                throw new Exception("更新失败：未更新任何数据，请联系技术人员");
            }
            return xDoc;
        }

        public override string AddGoods(Shop shop, PopGoods popGoods, float buyInPrice)
        {
            Dictionary<string, PddRspCatTemplateProperty[]> catTemplatesCaches = new Dictionary<string, PddRspCatTemplateProperty[]>();
            Dictionary<string, string> catIdCaches = new Dictionary<string, string>();
            Dictionary<string, PddRspGoodsSpecItem> specColorCaches = new Dictionary<string, PddRspGoodsSpecItem>();
            Dictionary<string, PddRspGoodsSpecItem> specSizeCaches = new Dictionary<string, PddRspGoodsSpecItem>();
            SortedDictionary<string, string> para = new SortedDictionary<string, string>();
            string id = "";

            //获取对应的运费模板
            var addressNode = Invoke<PddRspAddress>(shop, "pdd.logistics.address.get", para);
            para.Clear();
            para["page_size"] = "20";
            var lotemplates = Invoke<PddRspTemplate>(shop, "pdd.goods.logistics.template.get", para);
            if (lotemplates.logistics_template_list == null || lotemplates.logistics_template_list.Length < 1)
            {
                throw new Exception("拼多多店铺内没有运费模板");
            }
            para.Clear();
            var an = addressNode.logistics_address_list.FirstOrDefault(obj => obj.region_type == 2 && obj.region_name.Contains(popGoods.ShippingCity));
            if (an == null)
            {
                throw new Exception("拼多多地址区库没有找到对应的发货地区");
            }
            PddRspTemplateItem t = null;
            var tt = lotemplates.logistics_template_list.Where(obj => obj.city_id == an.id.ToString()).ToArray();
            if (tt.Length == 1)
            {
                t = tt.First();
            }
            else
            {
                t = tt.FirstOrDefault(obj => obj.template_name.Contains("默认"));
            }
            if (t == null)
            {
                throw new Exception("拼多多店铺内没有找到对发货地区的运费模板");
            }

            //获取商品目录
            para.Clear();
            para["parent_cat_id"] = "0";
            var cats = Invoke<PddRspGoodsCat>(shop, "pdd.goods.cats.get", para);
            var nvxieRootCat = cats.goods_cats_list != null ? cats.goods_cats_list.FirstOrDefault(obj => obj.cat_name == "女鞋") : null;
            if (nvxieRootCat == null)
            {
                throw new Exception("拼多多上没有找到 女鞋 类目");
            }
            if (catIdCaches.ContainsKey(popGoods.Type) == false)
            {
                //获取第二级目录
                para.Clear();
                para["parent_cat_id"] = nvxieRootCat.cat_id;
                cats = Invoke<PddRspGoodsCat>(shop, "pdd.goods.cats.get", para);
                var typeCat = cats.goods_cats_list != null ? cats.goods_cats_list.FirstOrDefault(obj => obj.cat_name == popGoods.Type) : null;
                if (typeCat == null)
                {
                    throw new Exception("拼多多上没有找到 " + popGoods.Type + " 类目");
                }
                //获取第三级目录
                para.Clear();
                para["parent_cat_id"] = typeCat.cat_id;
                cats = Invoke<PddRspGoodsCat>(shop, "pdd.goods.cats.get", para);
                var leve3Cat = cats.goods_cats_list != null ? cats.goods_cats_list.FirstOrDefault() : null;
                if (leve3Cat == null)
                {
                    throw new Exception("拼多多上没有找到第三级目录 " + popGoods.Type + " 类目");
                }
                if (popGoods.Type == "靴子")
                {
                    leve3Cat = cats.goods_cats_list.FirstOrDefault(obj => obj.cat_name.Contains("皮靴"));
                }
                catIdCaches[popGoods.Type] = leve3Cat.cat_id;
            }

            string level3CatId = catIdCaches[popGoods.Type];
            //获取颜色，尺码规格
            if (specColorCaches.ContainsKey(popGoods.Type) == false)
            {
                para.Clear();
                para["cat_id"] = level3CatId;
                var specs = Invoke<PddRspGoodsSpec>(shop, "pdd.goods.spec.get", para);
                var specColor = specs.goods_spec_list != null ? specs.goods_spec_list.FirstOrDefault(obj => obj.parent_spec_name == "颜色") : null;
                var specSize = specs.goods_spec_list != null ? specs.goods_spec_list.FirstOrDefault(obj => obj.parent_spec_name == "尺码") : null;
                if (specColor == null || specSize == null)
                {
                    throw new Exception("拼多多上获取颜色，尺码规格失败");
                }
                specColorCaches[popGoods.Type] = specColor;
                specSizeCaches[popGoods.Type] = specSize;
            }

            if (catTemplatesCaches.ContainsKey(level3CatId) == false)
            {
                //获取商品属性
                para.Clear();
                para["cat_id"] = level3CatId;
                var ct = Invoke<PddRspCatTemplate>(shop, "pdd.goods.cat.template.get", para);
                catTemplatesCaches[level3CatId] = ct.properties.Where(obj => string.IsNullOrWhiteSpace(obj.name) == false).ToArray();
            }

            //生成商品属性
            List<PddReqGoodsProperty> properties = new List<PddReqGoodsProperty>();
            foreach (var ctp in catTemplatesCaches[level3CatId])
            {
                string propertyValue = "";
                //根据拼多多的属性名称，找到对应映射关系
                PddGoodsPropertyMapItem pddGoodsPropertyMapItem = PddGoodsPropertyMap.GetMapPropertyByKey(popGoods.PopType, popGoods.Type, ctp.name_alias);
                if (pddGoodsPropertyMapItem == null)
                {
                    if (ctp.required)
                    {
                        throw new Exception("拼多多属性：" + ctp.name_alias + " 是必须项，但没有在映射配置中找到对应项，请添加映射关系");
                    }
                    id += "拼多多属性：" + ctp.name_alias + " 没有在映射配置中找到对应项，请添加映射关系";
                    continue;
                }

                if (ctp.name_alias == "品牌")
                {
                    if (ctp.required == false)
                    {
                        continue;
                    }
                    if (shop.PopShopName.Contains("旗舰店") == false && shop.PopShopName.Contains("专卖店") == false)
                    {
                        throw new Exception("属性 品牌 是必须值 但 店铺名称不包含旗舰店：" + shop.PopShopName);
                    }
                    propertyValue = shop.PopShopName.Replace("旗舰店", "").Replace("专卖店", "").Replace("官方", "");
                }
                else if (ctp.name_alias == "上市时节")
                {
                    propertyValue = DateTime.Now.Year + "年" + ((DateTime.Now.Month <= 6) ? "春季" : "冬季");
                }
                else
                {
                    var otherPopProperty = popGoods.Properties.FirstOrDefault(obj => obj.Key == pddGoodsPropertyMapItem.OtherPopName);
                    if (otherPopProperty == null && string.IsNullOrWhiteSpace(pddGoodsPropertyMapItem.DefaultValue))
                    {
                        if (ctp.required)
                        {
                            throw new Exception("拼多多属性：" + ctp.name_alias + " 是必须项，但没有在淘宝找到对应属性");
                        }
                        else
                        {
                            id += "属性：" + ctp.name_alias + " 没有在淘宝找到对应属性项";
                        }
                        continue;
                    }
                    propertyValue = otherPopProperty == null ? pddGoodsPropertyMapItem.DefaultValue : otherPopProperty.Value;
                    if (string.IsNullOrWhiteSpace(propertyValue))
                    {
                        if (ctp.required)
                        {
                            throw new Exception("拼多多属性：" + ctp.name_alias + " 是必须项，但没有在淘宝找到对应属性中没有相应值");
                        }
                        continue;
                    }
                }

                string[] values = propertyValue.Split(new string[] { "@#@" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var vv in values)
                {
                    var v = ctp.values.FirstOrDefault(obj => MatchValue(obj.value.Trim(), vv.Trim()));
                    if (v == null)
                    {
                        var pddvv = pddGoodsPropertyMapItem.SubValuesMap.ContainsKey(vv) ? pddGoodsPropertyMapItem.SubValuesMap[vv] : "";
                        v = ctp.values.FirstOrDefault(obj => MatchValue(obj.value.Trim(), pddvv.Trim()));
                    }
                    if (v != null)
                    {
                        properties.Add(new PddReqGoodsProperty { template_pid = ctp.id, vid = long.Parse(v.vid), value = v.value });
                    }
                    else
                    {
                        id += "属性值：" + ctp.name_alias + " 的值：" + vv + " 未匹配";
                    }
                    if (properties.Count(obj => obj.template_pid == ctp.id) >= ctp.choose_max_num)
                    {
                        break;
                    }
                }
            }

            //上传图片,拼多多不传白底图
            popGoods.Images = popGoods.Images.Where(obj => obj.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false).ToArray();
            string[] skuSourceImages = popGoods.Skus.Select(obj => obj.Image).Distinct().ToArray();
            string[] images = UploadImage(shop, popGoods.Images);
            string[] descImages = UploadImage(shop, popGoods.DescImages);
            string[] skuImages = UploadImage(shop, skuSourceImages);
            //拼多多图片张数达到10张，商品分值会高些
            if (images.Length < 10)
            {
                var im = new List<string>();
                while (im.Count < 10)
                {
                    im.AddRange(images);
                }
                images = im.GetRange(0, 10).ToArray();
            }

            //生成颜色尺码规格
            string[] sColors = popGoods.Skus.Select(obj => obj.Color.Trim()).Distinct().ToArray();
            string[] sSize = popGoods.Skus.Select(obj => obj.Size.Trim()).Distinct().ToArray();
            PddRspGoodsSpecId[] colors = new PddRspGoodsSpecId[sColors.Length];
            PddRspGoodsSpecId[] sizes = new PddRspGoodsSpecId[sSize.Length];
            for (int j = 0; j < sColors.Length; j++)
            {
                para.Clear();
                para["parent_spec_id"] = specColorCaches[popGoods.Type].parent_spec_id;
                para["spec_name"] = sColors[j];
                colors[j] = Invoke<PddRspGoodsSpecId>(shop, "pdd.goods.spec.id.get", para);
            }
            for (int j = 0; j < sizes.Length; j++)
            {
                para.Clear();
                para["parent_spec_id"] = specSizeCaches[popGoods.Type].parent_spec_id;
                para["spec_name"] = sSize[j];
                sizes[j] = Invoke<PddRspGoodsSpecId>(shop, "pdd.goods.spec.id.get", para);
            }
            //SKU
            List<PddReqSku> skus = new List<PddReqSku>();
            for (int j = 0; j < popGoods.Skus.Count; j++)
            {
                var sku = new PddReqSku();
                //价格
                sku.multi_price = (long)(100 * (float.Parse(popGoods.Skus[j].Price) > (buyInPrice * 2) ? (float.Parse(popGoods.Skus[j].Price) / 2) : float.Parse(popGoods.Skus[j].Price)));
                sku.price = sku.multi_price + 100;
                sku.out_sku_sn = popGoods.Skus[j].Code;
                sku.thumb_url = skuImages[Array.IndexOf(skuSourceImages, popGoods.Skus[j].Image)];
                sku.spec_id_list = string.Format("[{0},{1}]", colors[Array.FindIndex(colors, 0, o => o.spec_name == popGoods.Skus[j].Color)].spec_id, sizes[Array.FindIndex(sizes, 0, o => popGoods.Skus[j].Size.Trim() == o.spec_name)].spec_id);
                skus.Add(sku);
            }

            //拼装参数
            para.Clear();
            para["goods_properties"] = Newtonsoft.Json.JsonConvert.SerializeObject(properties);
            para["goods_name"] = popGoods.Title.Trim();
            para["goods_type"] = "1";
            para["goods_desc"] = "商品跟高，材质，尺码，内里，请往下滑，详情中均有说明";
            para["cat_id"] = level3CatId;
            para["country_id"] = "0";
            para["market_price"] = (((int)(buyInPrice * 3)) * 100).ToString();
            para["is_pre_sale"] = "false";
            para["shipment_limit_second"] = (48 * 60 * 60).ToString();
            para["cost_template_id"] = t.template_id.ToString();
            para["customer_num"] = "2";
            para["is_refundable"] = "true";
            para["second_hand"] = "false";
            para["is_folt"] = "true";
            para["out_goods_id"] = popGoods.Code.Trim();
            para["carousel_gallery"] = Newtonsoft.Json.JsonConvert.SerializeObject(images);
            para["detail_gallery"] = Newtonsoft.Json.JsonConvert.SerializeObject(descImages);
            para["sku_list"] = Newtonsoft.Json.JsonConvert.SerializeObject(skus);
            //第三步上传信息
            var ret = Invoke<PddRspGoodsAdd>(shop, "pdd.goods.add", para);
            id += ret.goods_id;
            return id;
        }

        /// <summary>
        /// 会删除括号及里面的内容进行比较
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        private bool MatchValue(string value1, string value2)
        {
            int i1 = value1.IndexOfAny(LEFT_S);
            int i2 = value2.IndexOfAny(LEFT_S);

            if (i1 > 0)
            {
                value1 = value1.Substring(0, i1).Trim();
            }
            if (i2 > 0)
            {
                value2 = value2.Substring(0, i2).Trim();
            }
            return value1.Equals(value2, StringComparison.OrdinalIgnoreCase);
        }

        private PddReqWaybillGetSenderAddress ConvertToAddress(string senderAddress, PopType sourceType)
        {
            string[] ss = AddressService.Parse5Address(senderAddress, sourceType, PopType.PINGDUODUO);
            PddReqWaybillGetSenderAddress address = new PddReqWaybillGetSenderAddress
            {
                province = ss[0],
                city = ss[1],
                district = ss[2],
                town = ss[3],
                detail = ss[4],
            };
            return address;
        }
    }
}
