/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;

namespace AutomationTool.Interfaces
{
    /// <summary>
    /// Interface to add item to the cache and get item from the cache.
    /// </summary>
    public interface IGlobalCaching
    {
        void AddItem(string Key, object Value, DateTimeOffset ExpirationTimeLimit);

        void RemoveItem(string Key);
        object GetItem(string Key, bool Remove);
    }
}
