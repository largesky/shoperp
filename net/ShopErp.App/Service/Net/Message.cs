using System;

namespace ShopErp.App.Service.Net
{
    [Serializable]
    public class Message
    {
        /// <summary>
        /// 发送者标识，用来过滤接收到的自身的消息，该值通常为计算名称
        /// </summary>
        public string SenderId { get; set; }

        public string SenderName { get; set; }

        public DateTime Time { get; set; }

        public string[] Targets { get; set; }
    }
}