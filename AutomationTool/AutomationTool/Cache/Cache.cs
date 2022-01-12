/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;

namespace AutomationTool.Cache
{
    /// <summary>
    /// Caches the data in TEMP folder
    /// </summary>
    public class Cache : CachingBase, Interfaces.IGlobalCaching
    {
        #region Singleton
        private Cache()
        {
        }

        public static Cache Instance
        {
            get
            {
                //return Nested.instance;
                return Singleton.SINGLETON<Cache>.Instance;
            }
        }

        //class Nested
        //{
        //    // Explicit static constructor to tell C# compiler
        //    // not to mark type as beforefieldinit
        //    static Nested()
        //    {
        //    }
        //    internal static readonly Cache instance = new Cache();
        //}
        #endregion

        #region ICachingProvider

        /// <summary>
        /// Adds item to the Cache
        /// </summary>
        /// <param name="Key">Key of the Cache</param>
        /// <param name="Value">Value of the Cache</param>
        public virtual new void AddItem(string Key, object Value, DateTimeOffset ExpirationTimeLimit)
        {
            base.AddItem(Key, Value, ExpirationTimeLimit);
        }

        /// <summary>
        /// Gets the value based on the Key provided
        /// </summary>
        /// <param name="Key">Key of the object</param>
        /// <param name="Remove">'True' if remove required else 'False'</param>
        /// <returns>Returns the object</returns>
        public virtual new object GetItem(string Key, bool Remove)
        {
            return base.GetItem(Key, Remove);
        }

        /// <summary>
        /// Removes the cache
        /// </summary>
        /// <param name="Key">Key to remove</param>
        public virtual new void RemoveItem(string Key)
        {
            base.RemoveItem(Key);
        }
        #endregion
    }
}
