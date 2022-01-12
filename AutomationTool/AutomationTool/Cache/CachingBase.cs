/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTool.Cache
{
    public abstract class CachingBase : IDisposable
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public CachingBase()
        {
            DeleteLog();
        }
        #endregion

        #region Declarations
        string LogPath = System.Environment.GetEnvironmentVariable("TEMP");
        protected MemoryCache cache = new MemoryCache("Cache_" + System.Windows.Forms.Application.ProductName);

        static readonly object padlock = new object();
        #endregion

        #region Methods
        /// <summary>
        /// Adds Items to cache
        /// </summary>
        /// <param name="Key">Key for cache</param>
        /// <param name="Value">Value for cache</param>
        /// <param name="ExpirationTimeLimit">Time limit of the cache to expire</param>
        protected virtual void AddItem(string Key, object Value, DateTimeOffset ExpirationTimeLimit)
        {
            lock (padlock)
            {
                cache.Add(Key, Value, ExpirationTimeLimit);
            }
        }

        /// <summary>
        /// Removes the cache
        /// </summary>
        /// <param name="Key">Key to remove</param>
        protected virtual void RemoveItem(string Key)
        {
            lock (padlock)
            {
                cache.Remove(Key);
            }
        }

        /// <summary>
        /// Get the item from cache
        /// </summary>
        /// <param name="Key">Key for cache</param>
        /// <param name="Remove">'True' if removal required else 'False'</param>
        /// <returns>object from the Cache</returns>
        protected virtual object GetItem(string Key, bool Remove)
        {
            lock (padlock)
            {
                var res = cache[Key];

                if (res != null)
                {
                    if (Remove == true)
                        cache.Remove(Key);
                }
                else
                    WriteToLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Don't contains key: " + Key);

                return res;
            }
        }
        #endregion

        #region Error Logs
        /// <summary>
        /// Deletes the error log
        /// </summary>
        protected void DeleteLog()
        {
            string LogFilePath = LogPath + "\\MacroCacheLog_" + System.Windows.Forms.Application.ProductName + ".txt";
            if (System.IO.File.Exists(LogFilePath))
                System.IO.File.Delete(LogFilePath);
        }

        /// <summary>
        /// Writes the error log
        /// </summary>
        /// <param name="text">Text to log the error</param>
        protected void WriteToLog(string text)
        {
            string LogFilePath = LogPath + "\\MacroCacheLog_" + System.Windows.Forms.Application.ProductName + ".txt";
            using (System.IO.TextWriter tw = System.IO.File.AppendText(LogFilePath))
            {
                tw.WriteLine(text);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                cache.Dispose();
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CachingBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
}
