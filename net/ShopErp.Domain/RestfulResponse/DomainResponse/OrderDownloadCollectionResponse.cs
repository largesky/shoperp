using ShopErp.Domain.Pop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse.DomainResponse
{
    public class OrderDownloadCollectionResponse : DataCollectionResponse<OrderDownload>
    {
        public bool IsTotalValid { get; set; }

        public OrderDownloadCollectionResponse()
        {
        }

        /// <summary>
        /// 参数可为空
        /// </summary>
        /// <param name="t"></param>
        public OrderDownloadCollectionResponse(OrderDownload t) : base(t)
        {
        }

        public OrderDownloadCollectionResponse(IEnumerable<OrderDownload> source) : base(source)
        {
        }

        public OrderDownloadCollectionResponse(IEnumerable<OrderDownload> source, int total) : base(source, total)
        {
        }
    }
}
