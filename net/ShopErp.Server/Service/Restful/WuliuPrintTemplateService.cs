using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using ShopErp.Server.Service.Pop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    class WuliuPrintTemplateService
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getwuliuprinttemplates.html")]
        public DataCollectionResponse<WuliuPrintTemplate> GetCloudPrintTemplates(Shop shop, string cpCode)
        {
            try
            {
                var dcs = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll();
                List<WuliuPrintTemplate> wuliuTemplates = new List<WuliuPrintTemplate>();
                var ps = new PopService();
                var wts = ps.GetWuliuPrintTemplates(shop, cpCode);
                wuliuTemplates.AddRange(wts);
                foreach (var wt in wts)
                {
                    var dc = dcs.Datas.FirstOrDefault(obj => wt.SourceType == WuliuPrintTemplateSourceType.CAINIAO ? wt.CpCode == obj.PopMapTaobaoWuliu : wt.CpCode == obj.PopMapPinduoduoWuliu);
                    if (dc == null)
                    {
                        throw new Exception("系统中快递公司没有配置相应的代码：" + wt.CpCode);
                    }
                    wt.DeliveryCompany = dc.Name;
                    System.Console.WriteLine(DateTime.Now.ToString() + wt.SourceType + " " + wt.Name + " " + wt.StandTemplateUrl);
                }

                System.Console.WriteLine(DateTime.Now.ToString() + "获取到物流模板数量：" + wuliuTemplates.Count);
                return new DataCollectionResponse<WuliuPrintTemplate>(wuliuTemplates);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(DateTime.Now.ToString() + ex.Message + Environment.NewLine + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine(DateTime.Now.ToString() + ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace);
                }
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}
