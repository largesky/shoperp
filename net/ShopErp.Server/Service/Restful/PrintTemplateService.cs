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

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    class PrintTemplateService
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getprinttemplate.html")]
        public DataCollectionResponse<PrintTemplate> GetPrintTemplate(Shop shop)
        {
            try
            {
                var shops = new List<Shop>();
                if (shop != null)
                {
                    shops.Add(shop);
                }
                else
                {
                    shops = Restful.ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.WuliuEnabled).ToList();
                }
                if (shops.Count < 1)
                {
                    throw new Exception("系统中没有启用电子面单接口的店铺");
                }
                var dcs = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll();
                List<PrintTemplate> wuliuTemplates = new List<PrintTemplate>();
                var ps = new PopService();
                foreach (var s in shops)
                {
                    var wts = ps.GetAllWuliuTemplates(s);
                    wuliuTemplates.AddRange(wts);
                    foreach (var wt in wts)
                    {
                        var dc = dcs.Datas.FirstOrDefault(obj => wt.SourceType == PrintTemplateSourceType.CAINIAO ? wt.CpCode == obj.PopMapTaobaoWuliu : wt.CpCode == obj.PopMapPinduoduoWuliu);
                        if (dc == null)
                        {
                            throw new Exception("系统中快递公司没有配置相应的代码：" + wt.CpCode);
                        }
                        wt.DeliveryCompany = dc.Name;
                    }
                }
                return new DataCollectionResponse<PrintTemplate>(wuliuTemplates);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}
