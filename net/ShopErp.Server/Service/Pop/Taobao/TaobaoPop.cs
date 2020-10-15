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
using System.Xml.Linq;

namespace ShopErp.Server.Service.Pop.Taobao
{
    public class TaobaoPop : PopBase
    {
        private const string API_SERVER_URL = "https://eco.taobao.com/router/rest";
        private const string API_SERVER_URL_SANDBOX = "http://gw.api.tbsandbox.com/router/rest";

        private static ITopClient GetTopClient(string appKey, string appSecret)
        {
            var client = new DefaultTopClient(API_SERVER_URL, appKey, appSecret);
            return client;
        }

        private static OrderState ConvertPopStateToSystemState(string state)
        {
            if (state == "NEW_CREATED")
            {
                return OrderState.WAITPAY;
            }
            else if (state == "ACCEPTED_BY_COMPANY" || state == "REJECTED_BY_COMPANY" || state == "RECIEVE_TIMEOUT" ||
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
                return OrderState.CLOSED;
            }
            else if (state == "SIGN_IN")
            {
                return OrderState.SHIPPED;
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
                return OrderState.SHIPPED;
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
                return OrderState.CLOSED;
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

        private static ColorFlag ConverPopFlagToOrderFlag(long flag)
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

        private static CainiaoWaybillIiGetRequest.AddressDtoDomain TaobaoConvertToAddressDtoDomain(string address, PopType sourceType)
        {
            string[] adds = AddressService.Parse5Address(address, sourceType, PopType.TAOBAO);
            var wd = new CainiaoWaybillIiGetRequest.AddressDtoDomain
            {
                Province = adds[0],
                City = adds[1],
                District = adds[2],
                Town = adds[3],
                Detail = adds[4],
            };
            return wd;
        }

        private static CainiaoWaybillIiUpdateRequest.UserInfoDtoDomain TaobaoConvertToUserInfoDtoDomain(string address, string name, string phone, string mobile, PopType sourceType)
        {
            string[] adds = AddressService.Parse5Address(address, sourceType, PopType.TAOBAO);
            var wd = new CainiaoWaybillIiUpdateRequest.UserInfoDtoDomain
            {
                Address = new CainiaoWaybillIiUpdateRequest.AddressDtoDomain
                {
                    Province = adds[0],
                    City = adds[1],
                    District = adds[2],
                    Town = adds[3],
                    Detail = adds[4],
                },
                Name = name,
                Phone = phone,
                Mobile = mobile,
            };
            return wd;
        }

        private static string GetNodeName(long type)
        {
            if (type == 1)
            {
                return AddressService.COUNTRY;
            }
            if (type == 2)
            {
                return AddressService.PROVINCE;
            }
            if (type == 3)
            {
                return AddressService.CITY;
            }
            if (type == 4)
            {
                return AddressService.DISTRICT;
            }
            if (type == 5)
            {
                return AddressService.TOWN;
            }
            throw new Exception("未知的行政级别");
        }

        private static void FindSub(XElement parent, long parentId, List<Top.Api.Domain.Area> areas)
        {
            var aa = areas.Where(obj => parentId == obj.ParentId).ToArray();
            if (aa.Length < 1)
            {
                return;
            }

            foreach (var a in aa)
            {
                string sn = a.Type == 2 ? AddressService.GetProvinceShortName(a.Name) : AddressService.GetCityShortName(a.Name);
                var xe = new XElement(GetNodeName(a.Type), new XAttribute("Name", a.Name.Trim()), new XAttribute("ShortName", sn));
                areas.Remove(a);
                parent.Add(xe);
                FindSub(xe, a.Id, areas);
            }
        }

        private static T InvokeOpenApi<T>(Domain.Shop shop, ITopRequest<T> request) where T : TopResponse
        {
            var topClient = GetTopClient(shop.AppKey, shop.AppSecret);
            var ret = topClient.Execute<T>(request, shop.AppAccessToken, DateTime.Now);
            if (ret.IsError)
            {
                throw new Exception("执行淘宝请求出错:" + ret.ErrCode + "," + ret.ErrMsg + ret.SubErrMsg);
            }
            return ret;
        }

        public override bool Accept(Domain.PopType popType)
        {
            return popType == Domain.PopType.TAOBAO || popType == PopType.TMALL;
        }

        #region 订单 商品 功能

        //private static Order ConvertPopOrderToDomainOrder(Top.Api.Domain.Trade trade)
        //{
        //    var order = new ShopErp.Domain.Order
        //    {
        //        PopOrderId = trade.Tid.ToString(),
        //        PopPayType = trade.Type.ToUpper().Contains("COD") ? PopPayType.COD : PopPayType.ONLINE,
        //        PopState = trade.Status,
        //        PopFlag = ConverPopFlagToOrderFlag(trade.SellerFlag),
        //        PopOrderTotalMoney = float.Parse(trade.Payment) + float.Parse(trade.DiscountFee ?? "0"),
        //        PopDeliveryTime = DateTime.Parse(trade.ConsignTime ?? "1970-01-01 00:00:01"),
        //        PopCodNumber = "",
        //        DeliveryCompany = "",
        //        DeliveryNumber = "",
        //        PopBuyerId = trade.BuyerNick,
        //        ReceiverName = trade.ReceiverName,
        //        ReceiverMobile = trade.ReceiverMobile ?? "",
        //        ReceiverPhone = trade.ReceiverPhone ?? "",
        //        ReceiverAddress = trade.ReceiverState + " " + trade.ReceiverCity + " " + trade.ReceiverDistrict + " " + trade.ReceiverAddress,
        //        PopBuyerComment = trade.BuyerMessage ?? "",
        //        PopSellerComment = trade.SellerMemo ?? "",
        //        PopType = PopType.TAOBAO,
        //        PopCreateTime = DateTime.Parse(trade.Created),
        //        PopPayTime = trade.Type.ToUpper().Contains("COD") ? DateTime.Parse(trade.Created) : DateTime.Parse(trade.PayTime ?? "1970-01-01 00:00:01"),
        //        PopCodSevFee = float.Parse(trade.SellerCodFee ?? "0.0"),
        //        OrderGoodss = new List<OrderGoods>(),
        //        CreateType = OrderCreateType.DOWNLOAD,
        //        Type = OrderType.NORMAL,
        //        CloseOperator = "",
        //        CloseTime = Utils.DateTimeUtil.DbMinTime,
        //        CreateOperator = "",
        //        CreateTime = Utils.DateTimeUtil.DbMinTime,
        //        DeliveryOperator = "",
        //        DeliveryTime = Utils.DateTimeUtil.DbMinTime,
        //        DeliveryMoney = 0,
        //        Id = 0,
        //        ParseResult = false,
        //        PopBuyerPayMoney = float.Parse(trade.Payment),
        //        PopSellerGetMoney = float.Parse(trade.Payment),
        //    };
        //    try
        //    {
        //        order.State = ConvertPopStateToSystemState(order.PopState);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("订单:" + order.PopOrderId + " " + ex.Message);
        //    }
        //    //解析商品
        //    foreach (var tg in trade.Orders)
        //    {
        //        OrderGoods og = new OrderGoods();
        //        og.PopOrderSubId = tg.Oid.ToString();
        //        og.PopUrl = tg.NumIid.ToString();
        //        og.PopInfo = tg.OuterSkuId + "||" + tg.SkuPropertiesName;
        //        og.Number = tg.OuterSkuId;
        //        og.Count = (int)tg.Num;
        //        og.PopPrice = float.Parse(tg.TotalFee) / (int)tg.Num;
        //        og.Image = tg.PicPath;

        //        if (string.IsNullOrWhiteSpace(tg.SkuPropertiesName) == false)
        //        {
        //            //货号，颜色，尺码
        //            string stockAttrs = tg.SkuPropertiesName;
        //            string[] pro = stockAttrs.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        //            if (pro.Length >= 2)
        //            {
        //                og.Color = pro[0].Split(':')[1];
        //                og.Size = pro[1].Split(':')[1];
        //                string color = og.Color;

        //                //版本包含在颜色中
        //                if (color.IndexOfAny(new char[] { '(', '（', '[', '【' }) >= 0)
        //                {
        //                    og.Color = color.Substring(0, color.IndexOfAny(new char[] { '(', '（', '[', '【' }));
        //                    og.Edtion = color.Substring(color.IndexOfAny(new char[] { '(', '（', '[', '【' }));
        //                    og.Edtion = og.Edtion.Replace("(", "").Replace("（", "").Replace(")", "").Replace("）", "").Replace("[", "").Replace("【", "").Replace("]", "").Replace("】", "").Replace(" ", "");
        //                }
        //                else if (color.IndexOf("色") >= 0 && color.IndexOf("色") < color.Length - 1)
        //                {
        //                    og.Color = color.Substring(0, color.IndexOf("色") + 1);
        //                    og.Edtion = color.Substring(color.IndexOf("色") + 1).Replace(" ", "");
        //                }
        //                else
        //                {
        //                    og.Color = color;
        //                }

        //                if (string.IsNullOrWhiteSpace(og.Edtion) == false)
        //                {
        //                    og.Edtion = og.Edtion.Replace("版", "");
        //                }
        //                else
        //                {
        //                    og.Edtion = "";
        //                }
        //            }
        //            else
        //            {
        //                og.Color = "";
        //                og.Size = "";
        //                og.Edtion = "";
        //            }
        //        }
        //        else
        //        {
        //            og.Color = "";
        //            og.Size = "";
        //            og.Edtion = "";
        //            og.Number = "";
        //        }
        //        //状态 
        //        string state = tg.RefundStatus;

        //        if (string.IsNullOrWhiteSpace(state) || state == "NO_REFUND")
        //        {
        //            og.State = order.State;
        //            og.PopRefundState = PopRefundState.NOT;
        //        }

        //        else if (state == "WAIT_SELLER_AGREE" || state == "WAIT_BUYER_RETURN_GOODS" || state == "WAIT_SELLER_CONFIRM_GOODS")
        //        {
        //            og.State = OrderState.RETURNING;
        //            og.PopRefundState = PopRefundState.ACCEPT;
        //        }
        //        else if (state == "SUCCESS")
        //        {
        //            og.State = OrderState.CLOSED;
        //            og.PopRefundState = PopRefundState.OK;
        //        }
        //        else if (state == "CLOSED")
        //        {
        //            og.State = order.State;
        //            og.PopRefundState = PopRefundState.CANCEL;
        //        }
        //        else if (state == "SELLER_REFUSE_BUYER")
        //        {
        //            og.State = order.State;
        //            og.PopRefundState = PopRefundState.REJECT;
        //        }
        //        else
        //        {
        //            throw new Exception("无法转换的订单商品状态:" + order.PopOrderId + " " + state);
        //        }
        //        og.PopNumber = "";
        //        order.OrderGoodss.Add(og);
        //    }
        //    if (order.OrderGoodss.Count == 1)
        //    {
        //        order.State = order.OrderGoodss[0].State;
        //    }
        //    return order;
        //}

        //public override OrderDownloadCollectionResponse GetOrders(Shop shop, string state, int pageIndex, int pageSize)
        //{
        //    List<OrderDownload> ods = new List<OrderDownload>();
        //    var request = new Top.Api.Request.TradesSoldGetRequest();
        //    request.Fields = "tid,receiver_name";
        //    request.Type = "guarantee_trade,auto_delivery,ec,cod,step,eticket";
        //    request.Status = state;

        //    int totalPage = 0;
        //    int page = 0;
        //    int totalCount = 0;
        //    while (true)
        //    {
        //        request.PageNo = page;
        //        request.PageSize = 20;
        //        request.UseHasNext = false;
        //        var res = this.InvokeOpenApi(shop, request);

        //        totalPage = ((int)res.TotalResults + 19) / 20;
        //        totalCount = (int)res.TotalResults;

        //        if (res.Trades == null || res.Trades.Count < 1)
        //        {
        //            break;
        //        }
        //        foreach (var trade in res.Trades)
        //        {
        //            var od = this.GetOrder(shop, trade.Tid.ToString());
        //            ods.Add(od);
        //        }
        //        page++;
        //        if (page >= totalPage)
        //        {
        //            break;
        //        }
        //    }
        //    return new OrderDownloadCollectionResponse(ods, totalCount) { IsTotalValid = true };
        //}

        //public override OrderDownload GetOrder(Domain.Shop shop, string popOrderId)
        //{
        //    var od = new OrderDownload();
        //    try
        //    {
        //        var request = new Top.Api.Request.TradeFullinfoGetRequest();
        //        //查询字段
        //        request.Tid = long.Parse(popOrderId);
        //        request.Fields = "tid,created,pay_time,modified,end_time,type,status,cod_status,buyer_nick,payment,discount_fee,post_fee,total_fee,seller_cod_fee,buyer_cod_fee,buyer_message,seller_memo,seller_flag,receiver_name,receiver_mobile,receiver_phone,receiver_state,receiver_city,receiver_district,receiver_address,promotion_details";
        //        request.Fields += "," + "orders.pic_path,orders.refund_status,orders.oid,orders.status,orders.total_fee,orders.num,orders.sku_properties_name,orders.outer_sku_id,orders.num_iid,orders.price,orders.discount_fee";
        //        var res = this.InvokeOpenApi(shop, request);
        //        if (res.Trade == null)
        //        {
        //            return null;
        //        }
        //        var body = res.Body;
        //        var order = this.ConvertPopOrderToDomainOrder(res.Trade);
        //        if (order.PopPayType == PopPayType.COD)
        //        {
        //            var di = this.GetDeliveryInfo(shop, order.PopOrderId);
        //            if (di != null)
        //            {
        //                order.PopCodNumber = di.PopCodNumber;
        //            }
        //        }
        //        od.Order = order;
        //    }
        //    catch (Exception ex)
        //    {
        //        od.Error = new OrderDownloadError(shop.Id, popOrderId, "", ex.Message, ex.StackTrace);
        //    }
        //    return od;
        //}

        //public override PopOrderState GetOrderState(Domain.Shop shop, string popOrderId)
        //{
        //    throw new NotImplementedException();
        //}

        //public override void ModifyComment(Domain.Shop shop, string popOrderId, string comment, Domain.ColorFlag flag)
        //{
        //    var request = new TradeMemoUpdateRequest();
        //    request.Tid = long.Parse(popOrderId);
        //    request.Memo = comment;
        //    request.Reset = string.IsNullOrWhiteSpace(comment);

        //    if (flag == ColorFlag.None || flag == ColorFlag.UN_LABEL)
        //    {
        //        request.Flag = 0;
        //    }

        //    if (flag == ColorFlag.RED)
        //    {
        //        request.Flag = 1;
        //    }

        //    if (flag == ColorFlag.YELLOW)
        //    {
        //        request.Flag = 2;
        //    }

        //    if (flag == ColorFlag.GREEN)
        //    {
        //        request.Flag = 3;
        //    }

        //    if (flag == ColorFlag.BLUE)
        //    {
        //        request.Flag = 4;
        //    }

        //    if (flag == ColorFlag.PINK)
        //    {
        //        request.Flag = 5;
        //    }
        //    this.InvokeOpenApi(shop, request);
        //}

        //public override void MarkDelivery(Domain.Shop shop, string popOrderId, Domain.PopPayType payType, string deliveryCompany, string deliveryNumber)
        //{
        //    //读取订单状态
        //    var request = new Top.Api.Request.TradeGetRequest();

        //    //查询字段
        //    request.Tid = long.Parse(popOrderId);
        //    request.Fields = "tid,type,status,cod_status";
        //    var res = this.InvokeOpenApi(shop, request);

        //    if (res.Trade == null)
        //    {
        //        throw new Exception("订单不存在");
        //    }

        //    var trade = res.Trade;

        //    if (trade.Status == "TRADE_NO_CREATE_PAY" || trade.Status == "WAIT_BUYER_PAY" ||
        //        trade.Status == "TRADE_CLOSED" || trade.Status == "TRADE_CLOSED_BY_TAOBAO" ||
        //        trade.Status == " PAY_PENDING" || trade.Status == "WAIT_PRE_AUTH_CONFIRM" ||
        //        trade.Status == "TRADE_BUYER_SIGNED")
        //    {
        //        throw new Exception("当前交易状态不允许发货:" + trade.Status);
        //    }

        //    if (trade.Status == "WAIT_BUYER_CONFIRM_GOODS")
        //    {
        //        return;
        //    }

        //    if (trade.Status != "WAIT_SELLER_SEND_GOODS")
        //    {
        //        return;
        //    }

        //    if (payType == PopPayType.COD && trade.CodStatus == "ACCEPTED_BY_COMPANY")
        //    {
        //        return;
        //    }
        //    var dc = ServiceContainer.GetService<DeliveryCompanyService>().GetDeliveryCompany(deliveryCompany);

        //    if (payType == PopPayType.COD)
        //    {
        //        var request2 = new LogisticsOnlineSendRequest();
        //        request2.Tid = long.Parse(popOrderId);
        //        request2.CompanyCode = dc.First.PopMapTaobao;
        //        request2.OutSid = deliveryNumber;
        //        this.InvokeOpenApi(shop, request2);
        //    }
        //    else
        //    {
        //        var request2 = new LogisticsOfflineSendRequest();
        //        request2.Tid = long.Parse(popOrderId);
        //        request2.CompanyCode = dc.First.PopMapTaobao;
        //        request2.OutSid = deliveryNumber;
        //        this.InvokeOpenApi(shop, request2);
        //    }
        //}

        //public override PopDeliveryInfo GetDeliveryInfo(Domain.Shop shop, string popOrderId)
        //{
        //    LogisticsOrdersDetailGetRequest request = new LogisticsOrdersDetailGetRequest { Tid = long.Parse(popOrderId) };
        //    request.Fields = "tid,order_code,status,out_sid,company_name,created";
        //    var response = this.InvokeOpenApi<LogisticsOrdersDetailGetResponse>(shop, request);

        //    if (response.Shippings.Count < 1)
        //    {
        //        return null;
        //    }

        //    var info = new PopDeliveryInfo
        //    {
        //        CreateTime = DateTime.Parse(response.Shippings[0].Created),
        //        DeliveryCompany = response.Shippings[0].CompanyName,
        //        DeliveryNumber = response.Shippings[0].OutSid,
        //        PopCodNumber = response.Shippings[0].OrderCode,
        //        StateDesc = response.Shippings[0].Status,
        //    };
        //    return info;
        //}

        //public string GetSellerNumberId(Domain.Shop shop)
        //{
        //    var req = new Top.Api.Request.UserSellerGetRequest { Fields = "user_id,nick" };
        //    var rsp = this.InvokeOpenApi<UserSellerGetResponse>(shop, req);
        //    return rsp.User.UserId.ToString();
        //}

        //private List<Top.Api.Domain.Item> GetInStockGoods(Domain.Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        //{
        //    var req = new Top.Api.Request.ItemsInventoryGetRequest();
        //    req.PageNo = pageIndex + 1;
        //    req.PageSize = pageSize;
        //    req.Fields = "approve_status,num_iid,title,type,cid,pic_url,num,props,valid_thru, list_time,price,has_discount,has_invoice,has_warranty,has_showcase, modified,delist_time,postage_id,seller_cids,outer_id,Skus";
        //    req.Banner = PopGoodsState.NEVERSALE == state ? "never_on_shelf" : "";
        //    var res = this.InvokeOpenApi<Top.Api.Response.ItemsInventoryGetResponse>(shop, req);
        //    return res.Items;
        //}

        //private List<Top.Api.Domain.Item> GetOnsaleGoods(Domain.Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        //{
        //    var req = new Top.Api.Request.ItemsOnsaleGetRequest();
        //    req.PageNo = pageIndex + 1;
        //    req.PageSize = pageSize;
        //    req.Fields = "approve_status,num_iid,title,type,cid,pic_url,num,props,valid_thru, list_time,price,has_discount,has_invoice,has_warranty,has_showcase, modified,delist_time,postage_id,seller_cids,outer_id,Skus";
        //    var res = this.InvokeOpenApi<Top.Api.Response.ItemsOnsaleGetResponse>(shop, req);
        //    return res.Items;
        //}

        //public override List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        //{
        //    List<Top.Api.Domain.Item> goods = null;

        //    if (state == PopGoodsState.NEVERSALE || state == PopGoodsState.NOTSALE)
        //    {
        //        goods = this.GetInStockGoods(shop, state, pageIndex, pageSize);
        //    }
        //    else if (state == PopGoodsState.ONSALE)
        //    {
        //        goods = this.GetOnsaleGoods(shop, state, pageIndex, pageSize);
        //    }
        //    else if (state == PopGoodsState.NONE)
        //    {
        //        var good1 = this.GetInStockGoods(shop, state, pageIndex, pageSize);
        //        var good2 = this.GetOnsaleGoods(shop, state, pageIndex, pageSize);
        //        good1.AddRange(good2);
        //        goods = good1;
        //    }
        //    else
        //    {
        //        throw new Exception("无法处理的状态:" + state);
        //    }

        //    var ggs = goods.Select(g => new PopGoods
        //    {
        //        AddTime = g.Created,
        //        UpdateTime = g.Modified,
        //        Code = g.OuterId,
        //        Id = g.NumIid.ToString(),
        //        Image = g.PicUrl,
        //        SaleNum = 0,
        //        State = PopGoodsState.NONE,
        //        Title = g.Title,
        //        CatId = g.Cid.ToString(),
        //    }).ToList();

        //    int PAGE_SIZE = 20;

        //    for (int i = 0; i < ggs.Count; i += PAGE_SIZE)
        //    {
        //        var req = new ItemsSellerListGetRequest();
        //        req.Fields = "num_iid,approve_status,sku,price,created,sold_quantity";
        //        req.NumIids = "";
        //        for (int j = 0; j < PAGE_SIZE && j + i < ggs.Count; j++)
        //        {
        //            req.NumIids += ggs[j + i].Id + ",";
        //        }
        //        req.NumIids = req.NumIids.TrimEnd(',');
        //        var res = this.InvokeOpenApi<ItemsSellerListGetResponse>(shop, req);

        //        for (int j = 0; j < PAGE_SIZE && j + i < ggs.Count; j++)
        //        {
        //            var item = res.Items.FirstOrDefault(obj => obj.NumIid.ToString() == ggs[j + i].Id);
        //            if (item == null)
        //            {
        //                continue;
        //            }
        //            ggs[j + i].SaleNum = (int)item.PeriodSoldQuantity;
        //            ggs[j + i].AddTime = item.Created;
        //            ggs[j + i].SaleNum = (int)item.SoldQuantity;
        //            if (item.Skus != null)
        //            {
        //                foreach (var s in item.Skus)
        //                {
        //                    if (s.Status != null && s.Status == "delete")
        //                    {
        //                        continue;
        //                    }
        //                    ggs[j + i].Skus.Add(new PopGoodsSku
        //                    {
        //                        Code = s.OuterId,
        //                        Id = s.SkuId.ToString(),
        //                        Price = s.Price,
        //                        PropId = s.Properties,
        //                        Status = s.Status,
        //                        Stock = s.Quantity.ToString(),
        //                        Value = s.Properties,
        //                    });
        //                }
        //            }
        //            if (state == PopGoodsState.NEVERSALE)
        //            {
        //                ggs[j + i].State = PopGoodsState.NEVERSALE;
        //            }
        //            else
        //            {
        //                ggs[j + i].State = item.ApproveStatus == "onsale" ? PopGoodsState.ONSALE : PopGoodsState.NOTSALE;
        //            }
        //        }
        //    }
        //    return ggs;
        //}

        #endregion

        #region 订单商品功能未实现

        public override OrderDownloadCollectionResponse GetOrders(Shop shop, string state, DateTime dateTime, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public override PopOrderState GetOrderState(Shop shop, string popOrderId)
        {
            throw new NotImplementedException();
        }

        public override List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public override void ModifyComment(Shop shop, string popOrderId, string comment, ColorFlag flag)
        {
            throw new NotImplementedException();
        }

        public override void MarkDelivery(Shop shop, string popOrderId, PopPayType payType, string deliveryCompany, string deliveryNumber)
        {
            throw new NotImplementedException();
        }

        public override PopDeliveryInfo GetDeliveryInfo(Shop shop, string popOrderId)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override string GetShopOauthUrl(Shop shop)
        {
            string url = "https://oauth.taobao.com/authorize?response_type=token&view=web&client_id=" + shop.AppKey;
            return url;
        }

        public override Shop GetAcessTokenInfo(Shop shop, string code)
        {
            throw new NotImplementedException();
        }

        public override Shop GetRefreshTokenInfo(Shop shop)
        {
            throw new NotImplementedException();
        }

        public override List<WuliuBranch> GetWuliuBranchs(Shop shop)
        {
            var req = new CainiaoWaybillIiSearchRequest();
            var rep = InvokeOpenApi<CainiaoWaybillIiSearchResponse>(shop, req);
            var datas = new List<WuliuBranch>();

            foreach (var v in rep.WaybillApplySubscriptionCols)
            {
                foreach (var vv in v.BranchAccountCols)
                {
                    foreach (var vvv in vv.ShippAddressCols)
                    {
                        var data = new WuliuBranch();
                        data.Type = v.CpCode;
                        data.Name = vv.BranchName;
                        data.Number = vv.BranchCode;
                        data.Quantity = vv.Quantity;
                        data.SenderName = "";
                        data.SenderPhone = "";
                        data.SenderAddress = vvv.Province + " " + vvv.City + " " + vvv.District + " " + vvv.Detail;
                        datas.Add(data);
                    }
                }
            }
            return datas;
        }

        public override List<WuliuPrintTemplate> GetWuliuPrintTemplates(Shop shop, string cpCode)
        {
            //系统需要使用到标准模板，所以需要先下载
            var stdWuliuTemplats = new List<WuliuPrintTemplate>();
            CainiaoCloudprintStdtemplatesGetRequest reqStd = new CainiaoCloudprintStdtemplatesGetRequest() { };
            CainiaoCloudprintStdtemplatesGetResponse rspStd = InvokeOpenApi<CainiaoCloudprintStdtemplatesGetResponse>(shop, reqStd);
            foreach (var v in rspStd.Result.Datas)
            {
                if (string.IsNullOrWhiteSpace(cpCode) == false && cpCode.Equals(v.CpCode, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }
                foreach (var vv in v.StandardTemplates)
                {
                    var wuliuTemplate = new WuliuPrintTemplate { CpCode = v.CpCode, Name = vv.StandardTemplateName, SourceType = WuliuPrintTemplateSourceType.CAINIAO, StandTemplateUrl = vv.StandardTemplateUrl, StandTemplateId = vv.StandardTemplateId.ToString(), UserOrIsvTemplateAreaId = "", UserOrIsvTemplateAreaUrl = "" };
                    stdWuliuTemplats.Add(wuliuTemplate);
                }
            }

            var wuliuTemplates = new List<WuliuPrintTemplate>();
            //用户自定义模板
            CainiaoCloudprintMystdtemplatesGetRequest req = new CainiaoCloudprintMystdtemplatesGetRequest();
            CainiaoCloudprintMystdtemplatesGetResponse rsp = InvokeOpenApi<CainiaoCloudprintMystdtemplatesGetResponse>(shop, req);
            foreach (var v in rsp.Result.Datas)
            {
                if (string.IsNullOrWhiteSpace(cpCode) == false && cpCode.Equals(v.CpCode, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }
                foreach (var vv in v.UserStdTemplates)
                {
                    var wuliuTemplate = new WuliuPrintTemplate { CpCode = v.CpCode, Name = vv.UserStdTemplateName, SourceType = WuliuPrintTemplateSourceType.CAINIAO, StandTemplateUrl = vv.UserStdTemplateUrl, StandTemplateId = vv.UserStdTemplateId.ToString() };
                    CainiaoCloudprintCustomaresGetRequest r = new CainiaoCloudprintCustomaresGetRequest() { TemplateId = vv.UserStdTemplateId };
                    var rs = InvokeOpenApi<CainiaoCloudprintCustomaresGetResponse>(shop, r);
                    wuliuTemplate.UserOrIsvTemplateAreaUrl = rs.Result.Datas.FirstOrDefault().CustomAreaUrl;
                    wuliuTemplate.UserOrIsvTemplateAreaId = rs.Result.Datas.FirstOrDefault().CustomAreaId.ToString();
                    wuliuTemplates.Add(wuliuTemplate);
                }
            }
            return wuliuTemplates;
        }

        public override WuliuNumber GetWuliuNumber(Shop shop, string popSellerNumberId, WuliuPrintTemplate wuliuTemplate, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress)
        {
            if (string.IsNullOrWhiteSpace(senderName) || string.IsNullOrWhiteSpace(senderPhone))
            {
                throw new Exception("淘宝接口发货人不完整请配置");
            }

            if (string.IsNullOrWhiteSpace(popSellerNumberId))
            {
                throw new Exception("淘宝卖家数据编号为空");
            }

            //生成请求参数
            CainiaoWaybillIiGetRequest req = new CainiaoWaybillIiGetRequest();
            var reqBody = new CainiaoWaybillIiGetRequest.WaybillCloudPrintApplyNewRequestDomain();
            reqBody.CpCode = wuliuTemplate.CpCode;
            reqBody.Sender = new CainiaoWaybillIiGetRequest.UserInfoDtoDomain { Phone = "", Name = senderName, Mobile = senderPhone, Address = TaobaoConvertToAddressDtoDomain(senderAddress, PopType.TAOBAO) };
            reqBody.NeedEncrypt = true;
            reqBody.TradeOrderInfoDtos = new List<CainiaoWaybillIiGetRequest.TradeOrderInfoDtoDomain>();//订单信息，一个请求里面可以包含多个订单，我们系统里面，默认一个
            var or = new CainiaoWaybillIiGetRequest.TradeOrderInfoDtoDomain { ObjectId = Guid.NewGuid().ToString() };
            or.UserId = long.Parse(popSellerNumberId);
            or.TemplateUrl = wuliuTemplate.StandTemplateUrl;
            or.Recipient = new CainiaoWaybillIiGetRequest.UserInfoDtoDomain { Phone = order.ReceiverPhone, Mobile = order.ReceiverMobile, Name = order.ReceiverName, Address = TaobaoConvertToAddressDtoDomain(order.ReceiverAddress, order.PopType), };
            or.OrderInfo = new CainiaoWaybillIiGetRequest.OrderInfoDtoDomain { OrderChannelsType = "OTHERS", TradeOrderList = new List<string>(wuliuIds) };
            or.PackageInfo = new CainiaoWaybillIiGetRequest.PackageInfoDtoDomain { Id = packageId == "" ? null : packageId, Items = new List<CainiaoWaybillIiGetRequest.ItemDomain>() };
            or.PackageInfo.Items.AddRange(order.OrderGoodss.Where(obj => (int)obj.State >= (int)OrderState.PAYED && (int)obj.State <= (int)OrderState.SUCCESS).Select(obj => new CainiaoWaybillIiGetRequest.ItemDomain { Name = obj.Number + "," + obj.Edtion + "," + obj.Color + "," + obj.Size, Count = obj.Count }));
            if (or.PackageInfo.Items.Count < 1)
            {
                or.PackageInfo.Items.Add(new CainiaoWaybillIiGetRequest.ItemDomain { Name = "没有商品或者其它未定义商品", Count = 1 });
            }
            reqBody.TradeOrderInfoDtos.Add(or);
            req.ParamWaybillCloudPrintApplyNewRequest_ = reqBody;
            var rsp = InvokeOpenApi<CainiaoWaybillIiGetResponse>(shop, req);
            if (rsp.Modules == null || rsp.Modules.Count < 1)
            {
                throw new Exception("菜鸟电子面单未返回数据:" + rsp.ErrMsg);
            }
            var wuliuNumber = new WuliuNumber { CreateTime = DateTime.Now };
            wuliuNumber.ReceiverAddress = order.ReceiverAddress;
            wuliuNumber.ReceiverMobile = order.ReceiverMobile;
            wuliuNumber.ReceiverName = order.ReceiverName;
            wuliuNumber.ReceiverPhone = order.ReceiverPhone;
            wuliuNumber.DeliveryCompany = wuliuTemplate.DeliveryCompany;
            wuliuNumber.DeliveryNumber = rsp.Modules[0].WaybillCode;
            wuliuNumber.WuliuIds = string.Join(",", wuliuIds);
            wuliuNumber.PackageId = packageId;
            wuliuNumber.PrintData = rsp.Modules[0].PrintData;
            wuliuNumber.SourceType = WuliuPrintTemplateSourceType.CAINIAO;
            return wuliuNumber;
        }

        public override void UpdateWuliuNumber(Shop shop, WuliuPrintTemplate wuliuTemplate, Order order, WuliuNumber wuliuNumber)
        {
            //需要更新菜鸟面单以打印正确的信息
            var updateReq = new CainiaoWaybillIiUpdateRequest { };
            var updateReqBody = new CainiaoWaybillIiUpdateRequest.WaybillCloudPrintUpdateRequestDomain
            {
                CpCode = wuliuTemplate.CpCode,
                WaybillCode = wuliuNumber.DeliveryNumber,
                TemplateUrl = wuliuTemplate.StandTemplateUrl,
                Recipient = TaobaoConvertToUserInfoDtoDomain(order.ReceiverAddress, order.ReceiverName, order.ReceiverPhone, order.ReceiverMobile, order.PopType),
            };
            updateReq.ParamWaybillCloudPrintUpdateRequest_ = updateReqBody;
            var rsp = InvokeOpenApi<CainiaoWaybillIiUpdateResponse>(shop, updateReq);
            wuliuNumber.PrintData = rsp.PrintData;
        }

        public override XDocument GetAddress(Shop shop)
        {
            var req = new Top.Api.Request.AreasGetRequest { Fields = "id,type,name,parent_id,zip" };
            var resp = InvokeOpenApi<Top.Api.Response.AreasGetResponse>(shop, req);
            XDocument xDoc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf - 8\"?><Address/>");
            var newList = new List<Top.Api.Domain.Area>(resp.Areas);
            FindSub(xDoc.Root, 1, newList);
            if (newList.Count == resp.Areas.Count)
            {
                throw new Exception("更新失败：未更新任何数据，请联系技术人员");
            }
            return xDoc;
        }

        public override string AddGoods(Shop shop, PopGoods popGoods, float buyInPrice)
        {
            throw new NotImplementedException();
        }
    }
}
