using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomationTool.Constants;
using AutomationTool.Interfaces;
using AutomationTool.Structs;

using mshtml;


namespace AutomationTool.Decorator
{
    public abstract class CombinedFilterDecorator : IFilter
    {
        private readonly IFilter myFilter;

        protected CombinedFilterDecorator(IFilter filter)
        {
            myFilter = filter;
        }

        public BrowserResult<IHTMLElementCollection> FilterBy(IHTMLElementCollection thisCollection, StringMatcher matchStringBy, string value)
        {
            return myFilter.FilterBy(thisCollection, matchStringBy, value);
        }
    }
}
