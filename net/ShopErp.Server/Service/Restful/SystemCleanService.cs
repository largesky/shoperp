using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class SystemCleanService
    {
        private OrderDao dao = new OrderDao();

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/gettablecountall.html")]
        public LongResponse GetTableCountAll(string table)
        {
            try
            {
                string sql = "select count(Id) from `" + table + "`";
                return new LongResponse(this.dao.GetColumnValueBySqlQuery<long>(sql).First());
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/gettablecount.html")]
        public LongResponse GetTableCount(string table, DateTime start)
        {
            try
            {
                string sql = "select count(Id) from `" + table + "` where CreateTime<='" + start + "'";
                return new LongResponse(this.dao.GetColumnValueBySqlQuery<long>(sql).First());
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/deletetabledata.html")]
        public LongResponse DeleteTableData(string table, DateTime start)
        {
            try
            {
                string sql = "delete from `" + table + "` where CreateTime<='" + start + "'";
                var ret = new LongResponse(this.dao.ExcuteSqlUpdate(sql));

                if (table.Equals("order", StringComparison.OrdinalIgnoreCase))
                {
                    this.dao.ExcuteSqlUpdate("delete from OrderGoods where OrderId not in (select distinct Id from `Order`)");
                }
                return ret;
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }
    }
}
