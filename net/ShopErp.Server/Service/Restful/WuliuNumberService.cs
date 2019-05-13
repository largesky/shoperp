using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using ShopErp.Server.Service.Pop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    class WuliuNumberService : ServiceBase<WuliuNumber, WuliuNumberDao>
    {
        /// <summary>
        /// 淘宝平台接口
        /// </summary>
        private const string API_SERVER_URL = "https://eco.taobao.com/router/rest";

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/delete.html")]
        public ResponseBase Delete(long id)
        {
            try
            {
                this.dao.ExcuteSqlUpdate("delete from `WuliuNumber` where Id=" + id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<WuliuNumber> GetByAll(string wuliuIds, string deliveryCompany, string deliveryNumber, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetByAll(wuliuIds, deliveryCompany, deliveryNumber, "", start, end, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 获取菜鸟电子面单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/genwuliunumber.html")]
        public DataCollectionResponse<WuliuNumber> GenWuliuNumber(Shop shop, PrintTemplate wuliuTemplate, Order order, string[] wuliuIds, string packageId, string senderName, string senderPhone, string senderAddress)
        {
            try
            {
                var ps = new PopService();
                string wuliuId = string.Join(",", wuliuIds);
                var wuliuNumber = this.dao.GetByAll(wuliuId, wuliuTemplate.DeliveryCompany, "", packageId, DateTime.MinValue, DateTime.MinValue, 0, 0).Datas.FirstOrDefault(obj => obj.SourceType == wuliuTemplate.SourceType);

                //如果已拉取过快递单号，且订单没有变，只是收货人信息变了，则需要更新物流信息
                if (wuliuNumber != null && wuliuId == wuliuNumber.WuliuIds && (wuliuNumber.ReceiverAddress != order.ReceiverAddress || wuliuNumber.ReceiverName != order.ReceiverName || wuliuNumber.ReceiverPhone != wuliuNumber.ReceiverPhone || wuliuNumber.ReceiverMobile != wuliuNumber.ReceiverMobile))
                {
                    ps.UpdateWuliuNumber(shop, wuliuTemplate, order, wuliuNumber);
                    this.dao.Update(wuliuNumber);
                }
                else
                {
                    //这种情况是属于以前合并打印后，某个订单又拆分出来,此时需要增加包裹编号，否则菜鸟会返回相同的快递信息
                    if (wuliuNumber != null && wuliuId != wuliuNumber.WuliuIds)
                    {
                        packageId = (string.IsNullOrWhiteSpace(packageId) || packageId == "0") ? "1" : packageId + "1";
                    }
                    var wu = ps.GetWuliuNumber(shop, shop.PopSellerNumberId, wuliuTemplate, order, wuliuIds, packageId, senderName, senderPhone, senderAddress);
                    if (wuliuNumber != null)
                    {
                        wu.Id = wuliuNumber.Id;
                    }
                    wuliuNumber = wu;
                }
                if (wuliuNumber.Id < 1)
                {
                    this.dao.Save(wuliuNumber);
                }
                return new DataCollectionResponse<WuliuNumber>(wuliuNumber);
            }
            catch (WebFaultException<ResponseBase>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getwuliubrachs.html")]
        public DataCollectionResponse<WuliuBranch> GetWuliuBrachs(PrintTemplate wuliuTemplate)
        {
            try
            {
                var ps = new PopService();
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.WuliuEnabled).ToArray();
                var shop = shops.FirstOrDefault(obj => wuliuTemplate.SourceType == PrintTemplateSourceType.PINDUODUO ? obj.PopType == PopType.PINGDUODUO : obj.PopType == PopType.TMALL);
                if (shop == null)
                {
                    throw new Exception("没有找到合适的电子面单店铺，请在店铺管理里面配置");
                }
                var wbs = ps.GetWuliuBranchs(shop, wuliuTemplate.CpCode);
                return new DataCollectionResponse<WuliuBranch>(wbs);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 更新淘宝地址库
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/updateaddressarea.html")]
        public ResponseBase UpdateAddressArea()
        {
            try
            {
                var shop = ServiceContainer.GetService<ShopService>().GetByAll().Datas.FirstOrDefault(obj => (obj.PopType == PopType.TAOBAO || obj.PopType == PopType.TMALL) && obj.WuliuEnabled);
                if (shop == null)
                {
                    throw new Exception("没有可以用于更新地址库的淘宝天猫，启用了物流的店铺接口");
                }
                var xDoc = new PopService().GetAddress(shop);
                AddressService.UpdateAndSaveAreas(xDoc);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        string GetNodeName(long type)
        {
            if (type == 1)
            {
                return "Country";
            }
            if (type == 2)
            {
                return "Province";
            }
            if (type == 3)
            {
                return "City";
            }
            if (type == 4)
            {
                return "District";
            }
            if (type == 5)
            {
                return "Town";
            }
            throw new Exception("未知的行政级别");
        }

        private void FindSub(XElement parent, long parentId, List<Top.Api.Domain.Area> areas)
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
                if (a.Type <= 3)
                    FindSub(xe, a.Id, areas);
            }
        }

        public static T InvokeOpenApi<T>(string appKey, string appSecret, string session, ITopRequest<T> request) where T : TopResponse
        {
            var topClient = new DefaultTopClient(API_SERVER_URL, appKey, appSecret);
            var ret = topClient.Execute<T>(request, session, DateTime.Now);
            if (ret.IsError)
            {
                throw new Exception("执行淘宝请求出错:" + ret.ErrCode + "," + ret.ErrMsg + ret.SubErrMsg);
            }
            return ret;
        }
    }
}
