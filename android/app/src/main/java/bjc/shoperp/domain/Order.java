package bjc.shoperp.domain;

import java.util.Date;
import java.util.List;

/**
 * Created by hcq on 2018/2/15.
 */

public class Order {
    public long Id;
    public String PopOrderId;
    public String ReceiverName;
    public String ReceiverPhone;
    public String ReceiverMobile;
    public String ReceiverAddress;
    public String DeliveryCompany;
    public String DeliveryNumber;
    public Date PopPayTime;

    public List<OrderGoods> OrderGoodss;
}
