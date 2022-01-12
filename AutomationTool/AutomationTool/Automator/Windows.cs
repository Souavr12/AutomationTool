/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutomationTool.Extension;
using AutomationTool.Native;

namespace AutomationTool.Automator
{
    /// <summary>
    /// Created By: Sourav Nanda
    /// Creation Date: 11/25/2016
    /// Description: Added comments to the method
    /// </summary>
    public class Window
    {
        #region Properties And Variables
        private const int GW_CHILD = 5;
        private const int GW_HWNDNEXT = 2;
        private const int BM_CLICK = 0xf5;
        private const int FW_ALLWINDOWCLASS = 0x0;
        private const int WM_GETTEXT = 0xd;
        private const int WM_GETTEXTLENGTH = 0xe;
        private const int WM_CHAR = 0x102;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int VK_TAB = 0x09;
        private const int VK_RETURN = 0x0D;

        private const int WM_SETTEXT = 0xc;
        private const uint CB_GETLBTEXT = 0x0148;

        private const uint CB_SELECTSTRING = 0x014D;
        private const uint WM_COMMAND = 0x0111;
        public const int CB_SETCURSEL = 0x014E;
        public const int CBN_SELCHANGE = 0x0001;

        static int MakeWParam(int loWord, int hiWord)
        {
            return (loWord & 0xFFFF) + ((hiWord & 0xFFFF) << 16);
        }

        public enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        private static IntPtr Hwnd_select_control_parent = IntPtr.Zero;
        private static IntPtr Hwnd_select_control = IntPtr.Zero;

        
        private const int SMTO_ABORTIFHUNG = 0x2;
        private static Guid IID_IHTMLDocument = new Guid("626FC520-A41E-11CF-A731-00A0C9082637");

        
        #endregion

        #region Public Methods
        /// <summary>
        /// Finds window by title
        /// </summary>
        /// <param name="WindowTitle">Window title to be found</param>
        /// <returns></returns>
        public static IntPtr FindMainWindowHandle(string WindowTitle)
        {
            FindWindowParameters MyParameters = default(FindWindowParameters);
            try
            {
                MyParameters.strTitle = WindowTitle;

                NativeMethods.MyDelegateCallBack myDel = new NativeMethods.MyDelegateCallBack(EnumWindowProc);
                NativeMethods.EnumWindows(myDel, ref MyParameters);
            }
            catch (Exception ex)
            {
                ex.LogError(UniqueId: WindowTitle, MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MyParameters.hWnd;
        }

        /// <summary>
        /// Click button in window by HANDLE
        /// </summary>
        /// <param name="WindowHandle">Handle of the window</param>
        /// <param name="ButtonText">Text of the button</param>
        public static void ClickButtonWindow(IntPtr WindowHandle, string ButtonText)
        {
            try
            {
                do
                {
                    //   Application.DoEvents();
                } while (!(NativeMethods.IsWindowVisible(WindowHandle)));

                IntPtr[] ChildWindows = GetChildWindows(WindowHandle);
                if (ChildWindows == null)
                {
                    return;
                }
                else
                {
                    CheckForChildWindows(ChildWindows, 1, ButtonText, true);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Click button in window by TITLE
        /// </summary>
        /// <param name="WindowTitle">Title of the window</param>
        /// <param name="ButtonText">Text of the button</param>
        public static void ClickButtonWindow(string WindowTitle, string ButtonText)
        {
            try
            {
                IntPtr WindowHandle = IntPtr.Zero;
                do
                {
                    //Application.DoEvents();
                    WindowHandle = CheckForWindow(WindowTitle);
                } while (!(!(WindowHandle == IntPtr.Zero)));

                do
                {
                    // Application.DoEvents();
                } while (!(NativeMethods.IsWindowVisible(WindowHandle)));

                System.Threading.Thread.Sleep(1000);

                ////Interaction.AppActivate(WindowTitle);

                IntPtr[] ChildWindows = GetChildWindows(WindowHandle);
                if (ChildWindows == null)
                {
                    return;
                }
                else
                {
                    CheckForChildWindows(ChildWindows, 1, ButtonText, true);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(UniqueId: WindowTitle, MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Sets value by HANDLE.
        /// </summary>
        /// <param name="WindowHandle">Handle of the window</param>
        /// <param name="TextToSet">Text To Set</param>
        /// <param name="TargetInd">Target Index</param>
        public static void SendTextWindow(IntPtr WindowHandle, string TextToSet, int TargetInd)
        {
            try
            {
                do
                {
                    ////  Application.DoEvents();
                } while (!(NativeMethods.IsWindowVisible(WindowHandle)));

                IntPtr[] ChildWindows = GetChildWindows(WindowHandle);
                if (ChildWindows == null)
                {
                    return;
                }
                else
                {
                    CheckTextForChildWindows(ChildWindows, 1, TextToSet, false, TargetInd);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Sets value by TITLE.
        /// </summary>
        /// <param name="WindowTitle">Title of the window</param>
        /// <param name="TextToSet">Text To Set</param>
        public static void SendTextWindow(string WindowTitle, string TextToSet)
        {
            try
            {
                IntPtr WindowHandle = IntPtr.Zero;
                do
                {
                    ////    Application.DoEvents();
                    WindowHandle = CheckForWindow(WindowTitle);
                } while (!(!(WindowHandle == IntPtr.Zero)));

                do
                {
                    ////   Application.DoEvents();
                } while (!(NativeMethods.IsWindowVisible(WindowHandle)));

                IntPtr[] ChildWindows = GetChildWindows(WindowHandle);
                if (ChildWindows == null)
                {
                    return;
                }
                else
                {
                    CheckForChildWindows(ChildWindows, 1, TextToSet, false);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Gets text by index of the control using window HANLDE
        /// </summary>
        /// <param name="WindowHandle">Handle of the window</param>
        /// <param name="TargetIndex">Target Index</param>
        /// <returns>value from the control</returns>
        public static string GetTextByIndex(IntPtr WindowHandle, int TargetIndex)
        {
            string value = "";
            try
            {
                do
                {
                    ////  Application.DoEvents();
                } while (!(NativeMethods.IsWindowVisible(WindowHandle)));

                IntPtr[] ChildWindows = GetChildWindows(WindowHandle);
                if (ChildWindows == null)
                {
                    value = string.Empty;
                }
                else
                {
                    value = CheckForChildWindows(ChildWindows, TargetIndex);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return value;
        }

        /// <summary>
        /// Gets the text by window title
        /// </summary>
        /// <param name="WindowTitle">Title of the window</param>
        /// <param name="ChildControlType">Control Type</param>
        /// <returns>Text of the window</returns>
        public static string GetTextByTitle(string WindowTitle, string ChildControlType)
        {
            string Text = null;
            IntPtr WindowHandle = IntPtr.Zero;
            try
            {
                do
                {
                    Application.DoEvents();
                    WindowHandle = FindMainWindowHandle(WindowTitle);
                } while (!(!(WindowHandle == IntPtr.Zero)));

                System.Threading.Thread.Sleep(2000);

                IntPtr[] ChildWindows = GetChildWindows(WindowHandle);

                for (int ChildCounter = 0; ChildCounter <= ChildWindows.Length - 1; ChildCounter++)
                {
                    IntPtr CurrentChild = ChildWindows[ChildCounter];
                    string ChildText = GetWindowText(CurrentChild);
                    string ChildClassName = GetWindowClassName(CurrentChild);

                    if (!((string.IsNullOrEmpty(ChildText.Trim())) & (ChildText.Trim() != WindowTitle)))
                    {
                        if (ChildClassName.ToLower() == ChildControlType.ToLower())
                        {
                            Text = ChildText;
                            return Text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return Text;
        }

        /// <summary>
        /// Gets the child windows for the handle provided
        /// </summary>
        /// <param name="ParentWindowHandle">Handle of the parent window</param>
        /// <returns>Array of IntPtr</returns>
        public static IntPtr[] GetChildWindows(IntPtr ParentWindowHandle)
        {
            IntPtr ChildPointer = IntPtr.Zero;
            IntPtr[] ReturnValue = null;
            int ChildCounter = 0;
            try
            {
                //get first child handle... 
                ChildPointer = NativeMethods.GetWindow(ParentWindowHandle, GW_CHILD);

                //loop through and collect all child window handles... 
                while (!(ChildPointer.Equals(IntPtr.Zero)))
                {
                    //process child... 
                    Array.Resize(ref ReturnValue, ChildCounter + 1);
                    ReturnValue[ChildCounter] = ChildPointer;
                    //get next child... 
                    ChildPointer = NativeMethods.GetWindow(ChildPointer, GW_HWNDNEXT);
                    ChildCounter += 1;
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            //return... 
            return ReturnValue;
        }

        /// <summary>
        /// Checks and gets the window by window title.
        /// </summary>
        /// <param name="WindowTitle">Title of the window</param>
        /// <returns>IntPtr</returns>
        public static IntPtr CheckForWindow(string WindowTitle)
        {
            IntPtr FindWindowResult = IntPtr.Zero;
            try
            {
                FindWindowResult = NativeMethods.FindWindow(FW_ALLWINDOWCLASS, WindowTitle);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name, UniqueId: WindowTitle);
            }
            return FindWindowResult;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets window text by handle
        /// </summary>
        /// <param name="WindowHandle">Handle of the window</param>
        /// <returns>value in string</returns>
        public static string GetWindowText(IntPtr WindowHandle)
        {
            IntPtr SendMessageResult = default(IntPtr);
            IntPtr TextLength = default(IntPtr);
            IntPtr length = default(IntPtr);
            //create buffer for return value... 
            System.Text.StringBuilder ControlText = new System.Text.StringBuilder(TextLength.ToInt32() + 1);
            try
            {
                //get length for buffer... 
                TextLength = NativeMethods.SendMessage(WindowHandle, WM_GETTEXTLENGTH, IntPtr.Zero, ref length);
                //create buffer for return value... 
                ControlText = new System.Text.StringBuilder(TextLength.ToInt32() + 1);
                //get window text... 
                SendMessageResult = NativeMethods.SendMessage(WindowHandle, WM_GETTEXT, TextLength.ToInt32() + 1, ControlText);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            //get return value... 
            return ControlText.ToString();
        }

        /// <summary>
        /// Gets window class name for the handle provided
        /// </summary>
        /// <param name="WindowHandle"></param>
        /// <returns></returns>
        private static string GetWindowClassName(IntPtr WindowHandle)
        {
            System.Text.StringBuilder ClassName = new System.Text.StringBuilder(255);
            try
            {
                NativeMethods.GetClassName(WindowHandle, ClassName, ClassName.Capacity);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return ClassName.ToString();
        }

        /// <summary>
        /// Delegat to triger and get the window
        /// </summary>
        /// <param name="hWnd">Gets the handle</param>
        /// <param name="lParam">Parameter for find the window</param>
        /// <returns>IntPtr</returns>
        private static IntPtr EnumWindowProc(IntPtr hWnd, ref FindWindowParameters lParam)
        {
            IntPtr functionReturnValue = default(IntPtr);
            IntPtr SendMessageResult = default(IntPtr);
            IntPtr TextLength = default(IntPtr);
            IntPtr length = default(IntPtr);
            try
            {
                //get length for buffer... 
                TextLength = NativeMethods.SendMessage(hWnd, WM_GETTEXTLENGTH, IntPtr.Zero, ref length); //ref IntPtr.Zero

                //create buffer for return value... 
                if (TextLength.ToInt32() > 0)
                {
                    System.Text.StringBuilder ControlText = new System.Text.StringBuilder(TextLength.ToInt32() + 1);

                    //get window text... 
                    SendMessageResult = NativeMethods.SendMessage(hWnd, WM_GETTEXT, TextLength.ToInt32() + 1, ControlText);

                    //Debug.Print("|" & ControlText.ToString & "|")
                    //if (ControlText.ToString().Contains(lParam.strTitle))
                    if (ControlText.ToString().Trim() == lParam.strTitle.Trim())
                    {
                        lParam.hWnd = hWnd;
                        functionReturnValue = IntPtr.Zero;
                    }
                    else
                    {
                        functionReturnValue = new IntPtr(1);
                    }
                }
                else
                {
                    functionReturnValue = new IntPtr(1);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return functionReturnValue;
        }

        /// <summary>
        /// Checks text for the child window.
        /// </summary>
        /// <param name="MyList">List of child handles</param>
        /// <param name="NumTabs">No. of tabs required</param>
        /// <param name="ButtonText">Text of  the control</param>
        /// <param name="UseButton">Checks whether to user button or not</param>
        /// <param name="TargetInd">Target Index</param>
        private static void CheckTextForChildWindows(IntPtr[] MyList, int NumTabs, string ButtonText, bool UseButton, int TargetInd)
        {
            try
            {
                for (int ChildCounter = 0; ChildCounter <= MyList.Length - 1; ChildCounter++)
                {
                    IntPtr CurrentChild = MyList[ChildCounter];
                    string ChildText = GetWindowText(CurrentChild);
                    string ChildClassName = GetWindowClassName(CurrentChild);

                    if (ChildCounter == TargetInd)
                    {
                        if (!ChildClassName.Contains("COMBOBOX"))
                        {

                            if (TargetInd == 100 || TargetInd == 101)
                            {
                                // SetEditText(CurrentChild, ButtonText);
                                SetComboItem(CurrentChild, ButtonText);
                            }
                            else
                            {
                                // DCN , scan date , scan time , DOB field,  department received date and time
                                if (TargetInd == 99 || TargetInd == 14 || TargetInd == 17 || TargetInd == 18 || TargetInd == 19 || TargetInd == 20)
                                {
                                    string checkstatus = "NO";
                                    if (TargetInd == 99)
                                    {
                                        checkstatus = "YES";  // Only for unique image id - clear text and paste 
                                    }

                                    //SetEditTextunique(CurrentChild, ButtonText);
                                    SendTextToMaskControls(CurrentChild, ButtonText, checkstatus);
                                }
                                else
                                {
                                    SetEditText(CurrentChild, ButtonText);
                                }
                            }
                            return;
                        }
                        else
                        {
                            SetComboItem(CurrentChild, ButtonText);
                            return;
                        }
                    }

                    IntPtr[] ChildWindows = GetChildWindows(CurrentChild);
                    if (ChildWindows == null)
                    {
                        continue;
                    }
                    else
                    {
                        CheckTextForChildWindows(ChildWindows, NumTabs + 1, ButtonText, UseButton, TargetInd);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Checks for the child window.
        /// </summary>
        /// <param name="MyList">List of child handles</param>
        /// <param name="NumTabs">No. of tabs required</param>
        /// <param name="ButtonText">Text of  the control</param>
        /// <param name="UseButton">Checks whether to user button or not</param>
        private static void CheckForChildWindows(IntPtr[] MyList, int NumTabs, string ButtonText, bool UseButton)
        {
            try
            {
                for (int ChildCounter = 0; ChildCounter <= MyList.Length - 1; ChildCounter++)
                {
                    IntPtr CurrentChild = MyList[ChildCounter];
                    string ChildText = GetWindowText(CurrentChild);
                    string ChildClassName = GetWindowClassName(CurrentChild);
                    if (UseButton)
                    {
                        if (ChildClassName == "Button" && (ChildText == ButtonText || ChildText == "&" + ButtonText))
                        {
                            ClickButton(CurrentChild);
                            return;
                        }
                    }
                    else
                    {
                        if (ChildClassName == "Edit")
                        {
                            SetEditText(CurrentChild, ButtonText);
                            return;
                        }
                    }

                    IntPtr[] ChildWindows = GetChildWindows(CurrentChild);
                    if (ChildWindows == null)
                    {
                        continue;
                    }
                    else
                    {
                        CheckForChildWindows(ChildWindows, NumTabs + 1, ButtonText, UseButton);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Checks for the child window.
        /// </summary>
        /// <param name="MyList">List of child handles</param>
        /// <param name="TargetIndex">Target Index</param>
        /// <returns></returns>
        private static string CheckForChildWindows(IntPtr[] MyList, int TargetIndex)
        {
            try
            {
                for (int ChildCounter = 0; ChildCounter <= MyList.Length - 1; ChildCounter++)
                {
                    IntPtr CurrentChild = MyList[ChildCounter];
                    string ChildText = GetWindowText(CurrentChild);
                    string ChildClassName = GetWindowClassName(CurrentChild);

                    if (ChildCounter == TargetIndex)
                    {
                        return ChildText;
                    }

                    IntPtr[] ChildWindows = GetChildWindows(CurrentChild);
                    if (ChildWindows == null)
                    {
                        continue;
                    }
                    else
                    {
                        return CheckForChildWindows(ChildWindows, TargetIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return string.Empty;
        }

        /// <summary>
        /// Clicks button by control handle
        /// </summary>
        /// <param name="ButtonHandle">Handle of the control</param>
        private static void ClickButton(IntPtr ButtonHandle)
        {
            try
            {
                IntPtr length = default(IntPtr);
                NativeMethods.SendMessage(ButtonHandle, BM_CLICK, IntPtr.Zero, ref length);
                NativeMethods.SendMessage(ButtonHandle, BM_CLICK, IntPtr.Zero, ref length);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Sets value by unique id.
        /// </summary>
        /// <param name="EditHandle">Handle for text box</param>
        /// <param name="TextToSet">value to set</param>
        private static void UniqueIDText(IntPtr EditHandle, string TextToSet)
        {
            try
            {
                IntPtr intPtrOne = new IntPtr(1);
                char[] charsToSend = TextToSet.ToCharArray();
                for (int i = 0; i <= charsToSend.Length - 1; i++)
                {
                    // IntPtr res = SendMessage(handle, WM_CHAR, new IntPtr(Strings.Asc(charsToSend(i))), intPtrOne);
                    // SendMessage(EditHandle, WM_CHAR, new IntPtr(Strings.Asc(charsToSend(i))), intPtrOne);

                }



                for (int i = 0; i <= TextToSet.Length - 1; i++)
                {
                    NativeMethods.SendMessage(EditHandle, WM_SETTEXT, 0, TextToSet);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        ///  Sets value by text.
        /// </summary>
        /// <param name="EditHandle">Handle for text box</param>
        /// <param name="TextToSet">value to set</param>
        private static void SetEditTextunique(IntPtr EditHandle, string TextToSet)
        {
            try
            {
                for (int i = 0; i <= TextToSet.Length - 1; i++)
                {
                    string strchar = Convert.ToString(TextToSet.ElementAt(i));
                    NativeMethods.SendMessage(EditHandle, WM_SETTEXT, i, strchar);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        ///  Sets value to masked controls.
        /// </summary>
        /// <param name="handle">Handle for text box</param>
        /// <param name="uniqueID">Unique ID</param>
        /// <param name="chkflag">Checking Flag</param>
        private static void SendTextToMaskControls(IntPtr handle, string uniqueID, string chkflag)
        {
            try
            {
                char[] charsToSend = uniqueID.ToCharArray();
                IntPtr intPtrOne = new IntPtr(1);

                if (chkflag == "YES")
                {
                    NativeMethods.SendMessage(handle, WM_SETTEXT, 0, "");
                }
                for (int i = 0; i <= charsToSend.Length - 1; i++)
                {

                    IntPtr res = NativeMethods.SendMessage(handle, WM_CHAR, (IntPtr)charsToSend[i], intPtrOne);
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Set value to text box
        /// </summary>
        /// <param name="EditHandle">Handle for text box</param>
        /// <param name="TextToSet">value to set</param>
        private static void SetEditText(IntPtr EditHandle, string TextToSet)
        {
            try
            {
                NativeMethods.SendMessage(EditHandle, WM_SETTEXT, 0, TextToSet);  //WM_SETTEXT
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Sets event to combo box
        /// </summary>
        /// <param name="EditHandle">Handle for text box</param>
        /// <param name="combotext">value to set in combo box</param>
        private static void SetComboChangeEvent(IntPtr EditHandle, string combotext)
        {
            try
            {
                NativeMethods.SendMessage(EditHandle, (int)CB_SELECTSTRING, -1, combotext);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Sets value and triggers the event
        /// </summary>
        /// <param name="EditHandle">Handle for text box</param>
        /// <param name="TextToSet">value to set</param>
        private static void SetAndTriggerTextchanged(IntPtr EditHandle, string TextToSet)
        {
            try
            {
                NativeMethods.SendMessage(EditHandle, WM_SETTEXT, 0, TextToSet);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Set value to combo box
        /// </summary>
        /// <param name="EditHandle">Handle for text box</param>
        /// <param name="combotext">value to set in combo box</param>
        private static void SetComboItem(IntPtr EditHandle, string combotext)
        {
            try
            {
                // IntPtr nID = GetWindowLongPtr(Hwnd_select_control, (int)GWL.GWL_ID);
                // int ctrl_id = nID.ToInt32();

                NativeMethods.SendMessage(EditHandle, (int)CB_SELECTSTRING, -1, combotext);
                //SendMessage(EditHandle, (int)CBN_SELCHANGE, -1, combotext);

                int send_cbn_selchange = MakeWParam((int)EditHandle, CBN_SELCHANGE);

                Hwnd_select_control_parent = FindMainWindowHandle("Citrus Intake - Processing");
                NativeMethods.SendMessage(Hwnd_select_control_parent, 0x111 /* WM_COMMAND */, send_cbn_selchange, (int)EditHandle);

            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Gets HTML Document from the Window Handler.
        /// </summary>
        /// <param name="WindowTitle">Title of the window</param>
        /// <returns>HTML Body of the window</returns>
        public static mshtml.IHTMLDocument2 GetHTMLDocumentFromWindowHandler(string WindowTitle)
        {
            mshtml.IHTMLDocument2 document = null;
            try
            {

                IntPtr hWnd = IntPtr.Zero;
                int lngMsg = 0;
                int lRes;
                int hr = 0;
                hWnd = NativeMethods.FindWindowByCaption(IntPtr.Zero, WindowTitle);

                if (hWnd != IntPtr.Zero)
                {
                    IntPtr[] ChildWindows = GetChildWindows(hWnd);
                    lngMsg = NativeMethods.RegisterWindowMessage("WM_HTML_GETOBJECT");
                    if (lngMsg != 0)
                    {
                        if (ChildWindows != null && ChildWindows.Length > 0)
                        {
                            NativeMethods.SendMessageTimeout(ChildWindows[0], lngMsg, 0, 0, SMTO_ABORTIFHUNG, 1000, out lRes);
                            if (!(bool)(lRes == 0))
                                hr = NativeMethods.ObjectFromLresult(lRes, ref IID_IHTMLDocument, 0, ref document);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return document;
        }
        #endregion
    }
}
