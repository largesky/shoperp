namespace ShopErp.App.Utils
{
    public class EnvironmentDirHelper
    {
        public static readonly string PROGRAM_DIR =new System.IO.FileInfo(typeof(EnvironmentDirHelper).Assembly.Location).DirectoryName;
        public static readonly string DIR_LOG = System.IO.Path.Combine(PROGRAM_DIR, "Log");
        public static readonly string DIR_CONFIG = System.IO.Path.Combine(PROGRAM_DIR, "Config");
        public static readonly string DIR_DATA = System.IO.Path.Combine(PROGRAM_DIR, "Data");

        /// <summary>
        /// 静态构造函数，创建相应目录
        /// </summary>
        static EnvironmentDirHelper()
        {
            if (System.IO.Directory.Exists(DIR_DATA) == false)
            {
                System.IO.Directory.CreateDirectory(DIR_DATA);
            }

            if (System.IO.Directory.Exists(DIR_LOG) == false)
            {
                System.IO.Directory.CreateDirectory(DIR_LOG);
            }

            if (System.IO.Directory.Exists(DIR_CONFIG) == false)
            {
                System.IO.Directory.CreateDirectory(DIR_CONFIG);
            }
        }
    }
}