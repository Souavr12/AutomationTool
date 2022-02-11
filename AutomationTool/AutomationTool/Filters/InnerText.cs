using System;
using System.Linq;

using AutomationTool.Constants;
using AutomationTool.Interfaces;
using AutomationTool.Structs;

using mshtml;


namespace AutomationTool.Filters
{
    internal class InnerText : IFilter
    {
        public BrowserResult<IHTMLElementCollection> FilterBy(IHTMLElementCollection thisCollection, StringMatcher matchStringBy, string value)
        {
            BrowserResult<IHTMLElementCollection> result = new BrowserResult<IHTMLElementCollection>();
            IHTMLElementCollection valueCollection;
            switch (matchStringBy)
            {
                case StringMatcher.Exact:
                    valueCollection = (thisCollection.Cast<IHTMLElement>().Where(elem => elem.innerText != null)
                                              .Where(elem => elem.innerText.Trim().ToLower() == value)) as IHTMLElementCollection;
                    break;
                case StringMatcher.Start:
                    valueCollection = (thisCollection.Cast<IHTMLElement>().Where(elem => elem.innerText != null)
                                              .Where(elem => elem.innerText.Trim().ToLower().StartsWith(value))) as
                                      IHTMLElementCollection;
                    break;
                case StringMatcher.End:
                    valueCollection = (thisCollection.Cast<IHTMLElement>().Where(elem => elem.innerText != null)
                                              .Where(elem => elem.innerText.Trim().ToLower().EndsWith(value))) as IHTMLElementCollection;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchStringBy), matchStringBy, null);
            }

            if (valueCollection != null)
                result.SetResult(isSuccess: true, myReturnValue: valueCollection,
                    myResultDescription: $"The result using the value: {value} is matching with matching type ({matchStringBy})");
            else
                result.SetResult(isSuccess: false, myReturnValue: thisCollection,
                    myResultDescription: $"The result using the value: {value} is not matching with matching type ({matchStringBy})");

            return result;
        }
    }
}