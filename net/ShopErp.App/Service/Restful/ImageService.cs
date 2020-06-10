using ShopErp.App.Service.Net;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Domain;

namespace ShopErp.App.Service.Restful
{
    class ImageService
    {
        string APIURL_SAVE_IMAGE = "/image/saveimage.html";

        public byte[] GetImage(string image)
        {
            Console.WriteLine("GetImage:" + image);
            string url = ServiceContainer.ServerAddress + "/image/getimage.html?image=" + image;
            return MsHttpRestful.GetReturnBytes(url);
        }

        public void SaveImage(string image, byte[] imBytes)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para["imagePath"] = image;
            var ret = ServiceBase<object>.DoPostFileWithUrl<ResponseBase>(APIURL_SAVE_IMAGE, para, imBytes);
        }
    }
}