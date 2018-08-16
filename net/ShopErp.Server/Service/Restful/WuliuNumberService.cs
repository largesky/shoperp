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
        private static List<CainiaoCloudprintStdtemplatesGetResponse.StandardTemplateResultDomain> caiNiaoTemplates;

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


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/gennormalwuliunumber.html")]
        public DataCollectionResponse<WuliuNumber> GenNormalWuliuNumber(string deliveryCompany, string current, string address)
        {
            try
            {
                throw new NotImplementedException();
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
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/gencainiaowuliunumber.html")]
        public DataCollectionResponse<WuliuNumber> GenCainiaoWuliuNumber(string deliveryCompany, Order order, string[] wuliuIds, string packageId)
        {
            try
            {
                //RefreshTaobaoCainiaoAccessInfo();
                //初始化信息及检查，这些信息有可能会随便更新，所以每次都要获取最新的
                var senderName = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, "");
                var senderPhone = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, "");
                var popSellerNumberId = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SELLER_ID, "");
                var appKey = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_APP_KEY, "");
                var appSecret = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_APP_SECRET, "");
                var appSession = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_APP_SESSION, "");

                if (string.IsNullOrWhiteSpace(senderName) || string.IsNullOrWhiteSpace(senderPhone))
                {
                    throw new Exception("淘宝接口发货人不完整请配置");
                }

                if (string.IsNullOrWhiteSpace(popSellerNumberId))
                {
                    throw new Exception("淘宝卖家数据编号为空");
                }

                if (string.IsNullOrWhiteSpace(appKey) || string.IsNullOrWhiteSpace(appSecret) || string.IsNullOrWhiteSpace(appSession))
                {
                    throw new Exception("淘宝菜鸟接口授权信息不完整请配置");
                }

                //拉取单号的时候，必须要有一个模板地址  获取菜鸟系统里面的模板，
                if (caiNiaoTemplates == null || caiNiaoTemplates.Count < 1)
                {
                    var reqT = new CainiaoCloudprintStdtemplatesGetRequest();
                    var rspT = InvokeOpenApi<CainiaoCloudprintStdtemplatesGetResponse>(appKey, appSecret, appSession, reqT);
                    if (rspT.Result.Datas.Count < 1)
                    {
                        throw new Exception("菜鸟系统中没有默认打印模板无法打印");
                    }
                    caiNiaoTemplates = rspT.Result.Datas;
                }
                var templte = caiNiaoTemplates.FirstOrDefault(obj => obj.CpCode == GetCPCodeCN(deliveryCompany));
                if (templte == null)
                {
                    throw new Exception("菜鸟系统中没有默认打印模板无法打印,快递公司:" + GetCPCodeCN(deliveryCompany) + " " + deliveryCompany);
                }
                if (templte.StandardTemplates == null || templte.StandardTemplates.Count < 1)
                {
                    throw new Exception("菜鸟系统中对应的标准模板没有具体模板,快递公司:" + GetCPCodeCN(deliveryCompany) + " " + deliveryCompany);
                }
                string printData = "";
                string cpCode = GetCPCodeCN(deliveryCompany);
                string wuliuId = string.Join(",", wuliuIds);
                //检查以前是否打印过
                var wuliuNumber = this.dao.GetByAll(wuliuId, deliveryCompany, "", packageId, DateTime.MinValue, DateTime.MinValue, 0, 0).First;
                if (wuliuNumber != null)
                {
                    if (wuliuId != wuliuNumber.WuliuIds)
                    {
                        //这种情况是属于以前合并打印后，某个订单又拆分出来,此时需要增加包裹编号，否则菜鸟会返回相同的快递信息
                        packageId = string.IsNullOrWhiteSpace(packageId) ? "1" : packageId + "1";
                    }
                    else
                    {
                        //有数据，则检查是否更新，
                        if (wuliuNumber.ReceiverAddress == order.ReceiverAddress && wuliuNumber.ReceiverName == order.ReceiverName && wuliuNumber.ReceiverPhone == wuliuNumber.ReceiverPhone && wuliuNumber.ReceiverMobile == wuliuNumber.ReceiverMobile)
                        {
                            ContactRouteCodeAndSortationName(wuliuNumber);
                            return new DataCollectionResponse<WuliuNumber>(wuliuNumber);
                        }
                        //需要更新菜鸟面单以打印正确的信息
                        var updateReq = new CainiaoWaybillIiUpdateRequest { };
                        var updateReqBody = new CainiaoWaybillIiUpdateRequest.WaybillCloudPrintUpdateRequestDomain
                        {
                            CpCode = cpCode,
                            WaybillCode = wuliuNumber.DeliveryNumber,
                            Recipient = ParseTaobaoAddressUpdate(order.ReceiverAddress, order.ReceiverName, order.ReceiverPhone, order.ReceiverMobile),
                        };
                        updateReq.ParamWaybillCloudPrintUpdateRequest_ = updateReqBody;
                        var updateResp = InvokeOpenApi<CainiaoWaybillIiUpdateResponse>(appKey, appSecret, appSession, updateReq);
                        printData = updateResp.PrintData;
                    }
                }
                if (string.IsNullOrWhiteSpace(printData))
                {
                    //生成请求参数
                    CainiaoWaybillIiGetRequest req = new CainiaoWaybillIiGetRequest();
                    var reqBody = new CainiaoWaybillIiGetRequest.WaybillCloudPrintApplyNewRequestDomain();
                    reqBody.CpCode = GetCPCodeCN(deliveryCompany);
                    reqBody.Sender = new CainiaoWaybillIiGetRequest.UserInfoDtoDomain { Phone = "", Name = senderName, Mobile = senderPhone, Address = GetShippingAddress() };
                    //订单信息，一个请求里面可以包含多个订单，我们系统里面，默认一个
                    reqBody.TradeOrderInfoDtos = new List<CainiaoWaybillIiGetRequest.TradeOrderInfoDtoDomain>();
                    var or = new CainiaoWaybillIiGetRequest.TradeOrderInfoDtoDomain { ObjectId = Guid.NewGuid().ToString() };
                    or.UserId = long.Parse(popSellerNumberId);
                    or.TemplateUrl = templte.StandardTemplates.First().StandardTemplateUrl;
                    or.Recipient = new CainiaoWaybillIiGetRequest.UserInfoDtoDomain { Phone = order.ReceiverPhone, Mobile = order.ReceiverMobile, Name = order.ReceiverName, Address = ParseTaobaoAddress(order.ReceiverAddress), };
                    or.OrderInfo = new CainiaoWaybillIiGetRequest.OrderInfoDtoDomain { OrderChannelsType = GetOrderChannleTypeCN(order.PopType), TradeOrderList = new List<string>(wuliuIds) };
                    or.PackageInfo = new CainiaoWaybillIiGetRequest.PackageInfoDtoDomain { Id = packageId == "" ? null : packageId, Items = new List<CainiaoWaybillIiGetRequest.ItemDomain>() };
                    or.PackageInfo.Items.AddRange(order.OrderGoodss.Where(obj => (int)obj.State >= (int)OrderState.PAYED && (int)obj.State <= (int)OrderState.NOTSALE).Select(obj => new CainiaoWaybillIiGetRequest.ItemDomain { Name = obj.Number + "," + obj.Edtion + "," + obj.Color + "," + obj.Size, Count = obj.Count }));
                    if (or.PackageInfo.Items.Count < 1)
                    {
                        or.PackageInfo.Items.Add(new CainiaoWaybillIiGetRequest.ItemDomain { Name = "没有商品或者其它未定义商品", Count = 1 });
                    }
                    reqBody.TradeOrderInfoDtos.Add(or);
                    req.ParamWaybillCloudPrintApplyNewRequest_ = reqBody;
                    var rsp = InvokeOpenApi<CainiaoWaybillIiGetResponse>(appKey, appSecret, appSession, req);
                    if (rsp.Modules == null || rsp.Modules.Count < 1)
                    {
                        throw new Exception("菜鸟电子面单未返回数据:" + rsp.ErrMsg);
                    }
                    printData = rsp.Modules[0].PrintData;
                    wuliuNumber = new WuliuNumber { CreateTime = DateTime.Now };
                }
                CainiaoPrintData resp = Newtonsoft.Json.JsonConvert.DeserializeObject<CainiaoPrintData>(printData);
                wuliuNumber.ReceiverAddress = order.ReceiverAddress;
                wuliuNumber.ReceiverMobile = order.ReceiverMobile;
                wuliuNumber.ReceiverName = order.ReceiverName;
                wuliuNumber.ReceiverPhone = order.ReceiverPhone;
                wuliuNumber.DeliveryCompany = deliveryCompany;
                wuliuNumber.DeliveryNumber = resp.data.waybillCode;
                wuliuNumber.ConsolidationCode = resp.data.routingInfo.consolidation.code ?? "";
                wuliuNumber.OriginCode = resp.data.routingInfo.origin.code ?? "";
                wuliuNumber.OriginName = resp.data.routingInfo.origin.name ?? "";
                wuliuNumber.RouteCode = resp.data.routingInfo.routeCode ?? "";
                wuliuNumber.SortationName = resp.data.routingInfo.sortation.name ?? "";
                wuliuNumber.SortationNameAndRouteCode = "";
                wuliuNumber.WuliuIds = wuliuId;
                wuliuNumber.PackageId = packageId;
                wuliuNumber.PrintData = printData;
                ContactRouteCodeAndSortationName(wuliuNumber);
                if (wuliuNumber.Id > 0)
                {
                    this.dao.Update(wuliuNumber);
                }
                else
                {
                    this.dao.Save(wuliuNumber);
                }
                return new DataCollectionResponse<WuliuNumber>(wuliuNumber);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        private bool MatchAddres(CainiaoPrintDataDataRecipientAddress address, string receiverAddress)
        {
            address.district = address.district ?? "";
            address.town = address.town ?? "";
            var tbAdd = ParseTaobaoAddress(receiverAddress);
            return tbAdd.Province.Equals(address.province) &&
               tbAdd.City.Equals(address.city) &&
               tbAdd.District.Equals(address.district) &&
               tbAdd.Town.Equals(address.town) &&
               tbAdd.Detail.Equals(address.detail);
        }

        /// <summary>
        /// 获取菜鸟电子面单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/cancelcainiaowuliunumber.html")]
        public ResponseBase CancelCainiaoWuliuNumber(string deliveryNumber)
        {
            try
            {
                throw new NotImplementedException();
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
                var senderName = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, "");
                var senderPhone = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, "");
                var popSellerNumberId = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SELLER_ID, "");
                var appKey = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_APP_KEY, "");
                var appSecret = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_APP_SECRET, "");
                var appSession = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_APP_SESSION, "");

                if (string.IsNullOrWhiteSpace(senderName) || string.IsNullOrWhiteSpace(senderPhone))
                {
                    throw new Exception("淘宝接口发货人不完整请配置");
                }

                if (string.IsNullOrWhiteSpace(popSellerNumberId))
                {
                    throw new Exception("淘宝卖家数据编号为空");
                }

                if (string.IsNullOrWhiteSpace(appKey) || string.IsNullOrWhiteSpace(appSecret) || string.IsNullOrWhiteSpace(appSession))
                {
                    throw new Exception("淘宝菜鸟接口授权信息不完整请配置");
                }
                var req = new Top.Api.Request.AreasGetRequest { Fields = "id,type,name,parent_id,zip" };
                var resp = InvokeOpenApi<Top.Api.Response.AreasGetResponse>(appKey, appSecret, appSession, req);
                XDocument xDoc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf - 8\"?><Address/>");
                var newList = new List<Top.Api.Domain.Area>(resp.Areas);
                FindSub(xDoc.Root, 1, newList);
                if (newList.Count == resp.Areas.Count)
                {
                    throw new Exception("更新失败：未更新任何数据，请联系技术人员");
                }
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

        #region 普通单号生成方法


        public static string GenBashihutong(string current)
        {
            if (current.Length != 12)
            {
                throw new Exception("百世汇通快递单号生成失败,当前快递快递单号不为12位:" + current);
            }

            long l = long.Parse(current);
            l++;
            return l.ToString();
        }


        public static string GenChinapostXiaobao(string current)
        {
            if (current.Length != 13)
            {
                throw new Exception("邮政快递单号生成失败,当前快递快递单号不为13位:" + current);
            }

            long l = long.Parse(current);
            l++;
            return l.ToString();
        }

        public static string GenEms(string current)
        {
            if (string.IsNullOrWhiteSpace(current) || current.Length != 13)
            {
                throw new Exception("EMS快递问题格式不正确" + current);
            }
            int[] multy = new int[] { 8, 6, 4, 2, 3, 5, 9, 7 };
            string pre = current.Substring(0, 2);
            string last = current.Substring(11, 2);
            long number = long.Parse(current.Substring(2, 8)) + 1;
            int[] numbers = number.ToString("D8").Select(c => int.Parse(c.ToString())).ToArray();
            int total = 0;

            for (int i = 0; i < numbers.Length; i++)
            {
                total += multy[i] * numbers[i];
            }

            int res = 11 - total % 11;
            if (res == 10)
            {
                res = 0;
            }
            else if (res == 11)
            {
                res = 5;
            }
            string newNumber = pre + number.ToString("D8") + res + last;
            return newNumber;
        }

        public static string GenSF(string current)
        {
            if (string.IsNullOrWhiteSpace(current) || current.Length != 12)
            {
                throw new Exception("顺丰快递单号不为12位:" + current);
            }

            string area = current.Substring(0, 3);
            string number = current.Substring(3, 8);
            string lastNumber = current.Substring(11, 1);
            string newNumber = number;
            long next = long.Parse(number) + 1;

            if (current[10] != '9')
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 9) % 10;
                return newNumber;
            }

            if ("0124578".Any(c => c == current[9]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 6) % 10;
                return newNumber;
            }

            if ("36".Any(c => c == current[9]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 5) % 10;
                return newNumber;
            }

            if ("02468".Any(c => c == current[8]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 3) % 10;
                return newNumber;
            }

            if ("1357".Any(c => c == current[8]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 2) % 10;
                return newNumber;
            }

            if ("036".Any(c => c == current[7]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 0) % 10;
                return newNumber;
            }

            if ("124578".Any(c => c == current[7]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 9) % 10;
                return newNumber;
            }

            if ("0".Any(c => c == current[6]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 7) % 10;
                return newNumber;
            }

            if ("12345678".Any(c => c == current[6]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 6) % 10;
                return newNumber;
            }

            if ("012345678".Any(c => c == current[5]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 3) % 10;
                return newNumber;
            }

            if ("0124578".Any(c => c == current[4]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 9) % 10;
                return newNumber;
            }

            if ("36".Any(c => c == current[4]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 8) % 10;
                return newNumber;
            }

            if ("02468".Any(c => c == current[3]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 5) % 10;
                return newNumber;
            }

            if ("1357".Any(c => c == current[3]))
            {
                newNumber = area + next.ToString("D8") + (int.Parse(lastNumber) + 4) % 10;
                return newNumber;
            }
            throw new Exception("顺丰快递无法计算下一个快递单号：" + current);
        }

        public static string GenSto(string current)
        {
            if (String.IsNullOrWhiteSpace(current) || current.Length != 12)
            {
                throw new Exception("申通快递当前单号不为12位:" + current);
            }

            long l = long.Parse(current);
            l++;
            return l.ToString();
        }

        public static string GenTiantian(string current)
        {
            if (string.IsNullOrWhiteSpace(current) || current.Length != 12)
            {
                throw new Exception("天天快递单号生成错误，当前单号不为12位：" + current);
            }
            long l = long.Parse(current);
            l++;
            return l.ToString();
        }

        public static string GenYto(string current)
        {
            if (string.IsNullOrWhiteSpace(current) || (current.Length != 10 && current.Length != 12 && current.Length != 16))
            {
                throw new Exception("圆通快递单号必须为10,12,16位:" + current);
            }
            int numberStartIndex = current.IndexOfAny("0123456789".ToArray());
            string prefix = numberStartIndex > 0 ? current.Substring(0, numberStartIndex) : "";
            long currentNumber = long.Parse(current.Substring(numberStartIndex));
            long nextNumber = currentNumber + 1;
            string newNumber = prefix + nextNumber.ToString("D" + (current.Length - prefix.Length));
            return newNumber;
        }

        public static string GenYunda(string current)
        {
            if (current.Length != 13)
            {
                throw new Exception("韵达快递单号生成失败,当前快递快递单号不为13位:" + current);
            }

            long l = long.Parse(current);
            l++;
            return l.ToString();
        }

        public static string GenZjs(string current)
        {
            if (current.Length != 10)
            {
                throw new Exception("宅急送运单号为长度为10");
            }

            int d1 = int.Parse(current.Substring(0, 3));
            int d2 = int.Parse(current.Substring(3, 3));
            int d3 = int.Parse(current.Substring(6, 3));
            int d4 = int.Parse(current.Substring(9, 1));

            d4 = (++d4) % 7;
            d2 += (++d3) / 1000;
            d1 += (d2 + 1) / 1000;

            d3 = d3 % 1000;
            d2 = d2 % 1000;

            string ret = string.Format("{0:d3}{1:d3}{2:d3}{3}", d1, d2, d3, d4);
            return ret;
        }

        public static string GenZto(string current)
        {
            //中通快递格式718616673883
            if (string.IsNullOrWhiteSpace(current) || current.Length != 12)
            {
                throw new Exception("中通快递单号生成失败，当前单号格式不为12位：" + current);
            }

            long l = long.Parse(current);
            l++;

            return l.ToString();
        }

        #endregion

        #region 菜鸟电子面单相关方法

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

        private static CainiaoWaybillIiGetRequest.AddressDtoDomain GetShippingAddress()
        {
            string add = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_SENDER_ADDRESS, "");
            if (string.IsNullOrWhiteSpace(add))
            {
                throw new Exception("系统没有配置发货地址，请在系统配置设置");
            }
            var adds = add.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (adds.Length < 4)
            {
                throw new Exception("系统中配置的发货地址不对，至少包含4部分以空格分开");
            }
            var wa = new CainiaoWaybillIiGetRequest.AddressDtoDomain { Province = adds[0], City = adds[1], District = adds[2] };
            if (adds.Length > 4)
            {
                wa.Town = adds[3];
                wa.Detail = adds[4];
            }
            else
            {
                wa.Detail = adds[3];
                wa.Town = "";
            }
            return wa;
        }

        private static CainiaoWaybillIiUpdateRequest.AddressDtoDomain GetShippingAddressUpdate()
        {
            string add = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, "TAOBAO_SENDER_ADDRESS", "");
            if (string.IsNullOrWhiteSpace(add))
            {
                throw new Exception("系统没有配置发货地址，请在系统配置设置");
            }
            var adds = add.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (adds.Length < 4)
            {
                throw new Exception("系统中配置的发货地址不对，至少包含4部分以空格分开");
            }
            var wa = new CainiaoWaybillIiUpdateRequest.AddressDtoDomain { Province = adds[0], City = adds[1], District = adds[2] };
            if (adds.Length > 4)
            {
                wa.Town = adds[3];
                wa.Detail = adds[4];
            }
            else
            {
                wa.Detail = adds[3];
                wa.Town = "";
            }
            return wa;
        }

        /// <summary>
        /// 获取菜鸟规定的快递公司简码
        /// </summary>
        /// <param name="deliveryCompany"></param>
        /// <returns></returns>
        private static string GetCPCodeCN(string deliveryCompany)
        {
            if (deliveryCompany.Contains("圆通"))
                return "YTO";
            if (deliveryCompany.Contains("顺丰"))
                return "SF";
            if (deliveryCompany.Contains("小包"))
                return "POSTB";
            if (deliveryCompany.Contains("EMS") && deliveryCompany.Contains("标准"))
                return "EMS";
            if (deliveryCompany.Contains("EMS") && deliveryCompany.Contains("经济"))
                return "EYB";
            if (deliveryCompany.Contains("宅急送"))
                return "ZJS";
            if (deliveryCompany.Contains("中通"))
                return "ZTO";
            if (deliveryCompany.Contains("汇通"))
                return "HTKY";
            if (deliveryCompany.Contains("优速"))
                return "UC";
            if (deliveryCompany.Contains("申通"))
                return "STO";
            if (deliveryCompany.Contains("天天"))
                return "TTKDEX";
            if (deliveryCompany.Contains("全峰"))
                return "QFKD";
            if (deliveryCompany.Contains("快捷"))
                return "FAST";
            if (deliveryCompany.Contains("国通"))
                return "GTO";
            if (deliveryCompany.Contains("韵达"))
                return "YUNDA";
            throw new Exception("快递公司：" + deliveryCompany + " 不在淘宝电子面单内");
        }

        private static string GetOrderChannleTypeCN(PopType type)
        {
            return "OTHERS";

            //if (type == PopType.TAOBAO)
            //{
            //    return "TB";
            //}
            //if (type == PopType.TMALL)
            //{
            //    return "TM";
            //}
            //if (type == PopType.JINGDONG)
            //{
            //    return "JD";
            //}
            //return "OTHERS";
        }

        private static CainiaoWaybillIiGetRequest.AddressDtoDomain ParseTaobaoAddress(string address)
        {
            var p = AddressService.ParseProvince(address);
            var c = AddressService.ParseCity(address);
            var a = AddressService.ParseRegion(address);
            if (p == null)
            {
                throw new Exception("地址解析失败未找出省");
            }
            if (c == null)
            {
                throw new Exception("地址解析失败未找出市");
            }

            string ad = AddressService.TrimStart(address, p.Name, 2);
            ad = AddressService.TrimStart(ad, c.Name, 2);
            if (a != null)
            {
                ad = AddressService.TrimStart(ad, a.Name, 2);
            }
            var wd = new CainiaoWaybillIiGetRequest.AddressDtoDomain
            {
                Province = p.Name,
                City = c.Name,
                District = a == null ? "" : a.Name,
                Detail = ad,
                Town = "",
            };
            return wd;
        }

        private static CainiaoWaybillIiUpdateRequest.UserInfoDtoDomain ParseTaobaoAddressUpdate(string address, string name, string phone, string mobile)
        {
            var p = AddressService.ParseProvince(address);
            var c = AddressService.ParseCity(address);
            var a = AddressService.ParseRegion(address);
            if (p == null)
            {
                throw new Exception("地址解析失败未找出省");
            }
            if (c == null)
            {
                throw new Exception("地址解析失败未找出市");
            }

            string ad = AddressService.TrimStart(address, p.Name, 2);
            ad = AddressService.TrimStart(ad, c.Name, 2);
            if (a != null)
            {
                ad = AddressService.TrimStart(ad, a.Name, 2);
            }
            var wd = new CainiaoWaybillIiUpdateRequest.UserInfoDtoDomain
            {
                Address = new CainiaoWaybillIiUpdateRequest.AddressDtoDomain
                {
                    Province = p.Name,
                    City = c.Name,
                    District = a == null ? "" : a.Name,
                    Detail = ad,
                    Town = "",
                },
                Name = name,
                Phone = phone,
                Mobile = mobile,
            };
            return wd;
        }

        private static void ContactRouteCodeAndSortationName(WuliuNumber wn)
        {
            if (wn.DeliveryCompany.Contains("圆通"))
            {
                if (string.IsNullOrWhiteSpace(wn.RouteCode) == false)
                {
                    wn.SortationNameAndRouteCode = wn.RouteCode;
                    return;
                }
                wn.SortationNameAndRouteCode = wn.SortationName;
                return;
            }
            var cainiaoContactDeliveryCompanies = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, SystemNames.CONFIG_CAINIAO_CONTACT_DELIVERYCOMPANIES, "");
            if (string.IsNullOrWhiteSpace(cainiaoContactDeliveryCompanies))
            {
                throw new Exception("系统中没有配置需要拼接大头笔与三段码的快递公司");
            }

            string[] dcs = cainiaoContactDeliveryCompanies.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            if (dcs.Any(obj => obj == wn.DeliveryCompany))
            {
                wn.SortationNameAndRouteCode = wn.SortationName + " " + wn.RouteCode;
            }
            else
            {
                wn.SortationNameAndRouteCode = wn.SortationName;
            }
        }

        #endregion
    }

    #region 菜鸟电子面单相关的类

    /// <summary>
    /// 淘宝官网打印数据解释http://open.taobao.com/docs/doc.htm?spm=a219a.7629140.0.0.wIXAaM&docType=1&articleId=106054
    /// </summary>
    public class CainiaoPrintData
    {
        public CainiaoPrintDataData data;
        public string signature;
        public string templateURL;
    }

    public class CainiaoPrintDataData
    {
        public string cpCode;

        public string waybillCode;

        public CainiaoPrintDataRoutingInfo routingInfo;

        public CainiaoPrintDataDataRecipient recipient;
    }

    public class CainiaoPrintDataRoutingInfo
    {
        public CainiaoPrintDataRoutingInfoConsolidation consolidation;
        public CainiaoPrintDataRoutingInfoOrigin origin;
        public string routeCode;
        public CainiaoPrintDataRoutingInfoSortation sortation;
    }

    public class CainiaoPrintDataRoutingInfoConsolidation
    {
        public string code;
    }

    public class CainiaoPrintDataRoutingInfoOrigin
    {
        public string code;
        public string name;
    }

    public class CainiaoPrintDataRoutingInfoSortation
    {
        public string name;
    }

    public class CainiaoPrintDataDataRecipient
    {
        public string mobile;

        public string name;

        public string phone;

        public CainiaoPrintDataDataRecipientAddress address;
    }

    public class CainiaoPrintDataDataRecipientAddress
    {
        public string province;
        public string city;
        public string district;
        public string town;
        public string detail;
    }

    #endregion


}
