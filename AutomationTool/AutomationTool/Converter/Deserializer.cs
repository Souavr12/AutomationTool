/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTool.Converter
{
    internal class Deserializer
    {
        /// <summary>
        /// Parameterized constructors
        /// </summary>
        /// <param name="param">JSON Parameters</param>
        public Deserializer(JSONParameters param)
        {
            _params = param;
        }

        private JSONParameters _params;
        private bool _usingglobals = false;
        private Dictionary<object, int> _circobj = new Dictionary<object, int>();
        private Dictionary<int, object> _cirrev = new Dictionary<int, object>();

        /// <summary>
        /// JSON to object
        /// </summary>
        /// <typeparam name="T">Type parameter</typeparam>
        /// <param name="json">json string</param>
        /// <returns>Type t</returns>
        public T ToObject<T>(string json)
        {
            try
            {
                Type t = typeof(T);
                var o = ToObject(json, t);

                if (t.IsArray)
                {
                    if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
                    {
                        Type tt = t.GetElementType();
                        object oo = Array.CreateInstance(tt, 0);
                        return (T)oo;
                    }
                    else
                        return (T)o;
                }
                else
                    return (T)o;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// JSON to object
        /// </summary>
        /// <param name="json">json string</param>
        /// <returns>object</returns>
        public object ToObject(string json)
        {
            try
            {
                return ToObject(json, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// JSON to object
        /// </summary>
        /// <param name="json">json string</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        public object ToObject(string json, Type type)
        {
            try
            {
                //_params = Parameters;
                _params.FixValues();
                Type t = null;
                if (type != null && type.IsGenericType)
                    t = Reflection.Instance.GetGenericTypeDefinition(type);
                if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                    _params.UsingGlobalTypes = false;
                _usingglobals = _params.UsingGlobalTypes;

                object o = new JsonParser(json, _params.AllowNonQuotedKeys).Decode();
                if (o == null)
                    return null;
#if !SILVERLIGHT
                if (type != null && type == typeof(DataSet))
                    return CreateDataset(o as Dictionary<string, object>, null);
                else if (type != null && type == typeof(DataTable))
                    return CreateDataTable(o as Dictionary<string, object>, null);
#endif
                if (o is IDictionary)
                {
                    if (type != null && t == typeof(Dictionary<,>)) // deserialize a dictionary
                        return RootDictionary(o, type);
                    else // deserialize an object
                        return ParseDictionary(o as Dictionary<string, object>, null, type, null);
                }
                else if (o is List<object>)
                {
                    if (type != null && t == typeof(Dictionary<,>)) // kv format
                        return RootDictionary(o, type);
                    else if (type != null && t == typeof(List<>)) // deserialize to generic list
                        return RootList(o, type);
                    else if (type != null && type.IsArray)
                        return RootArray(o, type);
                    else if (type == typeof(Hashtable))
                        return RootHashTable((List<object>)o);
                    else if (type == null)
                    {
                        List<object> l = (List<object>)o;
                        if (l.Count > 0 && l[0].GetType() == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> globals = new Dictionary<string, object>();
                            List<object> op = new List<object>();
                            // try to get $types 
                            foreach (var i in l)
                                op.Add(ParseDictionary((Dictionary<string, object>)i, globals, null, null));
                            return op;
                        }
                        return l.ToArray();
                    }
                }
                else if (type != null && o.GetType() != type)
                    return ChangeType(o, type);

                return o;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region [   p r i v a t e   m e t h o d s   ]
        /// <summary>
        /// root hash table
        /// </summary>
        /// <param name="o">list of object</param>
        /// <returns>object</returns>
        private object RootHashTable(List<object> o)
        {
            try
            {

                Hashtable h = new Hashtable();

                foreach (Dictionary<string, object> values in o)
                {
                    object key = values["k"];
                    object val = values["v"];
                    if (key is Dictionary<string, object>)
                        key = ParseDictionary((Dictionary<string, object>)key, null, typeof(object), null);

                    if (val is Dictionary<string, object>)
                        val = ParseDictionary((Dictionary<string, object>)val, null, typeof(object), null);

                    h.Add(key, val);
                }

                return h;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Changes the type
        /// </summary>
        /// <param name="value">object values</param>
        /// <param name="conversionType">Conversion type</param>
        /// <returns>object</returns>
        private object ChangeType(object value, Type conversionType)
        {
            try
            {
                if (conversionType == typeof(int))
                {
                    string s = value as string;
                    if (s == null)
                        return (int)((long)value);
                    else
                        return CreateInteger(s, 0, s.Length);
                }
                else if (conversionType == typeof(long))
                {
                    string s = value as string;
                    if (s == null)
                        return (long)value;
                    else
                        return JSON.CreateLong(s, 0, s.Length);
                }
                else if (conversionType == typeof(string))
                    return (string)value;

                else if (conversionType.IsEnum)
                    return CreateEnum(conversionType, value);

                else if (conversionType == typeof(DateTime))
                    return CreateDateTime((string)value);

                else if (conversionType == typeof(DateTimeOffset))
                    return CreateDateTimeOffset((string)value);

                else if (Reflection.Instance.IsTypeRegistered(conversionType))
                    return Reflection.Instance.CreateCustom((string)value, conversionType);

                // 8-30-2014 - James Brooks - Added code for nullable types.
                if (IsNullable(conversionType))
                {
                    if (value == null)
                        return value;
                    conversionType = UnderlyingTypeOf(conversionType);
                }

                // 8-30-2014 - James Brooks - Nullable Guid is a special case so it was moved after the "IsNullable" check.
                if (conversionType == typeof(Guid))
                    return CreateGuid((string)value);

                // 2016-04-02 - Enrico Padovani - proper conversion of byte[] back from string
                if (conversionType == typeof(byte[]))
                    return Convert.FromBase64String((string)value);

                if (conversionType == typeof(TimeSpan))
                    return new TimeSpan((long)value);

                return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates date time offset
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>object</returns>
        private object CreateDateTimeOffset(string value)
        {
            try
            {
                //                   0123456789012345678 9012 9/3 0/4  1/5
                // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  _   +   00:00

                // ISO8601 roundtrip formats have 7 digits for ticks, and no space before the '+'
                // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  +   00:00  
                // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  Z  

                int year;
                int month;
                int day;
                int hour;
                int min;
                int sec;
                int ms = 0;
                int usTicks = 0; // ticks for xxx.x microseconds
                int th = 0;
                int tm = 0;

                year = CreateInteger(value, 0, 4);
                month = CreateInteger(value, 5, 2);
                day = CreateInteger(value, 8, 2);
                hour = CreateInteger(value, 11, 2);
                min = CreateInteger(value, 14, 2);
                sec = CreateInteger(value, 17, 2);

                int p = 20;

                if (value.Length > 21 && value[19] == '.')
                {
                    ms = CreateInteger(value, p, 3);
                    p = 23;

                    // handle 7 digit case
                    if (value.Length > 25 && char.IsDigit(value[p]))
                    {
                        usTicks = CreateInteger(value, p, 4);
                        p = 27;
                    }
                }

                if (value[p] == 'Z')
                    // UTC
                    return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, TimeSpan.Zero);

                if (value[p] == ' ')
                    ++p;

                // +00:00
                th = CreateInteger(value, p + 1, 2);
                tm = CreateInteger(value, p + 1 + 2 + 1, 2);

                if (value[p] == '-')
                    th = -th;

                return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, new TimeSpan(th, tm, 0));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates date time offset
        /// </summary>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <param name="day">day</param>
        /// <param name="hour">hour</param>
        /// <param name="min">minute</param>
        /// <param name="sec">second</param>
        /// <param name="milli">millisecond</param>
        /// <param name="extraTicks">extra ticks</param>
        /// <param name="offset">offset time</param>
        /// <returns>DateTime Offset</returns>
        private static DateTimeOffset CreateDateTimeOffset(
            int year, int month, int day, int hour, int min, int sec, int milli, int extraTicks, TimeSpan offset)
        {
            try
            {
                var dt = new DateTimeOffset(year, month, day, hour, min, sec, milli, offset);

                if (extraTicks > 0)
                    dt += TimeSpan.FromTicks(extraTicks);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Checks whether it is nullable or not
        /// </summary>
        /// <param name="t">type</param>
        /// <returns>boolean</returns>
        private bool IsNullable(Type t)
        {
            try
            {
                if (!t.IsGenericType) return false;
                Type g = t.GetGenericTypeDefinition();
                return (g.Equals(typeof(Nullable<>)));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Underlying type
        /// </summary>
        /// <param name="t">type</param>
        /// <returns>type</returns>
        private Type UnderlyingTypeOf(Type t)
        {
            try
            {
                return t.GetGenericArguments()[0];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Root list
        /// </summary>
        /// <param name="parse">object</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        private object RootList(object parse, Type type)
        {
            try
            {
                Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
                IList o = (IList)Reflection.Instance.FastCreateInstance(type);
                DoParseList(parse, gtypes[0], o);
                return o;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Parses to list
        /// </summary>
        /// <param name="parse">object</param>
        /// <param name="it">type</param>
        /// <param name="o">ilist type</param>
        private void DoParseList(object parse, Type it, IList o)
        {
            try
            {
                Dictionary<string, object> globals = new Dictionary<string, object>();
                foreach (var k in (IList)parse)
                {
                    _usingglobals = false;
                    object v = k;
                    if (k is Dictionary<string, object>)
                        v = ParseDictionary(k as Dictionary<string, object>, globals, it, null);
                    else
                        v = ChangeType(k, it);

                    o.Add(v);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// roots to array
        /// </summary>
        /// <param name="parse">object</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        private object RootArray(object parse, Type type)
        {
            try
            {
                Type it = type.GetElementType();
                IList o = (IList)Reflection.Instance.FastCreateInstance(typeof(List<>).MakeGenericType(it));
                DoParseList(parse, it, o);
                var array = Array.CreateInstance(it, o.Count);
                o.CopyTo(array, 0);

                return array;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Root doctionary
        /// </summary>
        /// <param name="parse">object</param>
        /// <param name="type">Type</param>
        /// <returns>object</returns>
        private object RootDictionary(object parse, Type type)
        {
            try
            {

                Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
                Type t1 = null;
                Type t2 = null;
                if (gtypes != null)
                {
                    t1 = gtypes[0];
                    t2 = gtypes[1];
                }
                var arraytype = t2.GetElementType();
                if (parse is Dictionary<string, object>)
                {
                    IDictionary o = (IDictionary)Reflection.Instance.FastCreateInstance(type);

                    foreach (var kv in (Dictionary<string, object>)parse)
                    {
                        object v;
                        object k = ChangeType(kv.Key, t1);

                        if (kv.Value is Dictionary<string, object>)
                            v = ParseDictionary(kv.Value as Dictionary<string, object>, null, t2, null);

                        else if (t2.IsArray && t2 != typeof(byte[]))
                            v = CreateArray((List<object>)kv.Value, t2, arraytype, null);

                        else if (kv.Value is IList)
                            v = CreateGenericList((List<object>)kv.Value, t2, t1, null);

                        else
                            v = ChangeType(kv.Value, t2);

                        o.Add(k, v);
                    }

                    return o;
                }
                if (parse is List<object>)
                    return CreateDictionary(parse as List<object>, type, gtypes, null);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// parses the doctionary
        /// </summary>
        /// <param name="d">dictionary of objects</param>
        /// <param name="globaltypes">dictionary of global types</param>
        /// <param name="type">type</param>
        /// <param name="input">object</param>
        /// <returns>object</returns>
        internal object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object> globaltypes, Type type, object input)
        {
            try
            {

                object tn = "";
                if (type == typeof(NameValueCollection))
                    return CreateNV(d);
                if (type == typeof(StringDictionary))
                    return CreateSD(d);

                if (d.TryGetValue("$i", out tn))
                {
                    object v = null;
                    _cirrev.TryGetValue((int)(long)tn, out v);
                    return v;
                }

                if (d.TryGetValue("$types", out tn))
                {
                    _usingglobals = true;
                    if (globaltypes == null)
                        globaltypes = new Dictionary<string, object>();
                    foreach (var kv in (Dictionary<string, object>)tn)
                    {
                        globaltypes.Add((string)kv.Value, kv.Key);
                    }
                }
                if (globaltypes != null)
                    _usingglobals = true;

                bool found = d.TryGetValue("$type", out tn);
#if !SILVERLIGHT
                if (found == false && type == typeof(System.Object))
                {
                    return d;   // CreateDataset(d, globaltypes);
                }
#endif
                if (found)
                {
                    if (_usingglobals)
                    {
                        object tname = "";
                        if (globaltypes != null && globaltypes.TryGetValue((string)tn, out tname))
                            tn = tname;
                    }
                    type = Reflection.Instance.GetTypeFromCache((string)tn);
                }

                if (type == null)
                    throw new Exception("Cannot determine type");

                string typename = type.FullName;
                object o = input;
                if (o == null)
                {
                    if (_params.ParametricConstructorOverride)
                        o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                    else
                        o = Reflection.Instance.FastCreateInstance(type);
                }
                int circount = 0;
                if (_circobj.TryGetValue(o, out circount) == false)
                {
                    circount = _circobj.Count + 1;
                    _circobj.Add(o, circount);
                    _cirrev.Add(circount, o);
                }

                Dictionary<string, myPropInfo> props = Reflection.Instance.Getproperties(type, typename);
                foreach (var kv in d)
                {
                    var n = kv.Key;
                    var v = kv.Value;

                    string name = n;//.ToLower();
                    if (name == "$map")
                    {
                        ProcessMap(o, props, (Dictionary<string, object>)d[name]);
                        continue;
                    }
                    myPropInfo pi;
                    if (props.TryGetValue(name.ToLower(), out pi) == false)
                        if (props.TryGetValue(name, out pi) == false)
                            continue;

                    if (pi.CanWrite)
                    {
                        if (v != null)
                        {
                            object oset = null;

                            switch (pi.Type)
                            {
                                case myPropInfoType.Int: oset = (int)AutoConv(v); break;
                                case myPropInfoType.Long: oset = AutoConv(v); break;
                                case myPropInfoType.String: oset = (string)v; break;
                                case myPropInfoType.Bool: oset = (bool)v; break;
                                case myPropInfoType.DateTime: oset = CreateDateTime((string)v); break;
                                case myPropInfoType.Enum: oset = CreateEnum(pi.pt, v); break;
                                case myPropInfoType.Guid: oset = CreateGuid((string)v); break;

                                case myPropInfoType.Array:
                                    if (!pi.IsValueType)
                                        oset = CreateArray((List<object>)v, pi.pt, pi.bt, globaltypes);
                                    // what about 'else'?
                                    break;
                                case myPropInfoType.ByteArray: oset = Convert.FromBase64String((string)v); break;
#if !SILVERLIGHT
                                case myPropInfoType.DataSet: oset = CreateDataset((Dictionary<string, object>)v, globaltypes); break;
                                case myPropInfoType.DataTable: oset = CreateDataTable((Dictionary<string, object>)v, globaltypes); break;
                                case myPropInfoType.Hashtable: // same case as Dictionary
#endif
                                case myPropInfoType.Dictionary: oset = CreateDictionary((List<object>)v, pi.pt, pi.GenericTypes, globaltypes); break;
                                case myPropInfoType.StringKeyDictionary: oset = CreateStringKeyDictionary((Dictionary<string, object>)v, pi.pt, pi.GenericTypes, globaltypes); break;
                                case myPropInfoType.NameValue: oset = CreateNV((Dictionary<string, object>)v); break;
                                case myPropInfoType.StringDictionary: oset = CreateSD((Dictionary<string, object>)v); break;
                                case myPropInfoType.Custom: oset = Reflection.Instance.CreateCustom((string)v, pi.pt); break;
                                default:
                                    {
                                        if (pi.IsGenericType && pi.IsValueType == false && v is List<object>)
                                            oset = CreateGenericList((List<object>)v, pi.pt, pi.bt, globaltypes);

                                        else if ((pi.IsClass || pi.IsStruct || pi.IsInterface) && v is Dictionary<string, object>)
                                            oset = ParseDictionary((Dictionary<string, object>)v, globaltypes, pi.pt, pi.getter(o));

                                        else if (v is List<object>)
                                            oset = CreateArray((List<object>)v, pi.pt, typeof(object), globaltypes);

                                        else if (pi.IsValueType)
                                            oset = ChangeType(v, pi.changeType);

                                        else
                                            oset = v;
                                    }
                                    break;
                            }

                            o = pi.setter(o, oset);
                        }
                    }
                }
                return o;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Auto converts to long
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>long values</returns>
        private long AutoConv(object value)
        {
            try
            {
                if (value is string)
                {
                    string s = (string)value;
                    return CreateLong(s, 0, s.Length);
                }
                else if (value is long)
                    return (long)value;
                else
                    return Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates String Dictionary
        /// </summary>
        /// <param name="d">dictionary of object</param>
        /// <returns>String Dictionary</returns>
        private StringDictionary CreateSD(Dictionary<string, object> d)
        {
            try
            {
                StringDictionary nv = new StringDictionary();

                foreach (var o in d)
                    nv.Add(o.Key, (string)o.Value);

                return nv;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates named value collection
        /// </summary>
        /// <param name="d">dictionary of object</param>
        /// <returns>named value collection</returns>
        private NameValueCollection CreateNV(Dictionary<string, object> d)
        {
            try
            {
                NameValueCollection nv = new NameValueCollection();

                foreach (var o in d)
                    nv.Add(o.Key, (string)o.Value);

                return nv;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Processes to map to object
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="props">dictionary of properties</param>
        /// <param name="dic">dictionary of object</param>
        private void ProcessMap(object obj, Dictionary<string, myPropInfo> props, Dictionary<string, object> dic)
        {
            try
            {
                foreach (KeyValuePair<string, object> kv in dic)
                {
                    myPropInfo p = props[kv.Key];
                    object o = p.getter(obj);
                    Type t = Type.GetType((string)kv.Value);
                    if (t == typeof(Guid))
                        p.setter(obj, CreateGuid((string)o));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates long from string
        /// </summary>
        /// <param name="s">value</param>
        /// <param name="index">index</param>
        /// <param name="count">count</param>
        /// <returns>long</returns>
        private long CreateLong(string s, int index, int count)
        {
            try
            {
                long num = 0;
                bool neg = false;
                for (int x = 0; x < count; x++, index++)
                {
                    char cc = s[index];

                    if (cc == '-')
                        neg = true;
                    else if (cc == '+')
                        neg = false;
                    else
                    {
                        num *= 10;
                        num += (int)(cc - '0');
                    }
                }
                if (neg) num = -num;

                return num;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates integer from string
        /// </summary>
        /// <param name="s">value</param>
        /// <param name="index">index</param>
        /// <param name="count">count</param>
        /// <returns>integer</returns>
        private int CreateInteger(string s, int index, int count)
        {
            try
            {

                int num = 0;
                bool neg = false;
                for (int x = 0; x < count; x++, index++)
                {
                    char cc = s[index];

                    if (cc == '-')
                        neg = true;
                    else if (cc == '+')
                        neg = false;
                    else
                    {
                        num *= 10;
                        num += (int)(cc - '0');
                    }
                }
                if (neg) num = -num;

                return num;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates ENUM
        /// </summary>
        /// <param name="pt">type</param>
        /// <param name="v">object</param>
        /// <returns>object</returns>
        private object CreateEnum(Type pt, object v)
        {
            try
            {
                // FEATURE : optimize create enum
#if !SILVERLIGHT
                return Enum.Parse(pt, v.ToString());
#else
            return Enum.Parse(pt, v, true);
#endif
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates GUID
        /// </summary>
        /// <param name="s">value to be converted</param>
        /// <returns>GUID</returns>
        private Guid CreateGuid(string s)
        {
            try
            {
                if (s.Length > 30)
                    return new Guid(s);
                else
                    return new Guid(Convert.FromBase64String(s));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates variables of datetime format
        /// </summary>
        /// <param name="value">value to be in Datetime format</param>
        /// <returns></returns>
        private DateTime CreateDateTime(string value)
        {
            try
            {
                if (value.Length < 19)
                    return DateTime.MinValue;

                bool utc = false;
                //                   0123456789012345678 9012 9/3
                // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  Z
                int year;
                int month;
                int day;
                int hour;
                int min;
                int sec;
                int ms = 0;

                year = CreateInteger(value, 0, 4);
                month = CreateInteger(value, 5, 2);
                day = CreateInteger(value, 8, 2);
                hour = CreateInteger(value, 11, 2);
                min = CreateInteger(value, 14, 2);
                sec = CreateInteger(value, 17, 2);
                if (value.Length > 21 && value[19] == '.')
                    ms = CreateInteger(value, 20, 3);

                if (value[value.Length - 1] == 'Z')
                    utc = true;

                if (_params.UseUTCDateTime == false && utc == false)
                    return new DateTime(year, month, day, hour, min, sec, ms);
                else
                    return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates array
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="pt">type</param>
        /// <param name="bt">type</param>
        /// <param name="globalTypes">global types</param>
        /// <returns>objects</returns>
        private object CreateArray(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            try
            {
                if (bt == null)
                    bt = typeof(object);

                Array col = Array.CreateInstance(bt, data.Count);
                var arraytype = bt.GetElementType();
                // create an array of objects
                for (int i = 0; i < data.Count; i++)
                {
                    object ob = data[i];
                    if (ob == null)
                    {
                        continue;
                    }
                    if (ob is IDictionary)
                        col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null), i);
                    else if (ob is ICollection)
                        col.SetValue(CreateArray((List<object>)ob, bt, arraytype, globalTypes), i);
                    else
                        col.SetValue(ChangeType(ob, bt), i);
                }

                return col;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// creates a generic lsit
        /// </summary>
        /// <param name="data">list of object type data</param>
        /// <param name="pt">type</param>
        /// <param name="bt">type</param>
        /// <param name="globalTypes">global types</param>
        /// <returns>objects</returns>
        private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            try
            {

                if (pt != typeof(object))
                {
                    IList col = (IList)Reflection.Instance.FastCreateInstance(pt);
                    var it = pt.GetGenericArguments()[0];
                    // create an array of objects
                    foreach (object ob in data)
                    {
                        if (ob is IDictionary)
                            col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, it, null));

                        else if (ob is List<object>)
                        {
                            if (bt.IsGenericType)
                                col.Add((List<object>)ob);//).ToArray());
                            else
                                col.Add(((List<object>)ob).ToArray());
                        }
                        else
                            col.Add(ChangeType(ob, it));
                    }
                    return col;
                }
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// creates object from dictionary
        /// </summary>
        /// <param name="reader">dictionary reader</param>
        /// <param name="pt">type</param>
        /// <param name="types">type array</param>
        /// <param name="globalTypes">global types</param>
        /// <returns>object</returns>
        private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            try
            {

                var col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
                Type arraytype = null;
                Type t2 = null;
                if (types != null)
                    t2 = types[1];

                Type generictype = null;
                var ga = t2.GetGenericArguments();
                if (ga.Length > 0)
                    generictype = ga[0];
                arraytype = t2.GetElementType();

                foreach (KeyValuePair<string, object> values in reader)
                {
                    var key = values.Key;
                    object val = null;

                    if (values.Value is Dictionary<string, object>)
                        val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null);

                    else if (types != null && t2.IsArray)
                    {
                        if (values.Value is Array)
                            val = values.Value;
                        else
                            val = CreateArray((List<object>)values.Value, t2, arraytype, globalTypes);
                    }
                    else if (values.Value is IList)
                        val = CreateGenericList((List<object>)values.Value, t2, generictype, globalTypes);

                    else
                        val = ChangeType(values.Value, t2);

                    col.Add(key, val);
                }

                return col;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates dictionary
        /// </summary>
        /// <param name="reader">list of readers</param>
        /// <param name="pt">type</param>
        /// <param name="types">type array</param>
        /// <param name="globalTypes">global types</param>
        /// <returns>object</returns>
        private object CreateDictionary(List<object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            try
            {
                IDictionary col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
                Type t1 = null;
                Type t2 = null;
                if (types != null)
                {
                    t1 = types[0];
                    t2 = types[1];
                }

                foreach (Dictionary<string, object> values in reader)
                {
                    object key = values["k"];
                    object val = values["v"];

                    if (key is Dictionary<string, object>)
                        key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
                    else
                        key = ChangeType(key, t1);

                    if (typeof(IDictionary).IsAssignableFrom(t2))
                        val = RootDictionary(val, t2);
                    else if (val is Dictionary<string, object>)
                        val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
                    else
                        val = ChangeType(val, t2);

                    col.Add(key, val);
                }

                return col;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates dataset
        /// </summary>
        /// <param name="reader">reader dictionary</param>
        /// <param name="globalTypes">global type</param>
        /// <returns>Dataset</returns>
        private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.EnforceConstraints = false;
                ds.BeginInit();

                // read dataset schema here
                var schema = reader["$schema"];

                if (schema is string)
                {
                    TextReader tr = new StringReader((string)schema);
                    ds.ReadXmlSchema(tr);
                }
                else
                {
                    DatasetSchema ms = (DatasetSchema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(DatasetSchema), null);
                    ds.DataSetName = ms.Name;
                    for (int i = 0; i < ms.Info.Count; i += 3)
                    {
                        if (ds.Tables.Contains(ms.Info[i]) == false)
                            ds.Tables.Add(ms.Info[i]);
                        ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                    }
                }

                foreach (KeyValuePair<string, object> pair in reader)
                {
                    if (pair.Key == "$type" || pair.Key == "$schema") continue;

                    List<object> rows = (List<object>)pair.Value;
                    if (rows == null) continue;

                    DataTable dt = ds.Tables[pair.Key];
                    ReadDataTable(rows, dt);
                }

                ds.EndInit();

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Reads datatable
        /// </summary>
        /// <param name="rows">row list</param>
        /// <param name="dt">datatbale</param>
        private void ReadDataTable(List<object> rows, DataTable dt)
        {
            try
            {
                dt.BeginInit();
                dt.BeginLoadData();
                List<int> guidcols = new List<int>();
                List<int> datecol = new List<int>();
                List<int> bytearraycol = new List<int>();

                foreach (DataColumn c in dt.Columns)
                {
                    if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
                        guidcols.Add(c.Ordinal);
                    if (_params.UseUTCDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
                        datecol.Add(c.Ordinal);
                    if (c.DataType == typeof(byte[]))
                        bytearraycol.Add(c.Ordinal);
                }

                foreach (List<object> row in rows)
                {
                    object[] v = new object[row.Count];
                    row.CopyTo(v, 0);
                    foreach (int i in guidcols)
                    {
                        string s = (string)v[i];
                        if (s != null && s.Length < 36)
                            v[i] = new Guid(Convert.FromBase64String(s));
                    }
                    foreach (int i in bytearraycol)
                    {
                        string s = (string)v[i];
                        if (s != null)
                            v[i] = Convert.FromBase64String(s);
                    }
                    if (_params.UseUTCDateTime)
                    {
                        foreach (int i in datecol)
                        {
                            string s = (string)v[i];
                            if (s != null)
                                v[i] = CreateDateTime(s);
                        }
                    }
                    dt.Rows.Add(v);
                }

                dt.EndLoadData();
                dt.EndInit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates the datatable
        /// </summary>
        /// <param name="reader">dictionary reader</param>
        /// <param name="globalTypes">global type variables</param>
        /// <returns>Datatable</returns>
        DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            try
            {

                var dt = new DataTable();

                // read dataset schema here
                var schema = reader["$schema"];

                if (schema is string)
                {
                    TextReader tr = new StringReader((string)schema);
                    dt.ReadXmlSchema(tr);
                }
                else
                {
                    var ms = (DatasetSchema)this.ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(DatasetSchema), null);
                    dt.TableName = ms.Info[0];
                    for (int i = 0; i < ms.Info.Count; i += 3)
                    {
                        dt.Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                    }
                }

                foreach (var pair in reader)
                {
                    if (pair.Key == "$type" || pair.Key == "$schema")
                        continue;

                    var rows = (List<object>)pair.Value;
                    if (rows == null)
                        continue;

                    if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    ReadDataTable(rows, dt);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
#endif
        #endregion
    }
}
