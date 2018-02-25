using System;
using NHibernate.Cfg;
using ShopErp.Server.Log;
using ShopErp.Server.Utils;

namespace ShopErp.Server.Dao.NHibernateDao
{
    class NHibernateHelper
    {
        static NHibernate.ISessionFactory factory;

        public static NHibernate.ISession OpenSession()
        {
            if (factory == null)
            {
                try
                {
                    var config = new Configuration();
                    //config.Configure(@"D:\workspace\shoperp\DataConvert\bin\Debug\hibernate1.cfg.xml");
                    string file = EnvironmentDirHelper.PROGRAM_DIR + "\\" + @"hibernate.cfg.xml";
                    Console.WriteLine("正在使用数据库连接配置文件：" + file);
                    config.Configure(file);
                    config.AddAssembly(typeof(NHibernateHelper).Assembly);
                    config.BuildMappings();
                    factory = config.BuildSessionFactory();
                }
                catch (Exception ex)
                {
                    Logger.Log("初始货NHibernate失败", ex);
                    throw new Exception("初始化数据库连接失败", ex);
                }
            }

            return factory.OpenSession();
        }
    }
}
