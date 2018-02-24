package bjc.shoperp.service.restful;

import java.util.HashMap;

import bjc.shoperp.domain.SystemConfig;
import bjc.shoperp.domain.restfulresponse.ResponseBase;
import bjc.shoperp.domain.restfulresponse.StringResponse;

/**
 * Created by hcq on 2018/2/19.
 */

public class SystemConfigService extends ServiceBase<SystemConfigService> {

    public String get(long ownerId,String name,String defaultValue) throws Exception{
        HashMap<String,Object> para=new HashMap<>(  );
        para.put( "ownerId",ownerId );
        para.put( "name",name );
        para.put( "defaultValue",defaultValue );
       StringResponse resp= DoPost(StringResponse.class,para,null);
       return resp.data;
    }

    public void saveOrUpdate(long ownerId,String name,String value) throws Exception{
        HashMap<String,Object> para=new HashMap<>(  );
        para.put( "ownerId",ownerId );
        para.put( "name",name );
        para.put( "value",value );
        ResponseBase resp= DoPost(ResponseBase.class,para,null);
    }
}
