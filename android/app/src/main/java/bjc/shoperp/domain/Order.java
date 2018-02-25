package bjc.shoperp.domain;

import java.util.List;

/**
 * Created by hcq on 2018/2/15.
 */

public class Order {
    public long Id;
    public String PopOrderId;
    public String RecevierName;
    public String RecevierPhone;
    public String RecevierMobile;
    public String RecevierAddress;
    public String DeliveryCompany;
    public String DeliveryNumber;

    public List<OrderGoods> OrderGoodss;
}
