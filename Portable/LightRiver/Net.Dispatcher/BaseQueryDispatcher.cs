using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public abstract class BaseQueryDispatcher<TConnector, TSender, TReceiver> : BaseDispatcher<TConnector, TSender, TReceiver>, IQueryDispatcher, IDisposable
        where TConnector : BaseSocketConnector, new()
        where TSender : BaseSocketSender, new()
        where TReceiver : BaseSocketReceiver, new()
    {
        private readonly object _historyMapLockObj = new object();

        private readonly Dictionary<int, Action<Telegram>> _historyMap = new Dictionary<int, Action<Telegram>>();
        protected Dictionary<int, Action<Telegram>> HistoryMap
        {
            get
            {
                return _historyMap;
            }
        }

        public BaseQueryDispatcher(ITelegramSocket socket)
            : base(socket)
        {
        }

        public void Enqueue(Telegram telegram, Action<Telegram> callback)
        {
            lock (_historyMapLockObj) {
                if (callback != null)
                    HistoryMap.Add(telegram.SerialNo, callback);
            }

            Enqueue(telegram);
        }

        protected void EnqueuePrority(Telegram telegram, Action<Telegram> callback)
        {
            lock (_historyMapLockObj) {
                if (callback != null)
                    HistoryMap.Add(telegram.SerialNo, callback);
            }

            EnqueuePrority(telegram);
        }
    }
}
