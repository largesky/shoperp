using System;
using System.Collections.Generic;
using System.Linq;
using ShopErp.Server.Service.Restful;
using ShopErp.Domain;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.Server.Service.Pop.Taobao
{
    public class TaobaoPop : PopBase
    {
        private const string API_SERVER_URL = "http://121.41.163.90/router/rest";
        private const string API_SERVER_URL_SANDBOX = "http://gw.api.tbsandbox.com/router/rest";

        public override PopOrderGetFunction OrderGetFunctionType { get { return PopOrderGetFunction.ALWAYS; } }

        private ITopClient GetTopClient(string appKey, string appSecret)
        {
            var client = new DefaultTopClient(API_SERVER_URL, appKey, appSecret);
            return client as DefaultTopClient;
        }

        private OrderState ConvertPopStateToSystemState(string state)
        {
            if (state == "NEW_CREATED" || state == "ACCEPTED_BY_COMPANY" || state == "REJECTED_BY_COMPANY" || state == "RECIEVE_TIMEOUT" ||
                state == "TAKEN_IN_FAILED" || state == "TAKEN_TIMEOUT" || state == "WAITING_TO_BE_SENT")
            {
                return OrderState.PAYED;
            }
            else if (state == "TAKEN_IN_SUCCESS")
            {
                return OrderState.SHIPPED;
            }
            else if (state == "CANCELED")
            {
                return OrderState.CANCLED;
            }
            else if (state == "SIGN_IN")
            {
                return OrderState.SIGNED;
            }
            else if (state == "TRADE_NO_CREATE_PAY" || state == "WAIT_BUYER_PAY")
            {
                return OrderState.WAITPAY;
            }
            else if (state == "WAIT_SELLER_SEND_GOODS")
            {
                return OrderState.PAYED;
            }
            else if (state == "SELLER_CONSIGNED_PART" || state == "WAIT_BUYER_CONFIRM_GOODS")
            {
                return OrderState.SHIPPED;
            }
            else if (state == "TRADE_BUYER_SIGNED")
            {
                return OrderState.SIGNED;
            }
            else if (state == "TRADE_FINISHED")
            {
                return OrderState.SUCCESS;
            }
            else if (state == "TRADE_CLOSED")
            {
                return OrderState.CLOSED;
            }
            else if (state == "TRADE_CLOSED_BY_TAOBAO")
            {
                return OrderState.CANCLED;
            }
            else if (state == "PAY_PENDING")
            {
                return OrderState.WAITPAY;
            }
            else if (state == "WAIT_PRE_AUTH_CONFIRM")
            {
                return OrderState.SHIPPED;
            }

            throw new Exception("未能转换的状态:" + state);
        }

        private ColorFlag ConverPopFlagToOrderFlag(long flag)
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

            throw new Exception("未能识别的淘宝旗帜:" + flag);
        }

        public T InvokeOpenApi<T>(Domain.Shop shop, ITopRequest<T> request) where T : TopResponse
        {
            var topClient = GetTopClient(shop.AppKey, shop.AppSecret);
            var ret = topClient.Execute<T>(request, shop.AppAccessToken, DateTime.Now);
            if (ret.IsError)
            {
                throw new Exception("执行淘宝请求出错:" + ret.ErrCode + "," + ret.ErrMsg + ret.SubErrMsg);
            }
            return ret;
        }

        private Order ConvertPopOrderToDomainOrder(Top.Api.Domain.Trade trade)
        {
            var order = new ShopErp.Domain.Order
            {
                PopOrderId = trade.Tid.ToString(),
                PopPayType = trade.Type.ToUpper().Contains("COD") ? PopPayType.COD : PopPayType.ONLINE,
                PopState = trade.Status,
                PopFlag = ConverPopFlagToOrderFlag(trade.SellerFlag),
                PopOrderTotalMoney = float.Parse(trade.Payment) + float.Parse(trade.DiscountFee ?? "0"),
                PopDeliveryTime = DateTime.Parse(trade.ConsignTime ?? "1970-01-01 00:00:01"),
                PopCodNumber = "",
                DeliveryCompany = "",
                DeliveryNumber = "",
                PopBuyerId = trade.BuyerNick,
                ReceiverName = trade.ReceiverName,
                ReceiverMobile = trade.ReceiverMobile ?? "",
                ReceiverPhone = trade.ReceiverPhone ?? "",
                ReceiverAddress = trade.ReceiverState + " " + trade.ReceiverCity + " " + trade.ReceiverDistrict + " " + trade.ReceiverAddress,
                PopBuyerComment = trade.BuyerMessage ?? "",
                PopSellerComment = trade.SellerMemo ?? "",
                PopType = PopType.TAOBAO,
                PopCreateTime = DateTime.Parse(trade.Created),
                PopPayTime = trade.Type.ToUpper().Contains("COD") ? DateTime.Parse(trade.Created) : DateTime.Parse(trade.PayTime ?? "1970-01-01 00:00:01"),
                PopCodSevFee = float.Parse(trade.SellerCodFee ?? "0.0"),
                OrderGoodss = new List<OrderGoods>(),
                CreateType = OrderCreateType.DOWNLOAD,
                Type = OrderType.NORMAL,
                CloseOperator = "",
                CloseTime = DateTime.MinValue,
                CreateOperator = "",
                CreateTime = DateTime.MinValue,
                DeliveryOperator = "",
                DeliveryTime = DateTime.MinValue,
                DeliveryMoney = 0,
                Id = 0,
                ParseResult = false,
                PopBuyerPayMoney = float.Parse(trade.Payment),
                PopSellerGetMoney = float.Parse(trade.Payment),
            };
            try
            {
                order.State = ConvertPopStateToSystemState(order.PopState);
            }
            catch (Exception ex)
            {
                throw new Exception("订单:" + order.PopOrderId + " " + ex.Message);
            }
            //解析商品
            foreach (var tg in trade.Orders)
            {
                OrderGoods og = new OrderGoods();
                og.PopOrderSubId = tg.Oid.ToString();
                og.PopUrl = tg.NumIid.ToString();
                og.PopInfo = tg.OuterSkuId + "||" + tg.SkuPropertiesName;
                og.Number = tg.OuterSkuId;
                og.Count = (int)tg.Num;
                og.PopPrice = float.Parse(tg.TotalFee) / (int)tg.Num;
                og.Image = tg.PicPath;

                if (string.IsNullOrWhiteSpace(tg.SkuPropertiesName) == false)
                {
                    //货号，颜色，尺码
                    string stockAttrs = tg.SkuPropertiesName;
                    string[] pro = stockAttrs.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (pro.Length >= 2)
                    {
                        og.Color = pro[0].Split(':')[1];
                        og.Size = pro[1].Split(':')[1];
                        string color = og.Color;

                        //版本包含在颜色中
                        if (color.IndexOfAny(new char[] { '(', '（', '[', '【' }) >= 0)
                        {
                            og.Color = color.Substring(0, color.IndexOfAny(new char[] { '(', '（', '[', '【' }));
                            og.Edtion = color.Substring(color.IndexOfAny(new char[] { '(', '（', '[', '【' }));
                            og.Edtion = og.Edtion.Replace("(", "").Replace("（", "").Replace(")", "").Replace("）", "").Replace("[", "").Replace("【", "").Replace("]", "").Replace("】", "").Replace(" ", "");
                        }
                        else if (color.IndexOf("色") >= 0 && color.IndexOf("色") < color.Length - 1)
                        {
                            og.Color = color.Substring(0, color.IndexOf("色") + 1);
                            og.Edtion = color.Substring(color.IndexOf("色") + 1).Replace(" ", "");
                        }
                        else
                        {
                            og.Color = color;
                        }

                        if (string.IsNullOrWhiteSpace(og.Edtion) == false)
                        {
                            og.Edtion = og.Edtion.Replace("版", "");
                        }
                        else
                        {
                            og.Edtion = "";
                        }
                    }
                    else
                    {
                        og.Color = "";
                        og.Size = "";
                        og.Edtion = "";
                    }
                }
                else
                {
                    og.Color = "";
                    og.Size = "";
                    og.Edtion = "";
                    og.Number = "";
                }
                //状态 
                string state = tg.RefundStatus;

                if (string.IsNullOrWhiteSpace(state) || state == "NO_REFUND")
                {
                    og.State = order.State;
                    og.PopRefundState = PopRefundState.NOT;
                }

                else if (state == "WAIT_SELLER_AGREE" || state == "WAIT_BUYER_RETURN_GOODS" || state == "WAIT_SELLER_CONFIRM_GOODS")
                {
                    og.State = OrderState.RETURNING;
                    og.PopRefundState = PopRefundState.ACCEPT;
                }
                else if (state == "SUCCESS")
                {
                    og.State = OrderState.CLOSED;
                    og.PopRefundState = PopRefundState.OK;
                }
                else if (state == "CLOSED")
                {
                    og.State = order.State;
                    og.PopRefundState = PopRefundState.CANCEL;
                }
                else if (state == "SELLER_REFUSE_BUYER")
                {
                    og.State = order.State;
                    og.PopRefundState = PopRefundState.REJECT;
                }
                else
                {
                    throw new Exception("无法转换的订单商品状态:" + order.PopOrderId + " " + state);
                }
                og.PopNumber = "";
                order.OrderGoodss.Add(og);
            }
            if (order.OrderGoodss.Count == 1)
            {
                order.State = order.OrderGoodss[0].State;
            }
            return order;
        }

        public override bool Accept(Domain.PopType popType)
        {
            return popType == Domain.PopType.TAOBAO || popType == PopType.TMALL;
        }

        public override OrderDownloadCollectionResponse GetOrders(Shop shop, string state, int pageIndex, int pageSize)
        {
            List<OrderDownload> ods = new List<OrderDownload>();
            var request = new Top.Api.Request.TradesSoldGetRequest();
            request.Fields = "tid,receiver_name";
            request.Type = "guarantee_trade,auto_delivery,ec,cod,step,eticket";
            request.Status = state;

            int totalPage = 0;
            int page = 0;
            int totalCount = 0;
            while (true)
            {
                request.PageNo = page;
                request.PageSize = 20;
                request.UseHasNext = false;
                var res = this.InvokeOpenApi(shop, request);

                totalPage = ((int)res.TotalResults + 19) / 20;
                totalCount = (int)res.TotalResults;

                if (res.Trades == null || res.Trades.Count < 1)
                {
                    break;
                }
                foreach (var trade in res.Trades)
                {
                    var od = this.GetOrder(shop, trade.Tid.ToString());
                    ods.Add(od);
                }
                page++;
                if (page >= totalPage)
                {
                    break;
                }
            }
            return new OrderDownloadCollectionResponse(ods, totalCount) { IsTotalValid = true };
        }

        public override OrderDownload GetOrder(Domain.Shop shop, string popOrderId)
        {
            var od = new OrderDownload();
            try
            {
                var request = new Top.Api.Request.TradeFullinfoGetRequest();
                //查询字段
                request.Tid = long.Parse(popOrderId);
                request.Fields = "tid,created,pay_time,modified,end_time,type,status,cod_status,buyer_nick,payment,discount_fee,post_fee,total_fee,seller_cod_fee,buyer_cod_fee,buyer_message,seller_memo,seller_flag,receiver_name,receiver_mobile,receiver_phone,receiver_state,receiver_city,receiver_district,receiver_address,promotion_details";
                request.Fields += "," + "orders.pic_path,orders.refund_status,orders.oid,orders.status,orders.total_fee,orders.num,orders.sku_properties_name,orders.outer_sku_id,orders.num_iid,orders.price,orders.discount_fee";
                var res = this.InvokeOpenApi(shop, request);
                if (res.Trade == null)
                {
                    return null;
                }
                var body = res.Body;
                var order = this.ConvertPopOrderToDomainOrder(res.Trade);
                if (order.PopPayType == PopPayType.COD)
                {
                    var di = this.GetDeliveryInfo(shop, order.PopOrderId);
                    if (di != null)
                    {
                        order.PopCodNumber = di.PopCodNumber;
                    }
                }
                od.Order = order;
            }
            catch (Exception ex)
            {
                od.Error = new OrderDownloadError { Error = ex.Message, PopOrderId = popOrderId, ReceiverName = "", ShopId = shop.Id };
            }
            return od;
        }

        public override PopOrderState GetOrderState(Domain.Shop shop, string popOrderId)
        {
            throw new NotImplementedException();
        }

        public override void ModifyComment(Domain.Shop shop, string popOrderId, string comment, Domain.ColorFlag flag)
        {
            var request = new TradeMemoUpdateRequest();
            request.Tid = long.Parse(popOrderId);
            request.Memo = comment;
            request.Reset = string.IsNullOrWhiteSpace(comment);

            if (flag == ColorFlag.None || flag == ColorFlag.UN_LABEL)
            {
                request.Flag = 0;
            }

            if (flag == ColorFlag.RED)
            {
                request.Flag = 1;
            }

            if (flag == ColorFlag.YELLOW)
            {
                request.Flag = 2;
            }

            if (flag == ColorFlag.GREEN)
            {
                request.Flag = 3;
            }

            if (flag == ColorFlag.BLUE)
            {
                request.Flag = 4;
            }

            if (flag == ColorFlag.PINK)
            {
                request.Flag = 5;
            }
            this.InvokeOpenApi(shop, request);
        }

        public override void MarkDelivery(Domain.Shop shop, string popOrderId, Domain.PopPayType payType, string deliveryCompany, string deliveryNumber)
        {
            //读取订单状态
            var request = new Top.Api.Request.TradeGetRequest();

            //查询字段
            request.Tid = long.Parse(popOrderId);
            request.Fields = "tid,type,status,cod_status";
            var res = this.InvokeOpenApi(shop, request);

            if (res.Trade == null)
            {
                throw new Exception("订单不存在");
            }

            var trade = res.Trade;

            if (trade.Status == "TRADE_NO_CREATE_PAY" || trade.Status == "WAIT_BUYER_PAY" ||
                trade.Status == "TRADE_CLOSED" || trade.Status == "TRADE_CLOSED_BY_TAOBAO" ||
                trade.Status == " PAY_PENDING" || trade.Status == "WAIT_PRE_AUTH_CONFIRM" ||
                trade.Status == "TRADE_BUYER_SIGNED")
            {
                throw new Exception("当前交易状态不允许发货:" + trade.Status);
            }

            if (trade.Status == "WAIT_BUYER_CONFIRM_GOODS")
            {
                return;
            }

            if (trade.Status != "WAIT_SELLER_SEND_GOODS")
            {
                return;
            }

            if (payType == PopPayType.COD && trade.CodStatus == "ACCEPTED_BY_COMPANY")
            {
                return;
            }
            var dc = ServiceContainer.GetService<DeliveryCompanyService>().GetDeliveryCompany(deliveryCompany);

            if (payType == PopPayType.COD)
            {
                var request2 = new LogisticsOnlineSendRequest();
                request2.Tid = long.Parse(popOrderId);
                request2.CompanyCode = dc.First.PopMapTaobao;
                request2.OutSid = deliveryNumber;
                this.InvokeOpenApi(shop, request2);
            }
            else
            {
                var request2 = new LogisticsOfflineSendRequest();
                request2.Tid = long.Parse(popOrderId);
                request2.CompanyCode = dc.First.PopMapTaobao;
                request2.OutSid = deliveryNumber;
                this.InvokeOpenApi(shop, request2);
            }
        }

        public override PopDeliveryInfo GetDeliveryInfo(Domain.Shop shop, string popOrderId)
        {
            LogisticsOrdersDetailGetRequest request = new LogisticsOrdersDetailGetRequest { Tid = long.Parse(popOrderId) };
            request.Fields = "tid,order_code,status,out_sid,company_name,created";
            var response = this.InvokeOpenApi<LogisticsOrdersDetailGetResponse>(shop, request);

            if (response.Shippings.Count < 1)
            {
                return null;
            }

            var info = new PopDeliveryInfo
            {
                CreateTime = DateTime.Parse(response.Shippings[0].Created),
                DeliveryCompany = response.Shippings[0].CompanyName,
                DeliveryNumber = response.Shippings[0].OutSid,
                PopCodNumber = response.Shippings[0].OrderCode,
                StateDesc = response.Shippings[0].Status,
            };
            return info;
        }

        public string GetSellerNumberId(Domain.Shop shop)
        {
            var req = new Top.Api.Request.UserSellerGetRequest { Fields = "user_id,nick" };
            var rsp = this.InvokeOpenApi<UserSellerGetResponse>(shop, req);
            return rsp.User.UserId.ToString();
        }

        private List<Top.Api.Domain.Item> GetInStockGoods(Domain.Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            var req = new Top.Api.Request.ItemsInventoryGetRequest();
            req.PageNo = pageIndex + 1;
            req.PageSize = pageSize;
            req.Fields = "approve_status,num_iid,title,type,cid,pic_url,num,props,valid_thru, list_time,price,has_discount,has_invoice,has_warranty,has_showcase, modified,delist_time,postage_id,seller_cids,outer_id,Skus";
            req.Banner = PopGoodsState.NEVERSALE == state ? "never_on_shelf" : "";
            var res = this.InvokeOpenApi<Top.Api.Response.ItemsInventoryGetResponse>(shop, req);
            return res.Items;
        }

        private List<Top.Api.Domain.Item> GetOnsaleGoods(Domain.Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            var req = new Top.Api.Request.ItemsOnsaleGetRequest();
            req.PageNo = pageIndex + 1;
            req.PageSize = pageSize;
            req.Fields = "approve_status,num_iid,title,type,cid,pic_url,num,props,valid_thru, list_time,price,has_discount,has_invoice,has_warranty,has_showcase, modified,delist_time,postage_id,seller_cids,outer_id,Skus";
            var res = this.InvokeOpenApi<Top.Api.Response.ItemsOnsaleGetResponse>(shop, req);
            return res.Items;
        }

        public override List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            List<Top.Api.Domain.Item> goods = null;

            if (state == PopGoodsState.NEVERSALE || state == PopGoodsState.NOTSALE)
            {
                goods = this.GetInStockGoods(shop, state, pageIndex, pageSize);
            }
            else if (state == PopGoodsState.ONSALE)
            {
                goods = this.GetOnsaleGoods(shop, state, pageIndex, pageSize);
            }
            else if (state == PopGoodsState.NONE)
            {
                var good1 = this.GetInStockGoods(shop, state, pageIndex, pageSize);
                var good2 = this.GetOnsaleGoods(shop, state, pageIndex, pageSize);
                good1.AddRange(good2);
                goods = good1;
            }
            else
            {
                throw new Exception("无法处理的状态:" + state);
            }

            var ggs = goods.Select(g => new PopGoods
            {
                AddTime = g.Created,
                UpdateTime = g.Modified,
                Code = g.OuterId,
                Id = g.NumIid.ToString(),
                Image = g.PicUrl,
                SaleNum = 0,
                State = PopGoodsState.NONE,
                Title = g.Title,
                CatId = g.Cid.ToString(),
            }).ToList();

            int PAGE_SIZE = 20;

            for (int i = 0; i < ggs.Count; i += PAGE_SIZE)
            {
                var req = new ItemsSellerListGetRequest();
                req.Fields = "num_iid,approve_status,sku,price,created,sold_quantity";
                req.NumIids = "";
                for (int j = 0; j < PAGE_SIZE && j + i < ggs.Count; j++)
                {
                    req.NumIids += ggs[j + i].Id + ",";
                }
                req.NumIids = req.NumIids.TrimEnd(',');
                var res = this.InvokeOpenApi<ItemsSellerListGetResponse>(shop, req);

                for (int j = 0; j < PAGE_SIZE && j + i < ggs.Count; j++)
                {
                    var item = res.Items.FirstOrDefault(obj => obj.NumIid.ToString() == ggs[j + i].Id);
                    if (item == null)
                    {
                        continue;
                    }
                    ggs[j + i].SaleNum = (int)item.PeriodSoldQuantity;
                    ggs[j + i].AddTime = item.Created;
                    ggs[j + i].SaleNum = (int)item.SoldQuantity;
                    if (item.Skus != null)
                    {
                        foreach (var s in item.Skus)
                        {
                            if (s.Status != null && s.Status == "delete")
                            {
                                continue;
                            }
                            ggs[j + i].Skus.Add(new PopGoodsSku
                            {
                                Code = s.OuterId,
                                Id = s.SkuId.ToString(),
                                Price = s.Price,
                                PropId = s.Properties,
                                Status = s.Status,
                                Stock = s.Quantity.ToString(),
                                Value = s.Properties,
                            });
                        }
                    }
                    if (state == PopGoodsState.NEVERSALE)
                    {
                        ggs[j + i].State = PopGoodsState.NEVERSALE;
                    }
                    else
                    {
                        ggs[j + i].State = item.ApproveStatus == "onsale" ? PopGoodsState.ONSALE : PopGoodsState.NOTSALE;
                    }
                }
            }
            return ggs;
        }

        public override string GetShopOauthUrl(Shop sop)
        {
            throw new NotImplementedException();
        }

        public override Shop GetAcessTokenInfo(Shop shop, string code)
        {
            throw new NotImplementedException();
        }

        public override Shop GetRefreshTokenInfo(Shop shop)
        {
            throw new NotImplementedException();
        }
    }
}
