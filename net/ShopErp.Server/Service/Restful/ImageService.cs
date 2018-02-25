using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Server.Utils;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    class ImageService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getimage.html?image={image}")]
        public Stream GetImage(string image)
        {
            Console.WriteLine("GetImage:" + image);
            var webImageDir = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, "WEB_IMAGE_DIR", "");
            if (string.IsNullOrWhiteSpace(webImageDir))
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase("系统没有配置储存图片的路径"), System.Net.HttpStatusCode.OK);
            }
            string path = webImageDir + "\\" + image;
            if (System.IO.File.Exists(path) == false)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase("文件不存在：" + image), System.Net.HttpStatusCode.OK);
            }

            try
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpg";
                return File.OpenRead(path);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase("服务器异常：" + ex.Message + Environment.NewLine + ex.StackTrace), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/saveimage.html?imagePath={imagePath}")]
        public ResponseBase SaveImage(string imagePath, Stream image)
        {
            try
            {
                var webImageDir = ServiceContainer.GetService<SystemConfigService>().GetEx(-1, "WEB_IMAGE_DIR", "");
                if (string.IsNullOrWhiteSpace(webImageDir))
                {
                    throw new Exception("系统没有配置储存图片的路径");
                }
                string fullPath = webImageDir + "\\" + imagePath;
                FileUtil.EnsureExits(new FileInfo(fullPath));
                using (FileStream fs = File.Create(fullPath))
                {
                    image.CopyTo(fs);
                }
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}
