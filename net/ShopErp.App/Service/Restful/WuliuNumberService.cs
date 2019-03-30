using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopErp.App.Service.Restful
{
    class WuliuNumberService : ServiceBase<WuliuNumber>
    {
        public DataCollectionResponse<WuliuNumber> GetByAll(string wuliuIds, string deliveryCompany, string deliveryNumber, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["wuliuIds"] = wuliuIds;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["start"] = start;
            para["end"] = end;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<WuliuNumber>>(para);
        }

        public DataCollectionResponse<WuliuNumber> GenNormalWuliuNumber(string deliveryCompany, string current, string address)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryCompany"] = deliveryCompany;
            para["current"] = current;
            para["address"] = address;
            return DoPost<DataCollectionResponse<WuliuNumber>>(para);
        }

        public DataCollectionResponse<WuliuNumber> GenCainiaoWuliuNumber(string deliveryCompany, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryCompany"] = deliveryCompany;
            para["order"] = order;
            para["wuliuIds"] = wuliuIds;
            para["packageId"] = packageId;
            para["senderName"] = senderName;
            para["senderPhone"] = senderPhone;
            para["senderAddress"] = senderAddress;
            return DoPost<DataCollectionResponse<WuliuNumber>>(para);
        }

        public ResponseBase CancelCainiaoWuliuNumber(string deliveryNumber)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryNumber"] = deliveryNumber;
            return DoPost<ResponseBase>(para);
        }

        public DataCollectionResponse<WuliuBranch> GetWuliuBrachs(string type, string code)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["type"] = type;
            para["code"] = code;
            return DoPost<DataCollectionResponse<WuliuBranch>>(para);
        }

        public StringResponse UpdateAddressArea()
        {
            return DoPost<StringResponse>(null);
        }
    }
}