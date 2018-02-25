using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Service.Restful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using ShopErp.Domain;
using ShopErp.Server.Dao.NHibernateDao;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.Server.Service.Restful
{
    public class ServiceContainer : ServiceAuthorizationManager
    {
        public static readonly List<WebServiceHost> service = new List<WebServiceHost>();

        private char[] spchar = "&".ToCharArray();

        /// <summary>
        /// 启动RESTFUL服务
        /// </summary>
        public void Start()
        {
            string def = Debugger.IsAttached ? "http://localhost/shoperpdebug/" : "http://localhost/shoperp/";
            string hostUrl = LocalConfigService.GetValue(SystemNames.CONFIG_SERVER_ADDRESS, def);
            if (hostUrl == def)
            {
                LocalConfigService.UpdateValue(SystemNames.CONFIG_SERVER_ADDRESS, hostUrl);
            }
            var binding = new WebHttpBinding
            {
                Name = "long_web_http_binding",
                AllowCookies = true,
                SendTimeout = new TimeSpan(0, 5, 0),
                CloseTimeout = new TimeSpan(0, 1, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                ReceiveTimeout = new TimeSpan(0, 5, 0),
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas { MaxDepth = 16, MaxArrayLength = 10485760, MaxBytesPerRead = 10485760, MaxStringContentLength = 10485760, MaxNameTableCharCount = 100 },
                MaxBufferPoolSize = 10485760,
                MaxBufferSize = 10485760,
                MaxReceivedMessageSize = 10485760,
                Security = new WebHttpSecurity { Mode = WebHttpSecurityMode.None },
            };

            Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(obj => obj.Namespace == "ShopErp.Server.Service.Restful").OrderBy(obj => obj.Name).ToArray();
            foreach (var v in types)
            {
                if (v.GetCustomAttribute<ServiceContractAttribute>() == null)
                {
                    continue;
                }

                var obj = Activator.CreateInstance(v);
                var s = new WebServiceHost(obj, new Uri(hostUrl));
                ContractDescription cd = ContractDescription.GetContract(v);
                ServiceEndpoint se = new ServiceEndpoint(cd, binding, new EndpointAddress(new Uri(hostUrl + v.Name.ToLower().Replace("restful", "").Replace("service", ""), UriKind.Absolute)));
                s.AddServiceEndpoint(se);
                s.Authorization.ServiceAuthorizationManager = this;
                service.Add(s);
                s.Open();
                foreach (var vv in s.Description.Endpoints)
                {
                    Console.WriteLine(vv.Address);
                }
            }
        }

        /// <summary>
        /// 停止RESTFUL服务
        /// </summary>
        public void Stop()
        {
            foreach (var s in service)
            {
                if (s.State == CommunicationState.Opened)
                    s.Close();
            }
        }

        public override bool CheckAccess(OperationContext operationContext)
        {
            try
            {
                var url = operationContext.RequestContext.RequestMessage.Headers.To;

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 收到请求：" + url);

                if (url.AbsolutePath.EndsWith("/login.html") || url.AbsolutePath.Contains("image/getimage.html"))
                {
                    return true;
                }

                if (url.AbsolutePath.EndsWith("pddoauth.html") || url.AbsolutePath.EndsWith("taobaooauth.html") || url.AbsolutePath.EndsWith("ccjoauth.html"))
                {
                    return true;
                }


                if (operationContext.RequestContext.RequestMessage.Properties.ContainsKey("httpRequest") == false)
                {
                    throw new Exception("ServiceContainer内部程序错误：请求不包含 httpRequest数据");
                }

                var requestMessage = operationContext.RequestContext.RequestMessage.Properties["httpRequest"] as HttpRequestMessageProperty;
                if (requestMessage == null)
                {
                    throw new Exception("ServiceContainer内部程序错误：httpRequest的数据类型不为HttpRequestMessageProperty");
                }

                var session = requestMessage.Headers.AllKeys.FirstOrDefault(obj => obj == "session");
                if (session == null)
                {
                    throw new Exception("参数错误：Header 中缺少 Session字段");
                }
                string ss = requestMessage.Headers.Get(session);
                if (string.IsNullOrWhiteSpace(ss))
                {
                    throw new Exception("参数错误：Header Session字段没有值");
                }

                lock (OperatorService.operators)
                {
                    var op = OperatorService.operators.FirstOrDefault(obj => obj.session == ss);
                    if (op != null)
                    {
                        op.lastOperateTime = DateTime.Now;
                        return true;
                    }
                }
                throw new Exception("用户未登录，请关闭程序重新登录");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = msg }, System.Net.HttpStatusCode.OK);
            }
        }

        public static LoginResponse GetCurrentLoginInfo()
        {
            try
            {
                if (OperationContext.Current == null || OperationContext.Current.RequestContext == null)
                {
                    throw new Exception("你在非WEB请求环境中调用该方法，不能获取请求的用户信息");
                }
                var operationContext = OperationContext.Current;
                var url = operationContext.RequestContext.RequestMessage.Headers.To;
                if (operationContext.RequestContext.RequestMessage.Properties.ContainsKey("httpRequest") == false)
                {
                    throw new Exception("RestfulServiceAuthorizationManager内部程序错误：请求不包含 httpRequest数据");
                }

                var requestMessage = operationContext.RequestContext.RequestMessage.Properties["httpRequest"] as HttpRequestMessageProperty;
                if (requestMessage == null)
                {
                    throw new Exception("RestfulServiceAuthorizationManager内部程序错误：httpRequest的数据类型不为HttpRequestMessageProperty");
                }

                var session = requestMessage.Headers.AllKeys.FirstOrDefault(obj => obj == "session");
                if (session == null)
                {
                    throw new Exception("参数错误：Header 中缺少 Session字段");
                }
                string ss = requestMessage.Headers.Get(session);
                if (string.IsNullOrWhiteSpace(ss))
                {
                    throw new Exception("参数错误：Header Session字段没有值");
                }

                lock (OperatorService.operators)
                {
                    //是否是操作员
                    var op = OperatorService.operators.FirstOrDefault(obj => obj.session == ss);
                    if (op != null)
                    {
                        return op;
                    }
                }

                throw new Exception("用户未登录");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw new WebFaultException<ResponseBase>(new ResponseBase { error = msg }, System.Net.HttpStatusCode.OK);
            }
        }

        public static long GetSellerId(long sellerId)
        {
            if (sellerId == -1)
                return -1;
            var loginInfo = GetCurrentLoginInfo();
            return loginInfo.op.Id;
        }

        public static T GetService<T>() where T : class
        {
            foreach (var v in service)
            {
                if (v.SingletonInstance == null)
                {
                    throw new InvalidProgramException("WEBSERVICEHOST 内没有对象");
                }
                if (v.SingletonInstance.GetType() == typeof(T))
                {
                    return v.SingletonInstance as T;
                }
            }
            throw new Exception("没找到指定的类型：" + typeof(T).FullName);
        }

        public static void DumpInfo()
        {
            foreach (var s in service)
            {
                Console.WriteLine(s.SingletonInstance.GetType().FullName + " State: " + s.State);

            }
        }
    }
}
