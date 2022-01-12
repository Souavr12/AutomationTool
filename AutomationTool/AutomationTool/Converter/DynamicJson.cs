/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   

#if net4
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace AutomationTool.Converter
{
    internal class DynamicJson : DynamicObject, IEnumerable
    {
        private IDictionary<string, object> _dictionary { get; set; }
        private List<object> _list { get; set; }

        /// <summary>
        /// parameterized constructors
        /// </summary>
        /// <param name="json">JSON String</param>
        public DynamicJson(string json)
        {
            try
            {
                var parse = Utility.Converter.JSON.Parse(json);

                if (parse is IDictionary<string, object>)
                    _dictionary = (IDictionary<string, object>)parse;
                else
                    _list = (List<object>)parse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// parameterized constructors
        /// </summary>
        /// <param name="json">JSON String</param>
        private DynamicJson(object dictionary)
        {
            try
            {
                if (dictionary is IDictionary<string, object>)
                    _dictionary = (IDictionary<string, object>)dictionary;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the dynamic member names
        /// </summary>
        /// <returns>IEnumerable</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            try
            {
                return _dictionary.Keys.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// checks the value is available or not
        /// </summary>
        /// <param name="binder">Index Binder</param>
        /// <param name="indexes">index array</param>
        /// <param name="result">Object</param>
        /// <returns>boolean</returns>
        public override bool TryGetIndex(GetIndexBinder binder, Object[] indexes, out Object result)
        {
            try
            {
                var index = indexes[0];
                if (index is int)
                {
                    result = _list[(int)index];
                }
                else
                {
                    result = _dictionary[(string)index];
                }
                if (result is IDictionary<string, object>)
                    result = new DynamicJson(result as IDictionary<string, object>);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// checks the value is available or not
        /// </summary>
        /// <param name="binder">Index Binder</param>
        /// <param name="result">Object</param>
        /// <returns>boolean</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                if (_dictionary.TryGetValue(binder.Name, out result) == false)
                    if (_dictionary.TryGetValue(binder.Name.ToLower(), out result) == false)
                        return false;// throw new Exception("property not found " + binder.Name);

                if (result is IDictionary<string, object>)
                {
                    result = new DynamicJson(result as IDictionary<string, object>);
                }
                else if (result is List<object>)
                {
                    List<object> list = new List<object>();
                    foreach (object item in (List<object>)result)
                    {
                        if (item is IDictionary<string, object>)
                            list.Add(new DynamicJson(item as IDictionary<string, object>));
                        else
                            list.Add(item);
                    }
                    result = list;
                }

                return _dictionary.ContainsKey(binder.Name);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the enumerator
        /// </summary>
        /// <returns>IEnumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var o in _list)
            {
                yield return new DynamicJson(o as IDictionary<string, object>);
            }
        }
    }
}
#endif