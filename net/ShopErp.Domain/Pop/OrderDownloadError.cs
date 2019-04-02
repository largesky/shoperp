using System;
using System.Text;

namespace ShopErp.Domain.Pop
{
    public class OrderDownloadError
    {
        public long ShopId { get; set; }

        public string PopOrderId { get; set; }

        public string ReceiverName { get; set; }

        public string Error { get; set; }

        public string StackTrace { get; set; }

        public OrderDownloadError() { }

        public OrderDownloadError(long shopId, string popOrderId, string receiverName, string error, string stackTrace)
        {
            this.ShopId = shopId;
            this.PopOrderId = popOrderId;
            this.ReceiverName = receiverName;
            this.Error = error;
            this.StackTrace = stackTrace;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(" ", this.PopOrderId, this.ReceiverName));
            sb.AppendLine(Error);
            return sb.ToString();
        }
    }
}
