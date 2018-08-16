using System;

namespace ShopErp.Domain
{
    public class WuliuNumber
    {
        public long Id { get; set; }

        public string WuliuIds { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverPhone { get; set; }

        public string ReceiverMobile { get; set; }

        public string ReceiverAddress { get; set; }

        public string PackageId { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 快递公司
        /// </summary>
        public string DeliveryCompany { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string DeliveryNumber { get; set; }

        /// <summary>
        /// 大头笔
        /// </summary>
        public string SortationName { get; set; }

        /// <summary>
        /// 三段码
        /// </summary>
        public string RouteCode { get; set; }

        /// <summary>
        /// 集包地数字编号
        /// </summary>
        public string ConsolidationCode { get; set; }

        /// <summary>
        /// 发货地数字编号
        /// </summary>
        public string OriginCode { get; set; }

        /// <summary>
        /// 发货地网点名称
        /// </summary>
        public string OriginName { get; set; }

        /// <summary>
        /// 菜鸟返回的打印JSON数据
        /// </summary>
        public string PrintData { get; set; }

        /// <summary>
        /// 大头笔和 三段码，根据一定规则拼接 http://open.taobao.com/docs/doc.htm?spm=a219a.7629140.0.0.wIXAaM&docType=1&articleId=106054
        /// </summary>
        public string SortationNameAndRouteCode { get; set; }

    }
}
