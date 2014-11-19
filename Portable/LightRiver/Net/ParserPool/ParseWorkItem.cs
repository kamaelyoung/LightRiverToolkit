using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public delegate void ParseWorkItemCallback<T>(ParseResult<T> parseResult);

    public class ParseWorkItem<TResult, TSource> : IParserWorkItem
    {
        private TSource _source;

        private BaseParser<TResult, TSource> _parser = null;

        private ParseWorkItemCallback<TResult> _callback = null;

        public ParseWorkItem(TSource source, BaseParser<TResult, TSource> parser, ParseWorkItemCallback<TResult> callback)
        {
            _source = source;
            _parser = parser;
            _callback = callback;
        }

        public void Execute()
        {
            var parseResult = _parser.Parse(_source);

            if (_callback != null)
                _callback(parseResult);
        }
    }
}
