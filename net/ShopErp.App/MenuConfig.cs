using ShopErp.App.Views.Config;
using ShopErp.App.Views.DataCenter;
using ShopErp.App.Views.Delivery;
using ShopErp.App.Views.Finance;
using ShopErp.App.Views.Goods;
using ShopErp.App.Views.Orders;
using ShopErp.App.Views.Print;
using ShopErp.App.Views.Shops;
using ShopErp.App.Views.Taobao;
using ShopErp.App.Views.Users;
using ShopErp.App.Views.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App
{
    class MenuConfig
    {
        public string Header { get; set; }

        public Type Type { get; set; }

        public bool IsChecked { get; set; }

        public List<MenuConfig> SubItems { get; set; }

        public static MenuConfig[] Menus { get; set; }


        static MenuConfig()
        {
            List<MenuConfig> mcs = new List<MenuConfig>();
            MenuConfig mc = null;

            mc = new MenuConfig("订单管理", null);
            mc.Add("所有订单", typeof(OrderAllUserControl));
            mc.Add("订单同步", typeof(OrderSyncUserControl));
            mc.Add("订单同步-网页", typeof(OrderSyncHtmlUserControl));
            mc.Add("退货管理", typeof(OrderReturnUserControl));
            mc.Add("退货出单", typeof(OrderReturnOutUserControl));
            mc.Add("货物统计", typeof(GoodsCountUserControl));
            mc.Add("拿货扫描", typeof(OrderGetedMarkUserControl));
            mc.Add("订单导出", typeof(OrderExportUserControl));
            mcs.Add(mc);

            mc = new MenuConfig("商品与厂家", null);
            mc.Add("商品管理", typeof(GoodsUserControl));
            mc.Add("厂家管理", typeof(VendorUserControl));
            mc.Add("店铺商品", typeof(PopGoodsUserControl));
            mc.Add("厂家更新", typeof(VendorSyncUserControl));
            mcs.Add(mc);


            mc = new MenuConfig("物流", null);
            mc.Add("快递分配", typeof(DeliveryDistributionUserControl));
            mc.Add("发货扫描", typeof(DeliveryOutScanUserControl));
            mc.Add("发货记录", typeof(DeliveryOutQueryUserControl));
            mc.Add("收件扫描", typeof(DeliveryInScanUserControl));
            mc.Add("收件记录", typeof(DeliveryInQueryUserControl));
            mc.Add("标记发货", typeof(ShippingCheckUserControl));
            mc.Add("标记发货-网页", typeof(ShippingCheckHtmlUserControl));
            mc.Add("物流时限", typeof(DeliveryCheckUserControl));
            mc.Add("快递配置", typeof(DeliveryCompanyUserControl));
            mc.Add("运费模板", typeof(DeliveryTemplateUserControl));
            mcs.Add(mc);

            mc = new MenuConfig("打印", null);
            mc.Add("打印订单", typeof(PrintUserControl));
            mc.Add("打印历史", typeof(PrintHistoryUserControl));
            mc.Add("打印模板", typeof(PrintTemplateUserControl));
            mc.Add("物流记录", typeof(WuliuNumberUserControl));
            mcs.Add(mc);

            mc = new MenuConfig("数据", null);
            mc.Add("销售汇总", typeof(SaleUserControl));
            mcs.Add(mc);

            mc = new MenuConfig("淘宝运营", null);
            mc.Add("淘宝关键词货号管理", typeof(TaobaoKeywordUserControl));
            mc.Add("淘宝关键词导入删除", typeof(TaobaoKeywordManagementUserControl));
            mc.Add("淘宝关键词统计", typeof(TaobaoKeywordCountUserControl));
            mc.Add("淘宝关键分日趋势", typeof(TaobaoKeywordStateUserControl));
            mc.Add("淘宝搜索排行关键词", typeof(TaobaoTopKeywordUserControl));
            mcs.Add(mc);


            mc = new MenuConfig("财务", null);
            mc.Add("好评返现", typeof(ReturnCashUserControl));
            mc.Add("日常记账", typeof(FinanceUserControl));
            mc.Add("账户管理", typeof(FinanceAccountUserControl));
            mcs.Add(mc);


            mc = new MenuConfig("选项", null);
            mc.Add("系统配置", typeof(ConfigUserControl));
            mc.Add("用户管理", typeof(UsersUserControl));
            mc.Add("店铺管理", typeof(ShopUserControl));
            mc.Add("数据清理", typeof(SystemCleanUserControl));
            mc.Add("图片清理", typeof(ImgCleanUserControl));
            mc.Add("工具栏配置", typeof(MenuItemConfigUserControl));
            mcs.Add(mc);

            Menus = mcs.ToArray();
        }


        public MenuConfig(string header, Type type)
        {
            this.Header = header;
            this.Type = type;
            this.SubItems = new List<MenuConfig>();
        }

        public void Add(string header, Type type)
        {
            if (this.SubItems.Any(obj => obj.Header.Equals(header)))
            {
                throw new Exception("已存在相同的菜单项");
            }
            this.SubItems.Add(new MenuConfig(header, type));
        }

    }
}
