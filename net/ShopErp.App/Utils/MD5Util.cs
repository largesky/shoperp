using System.Security.Cryptography;
using System.Text;

namespace ShopErp.App.Utils
{
    public class Md5Util
    {
        public static string Md5(string md5)
        {
            StringBuilder sb = new StringBuilder(100);
            byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(md5));
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            string s = sb.ToString();
            sb.Clear();
            return s;
        }

        public static byte[] Md5Bytes(string content)
        {
            MD5 md5Hash = MD5.Create();
            byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(content));
            return bytes;
        }
    }
}