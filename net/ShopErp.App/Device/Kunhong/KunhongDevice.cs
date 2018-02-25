using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using ShopErp.App.Service;

namespace ShopErp.App.Device.Kunhong
{
    public class KunhongDevice : IDevice
    {
        const int MAX_COUNT = 10;
        public const string SERIAL_PORT = "CONFIG_WEIGHTDEVICE_KUNHONG_SERIALPORT";
        private SerialPort port;
        private string serialPort = null;
        byte[] cmds = new byte[] {(byte) 'R', (byte) 'G', (byte) '1', (byte) '5', 13, 10};
        byte[] buf = new byte[512];

        public IDeviceConfigUI CreateNew()
        {
            return new KunhongConfigUI();
        }

        public string Name
        {
            get { return "苏州坤宏"; }
        }

        private void Open()
        {
            if (string.IsNullOrWhiteSpace(serialPort))
            {
                serialPort = LocalConfigService.GetValue(SERIAL_PORT, "");
                if (string.IsNullOrWhiteSpace(serialPort))
                {
                    throw new Exception("称重设备没有配置串口号");
                }
            }
            this.port = new SerialPort(serialPort, 9600, Parity.None, 8, StopBits.One);
            this.port.ReadTimeout = 2000;
            this.port.WriteTimeout = 2000;
            this.port.Encoding = Encoding.ASCII;
            this.port.Open();
        }

        public void Close()
        {
            if (this.port != null)
            {
                this.port.Close();
            }
        }

        private int ReadData()
        {
            //第一次读取
            this.port.ReadTimeout = 3000;
            int read = this.port.Read(this.buf, 0, this.buf.Length);
            if (read == 0)
            {
                return read;
            }
            int readTimeout = this.port.ReadTimeout;
            try
            {
                this.port.ReadTimeout = 300;

                while ((read += this.port.Read(this.buf, read, this.buf.Length - read)) > 0) ;
            }
            catch (TimeoutException)
            {
                //
            }
            finally
            {
                this.port.ReadTimeout = readTimeout;
            }
            return read;
        }

        private double Read()
        {
            if (this.port == null)
            {
                throw new Exception("设备尚未打开，无法读取重量");
            }

            for (int i = 0; i < MAX_COUNT; i++)
            {
                int read = 0;
                try
                {
                    this.port.DiscardInBuffer();
                    this.port.DiscardOutBuffer();
                    this.port.Write(cmds, 0, cmds.Length);
                    read = this.ReadData();
                    if (read == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                }
                catch (TimeoutException te)
                {
                    Debug.WriteLine(te.Message);
                    throw new Exception("与称重设备通信超时");
                }
                try
                {
                    //计算校验
                    byte xor = 0;
                    for (int j = 0; j < read - 4; j++)
                    {
                        xor = (byte) (xor ^ buf[j]);
                    }
                    int ec1 = ((xor & 0xF0) >> 4) <= 9 ? ((xor & 0xF0) >> 4) + '0' : (((xor & 0xF0) >> 4) + 'A' - 10);
                    int ec2 = (xor & 0x0F) <= 9 ? (xor & 0x0F) + '0' : ((xor & 0x0F) + 'A' - 10);

                    if (ec1 != buf[read - 4] || ec2 != buf[read - 3])
                    {
                        throw new Exception("设备校验出错，请设备是否开起校验功能");
                    }
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new Exception("设备检验出错，数据格式不合法", ie);
                }

                string ret = Encoding.ASCII.GetString(buf, 0, read - 4);
                string[] content = ret.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int startIndex = 0;
                if (content[0].StartsWith("@"))
                {
                    startIndex = 1;
                }

                if (content[startIndex].Equals("ST"))
                {
                    double value = double.Parse(content[startIndex + 2]);
                    if (content[startIndex + 3].Trim().Equals("g", StringComparison.OrdinalIgnoreCase))
                    {
                        value = value / 1000;
                    }
                    else if (content[startIndex + 3].Trim().Equals("kg", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else
                    {
                        throw new Exception("返回的数据单位未知:" + ret);
                    }
                    return value;
                }
                else if (content[startIndex].Equals("US"))
                {
                    Debug.WriteLine("称重设备读数不稳定:" + i);
                    Thread.Sleep(1000);
                    continue;
                }
                else if (content[startIndex].Equals("OV"))
                {
                    throw new Exception("商品超重,计算不准确");
                }
                else
                {
                    throw new Exception(this.GetType().FullName + " 返回的数据格式不在指定范围内:" + ret);
                }
            }

            throw new Exception("称重设备读数不稳定,已重试:" + MAX_COUNT);
        }

        public double ReadWeight()
        {
            try
            {
                this.Open();
                return this.Read();
            }
            finally
            {
                if (this.port != null)
                {
                    this.Close();
                }
            }
        }
    }
}