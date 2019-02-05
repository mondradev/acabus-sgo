using Microsoft.Office.Interop.Excel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ACABUS_Control_de_operacion.Utils
{
    public class Excel
    {
        private static Application _appExcel;
        private static Workbooks _workbooks;

        private Workbook _thisWorkbook;
        private Worksheet _worksheetActive;

        private static void InitializeApp()
        {
            if (_appExcel == null)
                _appExcel = new Application();
            if (_appExcel != null && _workbooks == null)
                _workbooks = _appExcel.Workbooks;
        }

        public static Excel Open(String filename)
        {
            InitializeApp();
            return new Excel()
            {
                _thisWorkbook = _workbooks.Open(filename)
            };
        }

        public void Close(Boolean save = false)
        {
            if (_worksheetActive != null)
            {
                Marshal.ReleaseComObject(_worksheetActive);
                _worksheetActive = null;
            }
            if (_thisWorkbook != null)
            {
                _thisWorkbook.Close(save);
                Marshal.ReleaseComObject(_thisWorkbook);
                _thisWorkbook = null;
            }
        }

        public void Save()
        {
            if (_thisWorkbook != null)
                _thisWorkbook.Save();
        }

        public void SaveAs(String filename)
        {
            if (_thisWorkbook != null)
                _thisWorkbook.SaveAs(filename);
        }

        public static void Quit()
        {
            if (_workbooks != null)
            {
                Marshal.ReleaseComObject(_workbooks);
                _workbooks = null;
            }

            if (_appExcel != null)
            {
                _appExcel.Quit();
                Marshal.ReleaseComObject(_appExcel);
                _appExcel = null;
            }
        }

        public void SetWorksheetActive(String name)
        {
            if (_thisWorkbook != null)
            {
                if (_worksheetActive != null)
                    Marshal.ReleaseComObject(_worksheetActive);
                var worksheets = _thisWorkbook.Worksheets;
                foreach (Worksheet worksheet in worksheets)
                {
                    if (worksheet.Name.Equals(name))
                    {
                        _worksheetActive = worksheet;
                        break;
                    }
                    else
                        Marshal.ReleaseComObject(worksheet);
                }
                Marshal.ReleaseComObject(worksheets);
            }
        }

        public String GetValue(String cellName)
        {
            ConvertPosExcelToIndex(cellName, out int row, out int col);
            return GetValue(row, col);
        }

        public String GetValue(int rowIndex, int columnIndex)
        {
            if (_worksheetActive == null) return null;
            Object value = null;
            Range range = null;
            Range cell = null;
            try
            {
                range = _worksheetActive.UsedRange;
                cell = range.get_Item(rowIndex, columnIndex);
                value = cell.Value2;
            }
            catch (Exception) { }
            finally
            {
                if (cell != null)
                    Marshal.ReleaseComObject(cell);
                if (range != null)
                    Marshal.ReleaseComObject(range);
            }
            return value?.ToString();
        }

        public void SetValue(String value, String cellName)
        {
            ConvertPosExcelToIndex(cellName, out int row, out int col);
            SetValue(value, row, col);
        }

        public void SetValue(String value, int rowIndex, int columnIndex)
        {
            if (_worksheetActive == null) return;
            Range range = null;
            Range cell = null;
            try
            {
                range = _worksheetActive.UsedRange;

                cell = range.get_Item(rowIndex, columnIndex);
                              
                cell.Value = value;
            }
            catch (Exception ex) {
                Trace.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (cell != null)
                    Marshal.ReleaseComObject(cell);
                if (range != null)
                    Marshal.ReleaseComObject(range);
            }
        }

        private void ConvertPosExcelToIndex(String cellName, out int rowIndex, out int columnIndex)
        {
            String row = Regex.Match(cellName, "[0-9]{1,}").Value;
            String column = Regex.Match(cellName.ToUpper(), "[A-Z]{1,}").Value;
            Char[] chars = column.ToCharArray();
            columnIndex = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                columnIndex += (int)(Double.Parse((chars[i] - 64).ToString()) * Math.Pow(27, chars.Length - i - 1)) - 1;
            }
            rowIndex = Int32.Parse(row) - 1;
        }

    }
}
