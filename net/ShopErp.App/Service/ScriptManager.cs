using System;

namespace ShopErp.App.Service
{
    public class ScriptManager
    {
        public static string GetBody(string file, string title)
        {
            string conent = System.IO.File.ReadAllText(file);
            int si = conent.IndexOf(title);
            int ei = conent.IndexOf(title, si + title.Length);

            if (ei <= si)
            {
                throw new Exception("未找到相应开始结束匹配符：" + title);
            }

            string con = conent.Substring(si + title.Length, ei - si - title.Length);
            return con;
        }
    }
}