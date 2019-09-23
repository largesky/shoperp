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

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    class PingduoduoPop : PopBase
    {
        private static char[] LEFT_S = "(（".ToCharArray();
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

        private string[] UploadImage(Shop shop, string[] images)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            string[] urls = new string[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                byte[] bytes = MsHttpRestful.DoWithRetry(() => MsHttpRestful.GetUrlEncodeBodyReturnBytes(images[i], null));
                string base64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
                param["image"] = "data:image/jpeg;base64," + base64;
                PingduoduoRspUploadImg ret = MsHttpRestful.DoWithRetry(() => Invoke<PingduoduoRspUploadImg>(shop, "pdd.goods.image.upload", param));
                urls[i] = ret.image_url;
            }
            return urls;
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
                od.Error = new OrderDownloadError(shop.Id, popOrderId, "", e.Message, e.StackTrace);
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

        public override List<WuliuBranch> GetWuliuBranchs(Shop shop, string cpCode)
        {
            throw new NotImplementedException();
        }

        public override List<PrintTemplate> GetAllWuliuTemplates(Shop shop)
        {
            throw new NotImplementedException();
        }

        public override WuliuNumber GetWuliuNumber(Shop shop, string popSellerNumberId, PrintTemplate wuliuTemplate, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress)
        {
            throw new NotImplementedException();
        }

        public override void UpdateWuliuNumber(Shop shop, PrintTemplate wuliuTemplate, Order order, WuliuNumber wuliuNumber)
        {
            throw new NotImplementedException();
        }

        public override XDocument GetAddress(Shop shop)
        {
            throw new NotImplementedException();
        }

        public override string[] AddGoods(Shop shop, PopGoods[] popGoodss, float[] buyInPrices)
        {
            string[] ids = new string[popGoodss.Length];
            //第一步获取，准备数据
            SortedDictionary<string, string> para = new SortedDictionary<string, string>();
            //获取所有地址信息
            para.Clear();
            var addressNode = Invoke<PingduoduoRspAddress>(shop, "pdd.logistics.address.get", new SortedDictionary<string, string>());

            //获取所有运费模板
            para.Clear();
            para["page_size"] = "20";
            var lotemplates = Invoke<PingduoduoRspTemplate>(shop, "pdd.goods.logistics.template.get", para);
            if (lotemplates.logistics_template_list == null || lotemplates.logistics_template_list.Length < 1)
            {
                throw new Exception("拼多多店铺内没有运费模板");
            }
            //获取商品目录
            para.Clear();
            para["parent_cat_id"] = "0";
            var cats = Invoke<PingduoduoRspGoodsCat>(shop, "pdd.goods.cats.get", para);
            var nvxieRootCat = cats.goods_cats_list != null ? cats.goods_cats_list.FirstOrDefault(obj => obj.cat_name == "女鞋") : null;
            if (nvxieRootCat == null)
            {
                throw new Exception("拼多多上没有找到 女鞋 类目");
            }

            Dictionary<string, PinduoduoRspCatTemplateProperty[]> catTemplatesCaches = new Dictionary<string, PinduoduoRspCatTemplateProperty[]>();
            Dictionary<string, string> catIdCaches = new Dictionary<string, string>();
            Dictionary<string, PingduoduoRspGoodsSpecItem> specColorCaches = new Dictionary<string, PingduoduoRspGoodsSpecItem>();
            Dictionary<string, PingduoduoRspGoodsSpecItem> specSizeCaches = new Dictionary<string, PingduoduoRspGoodsSpecItem>();
            for (int i = 0; i < popGoodss.Length; i++)
            {
                var popGoods = popGoodss[i];

                //获取对应商品的运费模板
                var an = addressNode.logistics_address_list.FirstOrDefault(obj => obj.region_type == 2 && obj.region_name.Contains(popGoods.ShippingCity));
                if (an == null)
                {
                    throw new Exception("拼多多地址区库没有找到对应的发货地区");
                }
                PingduoduoRspTemplateItem t = null;
                var tt = lotemplates.logistics_template_list.Where(obj => obj.city_id == an.id.ToString()).ToArray();
                if (tt.Length < 2)
                {
                    t = tt.First();
                }
                else
                {
                    t = tt.FirstOrDefault(obj => obj.template_name == "默认运费模板");
                }
                if (t == null)
                {
                    ids[i] = "拼多多店铺内没有找到对发货地区的运费模板";
                    continue;
                }

                if (catIdCaches.ContainsKey(popGoods.Type) == false)
                {
                    //获取第二级目录
                    para.Clear();
                    para["parent_cat_id"] = nvxieRootCat.cat_id;
                    cats = Invoke<PingduoduoRspGoodsCat>(shop, "pdd.goods.cats.get", para);
                    var typeCat = cats.goods_cats_list != null ? cats.goods_cats_list.FirstOrDefault(obj => obj.cat_name == popGoods.Type) : null;
                    if (typeCat == null)
                    {
                        throw new Exception("拼多多上没有找到 " + popGoods.Type + " 类目");
                    }
                    //获取第三级目录
                    para.Clear();
                    para["parent_cat_id"] = typeCat.cat_id;
                    cats = Invoke<PingduoduoRspGoodsCat>(shop, "pdd.goods.cats.get", para);
                    var leve3Cat = cats.goods_cats_list != null ? cats.goods_cats_list.FirstOrDefault() : null;
                    if (leve3Cat == null)
                    {
                        throw new Exception("拼多多上没有找到第三级目录 " + popGoods.Type + " 类目");
                    }
                    catIdCaches[popGoods.Type] = leve3Cat.cat_id;
                }

                string level3CatId = catIdCaches[popGoods.Type];

                //获取颜色，尺码规格
                if (specColorCaches.ContainsKey(popGoods.Type) == false)
                {
                    para.Clear();
                    para["cat_id"] = level3CatId;
                    var specs = Invoke<PingduoduoRspGoodsSpec>(shop, "pdd.goods.spec.get", para);
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
                    var ct = Invoke<PinduoduoRspCatTemplate>(shop, "pdd.goods.cat.template.get", para);
                    catTemplatesCaches[level3CatId] = ct.properties.Where(obj => string.IsNullOrWhiteSpace(obj.name) == false).ToArray();
                }

                //第一步上传图片,拼多多不传白底图
                popGoods.Images = popGoods.Images.Where(obj => obj.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false).ToArray();
                string[] skuSourceImages = popGoods.Skus.Select(obj => obj.Image).Distinct().ToArray();
                string[] images = UploadImage(shop, popGoods.Images);
                string[] descImages = UploadImage(shop, popGoods.DescImages);
                string[] skuImages = UploadImage(shop, skuSourceImages);

                //拼多多图片张数达到8张，商品分值会高些
                if (images.Length < 8)
                {
                    var im = new string[8];
                    Array.Copy(images, im, images.Length);
                    Array.Copy(images, 0, im, images.Length, 8 - images.Length);
                    images = im;
                }

                //第二步生成规格
                string[] sColors = popGoods.Skus.Select(obj => obj.Color.Trim()).Distinct().ToArray();
                string[] sSize = popGoods.Skus.Select(obj => obj.Size.Trim()).Distinct().ToArray();
                PingduoduoRspGoodsSpecId[] colors = new PingduoduoRspGoodsSpecId[sColors.Length];
                PingduoduoRspGoodsSpecId[] sizes = new PingduoduoRspGoodsSpecId[sSize.Length];

                for (int j = 0; j < sColors.Length; j++)
                {
                    para.Clear();
                    para["parent_spec_id"] = specColorCaches[popGoods.Type].parent_spec_id;
                    para["spec_name"] = sColors[j];
                    colors[j] = Invoke<PingduoduoRspGoodsSpecId>(shop, "pdd.goods.spec.id.get", para);
                }
                for (int j = 0; j < sizes.Length; j++)
                {
                    para.Clear();
                    para["parent_spec_id"] = specSizeCaches[popGoods.Type].parent_spec_id;
                    para["spec_name"] = sSize[j];
                    sizes[j] = Invoke<PingduoduoRspGoodsSpecId>(shop, "pdd.goods.spec.id.get", para);
                }

                //拼装参数
                para.Clear();
                para["goods_name"] = popGoods.Title.Trim();
                para["goods_type"] = "1";
                para["goods_desc"] = "商品跟高，面料，尺码情况请往下滑动查看详情页面";
                para["cat_id"] = level3CatId;
                para["country_id"] = "0";
                para["market_price"] = (((int)(buyInPrices[i] * 3)) * 100).ToString();
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

                //SKU
                List<PinduoduoReqSku> skus = new List<PinduoduoReqSku>();
                for (int j = 0; j < popGoods.Skus.Count; j++)
                {
                    var sku = new PinduoduoReqSku();
                    //价格
                    sku.multi_price = (long)(100 * (float.Parse(popGoods.Skus[j].Price) > (buyInPrices[i] * 2) ? (float.Parse(popGoods.Skus[j].Price) / 2) : float.Parse(popGoods.Skus[j].Price)));
                    sku.price = sku.multi_price + 100;
                    sku.out_sku_sn = popGoods.Skus[j].Code;
                    sku.thumb_url = skuImages[Array.IndexOf(skuSourceImages, popGoods.Skus[j].Image)];
                    sku.spec_id_list = string.Format("[{0},{1}]", colors[Array.FindIndex(colors, 0, o => o.spec_name == popGoods.Skus[j].Color)].spec_id, sizes[Array.FindIndex(sizes, 0, o => popGoods.Skus[j].Size.Trim() == o.spec_name)].spec_id);
                    skus.Add(sku);
                }
                para["sku_list"] = Newtonsoft.Json.JsonConvert.SerializeObject(skus);
                //商品类型
                List<PinduoduoReqGoodsProperty> properties = new List<PinduoduoReqGoodsProperty>();
                var catPropertyTemplate = catTemplatesCaches[level3CatId];
                foreach (var ctp in catPropertyTemplate)
                {
                    //根据淘宝天猫的属性名称，找到对应拼多多名称
                    string taobaoName = PinduoduoGoodsPropertyMap.GetMapPropertyNameByKey(popGoods.PopType, popGoods.Type, ctp.name_alias);
                    if (string.IsNullOrWhiteSpace(taobaoName))
                    {
                        ids[i] += ctp.name_alias + " 未找到对应的淘宝属性，";
                        continue;
                    }

                    if (ctp.name_alias == "品牌")
                    {
                        if (shop.PopShopName.Contains("旗舰店"))
                        {
                            properties.Add(new PinduoduoReqGoodsProperty { template_pid = ctp.id, vid = 128029, value = shop.PopShopName.Replace("旗舰店", "") });
                        }
                        continue;
                    }

                    var taobaoProperty = popGoods.Properties.FirstOrDefault(obj => obj.Key == taobaoName);
                    if (taobaoProperty == null)
                    {
                        if (ctp.required)
                            throw new Exception("拼多多属性：" + ctp.name_alias + " 是必须项，但没有在淘宝找到对应属性");
                        else
                            ids[i] += "属性值：" + ctp.name_alias + " 未匹配";
                        continue;
                    }
                    string[] values = taobaoProperty.Value.Split(new string[] { "@#@" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var vv in values)
                    {
                        var v = ctp.values.FirstOrDefault(obj => MatchValue(obj.value, vv));
                        if (v != null)
                        {
                            properties.Add(new PinduoduoReqGoodsProperty { template_pid = ctp.id, vid = long.Parse(v.vid), value = v.value });
                            if (ctp.choose_max_num <= 1)
                            {
                                break;
                            }
                        }
                        else
                        {
                            ids[i] += "属性值：" + ctp.name_alias + " 的值：" + vv + " 未匹配";
                        }
                    }
                }
                para["goods_properties"] = Newtonsoft.Json.JsonConvert.SerializeObject(properties);
                //第三步上传信息
                var ret = Invoke<PingduoduoRspGoodsAdd>(shop, "pdd.goods.add", para);
                ids[i] = ret.goods_id + "," + ids[i];
            }

            return ids;
        }

        private bool MatchValue(string value1, string value2)
        {
            int i1 = value1.IndexOfAny(LEFT_S);
            int i2 = value2.IndexOfAny(LEFT_S);

            if (i1 > 0)
            {
                value1 = value1.Substring(0, i1);
            }
            if (i2 > 0)
            {
                value2 = value2.Substring(0, i2);
            }
            return value1.Equals(value2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
