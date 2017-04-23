using System;
using System.IO;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Text;

namespace Table2Sharp
{
    public class TableFile
    {
        public class Cell
        {
            public string value = string.Empty;
            public string annotate = string.Empty;
            public string author = string.Empty;

            public override string ToString()
            {
                return value;
            }

            public static implicit operator string(Cell cell)
            {
                return cell == null ? null : cell.value;
            }
        }
        public string FilePath { get; private set; }

        private DataTable _data = null;

        public Cell[] this[int row]
        {
            get
            {
                Cell[] arr = new Cell[_data.Columns.Count];
                for(int i = 0; i < _data.Columns.Count; i++)
                {
                    arr[i] = _data.Rows[row][i] as Cell;
                }
                return arr;
            }
        }

        public Cell this[int row, int col]
        {
            get 
            {
                return _data.Rows[row][col] as Cell;
            }
        }

        public int RowCount { get { return _data.Rows.Count; } }

        public int ColCount { get { return _data.Columns.Count; } }

        public int Count { get { return _data.Rows.Count * _data.Columns.Count; } }

        public static TableFile Create(string path)
        {
            TableFile file = new TableFile();
            if(file.ReadFromExcel(path))
                return file;
            return null;
        }

        private TableFile()
        {
        }

        private bool ReadFromExcel(string path)
        {
            FilePath = path;

            using(FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = null;
                // 2007版本
                if (path.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook(fs);
                // 2003版本
                else if (path.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook(fs);
                else
                    throw new Exception("Unknow file");

                if(workbook == null) 
                    throw new Exception("Null workbook");

                ISheet sheet = workbook.GetSheetAt(0);
                if(sheet == null) 
                    throw new Exception(string.Format("No data in file {0}", path));

                _data = new DataTable();

                for (int i = 0; i < sheet.GetRow(0).LastCellNum; i++)
                    _data.Columns.Add(Convert.ToChar(((int)'A') + i).ToString(), typeof(Cell));

                for (int i = 0; i < sheet.LastRowNum + 1; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if(row == null) continue;

                    DataRow dataRow =  _data.NewRow();
                    for(int j = 0; j < row.LastCellNum; j++)
                    {
                        ICell cell = row.GetCell(j);
                        Cell myCell = new Cell();
                        myCell.value = cell != null ? cell.ToString().Trim() : "";
                        if (cell != null && cell.CellComment != null)
                        {
                            myCell.author = cell.CellComment.Author;
                            myCell.annotate = cell.CellComment.String.ToString();
                        }
                        dataRow[j] = myCell;
                    }
                    _data.Rows.Add(dataRow);
                }
            }

            return true;
        }

        public bool SaveToTSV(string path)
        {
            StringBuilder result = new StringBuilder();
            for (int row = 0; row < RowCount; ++row)
            {
                for (int col = 0; col < ColCount; ++col)
                {
                    result.Append(this[row, col]);
                    if (col < ColCount - 1) result.Append("\t");
                }
                result.Append(Environment.NewLine);
            }
            return WriteFile(result.ToString(), path);
        }

        private bool WriteFile(string content, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            FileStream fs;
            try
            {
                fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(Encoding.UTF8.GetBytes(content));
            }
            fs.Close();
            return true;
        }
    }
}