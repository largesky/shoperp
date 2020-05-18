package bjc.shoperp.service.restful;

import java.util.Date;
import java.util.HashMap;

import bjc.shoperp.domain.Order;
import bjc.shoperp.domain.Shop;
import bjc.shoperp.domain.restfulresponse.DataCollectionResponse;
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

    public OrderCollectionResponse markDelivery(String deliveryNumber, int goodsCount, boolean chkPopState, boolean chkLocalState) throws Exception {
        HashMap<String, Object> para = new HashMap<String, Object>();
        para.put("deliveryNumber", deliveryNumber);
        para.put("goodsCount", goodsCount);
        para.put("chkPopState", chkPopState);
        para.put("chkLocalState", chkLocalState);
        return DoPost(OrderCollectionResponse.class, para, null);
    }

    public OrderCollectionResponse getByAll(String phone) throws Exception {
        HashMap<String, Object> para = new HashMap<String, Object>();
        para.put("popBuyerId","");
        para.put("receiverMobile",phone);
        para.put("receiverName","");
        para.put("receiverAddress","");

        para.put("startTime",new Date( System.currentTimeMillis()-1000*60*60*24*90L ));
        para.put("endTime",new Date(0L));
        para.put("deliveryCompany","");
        para.put("deliveryNumber","");
        para.put("state",0);
        para.put("payType",0);

        para.put("vendorName","");
        para.put("number","");
        para.put("size","");
        para.put("ofs",null);
        para.put("parseResult",-1);
        para.put("comment","");
        para.put("shopId",0);

        para.put("createType",0);
        para.put("type",0);
        para.put("shipper","");
        para.put("pageIndex",0);
        para.put("pageSize",0);
        return DoPost(OrderCollectionResponse.class, para, null);
    }
}
