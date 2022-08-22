/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using Accessibility;
using AutomationTool.Extension;
using AutomationTool.Native;

namespace AutomationTool.Activators
{
    /// <summary>
    /// Modified By: Sourav Nanda
    /// Modified Date: 11/25/2016
    /// Description: Added comments to the method
    /// </summary>
    internal class TabActivator
    {
        #region Nested type: OBJID
        /// <summary>
        /// Enum for getting the OBJECT ID
        /// </summary>
        private enum OBJID : uint
        {
            OBJID_WINDOW = 0x00000000,
        }
        #endregion

        #region Declarations And Properties.
        private const int CHILDID_SELF = 0;
        private readonly IntPtr _hWnd;
        private IAccessible _accessible;
        
        /// <summary>
        /// Gte the chidern of the TAB
        /// </summary>
        private TabActivator[] Children
        {
            get
            {
                var num = 0;
                var res = GetAccessibleChildren(_accessible, out num);

                if (res == null)
                    return new TabActivator[0];

                var list = new List<TabActivator>(res.Length);

                foreach (object obj in res)
                {
                    var acc = obj as IAccessible;

                    if (acc != null)
                        list.Add(new TabActivator(acc));
                }

                return list.ToArray();
            }
        }

        /// <summary>
        /// Gte the count of the children present.
        /// </summary>
        private int ChildCount
        {
            get { return _accessible.accChildCount; }
        }

        /// <summary>
        /// Gets LocationUrl of the tab
        /// </summary>
        private string LocationUrl
        {
            get
            {
                var url = _accessible.accDescription[CHILDID_SELF];

                if (url.Contains(Environment.NewLine))
                    url = url.Split('\n')[1];

                return url;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// constructor getting Handle.
        /// </summary>
        /// <param name="ieHandle">Gets the Handle of the IE Browser</param>
        internal TabActivator(IntPtr ieHandle)
        {
            _hWnd = ieHandle;
            AccessibleObjectFromWindow(GetDirectUIHWND(ieHandle), OBJID.OBJID_WINDOW, ref _accessible);

            CheckForAccessible();
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="acc">Gets the 'IAccessible' interface object</param>
        private TabActivator(IAccessible acc)
        {
            if (acc == null)
                throw new Exception("Could not get accessible");

            _accessible = acc;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks for accesibility.
        /// </summary>
        private void CheckForAccessible()
        {
            try
            {
                if (_accessible == null)
                    throw new Exception("Could not get accessible.  Accessible can't be null");
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Activates the tabs by URL.
        /// </summary>
        /// <param name="tabsUrl">Get the URL of the Application</param>
        internal void ActivateByTabsUrl(string tabsUrl)
        {
            try
            {
                var tabIndexToActivate = GetTabIndexToActivate(tabsUrl);

                AccessibleObjectFromWindow(GetDirectUIHWND(_hWnd), OBJID.OBJID_WINDOW, ref _accessible);

                CheckForAccessible();

                var index = 0;
                var ieDirectUIHWND = new TabActivator(_hWnd);

                foreach (var accessor in ieDirectUIHWND.Children)
                {
                    foreach (var child in accessor.Children)
                    {
                        foreach (var tab in child.Children)
                        {
                            if (tabIndexToActivate >= child.ChildCount - 1)
                                return;

                            if (index == tabIndexToActivate)
                            {
                                tab.ActivateTab();
                                return;
                            }

                            index++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Activates tabs
        /// </summary>
        private void ActivateTab()
        {
            try
            {
                _accessible.accDoDefaultAction(CHILDID_SELF);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Gets tabindex to activate.
        /// </summary>
        /// <param name="tabsUrl">Get the URL of the Application</param>
        /// <returns>integer</returns>
        private int GetTabIndexToActivate(string tabsUrl)
        {
            try
            {
                AccessibleObjectFromWindow(GetDirectUIHWND(_hWnd), OBJID.OBJID_WINDOW, ref _accessible);

                CheckForAccessible();

                var index = 0;
                var ieDirectUIHWND = new TabActivator(_hWnd);

                foreach (var accessor in ieDirectUIHWND.Children)
                {
                    foreach (var child in accessor.Children)
                    {
                        foreach (var tab in child.Children)
                        {
                            try
                            {
                                if (tab.ChildCount > 0)
                                {
                                    foreach (var tabChild in tab.Children)
                                    {
                                        if (!string.IsNullOrEmpty(tabChild.LocationUrl))
                                        {
                                            var tabUrl = tabChild.LocationUrl;

                                            if (!string.IsNullOrEmpty(tabUrl))
                                            {
                                                if (tabChild.LocationUrl.Contains(tabsUrl))
                                                    return index;
                                            }

                                            index++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(tab.LocationUrl))
                                    {
                                        var tabUrl = tab.LocationUrl;

                                        if (!string.IsNullOrEmpty(tabUrl))
                                        {
                                            if (tab.LocationUrl.Contains(tabsUrl))
                                                return index;
                                        }

                                        index++;
                                    }
                                }

                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return -1;
        }

        /// <summary>
        /// Gets HWND.
        /// </summary>
        /// <param name="ieFrame">Frame Handle</param>
        /// <returns>IntPtr</returns>
        private IntPtr GetDirectUIHWND(IntPtr ieFrame)
        {
            IntPtr directUI = IntPtr.Zero;
            try
            {
                // For IE 8:
                directUI = NativeMethods.FindWindowEx(ieFrame, IntPtr.Zero, "CommandBarClass", null);
                directUI = NativeMethods.FindWindowEx(directUI, IntPtr.Zero, "ReBarWindow32", null);
                directUI = NativeMethods.FindWindowEx(directUI, IntPtr.Zero, "TabBandClass", null);
                directUI = NativeMethods.FindWindowEx(directUI, IntPtr.Zero, "DirectUIHWND", null);

                if (directUI == IntPtr.Zero)
                {
                    // For IE 9:
                    //directUI = FindWindowEx(ieFrame, IntPtr.Zero, "WorkerW", "Navigation Bar");
                    directUI = NativeMethods.FindWindowEx(ieFrame, IntPtr.Zero, "WorkerW", null);
                    directUI = NativeMethods.FindWindowEx(directUI, IntPtr.Zero, "ReBarWindow32", null);
                    directUI = NativeMethods.FindWindowEx(directUI, IntPtr.Zero, "TabBandClass", null);
                    directUI = NativeMethods.FindWindowEx(directUI, IntPtr.Zero, "DirectUIHWND", null);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return directUI;
        }

        /// <summary>
        /// Gets accessible object from window.
        /// </summary>
        /// <param name="hwnd">Handle of the browser</param>
        /// <param name="idObject">OBJECT ID</param>
        /// <param name="acc">IAccessible interface</param>
        /// <returns>integer</returns>
        private static int AccessibleObjectFromWindow(IntPtr hwnd, OBJID idObject, ref IAccessible acc)
        {
            var num = default(Int32);
            try
            {
                var guid = new Guid("{618736e0-3c3d-11cf-810c-00aa00389b71}"); // IAccessibleobject obj = null;
                object obj = null;

                num = NativeMethods.AccessibleObjectFromWindow(hwnd, (uint)idObject, ref guid, ref obj);

                acc = (IAccessible)obj;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return num;
        }

        /// <summary>
        /// Gets accessible children
        /// </summary>
        /// <param name="ao">IAccessible interface</param>
        /// <param name="childs">child count</param>
        /// <returns>Array of OBJECTS</returns>
        private static object[] GetAccessibleChildren(IAccessible ao, out int childs)
        {
            childs = 0;
            object[] ret = null;
            try
            {
                var count = ao.accChildCount;

                if (count > 0)
                {
                    ret = new object[count];
                    NativeMethods.AccessibleChildren(ao, 0, count, ret, out childs);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return ret;
        }
        #endregion
    }
}