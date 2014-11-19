using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public abstract class BaseParser<TResult, TSource> : IParser<TSource>
    {
        object IParser<TSource>.Parse(TSource source)
        {
            return Parse(source);
        }

        public abstract ParseResult<TResult> Parse(TSource source);
    }
}
