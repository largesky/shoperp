using ShopErp.App.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ShopErp.App.Utils;
using ShopErp.App.Service.Print;

namespace ShopErp.App.Views.Print
{
    public class FilePrintTemplateRepertory
    {
        private const string FILE_EXTENSION = ".pt";

        private const string FILE_EXTENSION_OLD = ".pto";

        private static readonly string DATA_DIR = EnvironmentDirHelper.DIR_DATA + @"\PrintTemplate";

        static FilePrintTemplateRepertory()
        {
            System.IO.Directory.CreateDirectory(DATA_DIR);
        }

        public static Domain.PrintTemplate[] GetAll()
        {
            List<Domain.PrintTemplate> templates = new List<Domain.PrintTemplate>();

            foreach (string file in Directory.GetFiles(DATA_DIR, "*" + FILE_EXTENSION_OLD))
            {
                using (Stream s = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    var template = bf.Deserialize(s) as Domain.PrintTemplate;
                    templates.Add(template);
                }
            }

            return templates.ToArray();
        }

        public static Service.Print.PrintTemplate ConvertOld(Domain.PrintTemplate pt)
        {
            var npt = new Service.Print.PrintTemplate
            {
                AttachFiles = pt.AttachFiles,
                BackgroundImage = pt.BackgroundImage,
                DeliveryCompany = pt.DeliveryCompany,
                Height = pt.Height,
                Name = pt.Name,
                PaperType = pt.PaperType,
                PrinterName = pt.PrinterName,
                Type = pt.Type,
                Width = pt.Width,
                XOffset = pt.XOffset,
                YOffset = pt.YOffset,
            };

            foreach (var v in pt.Items)
            {
                npt.Items.Add(new Service.Print.PrintTemplateItem
                {
                    FontName = v.FontName,
                    FontSize = v.FontSize,
                    Format = v.Format,
                    Height = v.Height,
                    Id = v.Id,
                    Opacity = v.Opacity,
                    ScaleFormat = v.ScaleFormat,
                    TextAlignment = v.TextAlignment,
                    Type = v.Type,
                    Value = v.Value,
                    Value1 = v.Value1,
                    Width = v.Width,
                    X = v.X,
                    Y = v.Y,
                    RunTimeTag = v.RunTimeTag,
                });
            }
            return npt;
        }

        public static Service.Print.PrintTemplate[] GetAllN()
        {
            List<Service.Print.PrintTemplate> templates = new List<Service.Print.PrintTemplate>();

            foreach (string file in Directory.GetFiles(DATA_DIR, "*" + FILE_EXTENSION))
            {
                string content = File.ReadAllText(file, Encoding.UTF8);
                var template = Newtonsoft.Json.JsonConvert.DeserializeObject<Service.Print.PrintTemplate>(content);
                templates.Add(template);
            }

            return templates.ToArray();
        }

        public static void InsertN(Service.Print.PrintTemplate deliveryTemplate)
        {
            string file = System.IO.Path.Combine(DATA_DIR, deliveryTemplate.Name + FILE_EXTENSION);
            using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                string content = Newtonsoft.Json.JsonConvert.SerializeObject(deliveryTemplate);
                var bytes = UTF8Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public static void UpdateN(Service.Print.PrintTemplate deliveryTemplate, string newName)
        {
            if (deliveryTemplate.Name.Equals(newName) == false)
            {
                var files = Directory.GetFiles(DATA_DIR, newName + FILE_EXTENSION);
                if (files.Count() > 0)
                {
                    throw new Exception("已有相同模板名称存在");
                }
            }
            DeleteN(deliveryTemplate.Name);
            deliveryTemplate.Name = newName;
            InsertN(deliveryTemplate);
        }

        public static void DeleteN(string name)
        {
            var files = Directory.GetFiles(DATA_DIR, name + FILE_EXTENSION);
            if (files.Length > 0)
            {
                File.Delete(files[0]);
            }
        }

    }
}