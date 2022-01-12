/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using AutomationTool.Constants;
using mshtml;

namespace AutomationTool.Interfaces
{
    internal interface IElementFinder
    {
        Structs.BrowserResult<IHTMLElement> FindElement(IHTMLElement ThisElement, FieldProperty fieldAttribute, string Value, StringMatcher MatchStringBy, string AttributName = "");
    }
}
