using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public abstract class BaseDispatcher<TConnector, TSender, TReceiver>
        where TConnector : BaseSocketConnector, new()
        where TSender : BaseSocketSender, new()
        where TReceiver : BaseSocketReceiver, new()
    {
        public event EventHandler<SocketConnectionStateChangeEventArgs> ConnectionStateChanged;
        protected void OnConnectionStateChanged(SocketConnectionStateChangeEventArgs e)
        {
            SafeRaise.Raise(ConnectionStateChanged, this, e);
        }

        public event EventHandler<SocketErrorEventArgs> ErrorOccured;
        protected void OnErrorOccured(SocketErrorEventArgs e)
        {
            SafeRaise.Raise(ErrorOccured, this, e);
        }

        #region Socket related field

        protected readonly ITelegramSocket _socket;

        private readonly TConnector _connector;

        private readonly TSender _sender;

        private readonly TReceiver _receiver;

        #endregion

        #region Task related field

        private Task _exchangeTask;

        private ManualResetEvent _exchangeSleepEvent;

        private bool _threadStopFlag;

        private const int _exchangeWaitPeriod = 50;

        private const int _waitToStopPeriod = 10;

        /// <summary>
        /// 上一次發生錯誤的時間
        /// </summary>
        private DateTime _lastErrorTime = DateTime.MinValue;

        /// <summary>
        /// 相差時間多少才可以允許在乎叫一次(秒)
        /// </summary>
        private const int _errorIgnoreDuration = 10;

        /// <summary>
        /// lock用的
        /// </summary>
        private object _lockErrorObj = new object();

        #endregion

        public bool IsStarted { get; private set; }

        private TelegramSocketConnectionState _connectionState = TelegramSocketConnectionState.Disconnected;
        public TelegramSocketConnectionState ConnectionState
        {
            get { return _connectionState; }
            private set
            {
                if (_connectionState == value)
                    return;

                _connectionState = value;
                OnConnectionStateChanged(new SocketConnectionStateChangeEventArgs(_connectionState));
            }
        }

        public HostProvider HostProvider
        {
            get { return _connector.HostProvider; }
        }

        public int HeartBeatPeriod
        {
            set
            {
                _sender.HeartBeatPeriod = value;
            }
        }

        public BaseDispatcher(ITelegramSocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException();

            _socket = socket;
            _socket.ErrorOccured += SocketOnErrorOccured;

            _connector = new TConnector();
            _sender = new TSender();
            _sender.RequestKeepAlive += SenderOnRequestKeepAlive;
            _receiver = new TReceiver();
            _receiver.ReceiveFinished += ReceiverOnReceiveFinished;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Stop();
            _socket.Dispose();
        }

        public void Start()
        {
            _exchangeSleepEvent = new ManualResetEvent(false);
            _exchangeTask = Task.Factory.StartNew(ExchangeTaskWorkMethod, TaskCreationOptions.LongRunning);

            IsStarted = true;
        }

        public void Stop()
        {
            if (!IsStarted)
                return;

            _threadStopFlag = true;

            _exchangeTask.Wait(_waitToStopPeriod);
            _exchangeSleepEvent.Dispose();

            _socket.Disconnect();

            IsStarted = false;
        }

        public void Enqueue(Telegram telegram)
        {
            _sender.Enqueue(telegram);
        }

        protected void EnqueuePrority(Telegram telegram)
        {
            _sender.EnqueuePrority(telegram);
        }

        private void ExchangeTaskWorkMethod()
        {
            while (!_threadStopFlag) {
                if (!Connect()) {
                    _exchangeSleepEvent.WaitOne(_exchangeWaitPeriod);
                    continue;
                }

                Send();

                Receive();

                _exchangeSleepEvent.WaitOne(_exchangeWaitPeriod);
            }
        }

        private bool Connect()
        {
            if (ConnectionState == TelegramSocketConnectionState.Connected)
                return true;

            var isSuccess = _connector.Connect(_socket);
            if (!isSuccess)
                return false;

            ConnectionState = TelegramSocketConnectionState.Connected;
            return true;
        }

        private void Send()
        {
            _sender.SendAsync(_socket);
        }

        private void Receive()
        {
            _receiver.ReceiveAsync(_socket);
        }

        private void SenderOnRequestKeepAlive(object sender, EventArgs eventArgs)
        {
            OnSenderRequestKeepAlive();
        }

        private void ReceiverOnReceiveFinished(object sender, SocketReceivedEventArgs e)
        {
            OnReceiverReceiveFinished(e);
        }

        private void SocketOnErrorOccured(object sender, SocketErrorEventArgs e)
        {
            Debug.WriteLine("BaseDispatcher error method:{0}, message:{1}", e.Method, e.Message);

            if (e.Method == SocketErrorEventArgs.SocketMethod.Connect)
                OnErrorOccured(e);

            lock (_lockErrorObj) {
                if (DateTime.Now.Subtract(_lastErrorTime) < TimeSpan.FromSeconds(_errorIgnoreDuration))
                    return;

                _lastErrorTime = DateTime.Now;
            }

            _socket.Dispose();
            _socket.Create();

            ConnectionState = TelegramSocketConnectionState.Disconnected;
        }

        protected abstract void OnSenderRequestKeepAlive();

        protected abstract void OnReceiverReceiveFinished(SocketReceivedEventArgs e);
    }
}
