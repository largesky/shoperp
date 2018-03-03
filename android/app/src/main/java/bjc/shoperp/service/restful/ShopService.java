package bjc.shoperp.service.restful;

import java.util.HashMap;
import java.util.List;

import bjc.shoperp.domain.Shop;
import bjc.shoperp.domain.restfulresponse.domainresponse.ShopCollectionResponse;

/**
 * Created by hcq on 2018/2/20.
 */

public class ShopService extends ServiceBase<Shop> {

    public List<Shop> getByAll() throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        return DoPost(ShopCollectionResponse.class, para, null).Datas;
    }
}
