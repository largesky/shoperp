using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class DeliveryTemplateService : ServiceBase<DeliveryTemplate, DeliveryTemplateDao>
    {
        private static string[] SPEICAL_ADDRESS = new string[] { "凉山", "甘孜", "阿坝", "克拉玛依市", "阿拉善右旗" };
        private static char[] SP_Char = new char[] { ',' };


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<DeliveryTemplate> GetById(long id)
        {
            try
            {
                return new DataCollectionResponse<DeliveryTemplate>(this.dao.GetById(id));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(DeliveryTemplate value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.Name) || string.IsNullOrWhiteSpace(value.DeliveryCompany))
                {
                    throw new Exception("运费模板名称或者快递公司名称为空");
                }
                if (this.GetFirstOrDefaultInCach(obj => obj.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    throw new Exception("已存在相同的运费模板");
                }
                value.CreateTime = DateTime.Now;
                value.UpdateTime = DateTime.Now;
                dao.Save(value);
                if (value.Areas != null && value.Areas.Count > 0)
                {
                    foreach (var v in value.Areas)
                    {
                        v.DeliveryTemplateId = value.Id;
                    }
                    dao.Save(value.Areas.ToArray());
                }
                this.AndOrReplaceInCach(value, obj => obj.Id == value.Id);
                return new LongResponse(value.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/update.html")]
        public ResponseBase Update(DeliveryTemplate value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.Name) || string.IsNullOrWhiteSpace(value.DeliveryCompany))
                {
                    throw new Exception("运费模板名称或者快递公司名称为空");
                }
                if (this.GetFirstOrDefaultInCach(obj => obj.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase) && obj.Id != value.Id) != null)
                {
                    throw new Exception("已存在相同的运费模板");
                }
                dao.Update(value);
                dao.ExcuteSqlUpdate("delete from DeliveryTemplateArea where DeliveryTemplateId=" + value.Id);
                value.UpdateTime = DateTime.Now;
                value.UpdateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number;
                foreach (var v in value.Areas)
                {
                    v.Id = 0;
                    v.DeliveryTemplateId = value.Id;
                }
                dao.Save(value.Areas.ToArray());
                this.AndOrReplaceInCach(value, obj => obj.Id == value.Id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/delete.html")]
        public ResponseBase Delete(long id)
        {
            try
            {
                this.dao.DeleteByLongId(id);
                dao.ExcuteSqlUpdate("delete from DeliveryTemplateArea where DeliveryTemplateId=" + id);
                this.RemoveCach(obj => obj.Id == id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<DeliveryTemplate> GetByAll()
        {
            try
            {
                return new DataCollectionResponse<DeliveryTemplate>(this.GetAllInCach());
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/computedeliverymoney.html")]
        public FloatResponse ComputeDeliveryMoney(string deliveryCompany, string address, bool empty, PopPayType popPayType, float weight)
        {
            try
            {
                return new FloatResponse(ComputeDeliveryMoneyImpl(deliveryCompany, address, empty, popPayType, weight));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }


        public float ComputeDeliveryMoneyImplByCount(string deliveryCompany, string address, bool empty, PopPayType popPayType, int goodsCount)
        {
            try
            {
                var dt = this.GetAllInCach().FirstOrDefault(obj => obj.DeliveryCompany == deliveryCompany && ((obj.OnlinePayTypeUse && popPayType == PopPayType.ONLINE) || (obj.CodPayTypeUse && popPayType == PopPayType.COD)));
                if (dt == null)
                {
                    throw new Exception("未找到匹配的模板");
                }

                if (empty)
                {
                    return dt.EmptyHotPaperMoney;
                }
                DeliveryTemplateArea da = dt.Areas.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.Areas));
                foreach (var v in dt.Areas)
                {
                    if (string.IsNullOrWhiteSpace(v.Areas) || string.IsNullOrWhiteSpace(address))
                    {
                        continue;
                    }
                    string[] ss = v.Areas.Split(SP_Char, StringSplitOptions.RemoveEmptyEntries);
                    //特殊地址
                    if (SPEICAL_ADDRESS.Any(obj => address.Contains(obj)) && ss.Any(obj => address.Contains(obj)))
                    {
                        da = v;
                        break;
                    }

                    if (ss.Any(obj => address.StartsWith(obj)))
                    {
                        da = v;
                        break;
                    }
                }

                if (da == null)
                {
                    throw new Exception("地址与运费模板:" + dt.Name + " 无法匹配且没有配置默认地区运费");
                }
                return da.StartPrice;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }

        public float ComputeDeliveryMoneyImpl(string deliveryCompany, string address, bool empty, PopPayType popPayType, float weight)
        {
            try
            {
                var dt = this.GetAllInCach().FirstOrDefault(obj => obj.DeliveryCompany == deliveryCompany && ((obj.OnlinePayTypeUse && popPayType == PopPayType.ONLINE) || (obj.CodPayTypeUse && popPayType == PopPayType.COD)));
                if (dt == null)
                {
                    throw new Exception("未找到匹配的模板");
                }

                if (empty)
                {
                    return dt.EmptyHotPaperMoney;
                }
                DeliveryTemplateArea da = dt.Areas.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.Areas));
                foreach (var v in dt.Areas)
                {
                    if (string.IsNullOrWhiteSpace(v.Areas) || string.IsNullOrWhiteSpace(address))
                    {
                        continue;
                    }
                    string[] ss = v.Areas.Split(SP_Char, StringSplitOptions.RemoveEmptyEntries);
                    //特殊地址
                    if (SPEICAL_ADDRESS.Any(obj => address.Contains(obj)) && ss.Any(obj => address.Contains(obj)))
                    {
                        da = v;
                        break;
                    }

                    if (ss.Any(obj => address.StartsWith(obj)))
                    {
                        da = v;
                        break;
                    }
                }

                if (da == null)
                {
                    throw new Exception("地址与运费模板:" + dt.Name + " 无法匹配且没有配置默认地区运费");
                }
                int iw = (int)(weight * 100);
                int isw = (int)(da.StartWeight * 100);
                int isp = (int)(da.StartPrice * 100);
                int isws = (int)(da.StepWeight * 100);
                int ispp = (int)(da.StepPrice * 100);

                //未超过起始重量,或者起始重量为0表示通票
                if (iw <= isw || isw <= 0)
                {
                    return isp * 1.0F / 100;
                }

                int money = isp + (iw - isw + isws - 1) / isws * ispp;
                return money / 100F;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = ex.Message }, System.Net.HttpStatusCode.OK);
            }
        }
    }
}
