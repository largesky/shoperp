package bjc.shoperp.domain;

import java.util.Date;

/**
 * Created by hcq on 2018/2/20.
 */

public class Shop {
    public long Id ;

    public int PopType ;

    public String PopSellerId ;

    public String PopSellerNumberId ;

    public String AppKey ;

    public String AppSecret ;

    public String AppAccessToken ;

    public String AppRefreshToken;

    public String AppCallbackUrl ;

    public String Mark;

    public float CommissionPer ;

    public Date CreateTime ;

    public Date UpdateTime ;

    public String LastUpdateOperator ;

    public boolean Enabled ;

    public int ShippingHours ;

    public int FirstDeliveryHours ;

    public int SecondDeliveryHours ;

    public boolean AppEnabled ;
}
