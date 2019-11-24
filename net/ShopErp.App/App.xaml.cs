using CefSharp;
using CefSharp.WinForms;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Service;
using ShopErp.App.Utils;
using ShopErp.App.Views;
using ShopErp.Domain;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var settings = new CefSettings() { LogSeverity = LogSeverity.Warning, LogFile = EnvironmentDirHelper.DIR_LOG + "\\CEF.txt", MultiThreadedMessageLoop = true };
                settings.ExternalMessagePump = !settings.MultiThreadedMessageLoop;
                if (Cef.Initialize(settings, true, new BrowserProcessHandler()) == false)
                {
                    throw new Exception("初始化CEF SHARP 失败");
                }
                Service.Net.MsHttpRestful.NETWORK_MAX_TIME_OUT = int.Parse(LocalConfigService.GetValue(SystemNames.CONFIG_NETWORK_MAX_TIMEOUT, "10"));
                var lw = new LoginWindow { Title = "登录网店ERP" };
                bool? ret = lw.ShowDialog();
                if (ret == null || ret == false)
                {
                    Cef.Shutdown();
                    Process.GetCurrentProcess().Kill();
                }
                base.OnStartup(e);
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