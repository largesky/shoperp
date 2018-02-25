using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShopErp.Domain;
using ShopErp.Server.Utils;

namespace ShopErp.Server.Service
{
    public class AddressService
    {
        private static readonly string[] D_CITY = new string[] { "北京", "上海", "天津", "重庆" };
        private static String[] D_PROVINCE = new string[] { "宁夏", "广西", "内蒙古", "西藏", "新疆", "香港", "澳门", "台湾" };
        private static string[] RemoveWords = new string[] { "阿昌族", "白族", "保安族", "布朗族", "布依族", "藏族", "朝鲜族", "达斡尔族", "傣族", "德昂族", "东乡族", "侗族", "独龙族", "俄罗斯族", "鄂伦春族", "鄂温克族", "高山族", "哈尼族", "哈萨克族", "赫哲族", "回族", "基诺族", "京族", "景颇族", "柯尔克孜族", "拉祜族", "黎族", "傈僳族", "珞巴族", "满族", "毛南族", "门巴族", "蒙古族", "苗族", "仫佬族", "纳西族", "怒族", "普米族", "羌族", "撒拉族", "畲族", "水族", "塔吉克族", "塔塔尔族", "土家族", "土族", "佤族", "维吾尔族", "乌孜别克族", "锡伯族", "瑶族", "彝族", "仡佬族", "裕固族", "壮族", "自治州", "自治县", "自治旗", "自治" };

        const string FILENAME = "Address.xml";

        private static readonly AddressNode instance = new AddressNode();

        public static AddressNode Address { get { return instance; } }

        static AddressService()
        {
            String path = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, FILENAME);

            if (File.Exists(path) == false)
            {
                return;
            }

            try
            {
                XDocument xDoc = XDocument.Load(path);
                FillNodes(xDoc);
            }
            catch
            {
            }
        }

        public static void UpdateAndSaveAreas(XDocument xDoc)
        {
            String path = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, FILENAME);
            FillNodes(xDoc);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                xDoc.Save(fs);
            }
        }

        private static void FillNodes(XDocument xDoc)
        {
            Address.SubNodes.Clear();

            foreach (var state in xDoc.Root.Elements())
            {
                AddressNode s = new AddressNode { Name = state.Attribute("Name").Value, ShortName = state.Attribute("ShortName").Value };
                Address.SubNodes.Add(s);
                foreach (var city in state.Elements())
                {
                    AddressNode cc = new AddressNode { Name = city.Attribute("Name").Value, ShortName = city.Attribute("ShortName").Value };
                    s.SubNodes.Add(cc);
                    foreach (var region in city.Elements())
                    {
                        AddressNode r = new AddressNode { Name = region.Attribute("Name").Value, ShortName = region.Attribute("ShortName").Value };
                        cc.SubNodes.Add(r);
                    }
                }
            }
        }


        /// <summary>
        /// 删除前面的字符如四川省成都市，四川，则返回 省成都市
        /// </summary>
        /// <param name="value"></param>
        /// <param name="trimValue"></param>
        /// <returns></returns>
        public static string TrimStart(String value, string trimValue, int minCount = 2)
        {
            if (value == null || trimValue == null)
            {
                return value;
            }
            value = value.Trim();
            trimValue = trimValue.Trim();
            for (int i = trimValue.Length; i >= 1 && i >= minCount; i--)
            {
                string subTrimValue = trimValue.Substring(0, i);
                if (value.StartsWith(subTrimValue))
                {
                    value = value.Substring(subTrimValue.Length).Trim();
                    return value.Trim();
                }
            }

            return value.Trim();
        }

        public static string GetProvinceShortName(string province)
        {
            if (province.Contains("黑龙江"))
            {
                return "黑龙江";
            }

            if (province.Contains("内蒙古"))
            {
                return "内蒙古";
            }

            if (province.Length < 2)
            {
                throw new Exception("中国的省名称至少2个字符");
            }
            return province.Substring(0, 2);
        }

        public static string GetCityShortName(string city)
        {
            //检查自治区
            string d_city = D_CITY.FirstOrDefault(c => c.StartsWith(city));
            if (string.IsNullOrWhiteSpace(d_city) == false)
            {
                return d_city;
            }

            string s = city.TrimEnd('市', '县', '盟', '区').Trim();
            foreach (string removeWord in RemoveWords)
            {
                s = s.Replace(removeWord, "");
            }
            return s;
        }

        /// <summary>
        /// 解析省
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static AddressNode ParseProvince(string address)
        {
            //地址库中的省进行匹配
            var state = AddressService.Address.SubNodes.FirstOrDefault(s => address.StartsWith(s.ShortName));
            return state;
        }

        /// <summary>
        /// 解析市
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static AddressNode ParseCity(string address)
        {
            //检查直辖市
            var dcity = D_CITY.FirstOrDefault(obj => address.StartsWith(obj));
            if (string.IsNullOrWhiteSpace(dcity) == false)
            {
                var n = AddressService.Address.SubNodes.FirstOrDefault(s => s.Name.StartsWith(dcity));
                return AddressService.Address.SubNodes.FirstOrDefault(s => s.Name.StartsWith(dcity)).SubNodes[0];
            }

            //先解析省
            AddressNode state = ParseProvince(address);
            string ad = null;
            if (state == null)
            {
                return null;
            }
            ad = TrimStart(address, state.Name, 2).TrimStart('省').Trim();
            //处理市,全字匹配
            var city = state.SubNodes.FirstOrDefault(c => ad.StartsWith(c.ShortName));
            return city;
        }

        /// <summary>
        /// 解析区或者县或者地级市
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static AddressNode ParseRegion(string address)
        {
            address = address.Replace(",", "").Replace("。", "").Trim();
            AddressNode s = ParseProvince(address);
            if (s == null)
            {
                return null;
            }

            AddressNode c = ParseCity(address);
            if (c == null)
            {
                return null;
            }

            //删除前面的省市
            string ad = TrimStart(address, s.Name, 2).TrimStart('省').TrimStart('市').Trim();
            ad = TrimStart(ad, c.Name, 2).TrimStart('市', '州', '县').Trim();

            //查找区
            var region = c.SubNodes.FirstOrDefault(r => ad.StartsWith(r.ShortName));
            if (region != null)
            {
                return region;
            }

            int townIndex = ad.IndexOf("镇", StringComparison.Ordinal);
            if (townIndex > 0)
            {
                return new AddressNode { ShortName = "", Name = ad.Substring(0, townIndex + 1) };
            }

            int streetIndex = ad.IndexOf("街道", StringComparison.Ordinal);
            if (streetIndex > 0 && streetIndex < 10)
            {
                return new AddressNode { ShortName = "", Name = ad.Substring(0, streetIndex + 2) };
            }

            int areaIndex = ad.IndexOf("区", StringComparison.Ordinal);
            if (areaIndex > 0 && areaIndex < 10)
            {
                return new AddressNode { ShortName = "", Name = ad.Substring(0, areaIndex + 1) };
            }

            return null;
        }
    }

}
