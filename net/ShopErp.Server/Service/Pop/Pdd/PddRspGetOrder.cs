namespace ShopErp.Server.Service.Pop.Pdd
{
    public class PddRspGetOrder : PddRspBase
    {
        public string order_sn;
        public string confirm_time;
        public string created_time;
        public string country;
        public string province;
        public string city;
        public string town;
        public string address;
        public string receiver_name;
        public string receiver_phone;
        public string pay_amount;
        public string goods_amount;
        public string discount_amount;
        public string postage;
        public string logistics_id;
        public string tracking_number;
        public string shipping_time;
        public string remark;
        public string is_lucky;
        public string order_status;
        public string last_ship_time;
        public string refund_status;
        public string seller_discount;
        public string capital_free_discount;
        public string platform_discount;

        public PddRspGetOrderGoods[] item_list;
    }
}
