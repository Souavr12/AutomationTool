/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTool.Converter
{
    /// <summary>
    /// Created By  : Sourav Nanda
    /// Created Date: 06/05/2018
    /// Description : Checks dictionary and creates a safe dictionary.
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public sealed class SafeDictionary<TKey, TValue>
    {
        #region Properties
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary;

        public int Count { get { lock (_Padlock) return _Dictionary.Count; } }

        public TValue this[TKey key]
        {
            get
            {
                lock (_Padlock)
                    return _Dictionary[key];
            }
            set
            {
                lock (_Padlock)
                    _Dictionary[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_Padlock)
            {
                if (_Dictionary.ContainsKey(key) == false)
                    _Dictionary.Add(key, value);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="capacity">Capacity of dictionary</param>
        public SafeDictionary(int capacity)
        {
            _Dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Default Doctionary
        /// </summary>
        public SafeDictionary()
        {
            _Dictionary = new Dictionary<TKey, TValue>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Try Get Value based on the Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>'True' if found else 'False'</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                lock (_Padlock)
                    return _Dictionary.TryGetValue(key, out value);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message + System.Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + System.Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
    }
}