using AutomationTool.Constants;
using mshtml;
using System.Collections.Generic;

namespace AutomationTool.Interfaces
{
    internal interface ICollectionFinder
    {
        Structs.BrowserResult<List<IHTMLElement>> FindCollection(IHTMLElementCollection ThisCollection, FieldProperty fieldAttribute, string Value, StringMatcher MatchStringBy, string AttributName = "");
    }
}
