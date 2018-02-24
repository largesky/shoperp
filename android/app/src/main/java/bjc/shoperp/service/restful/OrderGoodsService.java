package bjc.shoperp.service.restful;

import java.util.Date;
import java.util.HashMap;

import bjc.shoperp.domain.ColorFlag;
import bjc.shoperp.domain.OrderGoods;
import bjc.shoperp.domain.restfulresponse.domainresponse.GoodsCountCollectionResponse;

/**
 * Created by hcq on 2018/2/19.
 */

public class OrderGoodsService extends ServiceBase<OrderGoods> {

    public GoodsCountCollectionResponse GetGoodsCount(int[] flags, Date startTime, Date endTime, int pageIndex, int pageSize) throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        para.put( "flags", flags );
        para.put( "startTime", startTime );
        para.put( "endTime", endTime );
        para.put( "pageIndex", pageIndex );
        para.put( "pageSize", pageSize );
        return DoPost( GoodsCountCollectionResponse.class, para, null );
    }
}
