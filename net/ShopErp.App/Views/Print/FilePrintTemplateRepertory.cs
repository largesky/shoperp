using ShopErp.App.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ShopErp.App.Utils;

namespace ShopErp.App.Views.Print
{
    public class FilePrintTemplateRepertory
    {

        private const string FILE_EXTENSION = ".pt";

        private static readonly string DATA_DIR = EnvironmentDirHelper.DIR_DATA + @"\PrintTemplate";

        static FilePrintTemplateRepertory()
        {
            System.IO.Directory.CreateDirectory(DATA_DIR);
        }

        //public static PrintTemplate[] GetAll()
        //{
        //    List<PrintTemplate> templates = new List<PrintTemplate>();

        //    foreach (string file in Directory.GetFiles(DATA_DIR, "*" + FILE_EXTENSION))
        //    {
        //        using (Stream s = new FileStream(file, FileMode.Open, FileAccess.Read))
        //        {
        //            BinaryFormatter bf = new BinaryFormatter();
        //            var template = bf.Deserialize(s) as PrintTemplate;
        //            if (template != null)
        //                templates.Add(template);
        //        }
        //    }

        //    return templates.ToArray();
        //}

        //public static void Insert(PrintTemplate deliveryTemplate)
        //{
        //    string file = System.IO.Path.Combine(DATA_DIR, deliveryTemplate.Name + FILE_EXTENSION);
        //    using (Stream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
        //    {
        //        BinaryFormatter bf = new BinaryFormatter();
        //        bf.Serialize(stream, deliveryTemplate);
        //    }
        //}

        //public static void Update(PrintTemplate deliveryTemplate, string newName)
        //{
        //    if (deliveryTemplate.Name.Equals(newName) == false)
        //    {
        //        var files = Directory.GetFiles(DATA_DIR, newName + FILE_EXTENSION);
        //        if (files.Count() > 0)
        //        {
        //            throw new Exception("已有相同模板名称存在");
        //        }
        //    }
        //    Delete(deliveryTemplate.Name);
        //    deliveryTemplate.Name = newName;
        //    Insert(deliveryTemplate);
        //}

        //public static void Delete(string name)
        //{
        //    var files = Directory.GetFiles(DATA_DIR, name + FILE_EXTENSION);
        //    if (files.Length > 0)
        //    {
        //        File.Delete(files[0]);
        //    }
        //}

        public static PrintTemplate[] GetAllN()
        {
            List<PrintTemplate> templates = new List<PrintTemplate>();

            foreach (string file in Directory.GetFiles(DATA_DIR, "*" + FILE_EXTENSION))
            {
                using (Stream s = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    var template = bf.Deserialize(s) as PrintTemplate;
                    templates.Add(template);
                }
            }

            return templates.ToArray();
        }

        public static void InsertN(PrintTemplate deliveryTemplate)
        {
            string file = System.IO.Path.Combine(DATA_DIR, deliveryTemplate.Name + FILE_EXTENSION);
            using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, deliveryTemplate);
            }
        }

        public static void UpdateN(PrintTemplate deliveryTemplate, string newName)
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

        public static void InsertNew(PrintTemplate deliveryTemplate)
        {
            string file = System.IO.Path.Combine(DATA_DIR, deliveryTemplate.Name + FILE_EXTENSION + "n");
            using (Stream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, deliveryTemplate);
            }
        }
    }
}