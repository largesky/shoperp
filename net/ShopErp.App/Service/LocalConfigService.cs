using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ShopErp.App.Log;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Service
{
    [Serializable]
    public class LocalConfigService
    {
        private static readonly string CONFIG_PATH = System.IO.Path.Combine(EnvironmentDirHelper.DIR_CONFIG, "Config.xml");
        private const string DEFAULT_CONFIG_CONTENT = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><Config></Config>";
        private static System.Xml.Linq.XDocument xDoc = null;


        static LocalConfigService()
        {
            if (xDoc != null)
            {
                return;
            }
            if (System.IO.File.Exists(CONFIG_PATH) == false)
            {
                try
                {
                    System.IO.File.WriteAllText(CONFIG_PATH, DEFAULT_CONFIG_CONTENT);
                }
                catch (Exception ex)
                {
                    Logger.Log("生成默认配置文件出错", ex);
                    throw new Exception("生成默认配置文件出错");
                }
            }

            xDoc = XDocument.Load(CONFIG_PATH);

            if (xDoc.Root.Element(SystemNames.CONFIG_WEB_IMAGE_DIR) == null)
            {
                UpdateValue(SystemNames.CONFIG_WEB_IMAGE_DIR, @"\\host-bjc\images");
            }
        }

        private static XElement GetXElement(string name)
        {
            try
            {
                return xDoc.Root.Element(name);
            }
            catch
            {
                return null;
            }
        }


        public static bool ContainsValue(string name)
        {
            return xDoc.Element(name) != null;
        }

        public static string GetValue(String name, string value = null)
        {
            XElement xe = GetXElement(name);
            if (xe == null && value == null)
            {
                throw new Exception("配置信息不存在:" + name);
            }

            if (xe != null)
                return xe.Value.Trim();
            return value.Trim();
        }

        public static void UpdateValue(string name, string value)
        {
            XElement xe = GetXElement(name);
            if (xe == null)
            {
                xe = new XElement(name, value.Trim());
                xDoc.Root.Add(xe);
            }
            xe.Value = value.Trim();
            xDoc.Save(CONFIG_PATH);
        }

        public static int GetValueInt(string name, int? value = 0)
        {
            XElement xe = GetXElement(name);
            if (xe == null && value == null)
            {
                throw new Exception("配置信息不存在:" + name);
            }

            if (xe != null)
                return int.Parse(xe.Value.Trim());
            return value.Value;
        }

        public static void UpdateValueInt(string name, int value)
        {
            XElement xe = GetXElement(name);
            if (xe == null)
            {
                xe = new XElement(name, value);
                xDoc.Root.Add(xe);
            }
            else
            {
                xe.Value = value.ToString();
            }
            xDoc.Save(CONFIG_PATH);
        }


        public static double GetValueDouble(string name, double? value = 0)
        {
            XElement xe = GetXElement(name);
            if (xe == null && value == null)
            {
                throw new Exception("配置信息不存在:" + name);
            }

            if (xe != null)
                return double.Parse(xe.Value.Trim());
            return value.Value;
        }

        public static void UpdateValueDouble(string name, double value)
        {
            XElement xe = GetXElement(name);
            if (xe == null)
            {
                xe = new XElement(name, value.ToString("F4"));
                xDoc.Root.Add(xe);
            }
            else
            {
                xe.Value = value.ToString("F4");
            }
            xDoc.Save(CONFIG_PATH);
        }

        public static DateTime GetValueDateTime(string name, DateTime? value)
        {
            XElement xe = GetXElement(name);
            if (xe == null && value == null)
            {
                throw new Exception("配置信息不存在:" + name);
            }

            if (xe != null)
                return DateTime.Parse(xe.Value.Trim());
            return value.Value;
        }


        public static void UpdateValueDateTime(string name, DateTime value)
        {
            XElement xe = GetXElement(name);
            if (xe == null)
            {
                xe = new XElement(name, value.ToString("yyyy-MM-dd HH:mm:ss"));
                xDoc.Root.Add(xe);
            }
            else
            {
                xe.Value = value.ToString("yyyy-MM-dd HH:mm:ss");
            }
            xDoc.Save(CONFIG_PATH);
        }

        public static T GetValueEnum<T>(String name, T? value = null) where T : struct
        {
            XElement xe = GetXElement(name);
            if (xe == null && value == null)
            {
                throw new Exception("配置信息不存在:" + name);
            }
            if (xe != null)
                return (T)(Enum.Parse(typeof(T), xe.Value));
            return value.Value;
        }

        public static void UpdateValueEnum(string name, Enum value)
        {
            XElement xe = GetXElement(name);
            if (xe == null)
            {
                xe = new XElement(name, value);
                xDoc.Root.Add(xe);
            }
            else
            {
                xe.Value = value.ToString();
            }
            xDoc.Save(CONFIG_PATH);
        }
    }
}
