using AutomationTool.Constants;
using AutomationTool.Structs;

using mshtml;


namespace AutomationTool.Interfaces
{
    public interface IFilter
    {
        BrowserResult<IHTMLElementCollection> FilterBy(IHTMLElementCollection thisCollection, StringMatcher matchStringBy, string value);
    }
}