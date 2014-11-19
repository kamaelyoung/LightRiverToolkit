using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightRiver.Net
{
    public class JsonParser<TResult> : BaseParser<TResult, string>
    {
        public override ParseResult<TResult> Parse(string source)
        {
            try {
                var result = JsonConvert.DeserializeObject<TResult>(source);
                var parseResult = new ParseResult<TResult>(result);
                return parseResult;
            }
            catch (JsonException ex) {
                var error = new ParseError(ex.GetHashCode(), ex.Message);
                var failResult = new ParseResult<TResult>(error);
                return failResult;
            }
        }
    }
}
