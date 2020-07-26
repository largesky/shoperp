namespace ShopErp.Server.Service.Pop.Pdd
{
    public class PddRspOrderList : PddRspBase
    {
        public int total_count;
        public bool has_next;
        public PddRspOrderListOrder[] order_list;

    }
}
