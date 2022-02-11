using System;
using System.Linq;

using AutomationTool.Constants;
using AutomationTool.Interfaces;
using AutomationTool.Structs;

using mshtml;


namespace AutomationTool.Filters
{
    internal class OtherAttribute : IFilter
    {
        private string myAttributeName;
        public OtherAttribute(string attributeName)
        {
            myAttributeName = attributeName;
        }
        public BrowserResult<IHTMLElementCollection> FilterBy(IHTMLElementCollection thisCollection, StringMatcher MatchStringBy, string value)
        {
            BrowserResult<IHTMLElementCollection> result = new BrowserResult<IHTMLElementCollection>();
            IHTMLElementCollection valueCollection;
            switch (MatchStringBy)
            {
                case StringMatcher.Exact:
                    valueCollection = (thisCollection.Cast<IHTMLElement>().Where(elem => elem.getAttribute(myAttributeName) != null)
                                              .Where(elem => elem.getAttribute(myAttributeName).ToString().Trim().ToLower() == value)) as IHTMLElementCollection;
                    break;
                case StringMatcher.Start:
                    valueCollection = (thisCollection.Cast<IHTMLElement>().Where(elem => elem.getAttribute(myAttributeName) != null)
                                              .Where(elem => elem.getAttribute(myAttributeName).ToString().Trim().ToLower().StartsWith(value))) as
                                      IHTMLElementCollection;
                    break;
                case StringMatcher.End:
                    valueCollection = (thisCollection.Cast<IHTMLElement>().Where(elem => elem.getAttribute(myAttributeName) != null)
                                              .Where(elem => elem.getAttribute(myAttributeName).ToString().Trim().ToLower().EndsWith(value))) as IHTMLElementCollection;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(MatchStringBy), MatchStringBy, null);
            }

            if (valueCollection != null)
                result.SetResult(isSuccess: true, myReturnValue: valueCollection,
                    myResultDescription: $"The result using the value: {value} is matching with matching type ({MatchStringBy})");
            else
                result.SetResult(isSuccess: false, myReturnValue: thisCollection,
                    myResultDescription: $"The result using the value: {value} is not matching with matching type ({MatchStringBy})");

            return result;
        }
    }
}