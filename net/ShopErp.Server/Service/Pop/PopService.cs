using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Service.Pop.Chuchujie;
using ShopErp.Server.Service.Pop.Pingduoduo;
using ShopErp.Server.Service.Pop.Taobao;
using ShopErp.Server.Service.Restful;
using ShopErp.Domain.RestfulResponse.DomainResponse;

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

        public PopOrderGetFunction GetOrderGetFunction(PopType popType)
        {
            var first = this.pops.FirstOrDefault(obj => obj.Accept(popType));
            if (first == null)
            {
                throw new Exception("未找到支持的平台:" + popType);
            }
            return first.OrderGetFunctionType;
        }

        private T InvokeFuncWithRetry<T>(Shop shop, Func<T> func)
        {
            try
            {
                if (shop.AppEnabled == false)
                {
                    throw new Exception("店铺接口已禁用，无法调用相应接口操作");
                }
                return func();
            }
            catch (PopAccesstokenTimeOutException)
            {
                var s = this.GetPop(shop.PopType).GetRefreshTokenInfo(shop);
                var rs = ServiceContainer.GetService<ShopService>().Update(shop);
                if (rs.error != ResponseBase.SUCCESS.error)
                {
                    throw new Exception("店铺：" + shop.Id + "授权到期，刷新新授权码的时候无法保存到系统");
                }
                return func();
            }
        }

        private void InvokeActionWithRetry(Shop shop, Action action)
        {
            try
            {
                if (shop.AppEnabled == false)
                {
                    throw new Exception("店铺接口已禁用，无法调用相应接口操作");
                }
                action();
            }
            catch (PopAccesstokenTimeOutException)
            {
                var s = this.GetPop(shop.PopType).GetRefreshTokenInfo(shop);
                var rs = ServiceContainer.GetService<ShopService>().Update(shop);
                if (rs.error != ResponseBase.SUCCESS.error)
                {
                    throw new Exception("店铺：" + shop.Id + "授权到期，刷新新授权码的时候无法保存到系统");
                }
                action();
            }
        }

        public OrderDownload GetOrder(Shop shop, string popOrderId)
        {
            return this.InvokeFuncWithRetry<OrderDownload>(shop, () => this.GetPop(shop.PopType).GetOrder(shop, popOrderId));
        }

        public OrderDownloadCollectionResponse GetOrders(Shop shop, string state, int pageIndex, int pageSize)
        {
            return this.InvokeFuncWithRetry<OrderDownloadCollectionResponse>(shop, () => GetPop(shop.PopType).GetOrders(shop, state, pageIndex, pageSize));
        }

        public PopOrderState GetOrderState(Shop shop, string popOrderId)
        {
            return this.InvokeFuncWithRetry<PopOrderState>(shop, () => GetPop(shop.PopType).GetOrderState(shop, popOrderId));
        }

        public List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            return this.InvokeFuncWithRetry<List<PopGoods>>(shop, () => GetPop(shop.PopType).SearchPopGoods(shop, state, pageIndex, pageSize));
        }

        public void ModifyComment(Shop shop, string popOrderId, string comment, ColorFlag flag)
        {
            this.InvokeActionWithRetry(shop, () => GetPop(shop.PopType).ModifyComment(shop, popOrderId, comment, flag));
        }

        public void MarkDelivery(Shop shop, string popOrderId, PopPayType payType, string deliveryCompany, string deliveryNumber)
        {
            this.InvokeActionWithRetry(shop, () => GetPop(shop.PopType).MarkDelivery(shop, popOrderId, payType, deliveryCompany, deliveryNumber));
        }

        public PopDeliveryInfo GetDeliveryInfo(Shop shop, string popOrderId)
        {
            return this.InvokeFuncWithRetry<PopDeliveryInfo>(shop, () => GetPop(shop.PopType).GetDeliveryInfo(shop, popOrderId));
        }

        public string GetShopOauthUrl(Shop shop)
        {
            return this.GetPop(shop.PopType).GetShopOauthUrl(shop);
        }

        public Shop GetAcessTokenInfo(Shop shop, string code)
        {
            return this.GetPop(shop.PopType).GetAcessTokenInfo(shop, code);
        }
    }
}
