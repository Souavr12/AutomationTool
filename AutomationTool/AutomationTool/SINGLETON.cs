/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;

namespace AutomationTool.Singleton
{
    /// <summary>
    /// Generic class for creating only one object of the Class provided (Using Lazy Loading of the Singleton Design Pattern)
    /// </summary>
    /// <typeparam name="T">Type Object</typeparam>
    public class SINGLETON<T> where T : class
    {
        #region Members and Properties
        private static readonly Lazy<T> LazyObject = new Lazy<T>(() => InitiateObject());

        /// <summary>
        /// Gets the new object of the Type T
        /// </summary>
        public static T Instance { get { return LazyObject.Value; } }
        #endregion

        #region Methods
        /// <summary>
        /// Creates an instance of T via reflection since T's constructor is expected to be private.
        /// </summary>
        /// <returns>New object of Type T</returns>
        private static T InitiateObject()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }
        #endregion
    }
}
