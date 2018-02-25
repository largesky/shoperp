using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace ShopErp.App.Service.Net
{
    public class MessageCenter
    {
        /// <summary>
        /// 唯一实例，类加载器保证生成唯一的
        /// </summary>
        private static readonly MessageCenter instance = new MessageCenter();

        public static MessageCenter Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// 通信的通信的udp商品
        /// </summary>
        private Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        /// <summary>
        /// 获取远程IP，商品的参数
        /// </summary>
        private EndPoint remoteIP = new IPEndPoint(IPAddress.Any, 3666);

        /// <summary>
        /// 接收数据的本地商品
        /// </summary>
        private EndPoint listenSocket = new IPEndPoint(IPAddress.Any, 3668);

        /// <summary>
        /// 需要组播的网段，一台电脑可能有多块网卡，
        /// </summary>
        private EndPoint[] broadcastEndPoint = null;

        private byte[] reciveBuf = new byte[1024 * 1024];

        private byte[] sendBuf = new byte[1024 * 1024];

        private BinaryFormatter binaryFormatter = new BinaryFormatter();

        public event EventHandler<MessageArrviedEventArgs> MessageArrived;

        private bool hasStart = false;

        private object has_start_lock = new object();


        /// <summary>
        /// udp收到数据的回调
        /// </summary>
        /// <param name="ar"></param>
        private void DataReciveCallback(IAsyncResult ar)
        {
            try
            {
                int ret = this.udpSocket.EndReceiveFrom(ar, ref this.remoteIP);
                if (ret <= 0)
                {
                    return;
                }
                MemoryStream ms = new MemoryStream(this.reciveBuf, 0, ret);
                try
                {
                    Message m = this.binaryFormatter.Deserialize(ms) as Message;
                    if (this.MessageArrived != null && m.SenderId != GetHostName())
                    {
                        this.MessageArrived(this, new MessageArrviedEventArgs(m));
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss : ") + "网络事件处理出错:" + e.Message + Environment.NewLine + e.StackTrace);
                }
                //开始新的接收
                this.udpSocket.BeginReceiveFrom(this.reciveBuf, 0, this.reciveBuf.Length, SocketFlags.None, ref this.remoteIP, this.DataReciveCallback, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.udpSocket.Close();
                this.hasStart = false;
            }
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void Start()
        {
            lock (has_start_lock)
            {
                if (hasStart)
                {
                    return;
                }
                //绑定通信商品
                this.udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                this.udpSocket.Bind(listenSocket);
                //获取本地所有网络接口，并生成组播地址
                this.broadcastEndPoint = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(ipa => new IPEndPoint(IPAddress.Parse(ipa.ToString().Substring(0, ipa.ToString().LastIndexOf('.')) + ".255"), 3668)).ToArray();
                this.udpSocket.BeginReceiveFrom(this.reciveBuf, 0, this.reciveBuf.Length, SocketFlags.None, ref this.remoteIP, this.DataReciveCallback, null);
                hasStart = true;
            }
        }

        /// <summary>
        /// 停止接收数据
        /// </summary>
        public void Stop()
        {
            if (this.udpSocket != null)
            {
                this.udpSocket.Close();
                this.udpSocket = null;
            }
            this.hasStart = false;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(Message message)
        {
            message.SenderId = GetHostName();
            //序列化数据
            MemoryStream ms = new MemoryStream(this.sendBuf);
            this.binaryFormatter.Serialize(ms, message);
            var buf = ms.ToArray();

            //向计算中的每一个网络接口广播数据
            foreach (EndPoint ep in this.broadcastEndPoint)
            {
                this.udpSocket.SendTo(buf, (int)ms.Position, SocketFlags.None, ep);
            }
        }

        public static string GetHostName()
        {
            return Dns.GetHostName();
        }
    }
}