using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Excel
{
    public class ExcelFile
    {
        private Dictionary<string, string[][]> sheetDatas = new Dictionary<string, string[][]>();

        private Dictionary<string, ExcelColumn[]> columns = new Dictionary<string, ExcelColumn[]>();

        private string path = null;


        public ExcelFile()
        {

        }

        public ExcelFile(string path, string sheetName, ExcelColumn[] columns, string[][] contents)
        {
            this.path = path;
            this.sheetDatas.Add(sheetName, contents);
            this.columns.Add(sheetName, columns);
        }

        public ExcelFile(string path, Dictionary<string, string[][]> sheetDatas, Dictionary<string, ExcelColumn[]> columns)
        {
            this.path = path;
            this.sheetDatas = sheetDatas;
            this.columns = columns;
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


        public void WriteXlsx()
        {
            IWorkbook book = null;
            if (this.path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
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

                //输出列标题
                var row = sheet.CreateRow(0);

                for (int i = 0; i < columns[pair.Key].Length; i++)
                {
                    var cell = row.CreateCell(i, CellType.String);
                    cell.SetCellValue(columns[pair.Key][i].Name);
                }

                //数据
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    row = sheet.CreateRow(i + 1);
                    int maxNewLineCount = pair.Value[i].Select(obj => obj.Count(c => c == '\n')).Max();
                    row.HeightInPoints = 25 * (maxNewLineCount + 1);
                    for (int k = 0; k < pair.Value[i].Length; k++)
                    {
                        var cell = row.CreateCell(k, columns[pair.Key][k].IsNumber ? CellType.Numeric : CellType.String);
                        if (columns[pair.Key][k].IsNumber)
                        {
                            cell.SetCellValue(double.Parse(pair.Value[i][k]));
                        }
                        else
                        {
                            cell.SetCellValue(pair.Value[i][k]);
                        }
                    }
                }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    book.Write(fs);
                }
            }
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
            ExcelFile excelFile = new ExcelFile();
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
                    if (sheet == null || sheet.LastRowNum <= 0)
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
                    excelFile.sheetDatas.Add(sheet.SheetName, datas.ToArray());
                }
            }
            return excelFile;
        }
    }
}
