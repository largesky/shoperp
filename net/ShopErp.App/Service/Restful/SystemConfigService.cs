using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;

namespace ShopErp.App.Service.Restful
{
    public class SystemConfigService : ServiceBase<SystemConfig>
    {
        public string Get(long ownerId, string name, string defaultValue)
        {
            System.Collections.Generic.Dictionary<string, object> para = new System.Collections.Generic.Dictionary<string, object>();
            para["ownerId"] = ownerId;
            para["name"] = name;
            para["defaultValue"] = defaultValue;

            return DoPost<StringResponse>(para).data;
        }

        public long SaveOrUpdate(long ownerId, string name, string value)
        {
            System.Collections.Generic.Dictionary<string, object> para = new System.Collections.Generic.Dictionary<string, object>();
            para["ownerId"] = ownerId;
            para["name"] = name;
            para["value"] = value;
            return DoPost<LongResponse>(para).data;
        }
    
    }
}