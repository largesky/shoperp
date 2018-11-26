using CefSharp;
using CefSharp.WinForms;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Utils;
using ShopErp.App.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ShopErp.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static void PrintArray<T>(string title, T[] source)
        {
            string data = string.Join(",", source.Select(obj => obj.ToString()));
            Debug.WriteLine(title + ": " + data);
        }

        static void PrintArray2<T>(string title, T[][] source)
        {
            string data = string.Join("}", source.Select(obj => string.Join(",", obj.Select(o => o.ToString()))));
            Debug.WriteLine(title + ": " + data);
        }


        static void ParseAddress()
        {
            string content = "<div class='address-detail' data-show-address='false' style='visibility: visible;'>鄭小姐,61124583,<span class='address-detail-oversea'>[<span class='detail-oversea-info'>广东省<span class='forward-tip-container' style='display: none;'><span class='forward-tip-title'>转运仓库：</span><span>深圳市 龙华新区 观澜街道大富路新宏泽工业园顺丰仓储-淘宝集运香港仓@WIB562#D5JWEX3XUKZZ# 518101 075566858233</span><s class='forward-tip-arrow'></s></span></span>]<img src='//img.alicdn.com/tps/i3/T1nuKOXuteXXbXX2Hb-24-18.png'>转&nbsp;</span>香港特别行政区 九龙 观塘区 官塘秀茂坪南邨秀好樓36樓3602室 ,810205";
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(content);
            string hh = document.DocumentNode.InnerText;
            string nhh = hh.Substring(0, hh.IndexOf("]转&nbsp;") + 1);
            string read = nhh.Replace("[", "").Replace("]", "");
            string mark = "转运仓库";
            if (read.IndexOf(mark) > 0)
            {
                read = read.Remove(read.IndexOf(mark), mark.Length+1);
            }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                ParseAddress();

                var settings = new CefSettings();
                settings.LogSeverity = LogSeverity.Warning;
                settings.LogFile = EnvironmentDirHelper.DIR_LOG + "\\CEF.txt";
                settings.MultiThreadedMessageLoop = true;
                settings.ExternalMessagePump = !settings.MultiThreadedMessageLoop;
                if (Cef.Initialize(settings, true, new BrowserProcessHandler()) == false)
                {
                    throw new Exception("初始化CEF SHARP 失败");
                }

                var lw = new LoginWindow { Title = "登录网店ERP" };
                bool? ret = lw.ShowDialog();
                if (ret == null || ret == false)
                {
                    Process.GetCurrentProcess().Kill();
                }
                base.OnStartup(e);
                //new MainWindow().ShowDialog();
            }
            catch (TypeInitializationException te)
            {
                MessageBox.Show(te.InnerException.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.GetCurrentProcess().Kill();
            }

        }

        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
                e.Handled = true;
            e.Handled = false;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Cef.Shutdown();
            //Process.GetCurrentProcess().Kill();
        }
    }
}