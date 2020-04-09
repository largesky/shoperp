using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using ShopErp.Server.Service.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Windows;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class OrderGoodsService : ServiceBase<OrderGoods, OrderGoodsDao>
    {
        private GoodsCount[] MegerGoodsCount(IList<GoodsCount> gcs)
        {
            var goodsCountMarks = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas;
            Vendor[] vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas.ToArray();

            //合并数据
            var goodsCounts = new List<GoodsCount>();
            foreach (var orderGoods in gcs)
            {
                //预处理数据，删除空格等
                orderGoods.Vendor = orderGoods.Vendor.Trim();
                orderGoods.Number = orderGoods.Number.Trim();
                orderGoods.Edtion = orderGoods.Edtion.Trim().Replace("版本", "").Replace("版", "");
                //搜索当前是否存在相同属性的信息
                var gc = goodsCounts.FirstOrDefault(obj => obj.Vendor == orderGoods.Vendor && obj.Number == orderGoods.Number && obj.Edtion == orderGoods.Edtion && obj.Color == orderGoods.Color && obj.Size == orderGoods.Size);
                if (gc == null)
                {
                    gc = orderGoods;
                    gc.DeliveryCounts = new List<DeliveryCount>();
                    gc.DeliveryCompany = gc.DeliveryCompany ?? string.Empty;
                    gc.LastPayTime = gc.FirstPayTime;
                    gc.DeliveryCounts.Add(new DeliveryCount { DeliveryCompany = gc.DeliveryCompany, Count = orderGoods.Count });
                    goodsCounts.Add(gc);
                }
                else
                {
                    gc.OrderId += "," + orderGoods.OrderId;
                    gc.Count += orderGoods.Count;
                    if (gc.DeliveryCounts.FirstOrDefault(obj => obj.DeliveryCompany == gc.DeliveryCompany) != null)
                    {
                        gc.DeliveryCounts.FirstOrDefault(obj => obj.DeliveryCompany == gc.DeliveryCompany).Count += orderGoods.Count;
                    }
                    else
                    {
                        gc.DeliveryCounts.Add(new DeliveryCount { DeliveryCompany = orderGoods.DeliveryCompany ?? String.Empty, Count = gc.Count });
                    }
                }
                //最早时间
                if (gc.FirstPayTime >= orderGoods.FirstPayTime)
                {
                    gc.FirstPayTime = orderGoods.FirstPayTime;
                }
                //最晚时间
                if (gc.LastPayTime <= orderGoods.LastPayTime)
                {
                    gc.LastPayTime = orderGoods.LastPayTime;
                }
            }

            foreach (var goodsCount in goodsCounts)
            {
                //发货地简写
                Vendor vendor = vendors.FirstOrDefault(obj => obj.Name.Equals(goodsCount.Vendor));
                goodsCount.Address = vendor != null ? vendor.MarketAddressShort : "---";
                goodsCount.Vendor = VendorService.FormatVendorName(goodsCount.Vendor);

                //计算是否是其它快快递
                int count = 0;
                foreach (var vv in goodsCount.DeliveryCounts)
                {
                    var dc = goodsCountMarks.FirstOrDefault(obj => obj.Name == vv.DeliveryCompany);
                    if (dc != null && dc.PaperMark)
                    {
                        count += vv.Count;
                    }
                }
                goodsCount.Comment = count > 0 ? "☆" + count : "";
            }
            return goodsCounts.ToArray();
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getgoodscount.html")]
        public DataCollectionResponse<GoodsCount> GetGoodsCount(ColorFlag[] flags, string shipper, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            try
            {
                var gcs = this.dao.GetOrderGoodsCount(flags, shipper, startTime, endTime, pageIndex, pageSize).Datas;
                var ngcs = this.MegerGoodsCount(gcs);
                return new DataCollectionResponse<GoodsCount>(ngcs);
            }
            catch (Exception ex)
            {
                Log.Logger.Log("GetGoodsCount", ex);
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getsalecount.html")]
        public DataCollectionResponse<SaleCount> GetSaleCount(long shopId, OrderType type, int timeType, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            try
            {
                return new DataCollectionResponse<SaleCount>(this.dao.GetSaleCount(shopId, type, timeType, startTime, endTime, pageIndex, pageSize).Datas);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        public DataCollectionResponse<OrderGoods> GetByOrderId(long orderId)
        {
            try
            {
                return new DataCollectionResponse<OrderGoods>(this.dao.GetAllByField("OrderId", orderId, 0, 0).Datas);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}

