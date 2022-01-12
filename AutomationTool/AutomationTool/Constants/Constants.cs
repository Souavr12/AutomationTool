/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTool.Constants
{
    #region enum
    /// <summary>
    /// Field Type
    /// </summary>
    public enum FieldType
    {
        ftID = 0,
        ftName = 1,
        ftTagName = 2
    }

    /// <summary>
    /// Field Property
    /// </summary>
    public enum FieldProperty
    {
        fpValue = 0,
        fpInnerText = 1,
        fpInnerHTML = 2,
        fpControl = 3,
        fpText = 4,
        fpAttribute = 5,
        spNone = 6
    }

    /// <summary>
    /// Field Category
    /// </summary>
    public enum FieldCategory
    {
        INPUT = 0,
        TEXTAREA = 1,
        SELECT = 2
    }

    /// <summary>
    /// Enum for the query type
    /// </summary>
    public enum QueryType
    {
        ftInline = 0,
        ftStoredProcedure = 1
    }

    /// <summary>
    /// ENUM for Shows window.
    /// </summary>
    public enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1,
        ShowMinimized = 2,
        ShowMaximized = 3,
        Maximize = 3,
        ShowNormalNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActivate = 7,
        ShowNoActivate = 8,
        Restore = 9,
        ShowDefault = 10,
        ForceMinimized = 11
    }

    /// <summary>
    /// ENUM To Match string values.
    /// </summary>
    public enum StringMatcher
    {
        Exact = 0,
        Start = 1,
        End = 2
    }
    #endregion
}
