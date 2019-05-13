using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Excel
{
    public class ExcelFile
    {
        private Dictionary<string, string[][]> sheetDatas = new Dictionary<string, string[][]>();


        /// <summary>
        /// 将数字列如 3 转换成 EXCEL 列 C
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <returns></returns>
        public static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            if (columnNumber < 1)
            {
                throw new ArgumentException("columnNumber 不能小于1");
            }

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        /// <summary>
        /// 将 EXCEL 列 C 转换成 数字列 3 
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <returns></returns>
        public static int GetExcelColumnIndex(string colName)
        {
            if (string.IsNullOrWhiteSpace(colName))
            {
                throw new ArgumentException("colName不能为空");
            }

            var colIndex = 0;
            for (int ind = 0, pow = colName.Count() - 1; ind < colName.Count(); ++ind, --pow)
            {
                var cVal = Convert.ToInt32(colName[ind]) - 64; //col A is index 1
                colIndex += cVal * ((int)Math.Pow(26, pow));
            }
            return colIndex;
        }

        /// <summary>
        /// 搜索指定列下标 
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="conent"></param>
        /// <param name="fullMatch"></param>
        /// <returns></returns>
        public static int GetIndex(string[] datas, string conent, bool fullMatch)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (fullMatch)
                {
                    if (datas[i].Equals(conent, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
                else
                {
                    if (datas[i].IndexOf(conent, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取某个表格的所有数据
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public string[][] ReadAllRows(string sheetName)
        {
            if (sheetDatas.ContainsKey(sheetName) == false)
            {
                throw new Exception("指定的表不存在");
            }
            return sheetDatas[sheetName];
        }

        /// <summary>
        /// 读取第一个表格的数据
        /// </summary>
        /// <returns></returns>
        public string[][] ReadFirstSheet()
        {
            return this.sheetDatas.First().Value;
        }


        /// <summary>
        /// 打开一个文件
        /// </summary>
        /// <param name="file">要打开的文件路径</param>
        /// <returns></returns>
        public static ExcelFile Open(string file, string sheetName = "Sheet1")
        {
            ExcelFile xlsxFileReader = new ExcelFile();
            IWorkbook book = null;

            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                if (file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    book = new NPOI.XSSF.UserModel.XSSFWorkbook(fs);
                }
                else if (file.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    book = new NPOI.HSSF.UserModel.HSSFWorkbook(fs);
                }
                else
                {
                    throw new Exception("无法识别的文件格式");
                }

                for (int i = 0; i < book.NumberOfSheets; i++)
                {
                    var sheet = book.GetSheetAt(i);
                    if (sheet == null)
                    {
                        continue;
                    }

                    //获取最大的列数
                    List<int> cols = new List<int>();
                    for (int j = sheet.FirstRowNum; j <= sheet.LastRowNum; j++)
                    {
                        var row = sheet.GetRow(j);
                        if (row == null)
                        {
                            continue;
                        }
                        cols.Add(row.LastCellNum);
                    }
                    int maxColumn = cols.Max();
                    List<string[]> datas = new List<string[]>();
                    for (int j = sheet.FirstRowNum; j <= sheet.LastRowNum; j++)
                    {
                        var row = sheet.GetRow(j);
                        if (row == null)
                        {
                            continue;
                        }
                        var data = new string[maxColumn + 1];
                        data[0] = j.ToString();
                        datas.Add(data);
                        for (int k = 0; k < row.LastCellNum; k++)
                        {
                            var cell = row.GetCell(k);
                            if (cell == null)
                            {
                                data[k + 1] = string.Empty;
                            }
                            else
                            {
                                switch (cell.CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.Blank:
                                        data[k + 1] = string.Empty;
                                        break;
                                    case NPOI.SS.UserModel.CellType.String:
                                        data[k + 1] = cell.StringCellValue;
                                        break;
                                    case NPOI.SS.UserModel.CellType.Boolean:
                                        data[k + 1] = cell.BooleanCellValue.ToString();
                                        break;
                                    case NPOI.SS.UserModel.CellType.Error:
                                        data[k + 1] = string.Empty;
                                        break;
                                    case NPOI.SS.UserModel.CellType.Formula:
                                        data[k + 1] = cell.CellFormula;
                                        break;
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        data[k + 1] = cell.NumericCellValue.ToString("F4");
                                        break;
                                    case NPOI.SS.UserModel.CellType.Unknown:
                                        data[k + 1] = cell.StringCellValue;
                                        break;
                                }
                            }
                        }
                    }
                    xlsxFileReader.sheetDatas.Add(sheet.SheetName, datas.ToArray());
                }
            }
            return xlsxFileReader;
        }

        public static void WriteXlsx(string file, string[][] contents)
        {
            IWorkbook book = null;
            if (file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                book = new NPOI.XSSF.UserModel.XSSFWorkbook();
            }
            else
            {
                book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            }

            ISheet sheet = book.CreateSheet("Sheet1");

            for (int i = 0; i < contents.Length; i++)
            {
                var row = sheet.CreateRow(i);
                for (int k = 0; k < contents[i].Length; k++)
                {
                    var cell = row.CreateCell(k, CellType.String);
                    cell.SetCellValue(contents[i][k]);
                }
            }
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                book.Write(fs);
            }
        }

        public static void WriteXlsx(string file, Dictionary<string, string[][]> sheetDatas)
        {
            IWorkbook book = null;
            if (file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                book = new NPOI.XSSF.UserModel.XSSFWorkbook();
            }
            else
            {
                book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            }
            foreach (var pair in sheetDatas)
            {
                ISheet sheet = book.CreateSheet(pair.Key);

                for (int i = 0; i < pair.Value.Length; i++)
                {
                    var row = sheet.CreateRow(i);
                    for (int k = 0; k < pair.Value[i].Length; k++)
                    {
                        var cell = row.CreateCell(k, CellType.String);
                        cell.SetCellValue(pair.Value[i][k]);
                    }
                }
                using (FileStream fs = new FileStream(file, FileMode.Create))
                {
                    book.Write(fs);
                }
            }
        }

    }
}
