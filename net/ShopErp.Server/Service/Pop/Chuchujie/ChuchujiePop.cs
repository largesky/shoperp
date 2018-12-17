using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ShopErp.Server.Service.Restful;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Service.Net;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.Server.Service.Pop.Chuchujie
{
    public class ChuchujiePop : PopBase
    {
        private const string API_ORDER_GET_URL = "https://parter.api.chuchujie.com/sqe/Order/get_order_list_v2";
        private const string API_ORDER_GET_V3_URL = "https://parter.api.chuchujie.com/sqe/Order/get_order_list_v3";
        private const string API_ORDER_SHIPPING_URL = "https://parter.api.chuchujie.com/sqe/Order/api_order_shipping_v2";
        private const string API_GOODS_GET_URL = "https://parter.api.chuchujie.com/sqe/Order/get_goodsinfo_for_key";

        public override PopOrderGetFunction OrderGetFunctionType { get { return PopOrderGetFunction.ALWAYS; } }

        private T InvokeOpenApi<T>(string serviceUrl, Shop shop, Dictionary<string, string> para) where T : ChuchujieResponseBase
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            string timeStamp = ((long)DateTime.UtcNow.Subtract(UNIX_START_TIME).TotalSeconds).ToString();
            string rand = random.Next(1, 99999).ToString();
            string sign = string.Join("", sha.ComputeHash(Encoding.UTF8.GetBytes(rand + shop.AppAccessToken + timeStamp)).Select(obj => obj.ToString("X2").ToLower()));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Org-Name"] = shop.AppKey;
            headers["App-Key"] = shop.AppSecret;
            headers["Nonce"] = rand;
            headers["Timestamp"] = timeStamp;
            headers["Signature"] = sign;
            var content = MsHttpRestful.GetUrlEncodeBodyReturnString(serviceUrl, para, Encoding.UTF8, headers);

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("接口调用失败：返回空数据");
            }
            if (content[0] != '{')
            {
                if (content.IndexOf('{') < 0)
                {
                    throw new Exception("楚楚返回错误数据");
                }
                content = content.Substring(content.IndexOf('{'));
            }
            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
            if (ret.code != "0")
            {
                throw new Exception("调用失败:" + ret.message);
            }
            return ret;
        }

        protected Domain.Order ConvertToOrder(ChuchujieOrder order)
        {
            var minTime = new DateTime(1970, 01, 01);
            string add = AddressService.TrimStart(order.address.address, order.address.province ?? "");
            add = AddressService.TrimStart(add, order.address.city ?? "");
            add = AddressService.TrimStart(add, order.address.district ?? "");

            if (order.address.city == "省直辖行政单位")
            {
                order.address.city = order.address.district;
                order.address.district = "";
                add = AddressService.TrimStart(add, order.address.province);
                add = AddressService.TrimStart(add, order.address.city);
            }
            if (order.address.district == "行政区")
            {
                var a = AddressService.ParseRegion(order.address.address.Replace(order.address.district, ""));
                if (a != null)
                {
                    add = AddressService.TrimStart(add, a.Name);
                }
                order.address.district = a == null ? "" : a.Name;
            }

            var o = new Domain.Order
            {
                CloseOperator = "",
                CloseTime = minTime,
                PopState = order.order.status ?? "",
                DeliveryNumber = order.order.express_id ?? "",
                CreateOperator = "",
                CreateTime = DateTime.Now,
                DeliveryCompany = order.order.express_company ?? "",
                DeliveryOperator = "",
                PopDeliveryTime = string.IsNullOrWhiteSpace(order.order.send_time) ? minTime : DateTime.Parse(order.order.send_time),
                DeliveryMoney = 0,
                DeliveryTime = minTime,
                OrderGoodss = new List<Domain.OrderGoods>(),
                ParseResult = false,
                PopBuyerComment = order.order.comment ?? "",
                PopBuyerId = order.address.nickname ?? "",
                PopCodNumber = "",
                PopCodSevFee = 0,
                PopCreateTime = DateTime.Parse(order.order.ctime),
                PopFlag = Domain.ColorFlag.UN_LABEL,
                PopOrderId = order.order.order_id,
                PopOrderTotalMoney = order.order.total_price,
                PopPayTime = string.IsNullOrWhiteSpace(order.order.pay_time) ? minTime : DateTime.Parse(order.order.pay_time),
                PopPayType = Domain.PopPayType.ONLINE,
                PopSellerComment = order.order.seller_note ?? "",
                PopType = Domain.PopType.CHUCHUJIE,
                PrintOperator = "",
                PrintTime = minTime,
                ReceiverAddress = string.Join(" ", order.address.province, order.address.city, order.address.district, add),
                ReceiverMobile = order.address.phone ?? "",
                ReceiverName = order.address.nickname ?? "",
                ReceiverPhone = "",
                ShopId = 0,
                Weight = 0,
                State = Domain.OrderState.NONE,
                CreateType = Domain.OrderCreateType.DOWNLOAD,
                Type = Domain.OrderType.NORMAL,
                PopBuyerPayMoney = order.order.order_pay_price,
                PopSellerGetMoney = order.order.total_price - order.order.shop_coupon_price - order.order.shop_off_price,
            };

            if (order.order.status == "1")
            {
                o.State = Domain.OrderState.WAITPAY;
            }
            else if (order.order.status == "2")
            {
                o.State = Domain.OrderState.PAYED;
            }
            else if (order.order.status == "3")
            {
                o.State = Domain.OrderState.SHIPPED;
            }
            else if (order.order.status == "4")
            {
                o.State = Domain.OrderState.SUCCESS;
            }
            else if (order.order.status == "5")
            {
                o.State = Domain.OrderState.CLOSED;
            }
            else if (order.order.status == "6")
            {
                o.State = Domain.OrderState.CANCLED;
            }
            else if (order.order.status == null || order.order.status.Equals("NONE", StringComparison.OrdinalIgnoreCase) || order.order.status == "0") //交易关闭后，返回内容为null
            {
                if (o.PopPayTime == minTime)
                {
                    o.State = Domain.OrderState.CANCLED;
                }
                else
                {
                    o.State = Domain.OrderState.CLOSED;
                }
            }
            else if (order.order.status == "7")
            {
                o.State = Domain.OrderState.RETURNING;
            }
            else
            {
                throw new Exception("无法识别的订单状态:" + order.order.order_id + "," + order.order.status + "," + order.order.status_text);
            }
            foreach (var og in order.goods)
            {
                var no = new Domain.OrderGoods
                {
                    CloseOperator = "",
                    CloseTime = minTime,
                    Color = "",
                    Comment = "",
                    Count = int.Parse(og.amount),
                    Edtion = "",
                    GetedCount = 0,
                    Id = 0,
                    Image = og.goods_img,
                    Number = og.outer_id,
                    NumberId = 0,
                    OrderId = 0,
                    PopInfo = og.outer_id + " " + string.Join(" ", og.prop.Select(obj => obj.name + ":" + obj.value)),
                    PopNumber = og.goods_no,
                    PopOrderSubId = order.order.order_id + "_" + o.OrderGoodss.Count,
                    PopPrice = og.price,
                    PopRefundState = Domain.PopRefundState.NOT,
                    PopUrl = og.goods_id,
                    Price = 0,
                    Size = "",
                    State = o.State,
                    StockOperator = "",
                    StockTime = minTime,
                    Vendor = "",
                    Weight = 0,
                };

                //颜色，尺码
                if (og.prop != null)
                {
                    var cp = og.prop.FirstOrDefault(obj => obj.name.Contains("颜色"));
                    var sp = og.prop.FirstOrDefault(obj => obj.name.Contains("尺码"));
                    no.Color = cp == null ? "" : cp.value;
                    no.Size = sp == null ? "" : sp.value;
                }

                //检查商品是否在退款中
                if (string.IsNullOrWhiteSpace(og.refund_status_text) || og.refund_status_text == "0" || og.refund_status_text == "无")
                {
                    no.PopRefundState = Domain.PopRefundState.NOT;
                    no.State = o.State;
                }
                else if (og.refund_status_text == "3" || og.refund_status_text == "退款关闭")
                {
                    no.PopRefundState = Domain.PopRefundState.CANCEL;
                    no.State = o.State;
                }
                else if (og.refund_status_text == "退款已完成" || og.refund_status_text == "退款完成")
                {
                    no.PopRefundState = Domain.PopRefundState.OK;
                    no.State = Domain.OrderState.CLOSED;
                }
                else
                {
                    no.PopRefundState = Domain.PopRefundState.ACCEPT;
                    no.State = Domain.OrderState.RETURNING;
                }
                o.OrderGoodss.Add(no);
            }
            if (o.OrderGoodss.Count == 1)
            {
                o.State = o.OrderGoodss[0].State;
            }
            return o;

        }

        public override bool Accept(Domain.PopType popType)
        {
            return popType == Domain.PopType.CHUCHUJIE;
        }

        public override OrderDownload GetOrder(Shop shop, string popOrderId)
        {
            var ret = new OrderDownload();
            try

            {
                Dictionary<string, string> para = new Dictionary<string, string>();
                para["order_id"] = popOrderId;
                ChuchujieGetOrderResponse resp = this.InvokeOpenApi<ChuchujieGetOrderResponse>(API_ORDER_GET_URL, shop, para);
                if (resp.info == null || resp.info.Length < 1)
                {
                    throw new Exception("订单不存在");
                }
                ret.Order = ConvertToOrder(resp.info[0]);
            }
            catch (Exception ex)
            {
                ret.Error = new OrderDownloadError(popOrderId, "", ex.Message);
            }
            return ret;
        }

        public override OrderDownloadCollectionResponse GetOrders(Shop shop, string state, int pageIndex, int pageSize)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            var ret = new OrderDownloadCollectionResponse() { IsTotalValid = true };

            //状态参数
            if (state == PopService.QUERY_STATE_WAITSHIP)
            {
                para["status"] = "2";
            }
            else if (state == PopService.QUERY_STATE_WAITSHIP_COD)
            {
                return ret;
            }
            para["page"] = pageIndex.ToString();
            para["page_size"] = pageSize.ToString();

            var resp = this.InvokeOpenApi<ChuchujieGetOrderResponse>(API_ORDER_GET_URL, shop, para);

            if (resp.info == null || resp.info.Length < 1)
            {
                return ret;
            }

            ret.Total = resp.total_num;
            foreach (var or in resp.info)
            {
                var e = new OrderDownload();

                try
                {
                    e.Order = this.ConvertToOrder(or);
                }
                catch (Exception ex)
                {
                    e.Error = new OrderDownloadError(or.order.order_id, or.address.nickname, ex.Message);
                }
                ret.Datas.Add(e);
            }

            return ret;
        }

        public override PopOrderState GetOrderState(Shop shop, string popOrderId)
        {
            var order = this.GetOrder(shop, popOrderId).Order;
            var orderState = new PopOrderState
            {
                PopOrderId = popOrderId,
                PopOrderStateDesc = order.PopState,
                PopOrderStateValue = order.PopState,
                State = order.State,
            };
            return orderState;
        }

        public override void ModifyComment(Shop shop, string popOrderId, string comment, Domain.ColorFlag flag)
        {
        }

        public override void MarkDelivery(Shop shop, string popOrderId, Domain.PopPayType payType, string deliveryCompany, string deliveryNumber)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para["oid"] = popOrderId;
            para["express_no"] = deliveryNumber;
            para["express_company"] = ServiceContainer.GetService<DeliveryCompanyService>().GetDeliveryCompany(deliveryCompany).First.PopMapChuchujie;
            //读取订单状态
            var order = this.GetOrder(shop, popOrderId).Order;
            if (order == null)
            {
                throw new Exception("订单不存在:" + popOrderId);
            }

            if (order.PopState == "1")
            {
                throw new Exception("订单未付款不能发货");
            }
            if (order.PopState != "3" && order.PopState != "4" && order.PopState != "2")
            {
                throw new Exception("当前订单状态不允许发货:" + order.State + " " + order.PopState);
            }

            //已经发过货，且当前单号相同则不用再标记发货，否则就需要再次标记
            if (string.IsNullOrWhiteSpace(order.DeliveryNumber) == false && deliveryNumber.Equals(order.DeliveryNumber))
            {
                return;
            }
            var response = this.InvokeOpenApi<ChuchujieMarkDeliveryResponse>(API_ORDER_SHIPPING_URL, shop, para);
        }

        public override PopDeliveryInfo GetDeliveryInfo(Shop shop, string popOrderId)
        {
            //读取订单状态
            var order = this.GetOrder(shop, popOrderId).Order;
            if (order == null)
            {
                throw new Exception("订单不存在:" + popOrderId);
            }

            var di = new PopDeliveryInfo { DeliveryCompany = order.DeliveryCompany, DeliveryNumber = order.DeliveryNumber };
            return di;
        }

        public override List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            var goods = new List<PopGoods>();
            param["page"] = pageIndex.ToString();
            if (state == PopGoodsState.NONE)
            {
                //param["goods_status"] = "3";
            }
            else if (state == PopGoodsState.ONSALE)
            {
                param["goods_status"] = "1";
            }
            else if (state == PopGoodsState.NOTSALE)
            {
                param["goods_status"] = "2";
            }
            else if (state == PopGoodsState.NEVERSALE)
            {
                param["goods_status"] = "0";
            }
            else
            {
                throw new Exception("状态错误:" + state);
            }
            try
            {
                ChuchujieGoodsResponse resp = this.InvokeOpenApi<ChuchujieGoodsResponse>(ChuchujiePop.API_GOODS_GET_URL, shop, param);
                if (resp.info == null || resp.info.Count < 1)
                {
                    return goods;
                }
                foreach (var v in resp.info)
                {
                    if (v.sku != null)
                    {
                        v.sku.RemoveAll(new Predicate<ChuchujieGoodsResponseGoodsSku>(obj => obj.sku_status == "1"));
                    }
                    else
                    {
                        v.sku = new List<ChuchujieGoodsResponseGoodsSku>();
                    }
                    var g = new PopGoods
                    {
                        AddTime = v.add_time,
                        Code = v.goods_code,
                        Id = v.goods_id,
                        Image = v.goods_img,
                        SaleNum = v.sale_num,
                        State = PopGoodsState.NONE,
                        Title = v.goods_title,
                        UpdateTime = v.update_time,
                        CatId = "",
                    };
                    g.Skus.AddRange(v.sku.Select(obj => new PopGoodsSku { Code = obj.sku_code, Id = obj.sku_id, Price = obj.sku_price, PropId = obj.prop_id, Status = obj.sku_status, Stock = obj.sku_stock, Value = obj.value }));
                    if ("0" == v.goods_status)
                    {
                        g.State = PopGoodsState.NEVERSALE;
                    }
                    else if ("1" == v.goods_status)
                    {
                        g.State = PopGoodsState.ONSALE;
                    }
                    else if ("2" == v.goods_status)
                    {
                        g.State = PopGoodsState.NOTSALE;
                    }
                    else
                    {
                        g.State = PopGoodsState.NONE;
                    }

                    goods.Add(g);
                }
                return goods;
            }
            catch (Exception ex)
            {
                if (ex.Message != "调用失败:")
                    throw ex;
                return goods;
            }
        }

        public override string GetShopOauthUrl(Shop sop)
        {
            throw new NotImplementedException("楚楚街暂时不支持自动授权");
        }

        public override Shop GetAcessTokenInfo(Shop shop, string code)
        {
            throw new NotImplementedException("楚楚街暂时不支持自动授权");
        }

        public override Shop GetRefreshTokenInfo(Shop shop)
        {
            throw new NotImplementedException("楚楚街暂时不支持自动授权");
        }
    }
}
