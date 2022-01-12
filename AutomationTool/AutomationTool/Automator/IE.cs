/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using SHDocVw;
using mshtml;
using AutomationTool.Extension;
using AutomationTool.Forms;
using AutomationTool.Activators;
using AutomationTool.Native;
using AutomationTool.Constants;
using AutomationTool.Structs;

namespace AutomationTool.Automator
{
    /// <summary>
    /// Created By: Sourav Nanda
    /// Creation Date: 11/25/2016
    /// Description: Added description for the parameters of the emthod.
    /// </summary>
    public abstract class IE
    {
        #region properties
        public mshtml.IHTMLDocument2 _MyDocument { get; set; }
        public SHDocVw.InternetExplorer _MyWindow { get; set; }
        public int WindowCount { get; set; }
        public string SelectedWindowURL { get; set; }
        #endregion

        #region Abstract Method
        /// <summary>
        /// Abstract method for checking Connection for particular method.
        /// </summary>
        /// <param name="URL">Receives an array of URLs for which window needs to be found</param>
        /// <param name="Error">Returns back the error if any occured</param>
        /// <returns>Returs value as 'true' for window found else 'false'</returns>
        public abstract bool Connect(out string Error, params string[] URL);

        /// <summary>
        /// Checks session of GPS.
        /// </summary>
        /// <param name="Error">Returns back the error if any occured</param>
        /// <returns>Returs value as 'true' for window found else 'false'</returns>
        public abstract bool ToolSession(out string Error);
        #endregion

        #region Get Window
        /// <summary>
        /// To get the IE with the specified URL
        /// </summary>
        /// <param name="Url">Receies an url for which window need to be found</param>
        /// <param name="Error">Returns back the error if any occured</param>
        /// <returns>Returs value of 'InternetExplorer' type for window found</returns>
        public InternetExplorer GetWindow(string Url, out string Error)
        {
            InternetExplorer rtnValue = null;
            Error = string.Empty;
            ShellWindows windowShell = new ShellWindows();
            WindowCount = 0;
            try
            {
                if (windowShell.Count > 0)
                {
                    for (int i = 0; windowShell.Count > i; i++)
                    {
                        try
                        {
                            InternetExplorer ieWindow = windowShell.Item(i);
                            if (ieWindow != null)
                            {
                                if (ieWindow.LocationURL.ToString().Contains(Url))
                                {
                                    rtnValue = ieWindow;
                                    WindowCount++;
                                    return rtnValue;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            finally
            {
                if (windowShell != null)
                {
                    releaseObject(windowShell, out Error);
                }
            }
            return rtnValue;
        }

        /// <summary>
        /// Activates the window.
        /// </summary>
        /// <param name="Error">Returns back the error if any occured</param>
        public void ActivateWindow(string URL)
        {
            try
            {
                TabActivator tab = new TabActivator((IntPtr)_MyWindow.HWND);
                tab.ActivateByTabsUrl(URL);
                NativeMethods.ShowWindowAsync((IntPtr)_MyWindow.HWND, 3);
                NativeMethods.SetForegroundWindowNative((IntPtr)_MyWindow.HWND);

                //IntPtr hWnd = IntPtr.Zero;
                ////For IE with tab -- start
                //_MyWindow = GetWindows(UrlPart: URL);
                //uint ProcID = 0;
                //GetWindowThreadProcessId((IntPtr)_MyWindow.HWND, out ProcID);
                //hWnd = (IntPtr)_MyWindow.HWND;
                ////End            
                //new TabActivator(hWnd).ActivateByTabsUrl(URL);

                //Interaction.AppActivate((int)ProcID);
                //ShowWindow(_MyWindow.HWND, 9);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Getting window of passed url and storing into internet explorer windo MyWindow to get the document 
        /// </summary>
        /// <param name="TitlePart">Gets the title part for which application needs to be found</param>
        /// <param name="UrlPart">Gets the array of url for which windows need to be found</param>
        /// <returns>Returs value of 'InternetExplorer' type for window found</returns>
        public InternetExplorer GetWindows(string TitlePart = "", List<string> TitleList = null, params string[] UrlPart)
        {
            List<SHDocVw.InternetExplorer> OpenWindows = new List<InternetExplorer>();
            SHDocVw.InternetExplorer ReturnWindow = null;
            SHDocVw.ShellWindows WindowShell = null;
            do
            {
                System.Threading.Thread.Sleep(200);
                try
                {
                    WindowShell = new SHDocVw.ShellWindows();
                }
                catch (Exception ex)
                {
                    ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
                }
            } while (WindowShell == null);

            try
            {
                if (WindowShell.Count > 0)
                {
                    List<string> TotalTitleList = new List<string>();
                    if (TitleList != null && TitleList.Count > 0)
                        TotalTitleList = TitleList;
                    else if (!string.IsNullOrEmpty(TitlePart))
                        TotalTitleList.Add(TitlePart);

                    if (TotalTitleList != null && TotalTitleList.Count > 0)
                        TotalTitleList = TotalTitleList.Distinct().ToList();

                    for (int X = 0; X <= WindowShell.Count - 1; X++)
                    {
                        try
                        {
                            SHDocVw.InternetExplorer myWindow = WindowShell.Item(X);
                            if (myWindow != null)
                            {
                                if (UrlPart.Length > 0)
                                {
                                    if (!myWindow.LocationURL.ToLower().StartsWith("file:/"))
                                    {
                                        UrlPart.AsEnumerable().Distinct().Where(url => myWindow.LocationURL.ToLower().Contains(url.ToLower()))
                                              .ToList().ForEach(lst => OpenWindows.Add(myWindow));
                                        if (OpenWindows != null && OpenWindows.Count > 0)
                                        {
                                            if (TotalTitleList != null && TotalTitleList.Count > 0)
                                                OpenWindows = OpenWindows.FindAll(lst => lst.Document != null && TotalTitleList.Any(lst1 => !string.IsNullOrEmpty(lst1) && lst.Document.title.ToString().Trim().ToLower().Contains(lst1.ToLower())));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
                        }
                    }
                }
                if (OpenWindows.Count == 0)
                {
                    ReturnWindow = null;
                }
                else if (OpenWindows.Count == 1)
                {
                    ReturnWindow = OpenWindows[0];
                }
                else
                {
                    ReturnWindow = null;
                    SelectionForm WindowSelector = new SelectionForm("Select a Window", true, false, false);
                    WindowSelector.LoadList(ref OpenWindows);

                    System.Windows.Forms.DialogResult SelectionResult = WindowSelector.ShowDialog();
                    if (SelectionResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        ReturnWindow = null;
                    }
                    else
                    {
                        ReturnWindow = OpenWindows[WindowSelector.ResultsList.SelectedRows[0].Index];
                    }
                }

                if (ReturnWindow != null)
                    SelectedWindowURL = ((IHTMLDocument2)ReturnWindow.Document).domain;

                if (WindowShell != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(WindowShell);
                }
                WindowShell = null;
                OpenWindows = null;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return ReturnWindow;
        }
        #endregion

        #region Get Values
        /// <summary>
        /// getting the text in applications by using field name
        /// </summary>
        /// <param name="MyFieldName">Gets the field name as 'ID', 'Name', 'TAG NAME'</param>
        /// <param name="MyFieldType">Gets the type of the field</param>
        /// <param name="MyPropertyType">Gets the property for which value to be extracted</param>
        /// <param name="isExactMatchReq">Gets value whether exact match needed or not</param>
        /// <param name="ItemLabelName">Gets the label name for which value to be captured</param>
        /// <param name="ControlIndex">Gets the control number for the collection of controls</param>
        /// <param name="TargetDocument">Gets the target document inside which value is present</param>
        /// <param name="NextItemRequired">'True' if next control/item/value required</param>
        /// <param name="AttrName">Name of attribute</param>
        /// <param name="webBrowser">If document is a Windows control WebBrowser</param>
        /// <returns>Returns the value of type 'BrowserResult'</returns>
        public virtual BrowserResult<T> GetValues<T>(string MyFieldName, FieldType MyFieldType, FieldProperty MyPropertyType, bool isExactMatchReq = true, string ItemLabelName = "", int ControlIndex = 0, IHTMLDocument3 TargetDocument = null, bool NextItemRequired = true, string AttrName = "", System.Windows.Forms.WebBrowser webBrowser = null)
        {
            BrowserResult<T> MyResult = new BrowserResult<T>();
            IHTMLElement thisElement = null;
            IHTMLElementCollection thisCol;
            IHTMLDocument3 thisDoc;
            try
            {
                if (TargetDocument == null)
                    thisDoc = (IHTMLDocument3)_MyDocument;
                else
                    thisDoc = TargetDocument;

                if (thisDoc != null)
                {
                    switch (MyFieldType)
                    {
                        case FieldType.ftID:
                            thisElement = (thisDoc).getElementById(MyFieldName);
                            break;
                        case FieldType.ftName:
                            thisCol = (thisDoc).getElementsByName(MyFieldName);
                            if (thisCol.length == 0)
                                thisElement = null;
                            else
                                thisElement = thisCol.item(ControlIndex);
                            //thisElement = GetElement(thisCollection: thisCol, MyPropertyType: MyPropertyType, LabelName: ItemLabelName, isExactMatchReq: isExactMatchReq, index: ControlIndex);
                            break;
                        case FieldType.ftTagName:
                            thisCol = thisDoc.getElementsByTagName(MyFieldName);
                            if (thisCol.length == 0)
                                thisElement = null;
                            else
                                thisElement = GetElement<T>(NextItemRequired: NextItemRequired, thisCollection: thisCol, MyPropertyType: MyPropertyType, LabelName: ItemLabelName, isExactMatchReq: isExactMatchReq, AttributeName: AttrName);
                            break;
                        default:
                            thisElement = null;
                            break;
                    }
                }

                if (thisElement == null)
                {
                    MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: "Element " + MyFieldName + " not found");
                }
                else
                {
                    switch (MyPropertyType)
                    {
                        case FieldProperty.fpValue:
                            if (thisElement.getAttribute("value") != null)
                                MyResult.SetResult(true, thisElement.getAttribute("value").ToString().Trim(), string.Empty);
                            else
                                MyResult.SetResult(false, "", string.Empty);
                            break;
                        case FieldProperty.fpInnerHTML:
                            if (thisElement.innerHTML != null)
                                MyResult.SetResult(true, thisElement.innerHTML.Trim(), string.Empty);
                            else
                                MyResult.SetResult(false, string.Empty, "No value found");
                            break;
                        case FieldProperty.fpInnerText:
                            if (thisElement.innerText != null)
                                MyResult.SetResult(true, thisElement.innerText.Trim(), string.Empty);
                            else
                                MyResult.SetResult(false, string.Empty, "No value found");
                            break;
                        case FieldProperty.fpControl:
                            MyResult.SetResult(isSuccess: true, myReturnValue: thisElement, myResultDescription: string.Empty);
                            break;
                        case FieldProperty.fpAttribute:
                            if (thisElement.getAttribute(AttrName) != null)
                                MyResult.SetResult(true, thisElement.getAttribute(AttrName), string.Empty);
                            else
                                MyResult.SetResult(false, string.Empty, "No value found");
                            break;
                        default:
                            MyResult.SetResult(false, string.Empty, "Invalid property type chosen: " + MyPropertyType.ToString());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = "Error Encountered! \n" +
                "MESSAGE: " + ex.Message + "\n\n" +
                "SOURCE: " + ex.Source + "\n\n" +
                "STACK: " + ex.StackTrace;
                MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: ErrorMessage);
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MyResult;
        }

        /// <summary>
        /// Gets the element containing the value.
        /// </summary>
        /// <param name="thisCollection">Gets the collection of control</param>
        /// <param name="MyPropertyType">Gets the type of property for which value to be extracted</param>
        /// <param name="LabelName">Gets the label name against which value need to be captured</param>
        /// <param name="isExactMatchReq">Checks whether exact match is required or not.</param>
        /// <param name="NextItemRequired">'True' if next control/item/value required</param>
        /// <param name="AttributeName">Name of attribute</param>
        /// <returns>Gets the control for which value needed to be extracted in 'IHTMLElement' type</returns>
        private IHTMLElement GetElement<T>(IHTMLElementCollection thisCollection, FieldProperty MyPropertyType, string LabelName, bool isExactMatchReq, FieldProperty MyPropertyType2 = FieldProperty.spNone, bool NextItemRequired = true, string AttributeName = "")
        {
            BrowserResult<IHTMLElement> MyResult = new BrowserResult<IHTMLElement>();
            IHTMLElement thisElement = null;
            int itemNumber = 0;
            Interfaces.IElementFinder elementFinder = null;
            try
            {
                int counter = 0;
                foreach (IHTMLElement item in thisCollection)
                {
                    counter++;
                    if (itemNumber == 1)
                    {
                        thisElement = MyResult.ReturnValue as IHTMLElement;
                        return thisElement;
                    }

                    if (MyPropertyType2 != FieldProperty.spNone)
                    {
                        switch (MyPropertyType)
                        {
                            case FieldProperty.fpValue:
                                elementFinder = new ElementFinder.GetByValue();
                                break;
                            case FieldProperty.fpInnerText:
                            case FieldProperty.fpControl:
                                elementFinder = new ElementFinder.GetByInnerText();
                                break;
                            case FieldProperty.fpInnerHTML:
                                elementFinder = new ElementFinder.GetByInnerHtml();
                                break;
                            case FieldProperty.fpAttribute:
                                elementFinder = new ElementFinder.GetByAttribute();
                                break;
                        }
                    }
                    else
                    {

                    }

                    if (elementFinder != null)
                    {
                        MyResult = elementFinder.FindElement(item, fieldAttribute: MyPropertyType, Value: LabelName, MatchStringBy: StringMatcher.Exact);
                        if (!MyResult.Successful)
                            MyResult = elementFinder.FindElement(item, fieldAttribute: MyPropertyType, Value: LabelName, MatchStringBy: StringMatcher.Start);

                        if (MyResult.Successful)
                        {
                            itemNumber++;
                            if (counter != thisCollection.length)
                            {
                                if (NextItemRequired)
                                    continue;
                            }
                        }
                    }

                    if (itemNumber == 1)
                    {
                        thisElement = MyResult.ReturnValue as IHTMLElement;
                        return thisElement;
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = "Error Encountered! \n" +
                "MESSAGE: " + ex.Message + "\n\n" +
                "SOURCE: " + ex.Source + "\n\n" +
                "STACK: " + ex.StackTrace;
                MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: ErrorMessage);
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return thisElement;
        }

        /// <summary>
        /// Getting table from present frame/document.
        /// </summary>
        /// <param name="FirstColumnText">Gets the first column text of the table to be captured</param>
        /// <param name="SecondColumnText">Gets the second column text of the table to be captured</param>
        /// <param name="ThisIndex">Gets the index of the table</param>
        /// <param name="asHeaders">Checks whether any 'TH' is present or not</param>
        /// <param name="CanContainTables">Checks whether inner table is present or not</param>
        /// <param name="UseTopRow">Checks whether the Top row is required or not</param>
        /// <param name="FirstCellStartsWith">Gets the first cell text for the wild card match</param>
        /// <param name="ThisFrame">Gets the frame inside which table is present</param>
        /// <returns>Returns the table of type 'BrowserResult'</returns>
        public virtual BrowserResult<T> GetTable<T>(string FirstColumnText, string SecondColumnText, int ThisIndex, bool asHeaders, bool CanContainTables, bool UseTopRow, bool FirstCellStartsWith, IHTMLWindow2 ThisFrame = null)
        {
            BrowserResult<T> MyResult = new BrowserResult<T>();
            string thisField;
            HTMLDocument thisDoc;
            try
            {
                if (asHeaders)
                    thisField = "TH";
                else
                    thisField = "TD";

                if (ThisFrame != null)
                    thisDoc = (HTMLDocument)ThisFrame.document;// ((HTMLDocument)ThisFrame);
                else
                    thisDoc = _MyDocument as HTMLDocument;

                foreach (IHTMLElement2 thisTable in thisDoc.getElementsByTagName("TABLE"))
                {
                    if (thisTable.getElementsByTagName("TABLE").length > 0 && !CanContainTables)
                    {
                        continue;
                    }
                    if (thisTable.getElementsByTagName("TR").length > 0)
                    {
                        if (UseTopRow)
                        {
                            IHTMLElement2 thisRow = thisTable.getElementsByTagName("TR").item(0);
                            //Debug.WriteLine("---------------------------");
                            //Debug.WriteLine(((IHTMLElement)thisRow).innerHTML);
                            //Debug.WriteLine("---------------------------");
                            if (thisRow.getElementsByTagName(thisField).length >= ThisIndex + 1)
                            {
                                string firstCell = "";
                                string SecondCell = "";
                                if (FirstCellStartsWith)
                                {
                                    if (thisRow.getElementsByTagName(thisField).item(1).innerText != null)
                                        firstCell = thisRow.getElementsByTagName(thisField).item(1).innerText.Trim();
                                    if (thisRow.getElementsByTagName(thisField).item(ThisIndex).innerText != null)
                                        SecondCell = thisRow.getElementsByTagName(thisField).item(ThisIndex).innerText.Trim();

                                    if (thisRow.getElementsByTagName(thisField).item(0).innerText == null
                                        && !string.IsNullOrEmpty(firstCell)
                                        && firstCell.StartsWith(FirstColumnText)
                                        && SecondCell.Contains(SecondColumnText))
                                    {
                                        MyResult.SetResult(isSuccess: true, myReturnValue: (object)thisTable, myReturnType: thisTable.GetType(), myResultDescription: string.Empty);
                                        return MyResult;
                                    }

                                    if (thisRow.getElementsByTagName(thisField).item(0).innerText != null)
                                        firstCell = thisRow.getElementsByTagName(thisField).item(0).innerText.Trim();
                                    if (!string.IsNullOrEmpty(firstCell)
                                       && firstCell.StartsWith(FirstColumnText)
                                       && SecondCell.Contains(SecondColumnText))
                                    {
                                        MyResult.SetResult(isSuccess: true, myReturnValue: (object)thisTable, myReturnType: thisTable.GetType(), myResultDescription: string.Empty);
                                        return MyResult;
                                    }
                                }
                                else
                                {
                                    if (thisRow.getElementsByTagName(thisField).item(0).innerText != null)
                                        firstCell = thisRow.getElementsByTagName(thisField).item(0).innerText.Trim();
                                    if (thisRow.getElementsByTagName(thisField).item(ThisIndex).innerText != null)
                                        SecondCell = thisRow.getElementsByTagName(thisField).item(ThisIndex).innerText.Trim();

                                    if (firstCell == FirstColumnText
                                       && SecondCell.Contains(SecondColumnText))
                                    {
                                        MyResult.SetResult(isSuccess: true, myReturnValue: (object)thisTable, myReturnType: thisTable.GetType(), myResultDescription: string.Empty);
                                        return MyResult;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (IHTMLElement2 thisRow in thisTable.getElementsByTagName("TR"))
                            {
                                if (thisRow.getElementsByTagName(thisField).length >= ThisIndex + 1)
                                {
                                    string firstCell = "";
                                    string SecondCell = "";
                                    if (thisRow.getElementsByTagName(thisField).item(0).innerText != null)
                                        firstCell = thisRow.getElementsByTagName(thisField).item(0).innerText.Trim();
                                    if (thisRow.getElementsByTagName(thisField).item(ThisIndex).innerText != null)
                                        SecondCell = thisRow.getElementsByTagName(thisField).item(ThisIndex).innerText.Trim();

                                    if (FirstCellStartsWith)
                                    {
                                        if (firstCell.StartsWith(FirstColumnText)
                                           && SecondCell.Contains(SecondColumnText))
                                        {
                                            MyResult.SetResult(isSuccess: true, myReturnValue: (object)thisTable, myReturnType: thisTable.GetType(), myResultDescription: string.Empty);
                                            return MyResult;
                                        }
                                    }
                                    else
                                    {
                                        if (firstCell == FirstColumnText
                                           && SecondCell.Contains(SecondColumnText))
                                        {
                                            MyResult.SetResult(isSuccess: true, myReturnValue: (object)thisTable, myReturnType: thisTable.GetType(), myResultDescription: string.Empty);
                                            return MyResult;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                MyResult.SetResult(isSuccess: false, myReturnValue: null, myResultDescription: "Table matching parameters not found");
            }
            catch (Exception ex)
            {
                string ErrorMessage = "Error Encountered! \n" +
                "MESSAGE: " + ex.Message + "\n\n" +
                "SOURCE: " + ex.Source + "\n\n" +
                "STACK: " + ex.StackTrace;
                MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: ErrorMessage);
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MyResult;
        }

        /// <summary>
        /// Gets frame containing the control specified.
        /// </summary>
        /// <param name="ControlName">Control ID</param>
        /// <param name="AttributeType">Type of attribute</param>
        /// <param name="SearchType">search value type</param>
        /// <param name="ItemLabelName">Label name</param>
        /// <param name="ControlIndex">index of control</param>
        /// <param name="isExactMatchReq">'True' if exact match required else 'False'</param>
        /// <param name="NextItemRequired">'True' if next item required else 'False'</param>
        /// <param name="frameCount">Frame Count</param>
        /// <param name="AttrName">Name of attribute</param>
        /// <param name="FrameNames">Frame names</param>
        /// <returns>frame where control is present</returns>
        public virtual IHTMLWindow2 GetFrameContainingControl<T>(string ControlName, FieldType AttributeType, FieldProperty SearchType, string ItemLabelName = "", int ControlIndex = 0, bool isExactMatchReq = false, bool NextItemRequired = false, int frameCount = 0, string AttrName = "", params string[] FrameNames)
        {
            IHTMLWindow2 rtnWindow = null;
            try
            {
                IHTMLWindow2 tempWindow = null;
                if (FrameNames.Length > 0)
                {
                    for (int i = 0; i < FrameNames.Length; i++)
                    {
                        if (tempWindow == null)
                        {
                            //tempWindow = GetFrame(_MyDocument, FrameNames[i], out Error, TagName: "frame");
                            //if (tempWindow != null)
                            //{
                            //    if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                            //    {
                            //        IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                            //                                               SearchType: SearchType, ItemLabelName: ItemLabelName,
                            //                                               ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                            //                                               NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);

                            //        if (objParent != null)
                            //        {
                            //            rtnWindow = tempWindow;
                            //            break;
                            //        }
                            //    }
                            //}
                            //else
                            rtnWindow = VerifyControlAvailabilityInCurrentFrame<T>(TheDocument: _MyDocument, ControlName: ControlName, AttributeType: AttributeType, SearchType: SearchType,
                                                             ItemLabelName: ItemLabelName, ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                                             NextItemRequired: NextItemRequired, AttrName: AttrName, FrameName: FrameNames[i]);

                            if (rtnWindow == null)
                            {
                                rtnWindow = VerifyControlAvailabilityInCurrentFrame<T>(TheDocument: _MyDocument, ControlName: ControlName, AttributeType: AttributeType, SearchType: SearchType,
                                                                ItemLabelName: ItemLabelName, ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                                                NextItemRequired: NextItemRequired, AttrName: AttrName, FrameName: FrameNames[i]);

                                if (rtnWindow != null)
                                    break;

                                //tempWindow = GetFrame(_MyDocument, FrameNames[i], out Error, TagName: "iframe");
                                //if (tempWindow != null)
                                //{
                                //    if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                                //    {
                                //        IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                                //                                           SearchType: SearchType, ItemLabelName: ItemLabelName,
                                //                                           ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                //                                           NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);
                                //        if (objParent != null)
                                //        {
                                //            rtnWindow = tempWindow;
                                //            break;
                                //        }
                                //    }
                                //}
                            }
                        }
                        else if (tempWindow != null)
                        {
                            IHTMLWindow2 oldWindow = tempWindow;
                            //tempWindow = GetFrame(tempWindow.document, FrameNames[i], out Error, TagName: "frame");
                            //if (tempWindow != null)
                            //{
                            //    if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                            //    {
                            //        IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                            //                                               SearchType: SearchType, ItemLabelName: ItemLabelName,
                            //                                               ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                            //                                               NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);
                            //        if (objParent != null)
                            //        {
                            //            rtnWindow = tempWindow;
                            //            break;
                            //        }
                            //    }
                            //}
                            //else
                            rtnWindow = VerifyControlAvailabilityInCurrentFrame<T>(TheDocument: oldWindow.document, ControlName: ControlName, AttributeType: AttributeType, SearchType: SearchType,
                                                             ItemLabelName: ItemLabelName, ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                                             NextItemRequired: NextItemRequired, AttrName: AttrName, FrameName: FrameNames[i]);

                            if (rtnWindow == null)
                            {
                                rtnWindow = VerifyControlAvailabilityInCurrentFrame<T>(TheDocument: oldWindow.document, ControlName: ControlName, AttributeType: AttributeType, SearchType: SearchType,
                                                             ItemLabelName: ItemLabelName, ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                                             NextItemRequired: NextItemRequired, AttrName: AttrName, FrameName: FrameNames[i]);

                                if (rtnWindow != null)
                                    break;

                                //tempWindow = GetFrame(oldWindow.document, FrameNames[i], out Error, TagName: "iframe");
                                //if (tempWindow != null)
                                //{
                                //    if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                                //    {
                                //        IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                                //                                           SearchType: SearchType, ItemLabelName: ItemLabelName,
                                //                                           ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                //                                           NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);
                                //        if (objParent != null)
                                //        {
                                //            rtnWindow = tempWindow;
                                //            break;
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }
                else if (frameCount > 0)
                {
                    for (int i = 1; i <= frameCount; i++)
                    {
                        rtnWindow = VerifyControlAvailabilityInCurrentFrame<T>(TheDocument: tempWindow.document, ControlName: ControlName, AttributeType: AttributeType, SearchType: SearchType,
                                ItemLabelName: ItemLabelName, ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                NextItemRequired: NextItemRequired, AttrName: AttrName, frameIndex: i, frameCount: frameCount);
                        //tempWindow = GetiFrame(out Error, Doc: _MyDocument, frameindex: i);
                        //if (tempWindow != null)
                        //{
                        //    if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                        //    {
                        //        IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                        //                                            SearchType: SearchType, ItemLabelName: ItemLabelName,
                        //                                            ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                        //                                            NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);
                        //        if (objParent != null)
                        //        {
                        //            if (i == frameCount)
                        //            {
                        //                rtnWindow = tempWindow;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
                else
                    rtnWindow = null;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return rtnWindow;
        }

        /// <summary>
        /// Verifies control availability in current frame
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="TheDocument">The document which contains the frame</param>
        /// <param name="ControlName">Name of the control</param>
        /// <param name="AttributeType">Type of atribute</param>
        /// <param name="SearchType">Field property</param>
        /// <param name="ItemLabelName">Label name of the control</param>
        /// <param name="ControlIndex">Index of the control from the control array</param>
        /// <param name="isExactMatchReq">Is exact match control required or not</param>
        /// <param name="NextItemRequired">Is next control required or not</param>
        /// <param name="AttrName">Attribute Name</param>
        /// <param name="FrameName">Iframe name</param>
        /// <param name="frameIndex">frame number from the iframe array</param>
        /// <param name="frameCount">total number of iframes</param>
        /// <returns>the iframe in which control is available</returns>
        private IHTMLWindow2 VerifyControlAvailabilityInCurrentFrame<T>(IHTMLDocument2 TheDocument, string ControlName, FieldType AttributeType, FieldProperty SearchType, string ItemLabelName, int ControlIndex, bool isExactMatchReq, bool NextItemRequired, string AttrName = "", string FrameName = "", int frameIndex = 0, int frameCount = 0)
        {
            IHTMLWindow2 tempWindow = null;
            try
            {
                if (frameCount > 0)
                {
                    tempWindow = GetiFrame(out string Error, Doc: _MyDocument, frameindex: frameIndex);
                    if (tempWindow != null)
                    {
                        if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                        {
                            IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                                                                SearchType: SearchType, ItemLabelName: ItemLabelName,
                                                                ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                                                NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);
                            if (objParent != null)
                            {
                                if (frameIndex == frameCount)
                                    return tempWindow;
                            }
                        }
                    }
                }
                else
                {
                    tempWindow = GetFrame(TheDocument, FrameName, out string Error, TagName: "iframe");
                    if (tempWindow != null)
                    {
                        if ((tempWindow.document) is IHTMLDocument3 objHtmlDoc3)
                        {
                            IHTMLElement objParent = SearchControl<T>(ControlName: ControlName, AttributeType: AttributeType,
                                                               SearchType: SearchType, ItemLabelName: ItemLabelName,
                                                               ControlIndex: ControlIndex, isExactMatchReq: isExactMatchReq,
                                                               NextItemRequired: NextItemRequired, objHtmlDoc3: objHtmlDoc3, AttrName: AttrName);
                            if (objParent != null)
                                return tempWindow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

            return tempWindow;
        }

        /// <summary>
        /// Searches the control in the provided document
        /// </summary>
        /// <param name="ControlName">Control ID</param>
        /// <param name="AttributeType">Type of attribute</param>
        /// <param name="SearchType">search value type</param>
        /// <param name="ItemLabelName">Label name</param>
        /// <param name="ControlIndex">index of control</param>
        /// <param name="isExactMatchReq">'True' if exact match required else 'False'</param>
        /// <param name="NextItemRequired">'True' if next item required else 'False'</param>
        /// <param name="objHtmlDoc3">Document to be searched</param>
        /// <param name="AttrName">Name of attribute</param>
        /// <returns>frame where control is present</returns>
        public virtual IHTMLElement SearchControl<T>(string ControlName, FieldType AttributeType, FieldProperty SearchType, string ItemLabelName, int ControlIndex, bool isExactMatchReq, bool NextItemRequired, IHTMLDocument3 objHtmlDoc3, string AttrName = "")
        {
            IHTMLElement objParent = null;
            IHTMLElementCollection objCollection;
            try
            {
                switch (AttributeType)
                {
                    case FieldType.ftID:
                        objParent = (objHtmlDoc3).getElementById(ControlName);
                        break;
                    case FieldType.ftName:
                        objCollection = (objHtmlDoc3).getElementsByName(ControlName);
                        if (objCollection.length == 0)
                            objParent = null;
                        else
                            objParent = objCollection.item(ControlIndex);
                        break;
                    case FieldType.ftTagName:
                        objCollection = objHtmlDoc3.getElementsByTagName(ControlName);
                        if (objCollection.length == 0)
                            objParent = null;
                        else
                            objParent = GetElement<T>(NextItemRequired: NextItemRequired, thisCollection: objCollection, MyPropertyType: SearchType, LabelName: ItemLabelName, isExactMatchReq: isExactMatchReq, AttributeName: AttrName);
                        break;
                    default:
                        objParent = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return objParent;
        }
        #endregion

        #region Set Values
        /// <summary>
        /// Setting the text to the field which is passed and present in passed document by using field name
        /// </summary>
        /// <param name="TextToSet">Gets the value to set into the control</param>
        /// <param name="MyFieldName">Gets the name of the field</param>
        /// <param name="MyFieldType">Gets the file type as 'ID', 'Name' or 'Tag Name'</param>
        /// <param name="MyPropertyType">Gets the property for which the value need to set</param>
        /// <param name="MyFieldCategory">Category of the field</param>
        /// <param name="MySearchProperty">Search Field property type</param>
        /// <param name="TargetDocument">Gets the document inside which control is present</param>
        /// <param name="MyFieldIndex">Gets the index of the control for the collection of the same kind of control</param>
        /// <param name="isFireEventRequired">Checks whether fire event needed to be fired or not</param>
        /// <param name="FireEventName">Gets the event name for which event needs to be fired</param>
        /// <param name="isCheckBox">'True' if control is Checkbox else 'False'</param>
        /// <param name="isExactMatchReq">'True' if exact match of property required for the property search else 'False'</param>
        /// <param name="ItemLabelName">Item Label name for property match</param>
        /// <param name="NextItemRequired">'True' if next item is required else 'False'</param>
        /// <param name="AttrName">Name of the attribute while search by attribute</param>
        /// <param name="webBrowser">If document is a Windows control WebBrowser</param>
        /// <returns>Returns the success/fail details of type 'BrowserResult'</returns>
        public virtual BrowserResult<T> SetValues<T>(string TextToSet, 
                                                     string MyFieldName, 
                                                     FieldType MyFieldType, 
                                                     FieldProperty MyPropertyType, 
                                                     FieldCategory MyFieldCategory, 
                                                     FieldProperty MySearchProperty = FieldProperty.spNone, 
                                                     IHTMLDocument2 TargetDocument = null, 
                                                     int MyFieldIndex = 0, 
                                                     bool isFireEventRequired = false, 
                                                     string FireEventName = "", 
                                                     bool isCheckBox = false, 
                                                     bool isExactMatchReq = true, 
                                                     string ItemLabelName = "", 
                                                     bool NextItemRequired = true, 
                                                     string AttrName = "")
        {
            BrowserResult<T> MyResult = new BrowserResult<T>();
            string Error = "";
            IHTMLElement thisElement = null;
            IHTMLElementCollection thisCol;
            IHTMLDocument3 thisDoc;
            try
            {
                BrowserData browserData = new BrowserData
                {
                    TextToSet = TextToSet,
                    MyFieldType = MyFieldType,
                    MyPropertyType = MyPropertyType,
                    MySearchProperty = MySearchProperty,
                    MyFieldName = MyFieldName,
                    AttributeName = AttrName,
                    TargetDocument = TargetDocument,
                    IsAddlFireEventReq = isFireEventRequired,
                    FireEventName = FireEventName,
                    NextItemRequired =  NextItemRequired,
                    IsExactMatchReq = isExactMatchReq,
                    IsCheckBox = isCheckBox
                };

                if (TargetDocument == null)
                    thisDoc = _MyDocument as IHTMLDocument3;
                else
                    thisDoc = TargetDocument as IHTMLDocument3;

                if (thisDoc != null)
                {
                    switch (MyFieldType)
                    {
                        case FieldType.ftID:
                            thisElement = (thisDoc).getElementById(MyFieldName);
                            break;
                        case FieldType.ftName:
                            thisCol = (thisDoc).getElementsByName(MyFieldName);
                            if (thisCol.length == 0)
                            {
                                thisElement = null;
                            }
                            else
                            {
                                thisElement = thisCol.item(MyFieldIndex);
                            }
                            break;
                        case FieldType.ftTagName:
                            thisCol = (thisDoc).getElementsByTagName(MyFieldName);
                            if (thisCol.length == 0)
                            {
                                thisElement = null;
                            }
                            else
                            {
                                thisElement = GetElement<T>(NextItemRequired: NextItemRequired, thisCollection: thisCol, MyPropertyType: MySearchProperty, LabelName: ItemLabelName, isExactMatchReq: isExactMatchReq, AttributeName: AttrName);
                            }
                            break;
                        default:
                            thisElement = null;
                            break;
                    }
                }

                if (thisElement == null)
                {
                    MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: "Element " + MyFieldName + " not found");
                }
                else
                {
                    switch (MyPropertyType)
                    {
                        case FieldProperty.fpValue:
                            Type fieldType = thisElement.GetType();
                            string className = fieldType.Name;
                            switch (MyFieldCategory)
                            {
                                case FieldCategory.INPUT:
                                    if (isCheckBox)
                                        ((IHTMLInputElement)thisElement).@checked = Convert.ToBoolean(TextToSet);
                                    else
                                        ((IHTMLInputElement)thisElement).value = TextToSet;
                                    break;
                                case FieldCategory.TEXTAREA:
                                    ((IHTMLTextAreaElement)thisElement).value = TextToSet;
                                    break;
                                case FieldCategory.SELECT:
                                    ((IHTMLSelectElement)thisElement).value = TextToSet;
                                    break;
                                default:
                                    break;
                            }
                            MyResult.SetResult(true, string.Empty, "Element " + MyFieldName + " value set to " + TextToSet);
                            break;
                        case FieldProperty.fpInnerHTML:
                            thisElement.innerHTML = TextToSet;
                            MyResult.SetResult(true, string.Empty, "Element " + MyFieldName + " innerhtml set to " + TextToSet);
                            break;
                        case FieldProperty.fpInnerText:
                            thisElement.innerText = TextToSet;
                            MyResult.SetResult(true, string.Empty, "Element " + MyFieldName + " innertext set to " + TextToSet);
                            break;
                        case FieldProperty.fpAttribute:
                            thisElement.setAttribute(AttrName, TextToSet);
                            MyResult.SetResult(true, string.Empty, "Element " + MyFieldName + " innertext set to " + TextToSet);
                            break;
                        case FieldProperty.fpText:
                            fieldType = thisElement.GetType();
                            className = fieldType.Name;
                            switch (MyFieldCategory)
                            {
                                case FieldCategory.INPUT:
                                    if (isCheckBox)
                                        ((IHTMLInputElement)thisElement).@checked = Convert.ToBoolean(TextToSet);
                                    else
                                        ((IHTMLInputElement)thisElement).value = TextToSet;
                                    break;
                                case FieldCategory.TEXTAREA:
                                    ((IHTMLTextAreaElement)thisElement).value = TextToSet;
                                    break;
                                case FieldCategory.SELECT:
                                    IHTMLElementCollection lstOption = ((IHTMLElement2)thisElement).getElementsByTagName("OPTION");
                                    foreach (IHTMLElement objOption in lstOption)
                                    {
                                        if (objOption.innerText != null)
                                        {
                                            if (objOption.innerText.Trim().ToLower().StartsWith(TextToSet.ToLower()))
                                            {
                                                objOption.setAttribute("selected", "true");
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            MyResult.SetResult(true, string.Empty, "Element " + MyFieldName + " value set to " + TextToSet);
                            break;
                        default:
                            MyResult.SetResult(false, string.Empty, "Invalid property type chosen: " + MyPropertyType.ToString());
                            break;
                    }

                    if (isFireEventRequired)
                    {
                        ((IHTMLElement3)thisElement).FireEvent(FireEventName);
                        EndWaitIE(_MyDocument, out Error);
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = "Error Encountered! \n" +
                "MESSAGE: " + ex.Message + "\n\n" +
                "SOURCE: " + ex.Source + "\n\n" +
                "STACK: " + ex.StackTrace;
                MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: ErrorMessage);
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MyResult;
        }
        #endregion

        #region Navigation Methods
        /// <summary>
        /// Clicks button of given button/link Name.
        /// </summary>
        /// <param name="MyFieldValue">Gets the name for the link/button</param>
        /// <param name="MyFieldType">Gets the file type as 'ID', 'Name' or 'Tag Name'</param>
        /// <param name="MyPropertyType">Gets the property for which the navigation need to perform</param>
        /// <param name="MyPropertyType2">Gets the property for which the navigation need to perform</param>
        /// <param name="MyFieldName">Gets the 'ID', 'NAME' or 'TAG NAME' value</param>
        /// <param name="AttributeName">Name of the attribute for searching the control to click</param>
        /// <param name="TargetDocument">Gets the frame in which the navigation control is present</param>
        /// <param name="AttrValue">Attribute value</param>
        /// <param name="IsAddlFireEventReq">If any Additionaly fire event required then 'true'</param>
        /// <param name="FireEventName">Additional Fire Event name</param>
        /// <param name="webBrowser">If document is a Windows control WebBrowser</param>
        /// <returns>Returns the success/fail details of type 'BrowserResult'</returns>
        public virtual BrowserResult<T> NavigatePage<T>(string MyFieldValue, FieldType MyFieldType, FieldProperty MyPropertyType, FieldProperty MyPropertyType2 = FieldProperty.spNone, string MyFieldName = "", string AttributeName = "value", IHTMLDocument2 TargetDocument = null, string AttrValue = "", bool IsAddlFireEventReq = false, string FireEventName = "")
        {
            BrowserResult<T> MyResult = new BrowserResult<T>();
            IHTMLDocument3 thisDoc = null;
            try
            {
                if (TargetDocument == null)
                    thisDoc = _MyDocument as IHTMLDocument3;
                else
                    thisDoc = TargetDocument as IHTMLDocument3;
                
                BrowserData browserData = new BrowserData
                {
                    MyFieldValue = MyFieldValue,
                    MyFieldType = MyFieldType,
                    MyPropertyType = MyPropertyType,
                    MySearchProperty = MyPropertyType2,
                    MyFieldName = MyFieldName,
                    AttributeName = AttributeName,
                    TargetDocument = TargetDocument,
                    IsAddlFireEventReq = IsAddlFireEventReq,
                    FireEventName = FireEventName,
                    AttributeValue = AttrValue
                };
                
                MyResult = IENavigation<T>(
                    MyFieldValue: MyFieldValue, 
                    MyFieldType: MyFieldType, 
                    MyPropertyType: MyPropertyType, 
                    thisDoc: thisDoc, 
                    MySearchProperty: MyPropertyType2, 
                    MyFieldName: MyFieldName, 
                    AttributeName: AttributeName, 
                    AttrValue: AttrValue, 
                    IsAddlFireEventReq: IsAddlFireEventReq, 
                    FireEventName: FireEventName);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MyResult;
        }

        /// <summary>
        /// Clicks button of given button/link Name.
        /// </summary>
        /// <param name="MyFieldValue">Gets the name for the link/button</param>
        /// <param name="MyFieldType">Gets the file type as 'ID', 'Name' or 'Tag Name'</param>
        /// <param name="MyPropertyType">Gets the property for which the navigation need to perform</param>
        /// <param name="thisDoc">IE Document</param>
        /// <param name="MySearchProperty">Gets the property for which the navigation need to perform</param>
        /// <param name="MyFieldName">Gets the 'ID', 'NAME' or 'TAG NAME' value</param>
        /// <param name="AttributeName">Name of the attribute for searching the control to click</param>
        /// <param name="AttrValue">Attribute value</param>
        /// <param name="IsAddlFireEventReq">If any Additionaly fire event required then 'true'</param>
        /// <param name="FireEventName">Additional Fire Event name</param>
        /// <returns>Returns the success/fail details of type 'BrowserResult'</returns>
        private BrowserResult<T> IENavigation<T>(string MyFieldValue, 
                                                 FieldType MyFieldType, 
                                                 FieldProperty MyPropertyType, 
                                                 IHTMLDocument3 thisDoc, 
                                                 FieldProperty MySearchProperty = FieldProperty.spNone, 
                                                 string MyFieldName = "", 
                                                 string AttributeName = "value", 
                                                 string AttrValue = "", 
                                                 bool IsAddlFireEventReq = false, 
                                                 string FireEventName = "", 
                                                 bool NextItemRequired = true, 
                                                 bool isExactMatchReq = true)
        {
            BrowserResult<T> MyResult = new BrowserResult<T>();
            IHTMLElement thisElement = null;
            IHTMLElementCollection thisCol;
            IHTMLElementCollection Collection = null;
            string Error = "";
            try
            {
                if (thisDoc != null)
                {
                    switch (MyFieldType)
                    {
                        case FieldType.ftID:
                            thisElement = (thisDoc).getElementById(MyFieldName);
                            break;
                        case FieldType.ftName:
                            thisCol = (thisDoc).getElementsByName(MyFieldName);
                            if (thisCol.length == 0)
                                thisElement = null;
                            else
                            {
                                thisElement = GetElement<T>(NextItemRequired: NextItemRequired, thisCollection: thisCol, MyPropertyType: MySearchProperty, LabelName: MyFieldValue, isExactMatchReq: isExactMatchReq, AttributeName: AttributeName);
                                if (thisElement == null)
                                {
                                    Collection = thisCol;
                                    thisElement = thisCol.item(0);
                                }
                            }
                            break;
                        case FieldType.ftTagName:
                            thisCol = thisDoc.getElementsByTagName(MyFieldName);
                            if (thisCol.length == 0)
                                thisElement = null;
                            else
                            {
                                thisElement = GetElement<T>(NextItemRequired: NextItemRequired, thisCollection: thisCol, MyPropertyType: MyPropertyType, LabelName: MyFieldValue, isExactMatchReq: isExactMatchReq, AttributeName: AttributeName);
                                if (thisElement == null)
                                {
                                    Collection = thisCol;
                                    thisElement = thisCol.item(0);
                                }
                            }
                            break;
                        default:
                            thisElement = null;
                            break;
                    }

                    if (thisElement == null)
                    {
                        MyResult.SetResult(isSuccess: false, myReturnValue: string.Empty, myResultDescription: "Element " + MyFieldName + " not found");
                    }
                    else if (Collection != null)
                    {
                        IHTMLElement element = null;
                        switch (MyPropertyType)
                        {
                            case FieldProperty.fpValue:
                                if (MySearchProperty == FieldProperty.fpAttribute)
                                {
                                    element = Collection.Cast<IHTMLElement>().Where(elem => elem.getAttribute("value") != null)
                                                                                .Where(elem => elem.getAttribute(AttributeName) != null)
                                                                                .Where(elem => elem.getAttribute(AttributeName).ToString().Trim() == AttrValue)
                                                                                .Where(elem => elem.getAttribute("value").ToString().Trim() == MyFieldValue)
                                                                                .Select(elem => elem).FirstOrDefault();
                                }
                                else
                                {
                                    element = Collection.Cast<IHTMLElement>().Where(elem => elem.getAttribute("value") != null)
                                                                                        .Where(elem => elem.getAttribute("value").ToString().Trim() == MyFieldValue)
                                                                                        .Select(elem => elem).FirstOrDefault();
                                }
                                break;
                            case FieldProperty.fpInnerText:
                                if (MySearchProperty == FieldProperty.fpAttribute)
                                {
                                    element = Collection.Cast<IHTMLElement>().Where(elem => elem.innerText != null)
                                                                                .Where(elem => elem.getAttribute(AttributeName) != null)
                                                                                .Where(elem => elem.getAttribute(AttributeName).ToString().Trim() == AttrValue)
                                                                                .Where(elem => elem.innerText.Trim() == MyFieldValue)
                                                                                .Select(elem => elem).FirstOrDefault();
                                }
                                else
                                {
                                    element = Collection.Cast<IHTMLElement>().Where(elem => elem.innerText != null)
                                                                                        .Where(elem => elem.innerText.Trim() == MyFieldValue)
                                                                                        .Select(elem => elem).FirstOrDefault();
                                }
                                break;
                            case FieldProperty.fpAttribute:
                                element = Collection.Cast<IHTMLElement>().Where(elem => elem.getAttribute(AttributeName) != null)
                                                                                    .Where(elem => elem.getAttribute(AttributeName).ToString().Trim() == MyFieldValue)
                                                                                    .Select(elem => elem).FirstOrDefault();
                                break;
                            case FieldProperty.fpInnerHTML:
                                if (MySearchProperty == FieldProperty.fpAttribute)
                                {
                                    element = Collection.Cast<IHTMLElement>().Where(elem => elem.innerHTML != null)
                                                                                .Where(elem => elem.getAttribute(AttributeName) != null)
                                                                                .Where(elem => elem.getAttribute(AttributeName).ToString().Trim() == AttrValue)
                                                                                .Where(elem => elem.innerHTML.Trim() == MyFieldValue)
                                                                                .Select(elem => elem).FirstOrDefault();
                                }
                                else
                                {
                                    element = Collection.Cast<IHTMLElement>().Where(elem => elem.innerHTML != null)
                                                                                    .Where(elem => elem.innerHTML.Trim() == MyFieldValue)
                                                                                    .Select(elem => elem).FirstOrDefault();
                                }
                                break;
                            case FieldProperty.spNone:
                            default:
                                break;
                        }

                        if (element != null)
                        {
                            if (IsAddlFireEventReq)
                                ((IHTMLElement3)element).FireEvent(FireEventName);
                            else
                                element.click();
                            EndWaitIE(_MyDocument, out Error);
                            MyResult.SetResult(isSuccess: true, myReturnValue: element, myResultDescription: "Success");
                        }
                    }
                    else
                    {
                        if (IsAddlFireEventReq)
                            ((IHTMLElement3)thisElement).FireEvent(FireEventName);
                        else
                            thisElement.click();
                        EndWaitIE(_MyDocument, out Error);
                        MyResult.SetResult(isSuccess: true, myReturnValue: thisElement, myResultDescription: "Success");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MyResult;
        }
        #endregion

        #region Go Previous Page
        /// <summary>
        /// Method to go previous page in IE.
        /// </summary>
        /// <param name="Count">Gets the value for the number of 'BACK' to be performed on the IE</param>
        /// <param name="Error">Returns back the error if any occured</param>
        /// <returns>Returs value for success/fail operation</returns>
        public bool GoPreviousPage(int Count, out string Error)
        {
            bool rtnValue = false;
            Error = string.Empty;
            try
            {
                //object o = (object)i;
                //HTMLWindow2 Win = (HTMLWindow2)(_MyDocument).parentWindow;
                ////Win.history.back();
                //Win.history.go(ref o);
                for (int j = 0; j <= Count; j++)
                {
                    _MyWindow.GoBack();
                    EndWaitIE(_MyDocument, out Error);
                }

                rtnValue = true;

            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return rtnValue;

        }
        #endregion

        #region Get Frame
        /// <summary>
        /// Gets the frame using frame index.
        /// </summary>
        /// <param name="Doc">Gets the docuemnt in which frame is present</param>
        /// <param name="frameindex">Gets the count of the frame where frame is present</param>
        /// <param name="Error">Returns back the error if any occured</param>
        /// <returns>Returns the frame in 'IHTMLWindow2' format</returns>
        public IHTMLWindow2 GetiFrame(out string Error, IHTMLDocument2 Doc, int frameindex)
        {
            IHTMLWindow2 rtnValue = null;
            Error = "";
            try
            {
                EndWaitIE(_MyDocument, out Error);
                int count = 1;
                for (int i = 0; Doc.frames.length > i; i++)
                {
                    if (count == frameindex)
                    {
                        IHTMLWindow2 objHtmlWindow2 = Doc.frames.item(i);

                        if (objHtmlWindow2 != null)
                        {
                            rtnValue = objHtmlWindow2;
                            return rtnValue;
                        }
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            Error = "Unable to get frame";
            return rtnValue;
        }

        /// <summary>
        /// Get the total frames opened.
        /// </summary>
        /// <param name="Doc">Document Object</param>
        /// <param name="Name">Frame Name</param>
        /// <param name="Error">Error encountered in this method</param>
        /// <returns>integer</returns>
        public int GetFrameCount(IHTMLDocument2 Doc, string Name, out string Error)
        {
            int iFrameCount = 0;
            Error = "";
            try
            {
                EndWaitIE(_MyDocument, out Error);
                if (Doc.frames.length > 0)
                {
                    for (int i = 0; Doc.frames.length > i; i++)
                    {
                        IHTMLWindow2 objHtmlWindow2 = Doc.frames.item(i);
                        iFrameCount = Doc.frames.length;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            Error = "Unable to get frame";
            return iFrameCount;
        }

        /// <summary>
        /// Gets iFrame from document with 'TagName' provided.
        /// </summary>
        /// <param name="Doc">Document Object</param>
        /// <param name="FrameName">Name of the Frame</param>
        /// <param name="Error">Error encountered in this method</param>
        /// <param name="TagName">TAG NAME OF THE CONTROL</param>
        /// <returns>IHTMLWindow2</returns>
        public IHTMLWindow2 GetFrame(IHTMLDocument2 Doc, string FrameName, out string Error, string TagName = "")
        {
            Error = "";
            IHTMLWindow2 frame = null;
            try
            {
                HTMLDocument objDoc3 = ((HTMLDocument)Doc);
                int FrameCount = Doc.frames.length;
                int iFrameCount = objDoc3.getElementsByTagName(TagName).length;
                if (!(FrameCount == iFrameCount))
                {
                    return null;
                }
                else
                {
                    for (int i = 0; (i <= (FrameCount - 1)); i++)
                    {
                        if (((objDoc3.getElementsByTagName(TagName).item(i).name == FrameName)
                                    || (objDoc3.getElementsByTagName(TagName).item(i).id == FrameName)))
                        {
                            mshtml.IHTMLWindow2 tempFrame = Doc.frames.item(i);
                            //mshtml.IHTMLDocument2 FrameDoc = iframeAccess.CrossFrameIE.GetDocumentFromWindow(tempFrame);
                            frame = tempFrame;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return frame;
        }

        /// <summary>
        /// Gets iFrame from document with 'TagName' provided.
        /// </summary>
        /// <param name="Doc">Document Object</param>
        /// <param name="FrameName">Name of the Frame</param>
        /// <param name="TagName">TAG NAME OF THE CONTROL</param>
        /// <returns>IHTMLWindow2</returns>
        public IHTMLWindow2 GetFrameUsingiframeAccess(IHTMLDocument2 Doc, string FrameName, string TagName = "")
        {
            mshtml.IHTMLDocument2 FrameDoc = null;
            try
            {
                HTMLDocument objDoc3 = ((HTMLDocument)Doc);
                int FrameCount = Doc.frames.length;
                int iFrameCount = objDoc3.getElementsByTagName(TagName).length;
                if (!(FrameCount == iFrameCount))
                {
                    return null;
                }
                else
                {
                    for (int i = 0; (i <= (FrameCount - 1)); i++)
                    {
                        if (((objDoc3.getElementsByTagName(TagName).item(i).name == FrameName)
                                    || (objDoc3.getElementsByTagName(TagName).item(i).id == FrameName)))
                        {
                            mshtml.IHTMLWindow2 tempFrame = Doc.frames.item(i);
                            FrameDoc = CrossFrameIE.GetDocumentFromWindow(tempFrame);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return FrameDoc.parentWindow;
        }
        #endregion

        #region wait unitl ready
        /// <summary>
        /// Wait unit the Doc ready
        /// </summary>
        /// <param name="Doc">Document Object</param>
        /// <param name="Error">Error encountered in the method</param>
        /// <returns>boolean</returns>
        public bool EndWaitIE(IHTMLDocument2 Doc, out string Error)
        {
            bool isLoaded = false;
            Error = string.Empty;

            try
            {
                do
                {
                    switch (Doc.readyState.Trim())
                    {
                        case "uninitialized":
                            break;
                        case "loading":
                            break;
                        case "interactive":
                            break;
                        case "complete":
                            break;
                        default:
                            break;
                    }

                }
                while (Doc.readyState != "complete");
                isLoaded = true;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

            return isLoaded;
        }

        /// <summary>
        /// Wait for Fixed duration
        /// </summary>
        /// <param name="NoOfLoops">Wait Time in sec</param>
        /// <param name="Error">Error encountered in the method</param>
        public void WaitUntillReady(int NoOfLoops, out string Error)
        {
            Error = "";
            try
            {
                int numberOfTry = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    numberOfTry = numberOfTry + 1;


                } while (numberOfTry != NoOfLoops);

            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

        }
        #endregion

        #region Release COM Objects
        /// <summary>
        /// Disposes all COM objects.
        /// </summary>
        /// <param name="obj">Object to be disposed</param>
        /// <param name="Error">Error encountered in the method</param>
        public void releaseObject(object obj, out string Error)
        {
            Error = "";
            try
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        #endregion

        #region Close/Open Window
        /// <summary>
        /// Opens window in new Tab
        /// </summary>
        /// <param name="Url">URL to be opened</param>
        /// <returns>'True' if opens window else 'False'</returns>
        public IHTMLDocument2 OpenWindow(string Url)
        {
            bool rtnValue = false;
            IHTMLDocument2 NewWindow = null;
            InternetExplorer explorer = new InternetExplorer();
            try
            {
                ShellWindows iExplorerInstances = new ShellWindows();
                foreach (InternetExplorer iExplorer in iExplorerInstances)
                {
                    if (iExplorer.Name == "Internet Explorer")
                    {
                        do
                        {
                            try
                            {
                                if (!rtnValue)
                                {
                                    iExplorer.Navigate(Url, 0x800);
                                    rtnValue = true;
                                }
                                else
                                    break;
                            }
                            catch
                            {
                                rtnValue = false;
                            }
                        } while (!rtnValue);
                        break;
                    }
                }

                if (!rtnValue)
                {
                    SHDocVw.InternetExplorer ieBrowser = new SHDocVw.InternetExplorer();
                    ieBrowser.Navigate(URL: Url);
                    ieBrowser.Visible = true;
                }

                WaitUntillReady(7, out string Error);
                explorer = GetWindow(Url.Replace("http://", "").Replace("https://", ""), out Error);
                if (explorer == null)
                {
                    do
                    {
                        explorer = GetWindow(Url.Replace("http://", "").Replace("https://", ""), out Error);
                        if (explorer != null)
                            break;
                    } while (explorer == null);
                }

                NewWindow = explorer.Document;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return NewWindow;
        }

        /// <summary>
        /// Method that closes iexplorer window.
        /// </summary>
        /// <param name="url">URL of the aplication</param>
        /// <param name="Title">Title of the application</param>
        /// <param name="Error">Error encountered in the method</param>
        public void CloseWindow(string url, string Title, out string Error)
        {
            Error = "";
            ShellWindows _shellWindows = new ShellWindows();
            string processType;
            try
            {
                foreach (InternetExplorer ie in _shellWindows)
                {
                    //this parses the name of the process
                    processType = System.IO.Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                    //this could also be used for IE windows with processType of "iexplore"
                    if (processType.Equals("iexplore") && ie.LocationURL.Contains(url))
                    {
                        if (ie.Document.Title.Contains(Title))
                            ie.Quit();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
        #endregion
    }
}
