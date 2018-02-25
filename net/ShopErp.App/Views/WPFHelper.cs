using System;
using System.Windows;
using System.Windows.Threading;

namespace ShopErp.App.Views
{
    public class WPFHelper
    {
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}
