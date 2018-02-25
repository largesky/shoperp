using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShopErp.App.Utils
{
    public class CSV
    {
        static bool NextIsSp(int currentIndex, string content)
        {
            //没有下一个,则返回false
            if (currentIndex + 1 >= content.Length)
            {
                return false;
            }

            return IsSp(content[currentIndex + 1]);
        }

        static bool PreIsSp(int currentIndex, string content)
        {
            if (currentIndex - 1 < 0)
            {
                return false;
            }
            return IsSp(content[currentIndex - 1]);
        }


        static bool IsSp(char c)
        {
            return c == '"' || c == '“';
        }

        static bool IsSpilter(char c)
        {
            return c == ',' || c == '，';
        }

        public static int ReadLine(string content, int startIndex, List<String> strs)
        {
            if (startIndex >= content.Length - 1)
            {
                return startIndex;
            }
            int currentIndex = startIndex;
            StringBuilder sb = new StringBuilder(1024);
            bool isIn = false;
            while (currentIndex < content.Length)
            {
                char c = content[currentIndex];
                if (IsSp(c))
                {
                    //如果后一个也是",则当前的"是内容
                    if (NextIsSp(currentIndex, content))
                    {
                        sb.Append(c);
                        currentIndex++; //需要跳过下一个内容
                    }
                    else
                    {
                        //下一个不",则可能是开始,也可能是结束
                        //如果当前正一个字段中
                        if (isIn)
                        {
                            strs.Add(sb.ToString());
                            sb.Clear();
                            isIn = false;
                        }
                        else
                        {
                            //标记成正在处理字段中
                            isIn = true;
                            sb.Clear();
                        }
                    }
                }
                else if (IsSpilter(c))
                {
                    //该字符属于字段内容
                    if (isIn == false)
                    {
                        //向前搜索,如果在发现下一个,之前没有 ",则说明数据没有采用""包围,则也应该分段
                        int index = currentIndex - 1;
                        //向前搜索第一个不为空格的数据
                        while (index >= 0 && content[index] == ' ') index--;
                        if (index >= 0 && IsSp(content[index]))
                        {
                        }
                        else
                        {
                            strs.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                }
                else if (c == '\n')
                {
                    if (isIn == false)
                    {
                        return currentIndex + 1;
                    }
                    sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }

                currentIndex++;
            }
            return 0;
        }

        public static string[][] ReadFile(String filePath, Encoding encoding)
        {
            String content = File.ReadAllText(filePath, encoding);
            List<String[]> items = new List<string[]>();

            int startIndex = 0;
            List<string> tempItems = new List<string>();
            while (startIndex < content.Length)
            {
                tempItems.Clear();
                startIndex = ReadLine(content, startIndex, tempItems);
                if (tempItems.Count > 0)
                {
                    items.Add(tempItems.ToArray());
                }
            }
            return items.ToArray();
        }

        private static string FormatCSVContent(params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append("\"");
                if (values[i] == null)
                {
                    values[i] = "";
                }
                sb.Append(values[i].Replace("\"", "\"\"")); //双引号，需要转义
                sb.Append("\",");
            }
            sb.Length = sb.Length - 1; //支最后一个 , 号
            sb.AppendLine(); //增加换行
            return sb.ToString();
        }

        public static void WriteCSV(string filePath, Encoding encoding, params string[][] values)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            foreach (var v in values)
            {
                string s = FormatCSVContent(v);
                File.AppendAllText(filePath, s, encoding);
            }
        }
    }
}