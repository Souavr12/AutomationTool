/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
#if !SILVERLIGHT
using System.Data;
#endif
using System.Collections.Specialized;

namespace AutomationTool.Converter
{
    internal struct Getters
    {
        public string Name;
        public string lcName;
        public string memberName;
        public Reflection.GenericGetter Getter;
    }

    internal enum myPropInfoType
    {
        Int,
        Long,
        String,
        Bool,
        DateTime,
        Enum,
        Guid,

        Array,
        ByteArray,
        Dictionary,
        StringKeyDictionary,
        NameValue,
        StringDictionary,
#if !SILVERLIGHT
        Hashtable,
        DataSet,
        DataTable,
#endif
        Custom,
        Unknown,
    }

    internal struct myPropInfo
    {
        public Type pt;
        public Type bt;
        public Type changeType;
        public Reflection.GenericSetter setter;
        public Reflection.GenericGetter getter;
        public Type[] GenericTypes;
        public string Name;
#if net4
        public string memberName;
#endif
        public myPropInfoType Type;
        public bool CanWrite;

        public bool IsClass;
        public bool IsValueType;
        public bool IsGenericType;
        public bool IsStruct;
        public bool IsInterface;
    }

    internal sealed class Reflection
    {
        #region Properties and Variables
        public static Reflection Instance { get { return Singleton.SINGLETON<Reflection>.Instance; } }

        internal delegate object GenericSetter(object target, object value);
        internal delegate object GenericGetter(object obj);
        private delegate object CreateObject();

        private SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>();
        private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>();
        private SafeDictionary<Type, CreateObject> _constrcache = new SafeDictionary<Type, CreateObject>();
        private SafeDictionary<Type, Getters[]> _getterscache = new SafeDictionary<Type, Getters[]>();
        private SafeDictionary<string, Dictionary<string, myPropInfo>> _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        private SafeDictionary<Type, Type[]> _genericTypes = new SafeDictionary<Type, Type[]>();
        private SafeDictionary<Type, Type> _genericTypeDef = new SafeDictionary<Type, Type>();

        #region bjson custom types
        internal UnicodeEncoding unicode = new UnicodeEncoding();
        internal UTF8Encoding utf8 = new UTF8Encoding();
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Private constructor for making the class singleton
        /// </summary>
        private Reflection()
        {
        }
        #endregion

        #region json custom types
        // JSON custom
        internal SafeDictionary<Type, Serialize> _customSerializer = new SafeDictionary<Type, Serialize>();
        internal SafeDictionary<Type, Deserialize> _customDeserializer = new SafeDictionary<Type, Deserialize>();

        /// <summary>
        /// Creates Custom object
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="type">type</param>
        /// <returns>Custom Object</returns>
        internal object CreateCustom(string value, Type type)
        {
            try
            {
                Deserialize d;
                _customDeserializer.TryGetValue(type, out d);
                return d(value);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Registers Cutom Type Value
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="Deserializer">Deserializer</param>
        internal void RegisterCustomType(Type type, Serialize serializer, Deserialize Deserializer)
        {
            try
            {

                if (type != null && serializer != null && Deserializer != null)
                {
                    _customSerializer.Add(type, serializer);
                    _customDeserializer.Add(type, Deserializer);
                    // reset property cache
                    Instance.ResetPropertyCache();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Checks whether the type is registered or not
        /// </summary>
        /// <param name="t">type of object</param>
        /// <returns>'True' if registered else 'False'</returns>
        internal bool IsTypeRegistered(Type t)
        {
            try
            {
                if (_customSerializer.Count == 0)
                    return false;
                Serialize s;
                return _customSerializer.TryGetValue(t, out s);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        /// <summary>
        /// Gets the generic type defination
        /// </summary>
        /// <param name="t">the type object</param>
        /// <returns>the generic type</returns>
        public Type GetGenericTypeDefinition(Type t)
        {
            try
            {
                Type tt = null;
                if (_genericTypeDef.TryGetValue(t, out tt))
                    return tt;
                else
                {
                    tt = t.GetGenericTypeDefinition();
                    _genericTypeDef.Add(t, tt);
                    return tt;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the generic type args
        /// </summary>
        /// <param name="t">the type object</param>
        /// <returns>the generic type array</returns>
        public Type[] GetGenericArguments(Type t)
        {
            try
            {
                Type[] tt = null;
                if (_genericTypes.TryGetValue(t, out tt))
                    return tt;
                else
                {
                    tt = t.GetGenericArguments();
                    _genericTypes.Add(t, tt);
                    return tt;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the properties
        /// </summary>
        /// <param name="type">the type</param>
        /// <param name="typename">type name</param>
        /// <returns>Dictionary of properties</returns>
        public Dictionary<string, myPropInfo> Getproperties(Type type, string typename)
        {
            try
            {
                Dictionary<string, myPropInfo> sd = null;
                if (_propertycache.TryGetValue(typename, out sd))
                {
                    return sd;
                }
                else
                {
                    sd = new Dictionary<string, myPropInfo>();
                    var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                    PropertyInfo[] pr = type.GetProperties(bf);
                    foreach (PropertyInfo p in pr)
                    {
                        if (p.GetIndexParameters().Length > 0)// Property is an indexer
                            continue;

                        myPropInfo d = CreateMyProp(p.PropertyType, p.Name);
                        d.setter = Reflection.CreateSetMethod(type, p);
                        if (d.setter != null)
                            d.CanWrite = true;
                        d.getter = Reflection.CreateGetMethod(type, p);
#if net4
                    var att = p.GetCustomAttributes(true);
                    foreach (var at in att)
                    {
                        if (at is DataMemberAttribute)
                        {
                            var dm = (DataMemberAttribute)at;
                            if (dm.Name != "")
                                d.memberName = dm.Name;
                        }
                    }
                    if (d.memberName != null)
                        sd.Add(d.memberName, d);
                    else
#endif
                        sd.Add(p.Name.ToLower(), d);
                    }
                    FieldInfo[] fi = type.GetFields(bf);
                    foreach (FieldInfo f in fi)
                    {
                        myPropInfo d = CreateMyProp(f.FieldType, f.Name);
                        if (f.IsLiteral == false)
                        {
                            d.setter = Reflection.CreateSetField(type, f);
                            if (d.setter != null)
                                d.CanWrite = true;
                            d.getter = Reflection.CreateGetField(type, f);
#if net4
                        var att = f.GetCustomAttributes(true);
                        foreach (var at in att)
                        {
                            if (at is DataMemberAttribute)
                            {
                                var dm = (DataMemberAttribute)at;
                                if (dm.Name != "")
                                    d.memberName = dm.Name;
                            }
                        }
                        if (d.memberName != null)
                            sd.Add(d.memberName, d);
                        else
#endif
                            sd.Add(f.Name.ToLower(), d);
                        }
                    }

                    _propertycache.Add(typename, sd);
                    return sd;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates the properties
        /// </summary>
        /// <param name="t">type</param>
        /// <param name="name">type name</param>
        /// <returns>The property</returns>
        private myPropInfo CreateMyProp(Type t, string name)
        {
            try
            {
                myPropInfo d = new myPropInfo();
                myPropInfoType d_type = myPropInfoType.Unknown;

                if (t == typeof(int) || t == typeof(int?)) d_type = myPropInfoType.Int;
                else if (t == typeof(long) || t == typeof(long?)) d_type = myPropInfoType.Long;
                else if (t == typeof(string)) d_type = myPropInfoType.String;
                else if (t == typeof(bool) || t == typeof(bool?)) d_type = myPropInfoType.Bool;
                else if (t == typeof(DateTime) || t == typeof(DateTime?)) d_type = myPropInfoType.DateTime;
                else if (t.IsEnum) d_type = myPropInfoType.Enum;
                else if (t == typeof(Guid) || t == typeof(Guid?)) d_type = myPropInfoType.Guid;
                else if (t == typeof(StringDictionary)) d_type = myPropInfoType.StringDictionary;
                else if (t == typeof(NameValueCollection)) d_type = myPropInfoType.NameValue;
                else if (t.IsArray)
                {
                    d.bt = t.GetElementType();
                    if (t == typeof(byte[]))
                        d_type = myPropInfoType.ByteArray;
                    else
                        d_type = myPropInfoType.Array;
                }
                else if (t.Name.Contains("Dictionary"))
                {
                    d.GenericTypes = Reflection.Instance.GetGenericArguments(t);
                    if (d.GenericTypes.Length > 0 && d.GenericTypes[0] == typeof(string))
                        d_type = myPropInfoType.StringKeyDictionary;
                    else
                        d_type = myPropInfoType.Dictionary;
                }
#if !SILVERLIGHT
                else if (t == typeof(Hashtable)) d_type = myPropInfoType.Hashtable;
                else if (t == typeof(DataSet)) d_type = myPropInfoType.DataSet;
                else if (t == typeof(DataTable)) d_type = myPropInfoType.DataTable;
#endif
                else if (IsTypeRegistered(t))
                    d_type = myPropInfoType.Custom;

                if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof(decimal))
                    d.IsStruct = true;

                d.IsInterface = t.IsInterface;
                d.IsClass = t.IsClass;
                d.IsValueType = t.IsValueType;
                if (t.IsGenericType)
                {
                    d.IsGenericType = true;
                    d.bt = t.GetGenericArguments()[0];
                }

                d.pt = t;
                d.Name = name;
                d.changeType = GetChangeType(t);
                d.Type = d_type;

                return d;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the change type
        /// </summary>
        /// <param name="conversionType">Convert to type</param>
        /// <returns>the type</returns>
        private Type GetChangeType(Type conversionType)
        {
            try
            {
                if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    return Reflection.Instance.GetGenericArguments(conversionType)[0];

                return conversionType;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region [   PROPERTY GET SET   ]
        /// <summary>
        /// Gets the type assembly
        /// </summary>
        /// <param name="t">type</param>
        /// <returns>string</returns>
        internal string GetTypeAssemblyName(Type t)
        {
            try
            {
                string val = "";
                if (_tyname.TryGetValue(t, out val))
                    return val;
                else
                {
                    string s = t.AssemblyQualifiedName;
                    _tyname.Add(t, s);
                    return s;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the type from Cache
        /// </summary>
        /// <param name="typename">name of type</param>
        /// <returns>the type</returns>
        internal Type GetTypeFromCache(string typename)
        {
            try
            {
                Type val = null;
                if (_typecache.TryGetValue(typename, out val))
                    return val;
                else
                {
                    Type t = Type.GetType(typename);
                    //if (t == null) // RaptorDB : loading runtime assemblies
                    //{
                    //    t = Type.GetType(typename, (name) => {
                    //        return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
                    //    }, null, true);
                    //}
                    _typecache.Add(typename, t);
                    return t;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Creates the instance
        /// </summary>
        /// <param name="objtype">type of object</param>
        /// <returns>newly instantiated object</returns>
        internal object FastCreateInstance(Type objtype)
        {
            try
            {
                CreateObject c = null;
                if (_constrcache.TryGetValue(objtype, out c))
                {
                    return c();
                }
                else
                {
                    if (objtype.IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", objtype, null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(objtype, c);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", typeof(object), null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        var lv = ilGen.DeclareLocal(objtype);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, objtype);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, objtype);
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(objtype, c);
                    }
                    return c();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        /// <summary>
        /// Adds value to the fields created
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="fieldInfo">field info</param>
        /// <returns>Generic setter</returns>
        internal static GenericSetter CreateSetField(Type type, FieldInfo fieldInfo)
        {
            try
            {
                Type[] arguments = new Type[2];
                arguments[0] = arguments[1] = typeof(object);

                DynamicMethod dynamicSet = new DynamicMethod("_", typeof(object), arguments, type);

                ILGenerator il = dynamicSet.GetILGenerator();

                if (!type.IsClass) // structs
                {
                    var lv = il.DeclareLocal(type);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Unbox_Any, type);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, lv);
                    il.Emit(OpCodes.Ldarg_1);
                    if (fieldInfo.FieldType.IsClass)
                        il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
                    else
                        il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                    il.Emit(OpCodes.Stfld, fieldInfo);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Box, type);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    if (fieldInfo.FieldType.IsValueType)
                        il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                    il.Emit(OpCodes.Stfld, fieldInfo);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ret);
                }
                return (GenericSetter)dynamicSet.CreateDelegate(typeof(GenericSetter));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Adds value to the methods created
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="propertyInfo">property info</param>
        /// <returns>Generic setter</returns>
        internal static GenericSetter CreateSetMethod(Type type, PropertyInfo propertyInfo)
        {
            try
            {
                MethodInfo setMethod = propertyInfo.GetSetMethod();
                if (setMethod == null)
                    return null;

                Type[] arguments = new Type[2];
                arguments[0] = arguments[1] = typeof(object);

                DynamicMethod setter = new DynamicMethod("_", typeof(object), arguments);
                ILGenerator il = setter.GetILGenerator();

                if (!type.IsClass) // structs
                {
                    var lv = il.DeclareLocal(type);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Unbox_Any, type);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, lv);
                    il.Emit(OpCodes.Ldarg_1);
                    if (propertyInfo.PropertyType.IsClass)
                        il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                    else
                        il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                    il.EmitCall(OpCodes.Call, setMethod, null);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Box, type);
                }
                else
                {
                    if (!setMethod.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                        il.Emit(OpCodes.Ldarg_1);
                        if (propertyInfo.PropertyType.IsClass)
                            il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                        else
                            il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                        il.EmitCall(OpCodes.Callvirt, setMethod, null);
                        il.Emit(OpCodes.Ldarg_0);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        if (propertyInfo.PropertyType.IsClass)
                            il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                        else
                            il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                        il.Emit(OpCodes.Call, setMethod);
                    }
                }

                il.Emit(OpCodes.Ret);

                return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets value from the fields created
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="fieldInfo">field info</param>
        /// <returns>Generic setter</returns>
        internal static GenericGetter CreateGetField(Type type, FieldInfo fieldInfo)
        {
            try
            {
                DynamicMethod dynamicGet = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type);

                ILGenerator il = dynamicGet.GetILGenerator();

                if (!type.IsClass) // structs
                {
                    var lv = il.DeclareLocal(type);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Unbox_Any, type);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, lv);
                    il.Emit(OpCodes.Ldfld, fieldInfo);
                    if (fieldInfo.FieldType.IsValueType)
                        il.Emit(OpCodes.Box, fieldInfo.FieldType);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, fieldInfo);
                    if (fieldInfo.FieldType.IsValueType)
                        il.Emit(OpCodes.Box, fieldInfo.FieldType);
                }

                il.Emit(OpCodes.Ret);

                return (GenericGetter)dynamicGet.CreateDelegate(typeof(GenericGetter));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets value from the methods created
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="propertyInfo">property info</param>
        /// <returns>Generic setter</returns>
        internal static GenericGetter CreateGetMethod(Type type, PropertyInfo propertyInfo)
        {
            try
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod();
                if (getMethod == null)
                    return null;

                DynamicMethod getter = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type);

                ILGenerator il = getter.GetILGenerator();

                if (!type.IsClass) // structs
                {
                    var lv = il.DeclareLocal(type);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Unbox_Any, type);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, lv);
                    il.EmitCall(OpCodes.Call, getMethod, null);
                    if (propertyInfo.PropertyType.IsValueType)
                        il.Emit(OpCodes.Box, propertyInfo.PropertyType);
                }
                else
                {
                    if (!getMethod.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                        il.EmitCall(OpCodes.Callvirt, getMethod, null);
                    }
                    else
                        il.Emit(OpCodes.Call, getMethod);

                    if (propertyInfo.PropertyType.IsValueType)
                        il.Emit(OpCodes.Box, propertyInfo.PropertyType);
                }

                il.Emit(OpCodes.Ret);

                return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets all the getters
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="ShowReadOnlyProperties">'True' to show the read only properties else 'False'</param>
        /// <param name="IgnoreAttributes">List of attributes to be ignored</param>
        /// <returns>Getter array</returns>
        internal Getters[] GetGetters(Type type, bool ShowReadOnlyProperties, List<Type> IgnoreAttributes)
        {
            try
            {
                Getters[] val = null;
                if (_getterscache.TryGetValue(type, out val))
                    return val;
                //bool isAnonymous = IsAnonymousType(type);

                var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                //if (ShowReadOnlyProperties)
                //    bf |= BindingFlags.NonPublic;
                PropertyInfo[] props = type.GetProperties(bf);
                List<Getters> getters = new List<Getters>();
                foreach (PropertyInfo p in props)
                {
                    if (p.GetIndexParameters().Length > 0)
                    {// Property is an indexer
                        continue;
                    }
                    if (!p.CanWrite && (ShowReadOnlyProperties == false))//|| isAnonymous == false))
                        continue;
                    if (IgnoreAttributes != null)
                    {
                        bool found = false;
                        foreach (var ignoreAttr in IgnoreAttributes)
                        {
                            if (p.IsDefined(ignoreAttr, false))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            continue;
                    }
                    string mName = null;
#if net4
                var att = p.GetCustomAttributes(true);
                foreach (var at in att)
                {
                    if (at is DataMemberAttribute)
                    {
                        var dm = (DataMemberAttribute)at;
                        if (dm.Name != "")
                        {
                            mName = dm.Name;
                        }
                    }
                }
#endif
                    GenericGetter g = CreateGetMethod(type, p);
                    if (g != null)
                        getters.Add(new Getters { Getter = g, Name = p.Name, lcName = p.Name.ToLower(), memberName = mName });
                }

                FieldInfo[] fi = type.GetFields(bf);
                foreach (var f in fi)
                {
                    if (IgnoreAttributes != null)
                    {
                        bool found = false;
                        foreach (var ignoreAttr in IgnoreAttributes)
                        {
                            if (f.IsDefined(ignoreAttr, false))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            continue;
                    }
                    string mName = null;
#if net4
                var att = f.GetCustomAttributes(true);
                foreach (var at in att)
                {
                    if (at is DataMemberAttribute)
                    {
                        var dm = (DataMemberAttribute)at;
                        if (dm.Name != "")
                        {
                            mName = dm.Name;
                        }
                    }
                }
#endif
                    if (f.IsLiteral == false)
                    {
                        GenericGetter g = CreateGetField(type, f);
                        if (g != null)
                            getters.Add(new Getters { Getter = g, Name = f.Name, lcName = f.Name.ToLower(), memberName = mName });
                    }
                }
                val = getters.ToArray();
                _getterscache.Add(type, val);
                return val;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        /// <summary>
        /// Resets the property cache
        /// </summary>
        internal void ResetPropertyCache()
        {
            try
            {
                _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Clears the reflection cache
        /// </summary>
        internal void ClearReflectionCache()
        {
            try
            {
                _tyname = new SafeDictionary<Type, string>();
                _typecache = new SafeDictionary<string, Type>();
                _constrcache = new SafeDictionary<Type, CreateObject>();
                _getterscache = new SafeDictionary<Type, Getters[]>();
                _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
                _genericTypes = new SafeDictionary<Type, Type[]>();
                _genericTypeDef = new SafeDictionary<Type, Type>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}