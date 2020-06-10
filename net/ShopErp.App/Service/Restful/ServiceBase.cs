using System;
using System.Collections.Generic;
using ShopErp.Domain;
using System.Linq;
using Newtonsoft.Json;
using ShopErp.Domain.RestfulResponse;
using System.Text;
using ShopErp.App.Service.Net;

namespace ShopErp.App.Service.Restful
{
    public abstract class ServiceBase<E> where E : class, new()
    {
        static DateTime dbMinTime = new DateTime(1970, 01, 01, 0, 0, 0);

        static JsonSerializerSettings jsonDatetimeSetting = new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat };

        /// <summary>
        /// 这个API不能外部调用，这是内部专用，会处理和服务端相关的返回数据 ，包括引发错误等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        private static T DeserializeObject<T>(string json) where T : ResponseBase
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new Exception("服务端返回空JSON数据");
            }
            T ret = null;
            try
            {
                ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, jsonDatetimeSetting);
            }
            catch (Exception ex)
            {
                Log.Logger.Log("解析JSON数据出错,内容：" + Environment.NewLine, json + Environment.NewLine);
                throw ex;
            }
            if ("success" != ret.error)
            {
                Log.Logger.Log("服务端返回失败数据,内容：" + Environment.NewLine, json + Environment.NewLine);
                throw new Exception(ret.error);
            }
            return ret;
        }

        public static T DoPost<T>(IDictionary<string, object> para, IDictionary<string, string> headers = null) where T : ResponseBase
        {
            string apiUrl = typeof(E).Name.ToLower() + "/" + (new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name.ToLower() + ".html");
            return DoPostWithUrl<T>(apiUrl, para, headers);
        }

        public static T DoPostFile<T>(IDictionary<string, string> para, byte[] file, IDictionary<string, string> headers = null) where T : ResponseBase
        {
            string apiUrl = typeof(E).Name.ToLower() + "/" + (new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name.ToLower() + ".html");
            return DoPostFileWithUrl<T>(apiUrl, para, file, headers);
        }

        public static T DoPostWithUrl<T>(string url, IDictionary<string, object> para, IDictionary<string, string> headers = null) where T : ResponseBase
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            headers["session"] = ServiceContainer.AccessToken;
            string json = MsHttpRestful.PostJsonBodyReturnString(ServiceContainer.ServerAddress + "/" + url, para, headers);
            return DeserializeObject<T>(json);
        }

        public static T DoPostFileWithUrl<T>(string url, IDictionary<string, string> para, byte[] file, IDictionary<string, string> headers = null) where T : ResponseBase
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            headers["session"] = ServiceContainer.AccessToken;
            string param = string.Join("&", para.Select(obj => obj.Key + "=" + MsHttpRestful.UrlEncode(obj.Value, Encoding.UTF8)));
            string json = MsHttpRestful.PostBytesBodyReturnString(ServiceContainer.ServerAddress + "/" + url + "?" + param, file, headers);
            return DeserializeObject<T>(json);
        }

        public virtual E GetById(object id)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            var ret = DoPost<DataCollectionResponse<E>>(para);
            if (ret.Datas == null || ret.Datas.Count < 1)
            {
                return default(E);
            }
            return ret.Datas[0];
        }

        public virtual long Save(E e)
        {
            var para = new Dictionary<string, object>();
            para["value"] = e;
            var ret = DoPost<LongResponse>(para);
            return ret.data;
        }

        public virtual long Update(E e)
        {
            var para = new Dictionary<string, object>();
            para["value"] = e;
            var ret = DoPost<LongResponse>(para);
            return ret.data;
        }

        public virtual void Delete(long id)
        {
            var para = new Dictionary<string, object>();
            para["id"] = id;
            DoPost<ResponseBase>(para);
        }

        public virtual DateTime GetDBMinTime()
        {
            return dbMinTime;
        }

        public bool IsDBMinTime(DateTime cancelTime)
        {
            return cancelTime.Subtract(dbMinTime).TotalDays < 300;
        }

        public string FormatTime(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
}