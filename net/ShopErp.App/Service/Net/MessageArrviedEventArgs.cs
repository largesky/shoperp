using System;

namespace ShopErp.App.Service.Net
{
    public class MessageArrviedEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        public MessageArrviedEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}