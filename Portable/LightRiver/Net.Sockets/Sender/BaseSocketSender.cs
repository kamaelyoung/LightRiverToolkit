using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public abstract class BaseSocketSender
    {
        public event EventHandler RequestKeepAlive;
        private void OnRequestKeepAlive()
        {
            SafeRaise.Raise(RequestKeepAlive, this);
        }

        private readonly Queue<Telegram> _sendQueue = new Queue<Telegram>();

        private readonly Queue<Telegram> _proritySendQueue = new Queue<Telegram>();

        private readonly Dictionary<Telegram, int> _telegramSendRecord = new Dictionary<Telegram, int>();

        private DateTime _lastSendTime = DateTime.Now;

        public int HeartBeatPeriod { get; set; }

        public void Enqueue(Telegram telegram)
        {
            _sendQueue.Enqueue(telegram);
            _telegramSendRecord.Add(telegram, 0);
        }

        public void EnqueuePrority(Telegram telegram)
        {
            _proritySendQueue.Enqueue(telegram);
            _telegramSendRecord.Add(telegram, 0);
        }

        public void Send(ITelegramSocket socket)
        {
            if (_proritySendQueue.Count == 0 && _sendQueue.Count == 0) {
                var timeFromLastSend = DateTime.Now - _lastSendTime;
                if (timeFromLastSend.TotalMilliseconds > HeartBeatPeriod) {
                    OnRequestKeepAlive();
                    return;
                }
            }

            Telegram telegram = null;
            bool isFromProrityQueue = false;

            if (_proritySendQueue.Count > 0) {
                isFromProrityQueue = true;
                telegram = _proritySendQueue.Peek();
            }
            else if (_sendQueue.Count > 0) {
                telegram = _sendQueue.Peek();
            }

            if (telegram == null)
                return;

            var isSuccess = SendImplement(socket, telegram);
            if (isSuccess) {
                // 將_lastSendTime重置
                _lastSendTime = DateTime.Now;
                Dequeue(telegram, isFromProrityQueue);
                return;
            }

            // 失敗了, 將重送次數+1
            _telegramSendRecord[telegram]++;

            // 比對可重送的最大次數, 若已經達到最大次數則不重送
            if (_telegramSendRecord[telegram] >= telegram.MaxResendTimes) {
                Dequeue(telegram, isFromProrityQueue);
            }
        }

        public void SendAsync(ITelegramSocket socket)
        {
            if (!socket.IsSendComplete)
                return;
            
            if (_proritySendQueue.Count == 0 && _sendQueue.Count == 0) {
                var timeFromLastSend = DateTime.Now - _lastSendTime;
                if (timeFromLastSend.TotalMilliseconds > HeartBeatPeriod) {
                    OnRequestKeepAlive();
                    return;
                }
            }

            Telegram telegram = null;
            bool isFromProrityQueue = false;

            if (_proritySendQueue.Count > 0) {
                isFromProrityQueue = true;
                telegram = _proritySendQueue.Peek();
            }
            else if (_sendQueue.Count > 0) {
                telegram = _sendQueue.Peek();
            }

            if (telegram == null)
                return;

            SendAsyncImplement(socket, telegram, (isSuccess) => {
                if (isSuccess) {
                    // 將_lastSendTime重置
                    _lastSendTime = DateTime.Now;
                    Dequeue(telegram, isFromProrityQueue);
                    return;
                }

                // 失敗了, 將重送次數+1
                _telegramSendRecord[telegram]++;

                // 比對可重送的最大次數, 若已經達到最大次數則不重送
                if (_telegramSendRecord[telegram] >= telegram.MaxResendTimes) {
                    Dequeue(telegram, isFromProrityQueue);
                }
            });
        }

        private void Dequeue(Telegram telegram, bool isFromProrityQueue)
        {
            if (isFromProrityQueue) {
                _proritySendQueue.Dequeue();
            }
            else {
                _sendQueue.Dequeue();
            }

            _telegramSendRecord.Remove(telegram);
        }

        protected abstract bool SendImplement(ITelegramSocket socket, Telegram telegram);

        protected abstract void SendAsyncImplement(ITelegramSocket socket, Telegram telegram, Action<bool> callback);
    }
}
