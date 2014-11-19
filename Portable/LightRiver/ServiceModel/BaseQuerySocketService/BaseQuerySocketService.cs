using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LightRiver.Net;

namespace LightRiver.ServiceModel
{
    public abstract class BaseQuerySocketService<TResult, TParameter, TParser>
        where TParser : BaseParser<TResult, Telegram>, new()
    {
        class QueryWorkItem
        {
            public Telegram TelegramToSend { get; set; }

            public IQueryDispatcher Dispatcher { get; set; }

            public ParserPool ParserPool { get; set; }

            public int WaitTimeout { get; set; }

            private ManualResetEvent _waitEvent = new ManualResetEvent(false);

            public ParseResult<TResult> Result { get; private set; }

            public void Invoke()
            {
                Dispatcher.Enqueue(TelegramToSend, DispatcherCallback);
                _waitEvent.WaitOne(TimeSpan.FromMilliseconds(WaitTimeout));
                _waitEvent.Dispose();
                _waitEvent = null;
            }

            private void DispatcherCallback(Telegram telegram)
            {
                var parseWorkItem = new ParseWorkItem<TResult, Telegram>(telegram, new TParser(), ParserCallback);
                ParserPool.Enqueue(parseWorkItem);
            }

            private void ParserCallback(ParseResult<TResult> parseResult)
            {
                if (_waitEvent == null)
                    return;

                Result = parseResult;
                _waitEvent.Set();
            }
        }

        private readonly ParserPool _parserPool;

        private readonly IQueryDispatcher _queryDispatcher;
        public IQueryDispatcher QueryDispatcher
        {
            get
            {
                return _queryDispatcher;
            }
        }

        public BaseQuerySocketService(IQueryDispatcher queryDispatcher, ParserPool parserPool)
        {
            _queryDispatcher = queryDispatcher;
            _parserPool = parserPool;
        }

        public virtual Task<ParseResult<TResult>> InvokeAsync(TParameter parameter, int timeout = 60000)
        {
            return Task.Factory.StartNew(() => {
                var queryWorkItem = new QueryWorkItem {
                    TelegramToSend = PackParameterToSend(parameter),
                    Dispatcher = _queryDispatcher,
                    ParserPool =  _parserPool,
                    WaitTimeout = timeout,
                };
                queryWorkItem.Invoke();
                return queryWorkItem.Result;
            });
        }

        protected abstract Telegram PackParameterToSend(TParameter parameter);
    }
}
