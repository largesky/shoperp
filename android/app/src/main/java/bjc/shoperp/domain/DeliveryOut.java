package bjc.shoperp.domain;

import java.util.Date;

/**
 * Created by hth on 2018/2/25.
 */

public class DeliveryOut {
    public long Id;
    public long ShopId;
    public String OrderId ;
    public String DeliveryCompany ;
    public String DeliveryNumber ;
    public String ReceiverAddress ;
    public float Weight ;
    public float ERPDeliveryMoney ;
    public float ERPGoodsMoney ;
    public float PopDeliveryMoney ;
    public float PopCodSevFee ;
    public float PopGoodsMoney;
    public String GoodsInfo ;
    public String Operator ;
    public Date CreateTime ;

    public String CreateTimeStr;
}
