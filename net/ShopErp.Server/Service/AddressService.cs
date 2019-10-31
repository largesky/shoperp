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
        public const string COUNTRY = "Country";
        public const string PROVINCE = "Province";
        public const string CITY = "City";
        public const string DISTRICT = "District";
        public const string TOWN = "Town";

        private static readonly string[] PDD_DIS_MARK = new string[] { "省直辖县级行政区划", "自治区直辖县级行政区划", "县" };
        private static readonly string[] D_CITY = new string[] { "北京", "上海", "天津", "重庆" };
        private static string[] RemoveWords = new string[] { "阿昌族", "白族", "保安族", "布朗族", "布依族", "藏族", "朝鲜族", "达斡尔族", "傣族", "德昂族", "东乡族", "侗族", "独龙族", "俄罗斯族", "鄂伦春族", "鄂温克族", "高山族", "哈尼族", "哈萨克族", "赫哲族", "回族", "基诺族", "京族", "景颇族", "柯尔克孜族", "拉祜族", "黎族", "傈僳族", "珞巴族", "满族", "毛南族", "门巴族", "蒙古族", "苗族", "仫佬族", "纳西族", "怒族", "普米族", "羌族", "撒拉族", "畲族", "水族", "塔吉克族", "塔塔尔族", "土家族", "土族", "佤族", "维吾尔族", "乌孜别克族", "锡伯族", "瑶族", "彝族", "仡佬族", "裕固族", "壮族", "自治州", "自治县", "自治旗", "自治" };

        private static System.Collections.Generic.Dictionary<PopType, AddressNode> Address = new System.Collections.Generic.Dictionary<PopType, AddressNode>();

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
                return province;
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
        /// 更新缓存并保存到文件
        /// </summary>
        /// <param name="xDoc"></param>
        public static void UpdateAndSaveAreas(XDocument xDoc, PopType popType)
        {
            if (popType == PopType.TMALL)
            {
                popType = PopType.TAOBAO;
            }
            using (FileStream fs = new FileStream(System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, popType + "_Address.xml"), FileMode.Create))
            {
                xDoc.Save(fs);
            }
            LoadFile(popType);
        }

        /// <summary>
        /// 读取文件到缓存
        /// </summary>
        private static void LoadFile(PopType popType)
        {
            if (popType == PopType.TMALL)
            {
                popType = PopType.TAOBAO;
            }
            lock (Address)
            {
                var root = new AddressNode { Type = "Country", Name = "中国", ShortName = "中国" };
                XDocument xDoc = XDocument.Load(System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, popType + "_Address.xml"));
                foreach (var province in xDoc.Root.Elements())
                {
                    //省
                    AddressNode p = new AddressNode { Name = province.Attribute("Name").Value, ShortName = province.Attribute("ShortName").Value, Type = province.Name.LocalName };
                    root.SubNodes.Add(p);

                    //市或者省直辖行政区
                    foreach (var city in province.Elements())
                    {
                        AddressNode c = new AddressNode { Name = city.Attribute("Name").Value, ShortName = city.Attribute("ShortName").Value, Type = city.Name.LocalName };
                        p.SubNodes.Add(c);

                        //区县，二级市
                        foreach (var district in city.Elements())
                        {
                            c.SubNodes.Add(new AddressNode { Name = district.Attribute("Name").Value, ShortName = district.Attribute("ShortName").Value, Type = district.Name.LocalName });
                        }
                    }
                }
                Address[popType] = root;
            }
        }

        /// <summary>
        /// 返回省，市，区，镇，详细地址 格式
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string[] Parse5Address(string address, PopType sourceType, PopType targetType)
        {
            if (sourceType == PopType.TMALL)
            {
                sourceType = PopType.TAOBAO;
            }
            if (targetType == PopType.TMALL)
            {
                targetType = PopType.TAOBAO;
            }
            if (Address.ContainsKey(sourceType) == false)
            {
                LoadFile(sourceType);
            }
            if (Address.ContainsKey(targetType) == false)
            {
                LoadFile(targetType);
            }

            AddressNode sourceRoot = Address[sourceType];
            AddressNode targetRoot = Address[targetType];
            string add = address.Trim();
            string[] adds = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };

            //第一步省
            AddressNode p = sourceRoot.SubNodes.FirstOrDefault(obj => add.StartsWith(obj.Name));
            if (p == null)
            {
                throw new Exception("地址格式错误，无法解析出省：" + add);
            }
            adds[0] = p.Name;
            add = add.Substring(p.Name.Length).Trim();
            var city = p.SubNodes.FirstOrDefault(obj => add.StartsWith(obj.Name));
            if (city == null)
            {
                throw new Exception("地址格式错误，无法解析出市：" + add);
            }
            if (city.Type == DISTRICT)
            {
                //省级直属区，县
                adds[2] = city.Name;
                add = add.Substring(city.Name.Length).Trim();
            }
            else
            {
                adds[1] = city.Name;
                add = add.Substring(city.Name.Length).Trim();
                var district = city.SubNodes.FirstOrDefault(obj => add.StartsWith(obj.Name));
                if (district != null)
                {
                    adds[2] = district.Name;
                    add = add.Substring(district.Name.Length).Trim();
                }
            }
            //从详细地址删除存在省市区等信息
            for (int i = 0; i <= 2; i++)
            {
                if (string.IsNullOrWhiteSpace(adds[i]))
                {
                    continue;
                }
                if (add.StartsWith(adds[i]))
                {
                    add = add.Substring(adds[i].Length);
                }
            }
            if (sourceType == PopType.PINGDUODUO && targetType == PopType.TAOBAO)
            {
                //这种是直辖市
                if (adds[0].Contains("市"))
                {
                    adds[1] = adds[0];
                    adds[0] = adds[0].Replace("市", "");
                }

                //这种是省下面的直辖区，县
                if (PDD_DIS_MARK.Any(obj => obj == adds[1]))
                {
                    adds[1] = string.Empty;
                }
            }
            adds[4] = add;
            return adds;
        }
    }

}
