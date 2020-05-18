package bjc.shoperp.service.restful;

import java.util.HashMap;

import bjc.shoperp.domain.Order;
import bjc.shoperp.domain.restfulresponse.DataCollectionResponse;
import bjc.shoperp.domain.restfulresponse.domainresponse.StringCollectionResponse;
public class GoodsService extends ServiceBase<Order>  {

    public StringCollectionResponse getAllShippers() throws  Exception{
        HashMap<String, Object> para = new HashMap<>();
        return DoPost(StringCollectionResponse.class,para,null);
    }
}
