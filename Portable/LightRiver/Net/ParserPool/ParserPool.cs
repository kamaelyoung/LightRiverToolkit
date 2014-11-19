using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public enum ParserPoolType
    {
        Query,
        Push,
    }

    public sealed class ParserPool : IDisposable
    {
        #region field

        private readonly List<IParserWorkItem> _itemList = new List<IParserWorkItem>(); 

        private const int _parseSleepPeriod = 50; // ms

        private Task _parseTask = null;

        private ManualResetEvent _parseSleepEvent = new ManualResetEvent(false);

        private bool _threadStopFlag = false;

        private readonly object _queueLockObj = new object();

        private const int _waitToStopPeriod = 10;

        private static readonly Dictionary<ParserPoolType, ParserPool> _parserPoolMap =
            new Dictionary<ParserPoolType, ParserPool>();

        private readonly static object _parsePoolLockobj = new object();

        #endregion

        public bool IsStarted { get; private set; }

        private ParserPool()
        {
        }

        public static ParserPool Get(ParserPoolType parserPoolType)
        {
            lock (_parsePoolLockobj) {
                ParserPool parserPool;

                if (!_parserPoolMap.ContainsKey(parserPoolType)) {
                    parserPool = new ParserPool();
                    parserPool.Start();
                    _parserPoolMap.Add(parserPoolType, parserPool);
                }
                else {
                    parserPool = _parserPoolMap[parserPoolType];
                }

                return parserPool;
                
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Stop();
        }

        private void ParseWorkItemMethod()
        {
            while (!_threadStopFlag) {
                if (_itemList.Count == 0) {
                    _parseSleepEvent.WaitOne(_parseSleepPeriod);
                    continue;
                }

                // start parse data
                lock (_queueLockObj) {
                    Parallel.ForEach(_itemList, workItem => workItem.Execute());
                    _itemList.Clear();
                }

                if (_threadStopFlag)
                    break;
            }
        }

        public void Start()
        {
            if (IsStarted)
                return;

            _threadStopFlag = false;

            _parseSleepEvent = new ManualResetEvent(false);
            _parseTask = Task.Factory.StartNew(ParseWorkItemMethod, TaskCreationOptions.LongRunning);
            
            IsStarted = true;
        }

        public void Stop()
        {
            if (!IsStarted)
                return;

            _threadStopFlag = true;
            
            _parseTask.Wait(_waitToStopPeriod);
            _parseSleepEvent.Dispose();

            //_itemQueue.Clear();
            _itemList.Clear();

            IsStarted = false;
        }

        public void Enqueue(IParserWorkItem workItem)
        {
            lock (_queueLockObj) {
                Debug.Assert(workItem != null);

                if (workItem == null)
                    return;

                _itemList.Add(workItem);
            }
        }
    }
}
