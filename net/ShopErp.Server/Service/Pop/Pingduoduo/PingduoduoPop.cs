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

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    class PingduoduoPop : PopBase
    {
        private const string SERVER_URL = "http://gw-api.pinduoduo.com/api/router";

        public override PopOrderGetFunction OrderGetFunctionType { get { return PopOrderGetFunction.PAYED; } }

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

        private T Invoke<T>(Domain.Shop shop, string apiName, SortedDictionary<string, string> param, int trimJsonWrapperLevel = 1) where T : PingduoduoRspBase
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
                throw new Exception("拼多多调用失败：" + t.error_code + "," + t.error_msg);
            }
            return t;
        }

        private PingduoduoRspGetOrderStateOrder GetOrderStatePingduoduo(Domain.Shop shop, string popOrderId)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param["order_sns"] = popOrderId;
            var rsp = this.Invoke<PingduoduoRspGetOrderState>(shop, "pdd.order.status.get", param);

            if (rsp.order_status_list == null || rsp.order_status_list.Length < 1)
            {
                throw new Exception("拼多多查询状态返回空数据");
            }

            return rsp.order_status_list[0];
        }

        private Domain.OrderState ConvertToOrderState(string popOrderId, PingduoduoRspGetOrderStateOrder state)
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

        public override OrderDownload GetOrder(Domain.Shop shop, string popOrderId)
        {
            OrderDownload od = new OrderDownload();
            try
            {
                SortedDictionary<string, string> para = new SortedDictionary<string, string>();
                para["order_sn"] = popOrderId;
                var rsp = this.Invoke<PingduoduoRspGetOrder>(shop, "pdd.order.information.get", para, 2);

                var o = rsp;
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
                    PopDeliveryTime = DateTime.MinValue,
                    OrderGoodss = new List<Domain.OrderGoods>(),
                    ParseResult = false,
                    PopBuyerComment = "",
                    PopBuyerId = "",
                    PopBuyerPayMoney = float.Parse(o.pay_amount),
                    PopCodNumber = "",
                    PopCodSevFee = 0,
                    PopCreateTime = DateTime.Parse(o.created_time),
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
                    ShopId = 0,
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
                            CloseTime = DateTime.MinValue,
                            Color = "",
                            Comment = "",
                            Count = goods.goods_count,
                            Edtion = "",
                            GetedCount = 0,
                            Id = 0,
                            Image = goods.goods_img,
                            Number = goods.outer_id,
                            NumberId = 0,
                            OrderId = 0,
                            PopInfo = goods.outer_id + " " + goods.goods_spec,
                            PopNumber = "",
                            PopOrderSubId = "",
                            PopPrice = goods.goods_price,
                            PopRefundState = Domain.PopRefundState.NOT,
                            PopUrl = goods.goods_id,
                            Price = 0,
                            Size = "",
                            State = Domain.OrderState.NONE,
                            StockOperator = "",
                            StockTime = DateTime.MinValue,
                            Vendor = "",
                            Weight = 0,
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
                var os = ConvertToOrderState(popOrderId, new PingduoduoRspGetOrderStateOrder { orderSn = popOrderId, order_status = o.order_status, refund_status = o.refund_status });
                order.State = os;
                order.OrderGoodss[0].State = os;
                od.Order = order;
            }
            catch (Exception e)
            {
                od.Error = new OrderDownloadError(popOrderId, "", e.Message);
            }
            return od;
        }

        public override OrderDownloadCollectionResponse GetOrders(Shop shop, string state, int pageIndex, int pageSize)
        {
            SortedDictionary<string, string> para = new SortedDictionary<string, string>();
            var ret = new OrderDownloadCollectionResponse { IsTotalValid = false };

            if (state == PopService.QUERY_STATE_WAITSHIP_COD)
            {
                return ret;
            }
            para["order_status"] = "1";

            para["page"] = (pageIndex + 1).ToString();
            para["page_size"] = pageSize.ToString();
            var resp = this.Invoke<PingduoduoRspOrder>(shop, "pdd.order.number.list.get", para);

            if (resp.order_sn_list == null || resp.order_sn_list.Length < 1)
            {
                return ret;
            }

            ret.Total = resp.total_count;

            foreach (var or in resp.order_sn_list)
            {
                var e = this.GetOrder(shop, or.order_sn);
                ret.Datas.Add(e);
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
                PopOrderStateDesc = orderState.order_status,
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
            var rsp = this.Invoke<PingduoduoRspShipping>(shop, "pdd.logistics.online.send", param);
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
            var ret = this.Invoke<PingduoduoRspGoods>(shop, "pdd.goods.list.get", param);
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
                    Image = g.thumb_url,
                    CatId = "",
                    Code = "",
                    State = g.is_onsale == "1" ? PopGoodsState.ONSALE : PopGoodsState.NOTSALE,
                };
                if (g.sku_list == null || g.sku_list.Length < 1)
                {
                    continue;
                }
                foreach (var sku in g.sku_list)
                {
                    pg.Code = sku.outer_goods_id;
                    pg.Skus.Add(new PopGoodsSku
                    {
                        Id = sku.sku_id,
                        Code = sku.outer_id,
                        Value = sku.spec,
                        PropId = sku.outer_goods_id,
                        Stock = sku.sku_quantity.ToString(),
                        Status = "ONSALE",
                    });
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
            string url = string.Format("http://mms.pinduoduo.com/open.html?response_type=code&client_id={0}&redirect_uri={1}&state={2}", shop.AppKey, shop.AppCallbackUrl, shop.Id);
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
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<PingduoduoRspAccessToken>(bodyContent);
            if (string.IsNullOrWhiteSpace(t.error_code) == false)
            {
                throw new Exception("拼多多调用失败：" + t.error_code + "," + t.error_msg);
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
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<PingduoduoRspAccessToken>(bodyContent);
            if (string.IsNullOrWhiteSpace(t.error_code) == false)
            {
                throw new Exception("拼多多调用失败：" + t.error_code + "," + t.error_msg);
            }

            shop.AppAccessToken = t.access_token;
            shop.AppRefreshToken = t.refresh_token;
            shop.PopSellerNumberId = t.owner_id;
            return shop;
        }
    }
}
