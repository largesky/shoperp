using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class PrintTemplateService : ServiceBase<PrintTemplate>
    {
        private const string FILE_EXTENSION = ".pt";

        private static readonly string DATA_DIR = EnvironmentDirHelper.DIR_DATA + @"\PrintTemplate";

        static PrintTemplateService()
        {
            System.IO.Directory.CreateDirectory(DATA_DIR);
        }

        public static PrintTemplate[] GetAllLocal()
        {
            List<PrintTemplate> templates = new List<PrintTemplate>();

            foreach (string file in Directory.GetFiles(DATA_DIR, "*" + FILE_EXTENSION))
            {
                string content = File.ReadAllText(file, Encoding.UTF8);
                var template = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintTemplate>(content);
                templates.Add(template);
            }

            return templates.ToArray();
        }

        public static void InsertLocal(PrintTemplate deliveryTemplate)
        {
            var keys = deliveryTemplate.AttachFiles.Select(obj => obj.Name).ToArray();
            foreach (string key in keys)
            {
                if (deliveryTemplate.Items.Any(obj => obj.Id.ToString() == key) == false)
                {
                    var af = deliveryTemplate.AttachFiles.FirstOrDefault(obj => obj.Name == key);
                    deliveryTemplate.AttachFiles.Remove(af);
                }
            }
            string file = System.IO.Path.Combine(DATA_DIR, deliveryTemplate.Name + FILE_EXTENSION);
            using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                string content = Newtonsoft.Json.JsonConvert.SerializeObject(deliveryTemplate);
                var bytes = UTF8Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public static void UpdateLocal(PrintTemplate deliveryTemplate, string newName)
        {
            if (deliveryTemplate.Name.Equals(newName) == false)
            {
                var files = Directory.GetFiles(DATA_DIR, newName + FILE_EXTENSION);
                if (files.Count() > 0)
                {
                    throw new Exception("已有相同模板名称存在");
                }
            }
            DeleteLocal(deliveryTemplate.Name);
            deliveryTemplate.Name = newName;
            InsertLocal(deliveryTemplate);
        }

        public static void DeleteLocal(string name)
        {
            var files = Directory.GetFiles(DATA_DIR, name + FILE_EXTENSION);
            if (files.Length > 0)
            {
                File.Delete(files[0]);
            }
        }

        public DataCollectionResponse<PrintTemplate> GetPrintTemplate(Shop shop)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            var datas = DoPost<DataCollectionResponse<PrintTemplate>>(para);
            return datas;
        }
    }
}
