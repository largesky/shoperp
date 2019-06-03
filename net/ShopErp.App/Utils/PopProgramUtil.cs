using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using ShopErp.Domain;

namespace ShopErp.App.Utils
{
    class PopProgramUtil
    {
        public static void StartPopProgram(PopType popType, string popTalkId, string popBuyerId, string popOrderId)
        {
            if (popType == PopType.TAOBAO || popType == PopType.TMALL)
            {
                StartTaobaoProgram(popTalkId, popBuyerId, popOrderId);
            }
            else if (popType == PopType.PINGDUODUO)
            {
                StartPddPropgram(popTalkId, popBuyerId, popOrderId);
            }
            else
            {
                throw new Exception("暂时不支持的聊天平台");
            }
        }

        private static void StartTaobaoProgram(string popSellerId, string popBuyerId, string arg)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, Microsoft.Win32.RegistryView.Default);
            RegistryKey hTen = key.OpenSubKey("aliim");

            if (hTen == null)
            {
                throw new Exception("没有找到注册信息 HKEY_CLASSES_ROOT\\aliim");
            }

            string programPath = hTen.GetValue("URL Protocol", "").ToString();

            if (string.IsNullOrWhiteSpace(programPath))
            {
                var shellKey = hTen.OpenSubKey("Shell\\Open\\Command");
                programPath = shellKey.GetValue("", "").ToString();
            }
            if (string.IsNullOrWhiteSpace(programPath))
            {
                throw new Exception("未能在注册表中找到千牛或者旺旺启动程序");
            }
            if (programPath.Contains("%1"))
            {
                programPath = programPath.Substring(0, programPath.IndexOf("%1")).Trim();
            }
            Process.Start("\"" + programPath + "\"", string.Format("aliim:sendmsg?uid=cntaobao&touid=cntaobao{0}&siteid=cntaobao", popBuyerId));
        }

        private static void StartPddPropgram(string popSellerId, string popBuyerId, string orderId)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, Microsoft.Win32.RegistryView.Default);
            RegistryKey hTen = key.OpenSubKey("pddim");

            if (hTen == null)
            {
                throw new Exception("没有找到注册信息 HKEY_CLASSES_ROOT\\pddim");
            }

            var shellKey = hTen.OpenSubKey("Shell\\Open\\Command");
            string programPath = shellKey.GetValue("", "").ToString();
            if (string.IsNullOrWhiteSpace(programPath))
            {
                throw new Exception("未能在注册表中找到拼多多程序");
            }
            if (programPath.Contains("%1"))
            {
                programPath = programPath.Substring(0, programPath.IndexOf("%1")).Trim();
            }
            Process.Start("\"" + programPath + "\"", string.Format("pddim:sendmsg/?OpeId=open_order&mallcsid={0}&ordersn={1}", popSellerId, orderId));
        }

    }
}
