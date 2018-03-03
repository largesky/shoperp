package bjc.shoperp.service.restful;

import java.util.Date;
import java.util.HashMap;

import bjc.shoperp.domain.DeliveryOut;
import bjc.shoperp.domain.restfulresponse.domainresponse.DeliveryOutCollectionResponse;

/**
 * Created by hth on 2018/2/25.
 */

public class DeliveryOutService extends ServiceBase<DeliveryOut> {

    public DeliveryOutCollectionResponse GetByAll(long shopId, String deliveryCompany, String deliveryNumber, String vendor, String number, Date startTime, Date endTime, int pageIndex, int pageSize) throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        para.put("payType",0);
        para.put("shopId",shopId);
        para.put("deliveryCompany",deliveryCompany);
        para.put("deliveryNumber",deliveryNumber);
        para.put("vendor",vendor);
        para.put("number",number);
        para.put("startTime",startTime);
        para.put("endTime",endTime);
        para.put("pageIndex",pageIndex);
        para.put("pageSize",pageSize);
        return DoPost(DeliveryOutCollectionResponse.class,para,null);
    }
}
