using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ShopErp.App.Service.Restful
{
    public class VendorService : ServiceBase<Vendor>
    {
        private static readonly char[] NUMBERS = "0123456789０１２３４５６７８９一二三四五六七八九".ToArray();
        private static readonly List<char> NUMBERS_REPLAYCE = "０１２３４５６７８９".ToList();
        private static readonly List<char> NUMBERS_REPLAYCE1 = "一二三四五六七八九".ToList();

        private static readonly string[] KEY_WORDS = new string[]
        {
            "真皮", "女鞋", "鞋业", "有限", "公司", "鞋厂", "鞋贸", "推荐", "厂家", "生产", "GO2", "省", "-", ".", "商家", "商贸", "品质", "(",
            ")", "工厂", "直销", "四川省", "四川", "成都市", "成都", "省", "广州", "代发", "一件", "主推"
        };

        private static readonly List<Vendor> vendors_catch = new List<Vendor>();

        private void Reload()
        {
            lock (vendors_catch)
            {
                vendors_catch.Clear();
                var datas = this.GetByAll("", "", "", "", 0, 0).Datas;
                vendors_catch.AddRange(datas);
            }
        }

        private Vendor GetVendorInCach(Predicate<Vendor> match)
        {
            var vendor = vendors_catch.FirstOrDefault(obj => match(obj));
            if (vendor != null)
            {
                return vendor;
            }
            this.Reload();
            vendor = vendors_catch.FirstOrDefault(obj => match(obj));
            return vendor;
        }

        public DataCollectionResponse<Vendor> GetByAll(string name, string pingYingName, string homePage, string marketAddress, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["name"] = name;
            para["pingYingName"] = pingYingName;
            para["homePage"] = homePage;
            para["marketAddress"] = marketAddress;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<Vendor>>(para);
        }

        public DataCollectionResponse<long> GetAllVendorIdHasGoods()
        {
            return DoPost<DataCollectionResponse<long>>(null);
        }

        public string GetVendorName(long vendorId)
        {
            var vendor = GetVendorInCach(obj => obj.Id == vendorId);
            return vendor == null ? "" : vendor.Name;
        }

        public char GetVendorPingYingFirstChar(long vendorId)
        {
            var vendor = GetVendorInCach(obj => obj.Id == vendorId);
            if (vendor == null || string.IsNullOrWhiteSpace(vendor.PingyingName))
            {
                return ' ';
            }
            return vendor.PingyingName[0];
        }

        /// <summary>
        /// 在缓存中搜索厂家拼音名称
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        public string GetVendorPingyingName(string vendorName)
        {
            var vendor = GetVendorInCach(obj => obj.Name == vendorName);
            return vendor == null ? "" : vendor.PingyingName;
        }

        /// <summary>
        /// 在缓存中搜索厂家拼音名称
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        public string GetVendorPingyingName(long vendorId)
        {
            var vendor = GetVendorInCach(obj => obj.Id == vendorId);
            return vendor == null ? "" : vendor.PingyingName;
        }

        /// <summary>
        /// 在缓存中搜索厂家商场地址
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        public string GetVendorAddress_InCach(string vendorName)
        {
            var vendor = GetVendorInCach(obj => obj.Name == vendorName);
            return vendor == null ? "" : vendor.MarketAddress;
        }

        /// <summary>
        /// 比较电话名称地址
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool Match(Vendor v1, Vendor v2)
        {
            if (v1 == null && v2 == null)
            {
                return true;
            }

            if (v1 != null && v2 == null)
            {
                return false;
            }

            if (v1 == null && v2 != null)
            {
                return false;
            }

            if (v1 == v2)
            {
                return true;
            }

            return string.Compare(v1.Name, v2.Name) == 0 && string.Compare(v1.Phone, v2.Phone) == 0 && string.Compare(v1.MarketAddress, v2.MarketAddress) == 0;
        }

        /// <summary>
        /// 格式货厂家名称，删除不需要关键词，保留长度为6个字
        /// </summary>
        /// <param name="vendorName"></param>
        /// <returns></returns>
        public static string FormatVendorName(string vendorName)
        {
            StringBuilder sb = new StringBuilder(vendorName);
            foreach (var str in KEY_WORDS)
            {
                sb.Replace(str, "");
            }
            if (sb.Length > 6)
            {
                sb.Length = 6;
            }
            return sb.ToString();
        }

        /// <summary>
        /// 格式化厂家地址 成 3-22412-2 的形式
        /// </summary>
        /// <param name="marketAddress"></param>
        /// <returns></returns>
        public static string FormatVendorDoor(string marketAddress)
        {
            string area = "", street = "", door = "";

            area = FindAreaOrStreet(marketAddress, "区");
            street = FindAreaOrStreet(marketAddress, "街");
            door = FindDoor(marketAddress);

            return string.Format("{0}-{1}-{2}", area, door, street);
        }

        /// <summary>
        /// 在址中搜索街或者区
        /// </summary>
        /// <param name="address"></param>
        /// <param name="type">街 区</param>
        /// <returns></returns>
        public static string FindAreaOrStreet(string address, string type)
        {
            int areaIndex = address.IndexOf(type);
            string area = "";
            while (areaIndex - 1 >= 0 && NUMBERS.Any(c => c == address[areaIndex - 1]) == false)
            {
                areaIndex--;
            }
            if (areaIndex >= 0)
            {
                int endAreaIndex = areaIndex;
                while (areaIndex - 1 >= 0 && NUMBERS.Any(c => c == address[areaIndex - 1]))
                {
                    areaIndex--;
                }
                if (areaIndex < 0)
                {
                    areaIndex = 0;
                }
                area = address.Substring(areaIndex, endAreaIndex - areaIndex);
            }
            area = Format(area);
            return area;
        }

        /// <summary>
        /// 搜索门牌号 如 22322
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string FindDoor(string address)
        {
            string doorNumber = "";
            for (int i = 0; i < address.Length; i++)
            {
                if (NUMBERS.Any(c => c == address[i]) == false)
                {
                    continue;
                }
                int j = 1;
                for (; j < 5 && (j + i) < address.Length; j++)
                {
                    if (NUMBERS.Any(c => c == address[i + j]) == false)
                    {
                        break;
                    }
                }
                if (j == 5)
                {
                    doorNumber = address.Substring(i, 5);
                    break;
                }
            }
            doorNumber = Format(doorNumber);
            return doorNumber;
        }

        /// <summary>
        /// 将全角字符数据转成半角字符
        /// </summary>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        private static string Format(string oldValue)
        {
            char[] tmps = oldValue.ToArray();
            for (int i = 0; i < tmps.Length; i++)
            {
                int index = NUMBERS_REPLAYCE.IndexOf(tmps[i]);
                if (index >= 0)
                {
                    tmps[i] = NUMBERS[i];
                    continue;
                }

                index = NUMBERS_REPLAYCE1.IndexOf(tmps[i]);
                if (index >= 0)
                {
                    tmps[i] = NUMBERS[i + 1];
                }
            }
            return new string(tmps);
        }
    }
}