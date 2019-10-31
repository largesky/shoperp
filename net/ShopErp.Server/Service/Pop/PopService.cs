using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Service.Pop.Pdd;
using ShopErp.Server.Service.Pop.Taobao;
using ShopErp.Server.Service.Restful;
using ShopErp.Domain.RestfulResponse.DomainResponse;
using System.Xml.Linq;

namespace ShopErp.Server.Service.Pop
{
    public class PopService
    {
        public const String QUERY_SATE = "QUERY_STATE";
        public const String QUERY_STATE_WAITSHIP = "QUERY_STATE_WAITSHIP";
        public const String QUERY_STATE_WAITSHIP_COD = "QUERY_STATE_WAITSHIP_COD";
        private List<PopBase> pops = new List<PopBase>();

        public PopService()
        {
            Type[] types = typeof(PopService).Assembly.GetTypes().Where(obj => obj.BaseType == typeof(PopBase)).OrderBy(obj => obj.Name).ToArray();
            foreach (var v in types)
            {
                pops.Add(Activator.CreateInstance(v) as PopBase);
            }
        }

        PopBase GetPop(PopType popType)
        {
            var first = this.pops.FirstOrDefault(obj => obj.Accept(popType));
            if (first == null)
            {
                throw new Exception("未找到支持的平台:" + popType);
            }
            return first;
        }

        private void RefreshAccessToken(Shop shop)
        {
            var s = this.GetPop(shop.PopType).GetRefreshTokenInfo(shop);
            var rs = ServiceContainer.GetService<ShopService>().Update(shop);
        }

        private void RaiseExceptionIfShopInfoError(Shop shop)
        {
            if (string.IsNullOrWhiteSpace(shop.AppKey))
            {
                throw new Exception("店铺AppKey信息为空");
            }

            if (string.IsNullOrWhiteSpace(shop.AppSecret))
            {
                throw new Exception("店铺AppSecret信息为空");
            }

            if (string.IsNullOrWhiteSpace(shop.AppAccessToken))
            {
                throw new Exception("店铺AppAccessToken信息为空");
            }

            if (string.IsNullOrWhiteSpace(shop.AppRefreshToken))
            {
                throw new Exception("店铺AppRefreshToken信息为空");
            }
        }

        public PopOrderGetFunction GetOrderGetFunction(PopType popType)
        {
            var first = this.pops.FirstOrDefault(obj => obj.Accept(popType));
            if (first == null)
            {
                throw new Exception("未找到支持的平台:" + popType);
            }
            return first.OrderGetFunctionType;
        }

        private T InvokeWithRefreshAccessToken<T>(Shop shop, Func<T> func)
        {
            try
            {
                RaiseExceptionIfShopInfoError(shop);
                return func();
            }
            catch (PopAccesstokenTimeOutException)
            {
                RefreshAccessToken(shop);
                return func();
            }
        }

        private void InvokeWithRefreshAccessToken(Shop shop, Action action)
        {
            try
            {
                RaiseExceptionIfShopInfoError(shop);
                action();
            }
            catch (PopAccesstokenTimeOutException)
            {
                RefreshAccessToken(shop);
                action();
            }
        }

        public OrderDownload GetOrder(Shop shop, string popOrderId)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<OrderDownload>(shop, () => this.GetPop(shop.PopType).GetOrder(shop, popOrderId));
        }

        public OrderDownloadCollectionResponse GetOrders(Shop shop, string state, int pageIndex, int pageSize)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<OrderDownloadCollectionResponse>(shop, () => GetPop(shop.PopType).GetOrders(shop, state, pageIndex, pageSize));
        }

        public PopOrderState GetOrderState(Shop shop, string popOrderId)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<PopOrderState>(shop, () => GetPop(shop.PopType).GetOrderState(shop, popOrderId));
        }

        public List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<List<PopGoods>>(shop, () => GetPop(shop.PopType).SearchPopGoods(shop, state, pageIndex, pageSize));
        }

        public void ModifyComment(Shop shop, string popOrderId, string comment, ColorFlag flag)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            this.InvokeWithRefreshAccessToken(shop, () => GetPop(shop.PopType).ModifyComment(shop, popOrderId, comment, flag));
        }

        public void MarkDelivery(Shop shop, string popOrderId, PopPayType payType, string deliveryCompany, string deliveryNumber)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            this.InvokeWithRefreshAccessToken(shop, () => GetPop(shop.PopType).MarkDelivery(shop, popOrderId, payType, deliveryCompany, deliveryNumber));
        }

        public PopDeliveryInfo GetDeliveryInfo(Shop shop, string popOrderId)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<PopDeliveryInfo>(shop, () => GetPop(shop.PopType).GetDeliveryInfo(shop, popOrderId));
        }

        public string GetShopOauthUrl(Shop shop)
        {

            return this.GetPop(shop.PopType).GetShopOauthUrl(shop);
        }

        public Shop GetAcessTokenInfo(Shop shop, string code)
        {
            return this.GetPop(shop.PopType).GetAcessTokenInfo(shop, code);
        }

        public List<WuliuBranch> GetWuliuBranchs(Shop shop)
        {
            if (shop.WuliuEnabled == false)
            {
                throw new Exception("店铺电子面单接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<List<WuliuBranch>>(shop, () => GetPop(shop.PopType).GetWuliuBranchs(shop));
        }

        public List<WuliuPrintTemplate> GetWuliuPrintTemplates(Shop shop, string cpCode)
        {
            if (shop.WuliuEnabled == false)
            {
                throw new Exception("店铺电子面单接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<List<WuliuPrintTemplate>>(shop, () => GetPop(shop.PopType).GetWuliuPrintTemplates(shop, cpCode));
        }

        public WuliuNumber GetWuliuNumber(Shop shop, string popSellerNumberId, WuliuPrintTemplate wuliuTemplate, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress)
        {
            if (shop.WuliuEnabled == false)
            {
                throw new Exception("店铺电子面单接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<WuliuNumber>(shop, () => GetPop(shop.PopType).GetWuliuNumber(shop, popSellerNumberId, wuliuTemplate, order, wuliuIds, packageId, senderName, senderPhone, senderAddress));
        }

        public void UpdateWuliuNumber(Shop shop, WuliuPrintTemplate wuliuTemplate, Order order, WuliuNumber wuliuNumber)
        {
            if (shop.WuliuEnabled == false)
            {
                throw new Exception("店铺电子面单接口已禁用，无法调用相应接口操作");
            }
            this.InvokeWithRefreshAccessToken(shop, (Action)(() => GetPop(shop.PopType).UpdateWuliuNumber(shop, wuliuTemplate, order, wuliuNumber)));
        }

        public string[] AddGoods(Shop shop, PopGoods[] popGoods, float[] buyInPrices)
        {
            if (shop.AppEnabled == false)
            {
                throw new Exception("店铺订单发货接口已禁用，无法调用相应接口操作");
            }
            return GetPop(shop.PopType).AddGoods(shop, popGoods, buyInPrices);
        }

        public XDocument GetAddress(Shop shop)
        {
            if (shop.WuliuEnabled == false)
            {
                throw new Exception("店铺电子面单接口已禁用，无法调用相应接口操作");
            }
            return this.InvokeWithRefreshAccessToken<XDocument>(shop, () => GetPop(shop.PopType).GetAddress(shop));
        }
    }
}
