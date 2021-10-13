using System;
using System.Collections.Generic;
using System.Text;

namespace RandomAccess
{
    public class RandomAccessFile //Need to make this public. 
    {
        public class Column
        {
            public string Name { get; internal set; }
            public int Width { get; internal set; }
            public bool IsKey { get; internal set; }
            public string Text { get; set; }
        }

        private Dictionary<string, Dictionary<string, Column>> _rows = new Dictionary<string,
            Dictionary<string, Column>> { };
        private readonly Dictionary<string, Column> _columns = new Dictionary<string, Column> { };
        public readonly string Path;

        public Dictionary<string, Column> Columns
        {
            get { return _columns; }
        }

        public Dictionary<string, Dictionary<string, Column>> Rows
        {
            get { return _rows; }
        }

        public bool AddColumn(string name, int width, bool isKey)
        {
            if (_columns.ContainsKey(name))
            {
                return false;
            }
            if (isKey)
            {
                foreach (var column in _columns.Values)
                {
                    if (column.IsKey)
                        throw new Exception("Only one column can be key column.");
                }
            }
            _columns.Add(name, new Column() { Name = name, Width = width, IsKey = isKey });
            return true;
        }

        private bool CreateFile(string path)
        {
            try
            {
                System.IO.File.Create(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public RandomAccessFile(string path)
        {
            var fileExists = System.IO.File.Exists(path);
            if (!fileExists)
            {
                fileExists = CreateFile(path);
            }
            if (fileExists)
            {
                Path = path;
            }
        }

        public bool ReadFile()
        {
            string keyColumnName = null;
            foreach(var col in _columns.Values)
            {
                if (col.IsKey)
                {
                    keyColumnName = col.Name;
                    break;
                }
            }
            if (_columns.Count < 1)
            {
                throw new Exception("Columns must be defined before reading the file.");
            }
            if(!String.IsNullOrWhiteSpace(keyColumnName)&& !_columns.ContainsKey(keyColumnName))
            {
                throw new Exception($"Column '{keyColumnName}' does not exist.");
            }
            _rows = new Dictionary<string, Dictionary<string, Column>> { };
            using System.IO.StreamReader f = new System.IO.StreamReader(Path);
            var rowNumber = 0;
            while (!f.EndOfStream)
            {
                var input = f.ReadLine();
                var start = 0;
                var rowValues = new Dictionary<string, Column> { };
                foreach (var col in Columns.Values)
                {
                    var end = start + col.Width + 1;
                    if (end >= input.Length - 1)
                        end = input.Length;
                    rowValues.Add(col.Name, new Column() { Name = col.Name, Width = col.Width, IsKey = col.IsKey, Text = input[start..end].Trim() });
                    start = end + 1;
                }

                var key = String.IsNullOrWhiteSpace(keyColumnName) ? rowNumber.ToString() : rowValues[keyColumnName].Text;
                _rows.Add(key, rowValues);
                rowNumber++;

            }
            return true;
        }

        // UPDATE or INSERT

        public bool UpsertRow(string key, string columnName, string columnText)
        {
            if (_columns.ContainsKey(columnText))
            {
                var column = new Column() { Name = columnName, Width = _columns[columnName].Width, Text = columnText };
                if (_rows.ContainsKey(key))
                {
                    if (_rows[key].ContainsKey(columnName))
                    {
                        _rows[key][columnName].Text = columnText;
                    }
                    else
                    {
                        _rows[key].Add(columnName, column);
                    }
                }
                else
                {
                    var col = new Dictionary<string, Column> { { columnName, column } };
                    _rows.Add(key, col);
                }
                return true;
            }
            return false;
        }

        public bool DeleteRow(string key)
        {
            try
            {
                _rows.Remove(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, Dictionary<string, Column>> FindRows(string columnName, object searchValue)
        {
            var returnValue = new Dictionary<string, Dictionary<string, Column>>{ };
            foreach(var row in _rows)
            {
                if(row.Value.ContainsKey(columnName) && row.Value[columnName].Text.Contains(searchValue.ToString()))
                {
                    returnValue.Add(row.Key, row.Value);
                }
            }
            return returnValue;
        }

        public bool SaveFile()
        {
            using System.IO.StreamWriter f = new System.IO.StreamWriter(Path, false);
            foreach(var row in _rows)
            {
                var record = row.Value;
                var output = "";
                foreach(var outputColumn in _columns)
                {
                    var fieldText = "";
                    var fieldWidth = outputColumn.Value.Width;
                    if (outputColumn.Value.IsKey)
                        fieldText = row.Key;
                    else if (record.ContainsKey(outputColumn.Key))
                        fieldText = record[outputColumn.Key].Text;
                    else if (outputColumn.Value.IsKey)
                        output += outputColumn.Key.PadRight(outputColumn.Value.Width);

                    if (fieldText.Length > fieldWidth)
                        output += fieldText.Substring(0, fieldWidth);
                    else
                        output += fieldText.PadRight(fieldWidth);
                }
                f.WriteLine(output);
            }
            f.Close();
            return true;
        }


    }
}
