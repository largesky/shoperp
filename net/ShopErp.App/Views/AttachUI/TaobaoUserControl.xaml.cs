using CefSharp;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Domain.TaobaoHtml.Image;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopErp.App.Views.AttachUI
{
    /// <summary>
    /// TaobaoUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoUserControl : UserControl
    {
        string jsgetua = "window.uabModule && window.uabModule.getUA({Token: window.UA_TOKEN})";

        public TaobaoUserControl()
        {
            InitializeComponent();
        }

        private void cbbUrls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count < 1)
                {
                    return;
                }
                string text = e.AddedItems[0].ToString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }
                int index = text.IndexOf("https://");
                if (index < 0)
                {
                    throw new Exception("选择内容中不包含https://，无法分析网址");
                }
                string url = text.Substring(index).Trim();
                this.wb1.Load(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var str = EvaluateScript("window.uabModule && window.uabModule.getUA({Token: window.UA_TOKEN})");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGetUserNumberId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string uid = GetUid();
                if (string.IsNullOrWhiteSpace(uid))
                {
                    throw new Exception("网页中未找到 userid= 请检查是否登录");
                }
                MessageBox.Show(uid, "用户数字编号");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.wb1.IsBrowserInitialized == false)
                {
                    throw new Exception("浏览器还没有初始化，请先登录");
                }
                this.wb1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetUid()
        {
            if (this.wb1.IsBrowserInitialized == false)
            {
                throw new Exception("浏览器还没有初始化，请先登录");
            }
            string sourceHtml = this.wb1.GetSourceAsync().Result;
            string uid = "";
            int index = 0;
            do
            {
                index = sourceHtml.IndexOf("userid=", index);
                if (index < 0)
                {
                    throw new Exception("网页中未找到 userid= 请检查是否登录");
                }
                int start = index + "userid=".Length;
                int end = index + "userid=".Length;
                for (; end < sourceHtml.Length && Char.IsDigit(sourceHtml[end]); end++)
                {
                }
                uid = sourceHtml.Substring(start, end - start);
                if (string.IsNullOrWhiteSpace(uid) == false)
                {
                    break;
                }
                index = start;
            } while (index < sourceHtml.Length);
            return uid;
        }

        /// <summary>
        /// 执行js脚本
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string EvaluateScript(string script)
        {
            if (this.wb1.IsBrowserInitialized == false)
            {
                throw new Exception("浏览器还没有初始化，请先登录");
            }
            var task = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(script, "", 1, new TimeSpan(0, 0, 30));
            var ret = task.Result;
            if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
            {
                throw new Exception("执行操作失败：" + ret.Message);
            }
            return ret.Result.ToString();
        }

        public Shop GetLoginShop()
        {
            string uid = GetUid();
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new Exception("网页中未找到 userid= 请检查是否登录");
            }
            var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
            var shop = shops.FirstOrDefault(obj => obj.PopSellerNumberId == uid);
            if (shop == null)
            {
                throw new Exception("UID " + uid + " 未在本地找到匹配店铺，请检查登录或者本地店铺是否配置");
            }
            return shop;
        }

        public ImageDirRsp GetImageDirRsp()
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            string tbToken = CefCookieVisitor.GetSignleCookieValue("tadget.taobao.com", "_tb_token_");
            if (string.IsNullOrWhiteSpace(tbToken))
            {
                throw new Exception("获取_tb_token cookie 为空");
            }
            var url = new Uri("https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_dirTree_query&count=true&_input_charset=utf-8&_tb_token_=" + tbToken);
            string json = MsHttpRestful.GetReturnString(url.OriginalString, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageDirRsp>(json);
            return rsp;
        }

        public ImageFileRsp GetImageFileRsp(string catId)
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            string tbToken = CefCookieVisitor.GetSignleCookieValue("tadget.taobao.com", "_tb_token_");
            if (string.IsNullOrWhiteSpace(tbToken))
            {
                throw new Exception("获取_tb_token cookie 为空");
            }
            var url = new Uri("https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_batch_query&_input_charset=utf-8&cat_id=" + catId + "&ignore_cat=0&order_by=0&page=1&client_type=0&deleted=0&status=0&_tb_token_=" + tbToken);
            string json = MsHttpRestful.GetReturnString(url.OriginalString, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageFileRsp>(json);
            return rsp;
        }

        public ImageAddDirRsp AddDir(string catId, string name)
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            string tbToken = CefCookieVisitor.GetSignleCookieValue("tadget.taobao.com", "_tb_token_");
            if (string.IsNullOrWhiteSpace(tbToken))
            {
                throw new Exception("获取_tb_token cookie 为空");
            }
            var url = new Uri(" https://tadget.taobao.com/redaction/redaction/json.json?cmd=json_add_dir&_input_charset=utf-8&dir_id=" + catId + "&name=" + MsHttpRestful.UrlEncode(name, Encoding.UTF8) + "&_tb_token_=" + tbToken);
            string json = MsHttpRestful.GetReturnString(url.OriginalString, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageAddDirRsp>(json);
            return rsp;
        }
        public ImageAddFileRsp AddFile(string catId, FileInfo fi)
        {
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue("tadget.taobao.com");
            Dictionary<string, string> param = new Dictionary<string, string>();
            Dictionary<string, FileInfo> files = new Dictionary<string, FileInfo>();
            param["name"] = fi.Name;
            param["ua"] = EvaluateScript(jsgetua);
            Debug.WriteLine("File:" + fi.FullName + ", UA:" + param["ua"]);
            files["file"] = fi;
            var url = new Uri("https://stream-upload.taobao.com/api/upload.api?appkey=tu&folderId=" + catId + "&watermark=false&autoCompress=false&_input_charset=utf-8");
            string json = MsHttpRestful.PostMultipartFormDataBodyReturnString(url.OriginalString, param, files, headers);
            var rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageAddFileRsp>(json);
            return rsp;
        }


    }
}
