/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Data;
#endif
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Specialized;

namespace AutomationTool.Converter
{
    internal sealed class JSONSerializer
    {
        #region Properties
        private StringBuilder _output = new StringBuilder();
        //private StringBuilder _before = new StringBuilder();
        private int _before;
        private int _MAX_DEPTH = 20;
        int _current_depth = 0;
        private Dictionary<string, int> _globalTypes = new Dictionary<string, int>();
        private Dictionary<object, int> _cirobj = new Dictionary<object, int>();
        private JSONParameters _params;
        private bool _useEscapedUnicode = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Parameterized constructor to initiate JSON parameters
        /// </summary>
        /// <param name="param">JSON Parameters</param>
        internal JSONSerializer(JSONParameters param)
        {
            _params = param;
            _useEscapedUnicode = _params.UseEscapedUnicode;
            _MAX_DEPTH = _params.SerializerMaxDepth;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts the object to JSON
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>JSON String</returns>
        internal string ConvertToJSON(object obj)
        {
            try
            {
                WriteValue(obj);

                if (_params.UsingGlobalTypes && _globalTypes != null && _globalTypes.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.Append("\"$types\":{");
                    var pendingSeparator = false;
                    foreach (var kv in _globalTypes)
                    {
                        if (pendingSeparator) sb.Append(',');
                        pendingSeparator = true;
                        sb.Append('\"');
                        sb.Append(kv.Key);
                        sb.Append("\":\"");
                        sb.Append(kv.Value);
                        sb.Append('\"');
                    }
                    sb.Append("},");
                    _output.Insert(_before, sb.ToString());
                }
                return _output.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the values from object
        /// </summary>
        /// <param name="obj">Object</param>
        private void WriteValue(object obj)
        {
            try
            {
                if (obj == null || obj is DBNull)
                    _output.Append("null");

                else if (obj is string || obj is char)
                    WriteString(obj.ToString());

                else if (obj is Guid)
                    WriteGuid((Guid)obj);

                else if (obj is bool)
                    _output.Append(((bool)obj) ? "true" : "false"); // conform to standard

                else if (
                    obj is int || obj is long ||
                    obj is decimal ||
                    obj is byte || obj is short ||
                    obj is sbyte || obj is ushort ||
                    obj is uint || obj is ulong
                )
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));

                else if (obj is double || obj is Double)
                {
                    double d = (double)obj;
                    if (double.IsNaN(d))
                        _output.Append("\"NaN\"");
                    else if (double.IsInfinity(d))
                    {
                        _output.Append("\"");
                        _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                        _output.Append("\"");
                    }
                    else
                        _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (obj is float || obj is Single)
                {
                    float d = (float)obj;
                    if (float.IsNaN(d))
                        _output.Append("\"NaN\"");
                    else if (float.IsInfinity(d))
                    {
                        _output.Append("\"");
                        _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                        _output.Append("\"");
                    }
                    else
                        _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                }

                else if (obj is DateTime)
                    WriteDateTime((DateTime)obj);

                else if (obj is DateTimeOffset)
                    WriteDateTimeOffset((DateTimeOffset)obj);

                else if (obj is TimeSpan)
                    _output.Append(((TimeSpan)obj).Ticks);

#if net4
            else if (_params.KVStyleStringDictionary == false &&
                obj is IEnumerable<KeyValuePair<string, object>>)

                WriteStringDictionary((IEnumerable<KeyValuePair<string, object>>)obj);
#endif

                else if (_params.KVStyleStringDictionary == false && obj is IDictionary &&
                    obj.GetType().IsGenericType && obj.GetType().GetGenericArguments()[0] == typeof(string))

                    WriteStringDictionary((IDictionary)obj);
                else if (obj is IDictionary)
                    WriteDictionary((IDictionary)obj);
#if !SILVERLIGHT
                else if (obj is DataSet)
                    WriteDataset((DataSet)obj);

                else if (obj is DataTable)
                    this.WriteDataTable((DataTable)obj);
#endif
                else if (obj is byte[])
                    WriteBytes((byte[])obj);

                else if (obj is StringDictionary)
                    WriteSD((StringDictionary)obj);

                else if (obj is NameValueCollection)
                    WriteNV((NameValueCollection)obj);

                else if (obj is IEnumerable)
                    WriteArray((IEnumerable)obj);

                else if (obj is Enum)
                    WriteEnum((Enum)obj);

                else if (Reflection.Instance.IsTypeRegistered(obj.GetType()))
                    WriteCustom(obj);

                else
                    WriteObject(obj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the Datetimeoffset as provided
        /// </summary>
        /// <param name="d">Date and Time Offset</param>
        private void WriteDateTimeOffset(DateTimeOffset d)
        {
            try
            {
                DateTime dt = _params.UseUTCDateTime ? d.UtcDateTime : d.DateTime;

                write_date_value(dt);

                var ticks = dt.Ticks % TimeSpan.TicksPerSecond;
                _output.Append('.');
                _output.Append(ticks.ToString("0000000", NumberFormatInfo.InvariantInfo));

                if (_params.UseUTCDateTime)
                    _output.Append('Z');
                else
                {
                    if (d.Offset.Hours > 0)
                        _output.Append("+");
                    else
                        _output.Append("-");
                    _output.Append(d.Offset.Hours.ToString("00", NumberFormatInfo.InvariantInfo));
                    _output.Append(":");
                    _output.Append(d.Offset.Minutes.ToString("00", NumberFormatInfo.InvariantInfo));
                }

                _output.Append('\"');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the named values from collection provided
        /// </summary>
        /// <param name="nameValueCollection">Name Value Collection</param>
        private void WriteNV(NameValueCollection nameValueCollection)
        {
            try
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (string key in nameValueCollection)
                {
                    if (_params.SerializeNullValues == false && (nameValueCollection[key] == null))
                    {
                    }
                    else
                    {
                        if (pendingSeparator) _output.Append(',');
                        if (_params.SerializeToLowerCaseNames)
                            WritePair(key.ToLower(), nameValueCollection[key]);
                        else
                            WritePair(key, nameValueCollection[key]);
                        pendingSeparator = true;
                    }
                }
                _output.Append('}');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the 'String Dictionary' values from dictionary provided
        /// </summary>
        /// <param name="stringDictionary">String Dictionary</param>
        private void WriteSD(StringDictionary stringDictionary)
        {
            try
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in stringDictionary)
                {
                    if (_params.SerializeNullValues == false && (entry.Value == null))
                    {
                    }
                    else
                    {
                        if (pendingSeparator) _output.Append(',');

                        string k = (string)entry.Key;
                        if (_params.SerializeToLowerCaseNames)
                            WritePair(k.ToLower(), entry.Value);
                        else
                            WritePair(k, entry.Value);
                        pendingSeparator = true;
                    }
                }
                _output.Append('}');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the custom values from object
        /// </summary>
        /// <param name="obj">object</param>
        private void WriteCustom(object obj)
        {
            try
            {
                Serialize s;
                Reflection.Instance._customSerializer.TryGetValue(obj.GetType(), out s);
                WriteStringFast(s(obj));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the ENUM
        /// </summary>
        /// <param name="e">the enum to be written</param>
        private void WriteEnum(Enum e)
        {
            try
            {
                // FEATURE : optimize enum write
                if (_params.UseValuesOfEnums)
                    WriteValue(Convert.ToInt32(e));
                else
                    WriteStringFast(e.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the GUID provided
        /// </summary>
        /// <param name="g">GUID</param>
        private void WriteGuid(Guid g)
        {
            try
            {
                if (_params.UseFastGuid == false)
                    WriteStringFast(g.ToString());
                else
                    WriteBytes(g.ToByteArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the bytes from byte array
        /// </summary>
        /// <param name="bytes">byte array</param>
        private void WriteBytes(byte[] bytes)
        {
            try
            {
#if !SILVERLIGHT
                WriteStringFast(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
#else
            WriteStringFast(Convert.ToBase64String(bytes, 0, bytes.Length));
#endif
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void WriteDateTime(DateTime dateTime)
        {
            try
            {
                // datetime format standard : yyyy-MM-dd HH:mm:ss
                DateTime dt = dateTime;
                if (_params.UseUTCDateTime)
                    dt = dateTime.ToUniversalTime();

                write_date_value(dt);

                if (_params.DateTimeMilliseconds)
                {
                    _output.Append('.');
                    _output.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));
                }

                if (_params.UseUTCDateTime)
                    _output.Append('Z');

                _output.Append('\"');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the Date value
        /// </summary>
        /// <param name="dt">Date</param>
        private void write_date_value(DateTime dt)
        {
            try
            {
                _output.Append('\"');
                _output.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
                _output.Append('-');
                _output.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append('-');
                _output.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append('T'); // strict ISO date compliance 
                _output.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the table schema from table
        /// </summary>
        /// <param name="ds">The table</param>
        /// <returns>DataSet Schema</returns>
        private DatasetSchema GetSchema(DataTable ds)
        {
            try
            {
                if (ds == null) return null;

                DatasetSchema m = new DatasetSchema();
                m.Info = new List<string>();
                m.Name = ds.TableName;

                foreach (DataColumn c in ds.Columns)
                {
                    m.Info.Add(ds.TableName);
                    m.Info.Add(c.ColumnName);
                    m.Info.Add(c.DataType.ToString());
                }
                // FEATURE : serialize relations and constraints here

                return m;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the dataset schema from table
        /// </summary>
        /// <param name="ds">The dataset</param>
        /// <returns>DataSet Schema</returns>
        private DatasetSchema GetSchema(DataSet ds)
        {
            try
            {
                if (ds == null) return null;

                DatasetSchema m = new DatasetSchema();
                m.Info = new List<string>();
                m.Name = ds.DataSetName;

                foreach (DataTable t in ds.Tables)
                {
                    foreach (DataColumn c in t.Columns)
                    {
                        m.Info.Add(t.TableName);
                        m.Info.Add(c.ColumnName);
                        m.Info.Add(c.DataType.ToString());
                    }
                }
                // FEATURE : serialize relations and constraints here

                return m;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the table schema from table in XML format
        /// </summary>
        /// <param name="ds">The table</param>
        /// <returns>schema in XML format</returns>
        private string GetXmlSchema(DataTable dt)
        {
            try
            {
                using (var writer = new StringWriter())
                {
                    dt.WriteXmlSchema(writer);
                    return dt.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the dataset
        /// </summary>
        /// <param name="ds">DataSet</param>
        private void WriteDataset(DataSet ds)
        {
            try
            {
                _output.Append('{');
                if (_params.UseExtensions)
                {
                    WritePair("$schema", _params.UseOptimizedDatasetSchema ? (object)GetSchema(ds) : ds.GetXmlSchema());
                    _output.Append(',');
                }
                bool tablesep = false;
                foreach (DataTable table in ds.Tables)
                {
                    if (tablesep) _output.Append(',');
                    tablesep = true;
                    WriteDataTableData(table);
                }
                // end dataset
                _output.Append('}');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the table with values
        /// </summary>
        /// <param name="table">the table to be written</param>
        private void WriteDataTableData(DataTable table)
        {
            try
            {
                _output.Append('\"');
                _output.Append(table.TableName);
                _output.Append("\":[");
                DataColumnCollection cols = table.Columns;
                bool rowseparator = false;
                foreach (DataRow row in table.Rows)
                {
                    if (rowseparator) _output.Append(',');
                    rowseparator = true;
                    _output.Append('[');

                    bool pendingSeperator = false;
                    foreach (DataColumn column in cols)
                    {
                        if (pendingSeperator) _output.Append(',');
                        WriteValue(row[column]);
                        pendingSeperator = true;
                    }
                    _output.Append(']');
                }

                _output.Append(']');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the Data Table
        /// </summary>
        /// <param name="dt">Table</param>
        void WriteDataTable(DataTable dt)
        {
            try
            {
                this._output.Append('{');
                if (_params.UseExtensions)
                {
                    this.WritePair("$schema", _params.UseOptimizedDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
                    this._output.Append(',');
                }

                WriteDataTableData(dt);

                // end datatable
                this._output.Append('}');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
#endif

        bool _TypesWritten = false;
        /// <summary>
        /// Writes the object
        /// </summary>
        /// <param name="obj">the object</param>
        private void WriteObject(object obj)
        {
            try
            {
                int i = 0;
                if (_cirobj.TryGetValue(obj, out i) == false)
                    _cirobj.Add(obj, _cirobj.Count + 1);
                else
                {
                    if (_current_depth > 0 && _params.InlineCircularReferences == false)
                    {
                        //_circular = true;
                        _output.Append("{\"$i\":");
                        _output.Append(i.ToString());
                        _output.Append("}");
                        return;
                    }
                }
                if (_params.UsingGlobalTypes == false)
                    _output.Append('{');
                else
                {
                    if (_TypesWritten == false)
                    {
                        _output.Append('{');
                        _before = _output.Length;
                        //_output = new StringBuilder();
                    }
                    else
                        _output.Append('{');
                }
                _TypesWritten = true;
                _current_depth++;
                if (_current_depth > _MAX_DEPTH)
                    throw new Exception("Serializer encountered maximum depth of " + _MAX_DEPTH);


                Dictionary<string, string> map = new Dictionary<string, string>();
                Type t = obj.GetType();
                bool append = false;
                if (_params.UseExtensions)
                {
                    if (_params.UsingGlobalTypes == false)
                        WritePairFast("$type", Reflection.Instance.GetTypeAssemblyName(t));
                    else
                    {
                        int dt = 0;
                        string ct = Reflection.Instance.GetTypeAssemblyName(t);
                        if (_globalTypes.TryGetValue(ct, out dt) == false)
                        {
                            dt = _globalTypes.Count + 1;
                            _globalTypes.Add(ct, dt);
                        }
                        WritePairFast("$type", dt.ToString());
                    }
                    append = true;
                }

                Getters[] g = Reflection.Instance.GetGetters(t, _params.ShowReadOnlyProperties, _params.IgnoreAttributes);
                int c = g.Length;
                for (int ii = 0; ii < c; ii++)
                {
                    var p = g[ii];
                    object o = p.Getter(obj);
                    if (_params.SerializeNullValues == false && (o == null || o is DBNull))
                    {
                        //append = false;
                    }
                    else
                    {
                        if (append)
                            _output.Append(',');
                        if (p.memberName != null)
                            WritePair(p.memberName, o);
                        else if (_params.SerializeToLowerCaseNames)
                            WritePair(p.lcName, o);
                        else
                            WritePair(p.Name, o);
                        if (o != null && _params.UseExtensions)
                        {
                            Type tt = o.GetType();
                            if (tt == typeof(System.Object))
                                map.Add(p.Name, tt.ToString());
                        }
                        append = true;
                    }
                }
                if (map.Count > 0 && _params.UseExtensions)
                {
                    _output.Append(",\"$map\":");
                    WriteStringDictionary(map);
                }
                _output.Append('}');
                _current_depth--;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the JSON pairs using fast method
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        private void WritePairFast(string name, string value)
        {
            try
            {
                WriteStringFast(name);

                _output.Append(':');

                WriteStringFast(value);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes the JSON pairs
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        private void WritePair(string name, object value)
        {
            try
            {
                WriteString(name);

                _output.Append(':');

                WriteValue(value);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes array
        /// </summary>
        /// <param name="array">IEnumerable array</param>
        private void WriteArray(IEnumerable array)
        {
            try
            {
                _output.Append('[');

                bool pendingSeperator = false;

                foreach (object obj in array)
                {
                    if (pendingSeperator) _output.Append(',');

                    WriteValue(obj);

                    pendingSeperator = true;
                }
                _output.Append(']');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes dictionary in String
        /// </summary>
        /// <param name="dic">dictionary object</param>
        private void WriteStringDictionary(IDictionary dic)
        {
            try
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in dic)
                {
                    if (_params.SerializeNullValues == false && (entry.Value == null))
                    {
                    }
                    else
                    {
                        if (pendingSeparator) _output.Append(',');

                        string k = (string)entry.Key;
                        if (_params.SerializeToLowerCaseNames)
                            WritePair(k.ToLower(), entry.Value);
                        else
                            WritePair(k, entry.Value);
                        pendingSeparator = true;
                    }
                }
                _output.Append('}');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes dictionary in String
        /// </summary>
        /// <param name="dic">KeyValuePair object</param>
        private void WriteStringDictionary(IEnumerable<KeyValuePair<string, object>> dic)
        {
            try
            {
                _output.Append('{');
                bool pendingSeparator = false;
                foreach (KeyValuePair<string, object> entry in dic)
                {
                    if (_params.SerializeNullValues == false && (entry.Value == null))
                    {
                    }
                    else
                    {
                        if (pendingSeparator) _output.Append(',');
                        string k = entry.Key;

                        if (_params.SerializeToLowerCaseNames)
                            WritePair(k.ToLower(), entry.Value);
                        else
                            WritePair(k, entry.Value);
                        pendingSeparator = true;
                    }
                }
                _output.Append('}');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes dictionary
        /// </summary>
        /// <param name="dic">dictionary object</param>
        private void WriteDictionary(IDictionary dic)
        {
            try
            {
                _output.Append('[');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in dic)
                {
                    if (pendingSeparator) _output.Append(',');
                    _output.Append('{');
                    WritePair("k", entry.Key);
                    _output.Append(",");
                    WritePair("v", entry.Value);
                    _output.Append('}');

                    pendingSeparator = true;
                }
                _output.Append(']');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes string value very fast.
        /// </summary>
        /// <param name="s">String Value</param>
        private void WriteStringFast(string s)
        {
            try
            {
                _output.Append('\"');
                _output.Append(s);
                _output.Append('\"');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Writes string value.
        /// </summary>
        /// <param name="s">String Value</param>
        private void WriteString(string s)
        {
            try
            {

                _output.Append('\"');

                int runIndex = -1;
                int l = s.Length;
                for (var index = 0; index < l; ++index)
                {
                    var c = s[index];

                    if (_useEscapedUnicode)
                    {
                        if (c >= ' ' && c < 128 && c != '\"' && c != '\\')
                        {
                            if (runIndex == -1)
                                runIndex = index;

                            continue;
                        }
                    }
                    else
                    {
                        if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\' && c != '\0')// && c != ':' && c!=',')
                        {
                            if (runIndex == -1)
                                runIndex = index;

                            continue;
                        }
                    }

                    if (runIndex != -1)
                    {
                        _output.Append(s, runIndex, index - runIndex);
                        runIndex = -1;
                    }

                    switch (c)
                    {
                        case '\t': _output.Append("\\t"); break;
                        case '\r': _output.Append("\\r"); break;
                        case '\n': _output.Append("\\n"); break;
                        case '"':
                        case '\\': _output.Append('\\'); _output.Append(c); break;
                        case '\0': _output.Append("\\u0000"); break;
                        default:
                            if (_useEscapedUnicode)
                            {
                                _output.Append("\\u");
                                _output.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
                            }
                            else
                                _output.Append(c);

                            break;
                    }
                }

                if (runIndex != -1)
                    _output.Append(s, runIndex, s.Length - runIndex);

                _output.Append('\"');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
    }
}
