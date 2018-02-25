using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class VendorService : ServiceBase<Vendor, VendorDao>
    {
        private static readonly char[] NUMBERS = "0123456789０１２３４５６７８９一二三四五六七八九".ToArray();
        private static readonly List<char> NUMBERS_REPLAYCE = "０１２３４５６７８９".ToList();
        private static readonly List<char> NUMBERS_REPLAYCE1 = "一二三四五六七八九".ToList();
        private static readonly string[] KEY_WORDS = new string[] { "真皮", "女鞋", "鞋业", "有限", "公司", "鞋厂", "鞋贸", "推荐", "厂家", "生产", "GO2", "省", "-", ".", "商家", "商贸", "品质", "(", ")", "工厂", "直销", "四川", "四川省", "成都", "成都市", "省", "广州", "代发", "一件", "主推" };
        private static readonly List<Vendor> vendors_catch = new List<Vendor>();

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<Vendor> GetById(long id)
        {
            try
            {
                var item = this.GetFirstOrDefaultInCach(new Predicate<Vendor>(obj => obj.Id == id));
                return new DataCollectionResponse<Vendor>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(Vendor value)
        {
            try
            {
                var v = this.GetFirstOrDefaultInCach(obj => obj.HomePage == value.HomePage);
                if (v != null)
                {
                    throw new Exception("已存在相同网址的厂家");
                }
                this.dao.Save(value);
                this.CheckAndLoadCach();
                if (this.GetFirstOrDefaultInCach(obj => obj.Id == value.Id) == null)
                    this.AndInCach(value);
                return new LongResponse(value.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/update.html")]
        public ResponseBase Update(Vendor value)
        {
            try
            {
                if (value.Id < 1)
                {
                    throw new Exception("数据未保存过，不能直接更新");
                }
                this.dao.Update(value);
                this.RemoveCach(obj => obj.Id == value.Id);
                this.AndInCach(value);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/delete.html")]
        public ResponseBase Delete(long id)
        {
            try
            {
                this.dao.ExcuteSqlUpdate("delete from Vendor where Id=" + id);
                this.RemoveCach(new Predicate<Vendor>(obj => obj.Id == id));
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<Vendor> GetByAll(string name, string pingYingName, string homePage, string marketAddress, int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetByAll(name, pingYingName, homePage, marketAddress, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getallvendoridhasgoods.html")]
        public DataCollectionResponse<long> GetAllVendorIdHasGoods()
        {
            try
            {
                return new DataCollectionResponse<long>(this.dao.GetColumnValueBySqlQuery<long>("select distinct VendorId from goods"));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/updatecountandavgpriceall.html")]
        public ResponseBase UpdateCountAndAvgPriceAll()
        {
            try
            {
                string sql = "update vendor set count=0,AveragePrice=0 where id>0;";
                this.dao.ExcuteSqlUpdate(sql);

                sql = "update Vendor  set count=(select count(distinct Id) from Goods where VendorId=Vendor.Id),AveragePrice=(select Sum(Price)/ count(distinct Id) from Goods where VendorId=Vendor.Id) where (select count(distinct Id) from Goods where VendorId=Vendor.Id)>0 ";
                this.dao.ExcuteSqlUpdate(sql);

                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 在缓存中搜索厂家名称
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getvendorname.html")]
        public StringResponse GetVendorName(long vendorId)
        {
            try
            {
                var vendor = this.GetFirstOrDefaultInCach(obj => obj.Id == vendorId);
                string ret = vendor == null ? "" : vendor.Name ?? "";
                return new StringResponse(ret);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 在缓存中搜索厂家拼音名称
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getvendorpingyinfirstchar.html")]
        public StringResponse GetVendorPingYingFirstChar(int vendorId)
        {
            try
            {
                char c = ' ';
                var vendor = this.GetFirstOrDefaultInCach(obj => obj.Id == vendorId);
                if (vendor != null)
                {
                    c = vendor.PingyingName.Length < 1 ? ' ' : vendor.PingyingName[0];
                }
                return new StringResponse(c + "");
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }

        }

        /// <summary>
        /// 在缓存中搜索厂家拼音名称
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getvendorpingyinname.html")]
        public StringResponse GetVendorPingyingName(string vendorName)
        {
            try
            {
                var vendor = this.GetFirstOrDefaultInCach(obj => obj.Name == vendorName);
                string ret = vendor == null ? "" : vendor.PingyingName ?? "";
                return new StringResponse(ret);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 在缓存中搜索厂家商场地址
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        public StringResponse GetVendorAddress_InCach(string vendorName)
        {
            try
            {
                var vendor = this.GetFirstOrDefaultInCach(obj => obj.Name == vendorName);
                string ret = vendor == null ? "" : vendor.MarketAddress ?? "";
                return new StringResponse(ret);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        public static bool Match(Vendor v1, Vendor v2)
        {
            if (v1 == null && v2 == null)
            {
                return true;
            }

            if (v1 != null && v2 == null)
            {
                return false;
            }

            if (v1 == null && v2 != null)
            {
                return false;
            }

            if (v1 == v2)
            {
                return true;
            }

            return string.Compare(v1.HomePage, v2.HomePage) == 0 &&
                string.Compare(v1.Name, v2.Name) == 0 &&
                string.Compare(v1.Phone, v2.Phone) == 0 &&
                string.Compare(v1.MarketAddress, v2.MarketAddress) == 0 &&
                string.Compare(v1.PingyingName, v2.PingyingName) == 0;
        }

        /// <summary>
        /// 格式货厂家名称，删除不需要关键词，保留长度为6个字
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        public static string FormatVendorName(string vendorName)
        {
            StringBuilder sb = new StringBuilder(vendorName);
            foreach (var str in KEY_WORDS)
            {
                sb.Replace(str, "");
            }
            if (sb.Length > 6)
            {
                sb.Length = 6;
            }
            return sb.ToString();
        }

        /// <summary>
        /// 格式化厂家地址 成 3-22412-2 的形式
        /// </summary>
        /// <param name="marketAddress"></param>
        /// <returns></returns>
        public static string FormatVendorDoor(string marketAddress)
        {
            string area = "", street = "", door = "";

            area = FindAreaOrStreet(marketAddress, "区");
            street = FindAreaOrStreet(marketAddress, "街");
            door = FindDoor(marketAddress);

            return string.Format("{0}-{1}-{2}", area, door, street);
        }

        /// <summary>
        /// 在址中搜索街或者区
        /// </summary>
        /// <param name="address"></param>
        /// <param name="type">街 区</param>
        /// <returns></returns>
        public static string FindAreaOrStreet(string address, string type)
        {
            int areaIndex = address.IndexOf(type);
            string area = "";
            while (areaIndex - 1 >= 0 && NUMBERS.Any(c => c == address[areaIndex - 1]) == false)
            {
                areaIndex--;
            }
            if (areaIndex >= 0)
            {
                int endAreaIndex = areaIndex;
                while (areaIndex - 1 >= 0 && NUMBERS.Any(c => c == address[areaIndex - 1]))
                {
                    areaIndex--;
                }
                if (areaIndex < 0)
                {
                    areaIndex = 0;
                }
                area = address.Substring(areaIndex, endAreaIndex - areaIndex);
            }
            area = Format(area);
            return area;
        }

        /// <summary>
        /// 搜索门牌号 如 22322
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string FindDoor(string address)
        {
            string doorNumber = "";
            for (int i = 0; i < address.Length; i++)
            {
                if (NUMBERS.Any(c => c == address[i]) == false)
                {
                    continue;
                }
                int j = 1;
                for (; j < 5 && (j + i) < address.Length; j++)
                {
                    if (NUMBERS.Any(c => c == address[i + j]) == false)
                    {
                        break;
                    }
                }
                if (j == 5)
                {
                    doorNumber = address.Substring(i, 5);
                    break;
                }
            }
            doorNumber = Format(doorNumber);
            return doorNumber;
        }

        /// <summary>
        /// 将全角字符数据转成半角字符
        /// </summary>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        private static string Format(string oldValue)
        {
            char[] tmps = oldValue.ToArray();
            for (int i = 0; i < tmps.Length; i++)
            {
                int index = NUMBERS_REPLAYCE.IndexOf(tmps[i]);
                if (index >= 0)
                {
                    tmps[i] = NUMBERS[i];
                    continue;
                }

                index = NUMBERS_REPLAYCE1.IndexOf(tmps[i]);
                if (index >= 0)
                {
                    tmps[i] = NUMBERS[i + 1];
                }
            }
            return new string(tmps);
        }
    }
}
