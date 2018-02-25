package bjc.shoperp.service.restful;

import java.util.HashMap;

import bjc.shoperp.domain.Order;
import bjc.shoperp.domain.Shop;
import bjc.shoperp.domain.restfulresponse.domainresponse.OrderCollectionResponse;
import bjc.shoperp.domain.restfulresponse.domainresponse.OrderDownloadCollectionResponse;

/**
 * Created by hcq on 2018/2/15.
 */

public class OrderService extends ServiceBase<Order> {
    public OrderDownloadCollectionResponse getPopWaitSendOrders(Shop shop, int payType, int pageIndex, int pageSize) throws Exception {
        HashMap<String, Object> para = new HashMap<String, Object>();
        para.put("shop", shop);
        para.put("payType", payType);
        para.put("pageIndex", pageIndex);
        para.put("pageSize", pageSize);
        return DoPost(OrderDownloadCollectionResponse.class, para, null);
    }

    public OrderCollectionResponse markDelivery(String deliveryNumber, float weight, boolean ingorePopError, boolean ingoreWeightDetect, boolean ingoreStateCheck) throws Exception {
        HashMap<String, Object> para = new HashMap<String, Object>();
        para.put("deliveryNumber", deliveryNumber);
        para.put("weight", weight);
        para.put("ingorePopError", ingorePopError);
        para.put("ingoreWeightDetect", ingoreWeightDetect);
        para.put("ingoreStateCheck", ingoreStateCheck);
        return DoPost(OrderCollectionResponse.class, para, null);
    }
}
