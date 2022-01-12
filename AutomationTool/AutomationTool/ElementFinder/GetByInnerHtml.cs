using AutomationTool.Constants;
using AutomationTool.Structs;
using mshtml;
using System.Collections.Generic;
using System.Linq;

namespace AutomationTool.ElementFinder
{
    class GetByInnerHtml : Interfaces.IElementFinder, Interfaces.ICollectionFinder
    {
        public Structs.BrowserResult<IHTMLElement> FindElement(IHTMLElement ThisElement, FieldProperty fieldAttribute, string Value, StringMatcher MatchStringBy, string AttributName = "")
        {
            BrowserResult<IHTMLElement> result = new BrowserResult<IHTMLElement>();
            if (ThisElement.innerHTML != null)
            {
                string theValue = ThisElement.innerHTML.ToString().Trim().ToLower();
                bool flag = false;
                switch (MatchStringBy)
                {
                    case StringMatcher.Exact:
                        if (theValue == Value.ToLower())
                            flag = true;
                        break;
                    case StringMatcher.Start:
                        if (theValue.StartsWith(Value.ToLower()))
                            flag = true;
                        break;
                    case StringMatcher.End:
                        if (theValue.EndsWith(Value.ToLower()))
                            flag = true;
                        break;
                    default:
                        break;
                }

                if (flag)
                    result.SetResult(isSuccess: flag, myReturnValue: ThisElement, myResultDescription: $"The result using the value: {Value} is matching with matching type ({MatchStringBy})");
                else
                    result.SetResult(isSuccess: flag, myReturnValue: ThisElement, myResultDescription: $"The result using the value: {Value} is not matching with matching type ({MatchStringBy})");
            }
            else
                result.SetResult(isSuccess: false, myReturnValue: ThisElement, myResultDescription: $"The element doesn't have any value to match.");

            return result;
        }

        private BrowserResult<IHTMLElementCollection> GetCollection(IHTMLElement ThisElement, string MyTagName)
        {
            BrowserResult<IHTMLElementCollection> result = new BrowserResult<IHTMLElementCollection>();
            if (ThisElement != null)
                result.SetResult(isSuccess: true, myReturnValue: ((IHTMLElement2)ThisElement).getElementsByTagName(MyTagName), myResultDescription: $"Found collection using the tag name provided {MyTagName}");
            else
                result.SetResult(isSuccess: false, myReturnValue: null, myResultDescription: $"No collection found by the tag name provided {MyTagName}");
            return result;
        }

        public BrowserResult<List<IHTMLElement>> FindCollection(IHTMLElementCollection ThisCollection, FieldProperty fieldAttribute, string Value, StringMatcher MatchStringBy, string AttributName = "")
        {
            BrowserResult<List<IHTMLElement>> result = new BrowserResult<List<IHTMLElement>>();
            List<IHTMLElement> FinalCollection = new List<IHTMLElement>();
            if (ThisCollection != null && ThisCollection.length > 0)
            {
                FinalCollection = ThisCollection.Cast<IHTMLElement>()
                                                .Where(elem => elem.innerHTML != null)
                                                .Select(elem => elem).ToList();
                bool flag = false;
                switch (MatchStringBy)
                {
                    case StringMatcher.Exact:
                        if (FinalCollection.Any(elem => elem.innerHTML.ToString().Trim().ToLower() == Value.ToLower()))
                        {
                            FinalCollection = FinalCollection.FindAll(elem => elem.innerHTML.ToString().Trim().ToLower() == Value.ToLower());
                            flag = true;
                        }
                        break;
                    case StringMatcher.Start:
                        if (FinalCollection.Any(elem => elem.innerHTML.ToString().Trim().ToLower().StartsWith(Value.ToLower())))
                        {
                            FinalCollection = FinalCollection.FindAll(elem => elem.innerHTML.ToString().Trim().ToLower().StartsWith(Value.ToLower()));
                            flag = true;
                        }
                        break;
                    case StringMatcher.End:
                        if (FinalCollection.Any(elem => elem.innerHTML.ToString().Trim().ToLower().EndsWith(Value.ToLower())))
                        {
                            FinalCollection = FinalCollection.FindAll(elem => elem.innerHTML.ToString().Trim().ToLower().EndsWith(Value.ToLower()));
                            flag = true;
                        }
                        break;
                    default:
                        break;
                }

                if (flag)
                    result.SetResult(isSuccess: flag, myReturnValue: FinalCollection, myResultDescription: $"The result using the value: {Value} is matching with matching type ({MatchStringBy})");
                else
                    result.SetResult(isSuccess: flag, myReturnValue: ThisCollection.Cast<IHTMLElement>().ToList(), myResultDescription: $"The result using the value: {Value} is not matching with matching type ({MatchStringBy})");
            }
            else
                result.SetResult(isSuccess: false, myReturnValue: null, myResultDescription: $"The element doesn't have any value to match.");

            return result;
        }
    }
}
