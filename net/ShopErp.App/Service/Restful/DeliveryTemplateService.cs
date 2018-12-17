using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.App.Service.Restful
{
    public class DeliveryTemplateService : ServiceBase<DeliveryTemplate>
    {
        private static string[] SPEICAL_ADDRESS = new string[] { "凉山", "甘孜", "阿坝", "克拉玛依市", "阿拉善右旗" };
        private static char[] SP_Char = new char[] { ',' };

        public DataCollectionResponse<DeliveryTemplate> GetByAll()
        {
            return DoPost<DataCollectionResponse<DeliveryTemplate>>(null);
        }

        public FloatResponse ComputeDeliveryMoney(string deliveryCompany, string address, bool empty, PopPayType popPayType, float weight)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryCompany"] = deliveryCompany;
            para["address"] = address;
            para["empty"] = empty;
            para["popPayType"] = popPayType;
            para["weight"] = weight;

            return DoPost<FloatResponse>(para);
        }
    }
}