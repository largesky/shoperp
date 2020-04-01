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

        public Dictionary<string, string> Content { get; private set; }

        private static List<PddGoodsPropertyMap> maps = null;

        static PddGoodsPropertyMap()
        {
            maps = new List<PddGoodsPropertyMap>();

            //映射中，前面是拼多多，后面是淘宝或者天猫
            var map = new PddGoodsPropertyMap { MapTo = 6, Description = "天猫", GoodsType = "靴子", Content = new Dictionary<string, string>() };
            map.Content.Add("品牌", "品牌");
            map.Content.Add("闭合方式", "闭合方式");
            map.Content.Add("鞋面材质", "帮面材质");
            map.Content.Add("跟底款式", "跟底款式");
            map.Content.Add("后跟高", "后跟高");
            map.Content.Add("风格", "风格");
            map.Content.Add("靴款品名", "靴款品名");
            map.Content.Add("适用人群", "适用对象");
            map.Content.Add("帮高筒高", "筒高");
            map.Content.Add("流行元素", "流行元素");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "天猫", GoodsType = "低帮鞋", Content = new Dictionary<string, string>() };
            map.Content.Add("品牌", "品牌");
            map.Content.Add("鞋头款式", "鞋头款式");
            map.Content.Add("鞋面材质", "帮面材质");
            map.Content.Add("跟底款式", "跟底款式");
            map.Content.Add("后跟高", "后跟高");
            map.Content.Add("风格", "风格");
            map.Content.Add("鞋底材质", "鞋底材质");
            map.Content.Add("适用人群", "适用对象");
            map.Content.Add("款式", "款式");
            map.Content.Add("适用场合", "适用场景");
            map.Content.Add("闭合方式", "闭合方式");
            map.Content.Add("流行元素", "流行元素");
            map.Content.Add("开口深度", "开口深度");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "天猫", GoodsType = "凉鞋", Content = new Dictionary<string, string>() };
            map.Content.Add("品牌", "品牌");
            map.Content.Add("鞋头款式", "鞋头款式");
            map.Content.Add("鞋面材质", "帮面材质");
            map.Content.Add("闭合方式", "闭合方式");
            map.Content.Add("鞋底材质", "鞋底材质");
            map.Content.Add("风格", "风格");
            map.Content.Add("后跟高", "后跟高");
            map.Content.Add("适用人群", "适用对象");
            map.Content.Add("款式", "款式");
            map.Content.Add("适用场合", "适用场景");
            map.Content.Add("跟底款式", "跟底款式");
            map.Content.Add("流行元素", "流行元素");
            maps.Add(map);


            map = new PddGoodsPropertyMap { MapTo = 1, Description = "淘宝", GoodsType = "靴子", Content = new Dictionary<string, string>() };
            map.Content.Add("品牌", "品牌");
            map.Content.Add("闭合方式", "闭合方式");
            map.Content.Add("鞋面材质", "帮面材质");
            map.Content.Add("跟底款式", "跟底款式");
            map.Content.Add("后跟高", "后跟高");
            map.Content.Add("风格", "风格");
            map.Content.Add("靴款品名", "靴款品名");
            map.Content.Add("适用人群", "适用对象");
            map.Content.Add("帮高筒高", "筒高");
            map.Content.Add("流行元素", "流行元素");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "淘宝", GoodsType = "低帮鞋", Content = new Dictionary<string, string>() };
            map.Content.Add("品牌", "品牌");
            map.Content.Add("鞋头款式", "鞋头款式");
            map.Content.Add("鞋面材质", "帮面材质");
            map.Content.Add("跟底款式", "跟底款式");
            map.Content.Add("后跟高", "后跟高");
            map.Content.Add("风格", "风格");
            map.Content.Add("鞋底材质", "鞋底材质");
            map.Content.Add("适用人群", "适用对象");
            map.Content.Add("款式", "款式");
            map.Content.Add("适用场合", "适用场景");
            map.Content.Add("闭合方式", "闭合方式");
            map.Content.Add("流行元素", "流行元素");
            map.Content.Add("开口深度", "开口深度");
            maps.Add(map);

            map = new PddGoodsPropertyMap { MapTo = 6, Description = "淘宝", GoodsType = "凉鞋", Content = new Dictionary<string, string>() };
            map.Content.Add("品牌", "品牌");
            map.Content.Add("鞋头款式", "鞋头款式");
            map.Content.Add("鞋面材质", "帮面材质");
            map.Content.Add("闭合方式", "闭合方式");
            map.Content.Add("鞋底材质", "鞋底材质");
            map.Content.Add("风格", "风格");
            map.Content.Add("后跟高", "后跟高");
            map.Content.Add("适用人群", "适用对象");
            map.Content.Add("款式", "款式");
            map.Content.Add("适用场合", "适用场景");
            map.Content.Add("跟底款式", "跟底款式");
            map.Content.Add("流行元素", "流行元素");
            maps.Add(map);

        }

        public static string GetMapPropertyNameByKey(PopType popType, string goodsType, string key)
        {
            var map = maps.FirstOrDefault(obj => obj.MapTo == (int)popType && goodsType == obj.GoodsType);
            if (map == null)
            {
                throw new Exception("未能找到对应的映射关系:" + popType + "," + goodsType + "," + key);
            }

            foreach (var pair in map.Content)
            {
                if (pair.Key.Trim().Equals(key.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }
            return "";
        }
    }
}
