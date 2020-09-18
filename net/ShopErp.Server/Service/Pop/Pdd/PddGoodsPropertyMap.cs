using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddGoodsPropertyMap
    {
        public int MapTo { get; private set; }

        public string Description { get; private set; }

        public string GoodsType { get; private set; }

        public List<PddGoodsPropertyMapItem> Content { get; private set; }

        private static List<PddGoodsPropertyMap> maps = null;

        static PddGoodsPropertyMap()
        {
            maps = new List<PddGoodsPropertyMap>();

            //映射中，前面是拼多多，后面是淘宝或者天猫
            var map = new PddGoodsPropertyMap { MapTo = 6, Description = "天猫", GoodsType = "靴子" };
            map.Add("品牌", "品牌");
            map.Add("鞋面材质", "帮面材质").Add("绒面", "多材质拼接");
            map.Add("帮高筒高", "筒高").Add("及踝靴", "及踝").Add("过膝", "长筒");
            map.Add("后跟高", "后跟高");
            map.Add("鞋底材质", "鞋底材质", "橡胶");
            map.Add("闭合方式", "闭合方式");
            map.Add("靴款品名", "靴款品名");
            map.Add("鞋头款式", "鞋头款式");
            map.Add("风格", "风格");
            map.Add("上市时节", "上市年份季节");
            map.Add("制作工艺", "鞋制作工艺").Add("胶粘鞋", "胶粘").Add("注压鞋", "注塑").Add("缝制鞋", "缝制");
            map.Add("靴筒面材质", "靴筒面材质").Add("绒面", "多材质拼接");
            map.Add("靴筒内里材质", "靴筒内里材质").Add("人造短毛绒", "人造毛绒");
            map.Add("跟底款式", "跟底款式");
            map.Add("鞋面内里材质", "鞋面内里材质").Add("人造短毛绒", "人造毛绒");
            map.Add("鞋垫材质", "鞋垫材质", "人造毛绒").Add("人造短毛绒", "人造毛绒");
            map.Add("功能", "", "防滑@#@保暖@#@增高");
            map.Add("图案", "图案");
            map.Add("流行元素", "流行元素");
            map.Add("适用人群", "适用对象");

            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "天猫", GoodsType = "低帮鞋" };
            map.Add("品牌", "品牌");
            map.Add("鞋头款式", "鞋头款式");
            map.Add("鞋面材质", "帮面材质");
            map.Add("跟底款式", "跟底款式");
            map.Add("后跟高", "后跟高");
            map.Add("风格", "风格");
            map.Add("鞋底材质", "鞋底材质");
            map.Add("适用人群", "适用对象");
            map.Add("款式", "款式");
            map.Add("适用场合", "适用场景");
            map.Add("闭合方式", "闭合方式");
            map.Add("流行元素", "流行元素");
            map.Add("开口深度", "开口深度");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "天猫", GoodsType = "凉鞋" };
            map.Add("品牌", "品牌");
            map.Add("鞋头款式", "鞋头款式");
            map.Add("鞋面材质", "帮面材质");
            map.Add("闭合方式", "闭合方式");
            map.Add("鞋底材质", "鞋底材质");
            map.Add("风格", "风格");
            map.Add("后跟高", "后跟高");
            map.Add("适用人群", "适用对象");
            map.Add("款式", "款式");
            map.Add("适用场合", "适用场景");
            map.Add("跟底款式", "跟底款式");
            map.Add("流行元素", "流行元素");
            maps.Add(map);


            map = new PddGoodsPropertyMap { MapTo = 1, Description = "淘宝", GoodsType = "靴子" };
            map.Add("品牌", "品牌");
            map.Add("闭合方式", "闭合方式");
            map.Add("鞋面材质", "帮面材质");
            map.Add("跟底款式", "跟底款式");
            map.Add("后跟高", "后跟高");
            map.Add("风格", "风格");
            map.Add("靴款品名", "靴款品名");
            map.Add("适用人群", "适用对象");
            map.Add("帮高筒高", "筒高");
            map.Add("流行元素", "流行元素");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "淘宝", GoodsType = "低帮鞋" };
            map.Add("品牌", "品牌");
            map.Add("鞋头款式", "鞋头款式");
            map.Add("鞋面材质", "帮面材质");
            map.Add("跟底款式", "跟底款式");
            map.Add("后跟高", "后跟高");
            map.Add("风格", "风格");
            map.Add("鞋底材质", "鞋底材质");
            map.Add("适用人群", "适用对象");
            map.Add("款式", "款式");
            map.Add("适用场合", "适用场景");
            map.Add("闭合方式", "闭合方式");
            map.Add("流行元素", "流行元素");
            map.Add("开口深度", "开口深度");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "淘宝", GoodsType = "凉鞋" };
            map.Add("品牌", "品牌");
            map.Add("鞋头款式", "鞋头款式");
            map.Add("鞋面材质", "帮面材质");
            map.Add("闭合方式", "闭合方式");
            map.Add("鞋底材质", "鞋底材质");
            map.Add("风格", "风格");
            map.Add("后跟高", "后跟高");
            map.Add("适用人群", "适用对象");
            map.Add("款式", "款式");
            map.Add("适用场合", "适用场景");
            map.Add("跟底款式", "跟底款式");
            map.Add("流行元素", "流行元素");
            maps.Add(map);

        }

        public PddGoodsPropertyMap()
        {
            this.Content = new List<PddGoodsPropertyMapItem>();
        }

        public PddGoodsPropertyMapItem Add(string pddName, string otherPopName, string defaultValue = "")
        {
            if (this.Content.Any(obj => obj.PddName == pddName))
            {
                throw new Exception("指定的值：" + pddName + " 已经存在," + this.Description + "-->" + this.GoodsType);
            }
            var item = new PddGoodsPropertyMapItem { PddName = pddName, OtherPopName = otherPopName, DefaultValue = defaultValue };
            this.Content.Add(item);
            return item;
        }

        public static PddGoodsPropertyMapItem GetMapPropertyByKey(PopType popType, string goodsType, string pddName)
        {
            var map = maps.FirstOrDefault(obj => obj.MapTo == (int)popType && goodsType == obj.GoodsType);
            if (map == null)
            {
                throw new Exception("未能找到对应的映射关系:" + popType + "," + goodsType + "," + pddName);
            }

            foreach (var pair in map.Content)
            {
                if (pair.PddName.Trim().Equals(pddName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return pair;
                }
            }
            return null;
        }
    }
}
