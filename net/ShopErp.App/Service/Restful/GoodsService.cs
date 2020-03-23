using ShopErp.App.Service.Net;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ShopErp.Domain.Pop;
using ShopErp.App.Utils;

namespace ShopErp.App.Service.Restful
{
    public class GoodsService : ServiceBase<Goods>
    {
        private static byte[] GetImage(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return new byte[0];
            }
            if ("abcdefghijklmnopqrstuvwxyz".Any(c => char.ToLower(image[0]) == c) && image[1] == ':')
            {
                return File.ReadAllBytes(image);
            }

            if (image.StartsWith("http", StringComparison.OrdinalIgnoreCase) || image.StartsWith("ftp", StringComparison.OrdinalIgnoreCase))
            {
                return MsHttpRestful.GetUrlEncodeBodyReturnBytes(image, null);
            }

            throw new Exception("无法处理的图片网址：" + image);
        }

        public DataCollectionResponse<Goods> GetByAll(long shopId, GoodsState state, int timeType, DateTime start, DateTime end, string vendor, string number, GoodsType type, string comment, ColorFlag flag, GoodsVideoType videoType, string order, string vendorAdd, string shipper, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shopId"] = shopId;
            para["state"] = state;
            para["timeType"] = timeType;
            para["start"] = start;
            para["end"] = end;
            para["vendor"] = vendor;
            para["number"] = number;
            para["type"] = type;
            para["comment"] = comment;
            para["flag"] = flag;
            para["videoType"] = videoType;
            para["order"] = order;
            para["vendorAdd"] = vendorAdd;
            para["shipper"] = shipper;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<Goods>>(para);
        }

        public DataCollectionResponse<Goods> GetByNumberAndVendorNameLike(string number, string vendorNameOrPingName, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["number"] = number;
            para["vendorNameOrPingName"] = vendorNameOrPingName;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<Goods>>(para);
        }

        public DataCollectionResponse<Goods> ParseGoods(string vendorNameOrPingName, string number)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["vendorNameOrPingName"] = vendorNameOrPingName;
            para["number"] = number;
            return DoPost<DataCollectionResponse<Goods>>(para);
        }

        public DataCollectionResponse<string> GetAllShippers()
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            return DoPost<DataCollectionResponse<string>>(para);
        }

        public DataCollectionResponse<PopGoods> SearchPopGoods(Shop shop, PopGoodsState state, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["state"] = state;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<PopGoods>>(para);
        }

        public DataCollectionResponse<string> AddGoods(Shop shop, PopGoods[] goods, float[] buyInPrices)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["goods"] = goods;
            para["buyInPrices"] = buyInPrices;
            return DoPost<DataCollectionResponse<string>>(para);
        }

        public static void SaveImage(Goods goods, string imageUrl)
        {
            //图片路径为空，或者目标图片与当前图片相同
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            if (goods.VendorId < 1)
            {
                throw new Exception("商品VENDORID不能小于1");
            }

            string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new Exception(SystemNames.CONFIG_WEB_IMAGE_DIR + "不能为空");
            }
            goods.ImageDir = "goods\\" + goods.VendorId.ToString() + "\\" + goods.Number;
            goods.Image = goods.ImageDir + "\\index" + imageUrl.Substring(imageUrl.LastIndexOf("."));
            string fullPath = dir + "\\" + goods.Image;
            var imageBytes = GetImage(imageUrl);
            FileUtil.EnsureExits(new FileInfo(fullPath));
            File.WriteAllBytes(fullPath, imageBytes);
            //ServiceContainer.GetService<ImageService>().SaveImage(goods.Image, imageBytes);
        }

        public static void SaveVideo(Goods goods, String videoUrl)
        {
            //图片路径为空，或者目标图片与当前图片相同
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                return;
            }

            if (goods.VendorId < 1)
            {
                throw new Exception("商品VENDORID不能小于1");
            }

            string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new Exception(SystemNames.CONFIG_WEB_IMAGE_DIR + "不能为空");
            }
            goods.ImageDir = "goods\\" + goods.VendorId.ToString() + "\\" + goods.Number;
            string videoPath = goods.ImageDir + "\\" + goods.Number + videoUrl.Substring(videoUrl.LastIndexOf("."));
            string fullPath = dir + "\\" + videoPath;
            var imageBytes = GetImage(videoUrl);
            FileUtil.EnsureExits(new FileInfo(fullPath));
            File.WriteAllBytes(fullPath, imageBytes);
        }
    }
}