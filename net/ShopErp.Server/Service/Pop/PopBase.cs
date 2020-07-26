﻿using ShopErp.Domain;
using System;
using System.Collections.Generic;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Domain.RestfulResponse.DomainResponse;
using System.Xml.Linq;

namespace ShopErp.Server.Service.Pop
{
    public abstract class PopBase
    {
        protected static readonly DateTime UNIX_START_TIME = new DateTime(1970, 01, 01);
        protected static readonly Random random = new Random((int)DateTime.Now.Ticks);
        protected static readonly char[] SPILTE_CHAR = new char[] { '(', '（', '[', '【' };

        public abstract bool Accept(PopType popType);

        public abstract OrderDownloadCollectionResponse GetOrders(Shop shop, string state, DateTime time, int pageIndex, int pageSize);

        public abstract PopOrderState GetOrderState(Shop shop, string popOrderId);

        public abstract List<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize);

        public abstract void ModifyComment(Shop shop, string popOrderId, string comment, ColorFlag flag);

        public abstract void MarkDelivery(Shop shop, string popOrderId, PopPayType payType, string deliveryCompany, string deliveryNumber);

        public abstract PopDeliveryInfo GetDeliveryInfo(Shop shop, string popOrderId);

        public abstract string GetShopOauthUrl(Shop shop);

        public abstract Shop GetAcessTokenInfo(Shop shop, string code);

        public abstract Shop GetRefreshTokenInfo(Shop shop);

        public abstract List<WuliuBranch> GetWuliuBranchs(Shop shop);

        public abstract List<WuliuPrintTemplate> GetWuliuPrintTemplates(Shop shop, string cpCode);

        public abstract WuliuNumber GetWuliuNumber(Shop shop, string popSellerNumberId, WuliuPrintTemplate wuliuTemplate, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress);

        public abstract void UpdateWuliuNumber(Shop shop, WuliuPrintTemplate wuliuTemplate, Order order, WuliuNumber wuliuNumber);

        public abstract string AddGoods(Shop shop, PopGoods popGoods, float buyInPrice);

        public abstract XDocument GetAddress(Shop shop);
    }
}
