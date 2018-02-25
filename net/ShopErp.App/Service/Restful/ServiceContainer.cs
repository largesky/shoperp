using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace ShopErp.App.Service.Restful
{
    public class ServiceContainer
    {
        public static string ServerAddress = null;

        public static string AccessToken { get; set; }

        static List<object> services = new List<object>();

        static ServiceContainer()
        {
            Type[] types = typeof(ServiceContainer).Assembly.GetTypes();

            foreach (Type t in types)
            {
                if (t.BaseType != null && t.Namespace == "ShopErp.App.Service.Restful" && t.Name.EndsWith("Service"))
                {
                    services.Add(Activator.CreateInstance(t));
                }
            }
        }

        public static T GetService<T>()
        {
            object obj = services.FirstOrDefault(o => o.GetType() == typeof(T) || o.GetType().GetInterface(typeof(T).Name, true) != null);
            if (obj == null)
            {
                throw new Exception("未找到指定的服务类型:" + typeof(T).FullName);
            }
            return (T)obj;
        }
    }
}