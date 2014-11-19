using System;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using LightRiver.Net;

namespace LightRiver
{
    public sealed class TelegramSocket : ITelegramSocket
    {
        #region event

        public event EventHandler<SocketErrorEventArgs> ErrorOccured;
        private void OnConnectErrorOccured(Exception ex)
        {
            var args = CreateEventArgs(SocketErrorEventArgs.SocketMethod.Connect, ex);
            SafeRaise.Raise<SocketErrorEventArgs>(ErrorOccured, this, args);
        }

        private void OnSendErrorOccured(Exception ex)
        {
            var args = CreateEventArgs(SocketErrorEventArgs.SocketMethod.Send, ex);
            SafeRaise.Raise<SocketErrorEventArgs>(ErrorOccured, this, args);
        }

        private void OnReceiveErrorOccured(Exception ex)
        {
            var args = CreateEventArgs(SocketErrorEventArgs.SocketMethod.Receive, ex);
            SafeRaise.Raise<SocketErrorEventArgs>(ErrorOccured, this, args);
        }
        
        private SocketErrorEventArgs CreateEventArgs(SocketErrorEventArgs.SocketMethod method, Exception ex)
        {
            var errorMessage = string.Format("Method:{0}|{1}:{2}|{3}",
                method,
                _clientSocket.Information.RemoteAddress.DisplayName,
                _clientSocket.Information.RemotePort,
                SocketError.GetStatus(ex.HResult).ToString());

            return new SocketErrorEventArgs(method, errorMessage);
        }

        #endregion

        #region field

        private StreamSocket _clientSocket = null;

        /// <summary>
        /// 紀錄ConnectAsync的操作
        /// </summary>
        private IAsyncAction _connectOperation = null;

        /// <summary>
        /// 透過Socket發送資料的
        /// </summary>
        private DataWriter _dataWriter = null;

        /// <summary>
        /// 紀錄DataWriter的Async的操作
        /// </summary>
        private DataWriterStoreOperation _dataWriterOperation = null;

        /// <summary>
        /// 透過Socket讀取資料
        /// </summary>
        private DataReader _dataReader = null;

        /// <summary>
        /// 紀錄DataReader的Async的動作
        /// </summary>
        private DataReaderLoadOperation _dataReaderOperation = null;

        /// <summary>
        /// 重新建立Socket的LockObject
        /// </summary>
        private object _recreateLock = new object();

        private bool _isDisposed = false;

        #endregion

        public bool IsSendComplete
        {
            get
            {
                return _dataWriterOperation == null;
            }
        }

        public bool IsReceiveComplete
        {
            get
            {
                return _dataReaderOperation == null;
            }
        }

        public TelegramSocket()
        {
            Create();
        }

        public void Create()
        {
            _clientSocket = new StreamSocket();
            _dataReader = new DataReader(_clientSocket.InputStream) {
                InputStreamOptions = InputStreamOptions.Partial
            };
            _dataWriter = new DataWriter(_clientSocket.OutputStream);
        }

        public bool Connect(string host, string port)
        {
            _connectOperation = _clientSocket.ConnectAsync(new HostName(host), port);
            _connectOperation.AsTask().Wait();

            if (_connectOperation.ErrorCode == null) {
                _connectOperation = null;
                return true;
            }

            OnConnectErrorOccured(_connectOperation.ErrorCode);
            
            return false;
        }

        public bool Send(byte[] buffer)
        {
            _dataWriter.WriteBytes(buffer);
            _dataWriterOperation = _dataWriter.StoreAsync();
            _dataWriterOperation.AsTask().Wait();

            if (_dataWriterOperation.ErrorCode != null) {
                OnSendErrorOccured(_dataWriterOperation.ErrorCode);
                _dataWriterOperation = null;
                return false;
            }

            _dataWriterOperation = null;
            return true;
        }

        public void SendAsync(byte[] buffer, Action<bool> callback)
        {
            if (_dataWriterOperation != null)
                return;
            
            _dataWriter.WriteBytes(buffer);
            _dataWriterOperation = _dataWriter.StoreAsync();
            _dataWriterOperation.Completed = (info, status) => {
                var isSuccess = _dataWriterOperation.ErrorCode == null;
                if (!isSuccess) {
                    OnSendErrorOccured(_dataWriterOperation.ErrorCode);
                }

                callback(isSuccess);
                
                _dataWriterOperation = null;
            };
        }

        public bool Receive(int length, out byte[] receiveBuffer)
        {
            receiveBuffer = null;

            _dataReaderOperation = _dataReader.LoadAsync(Convert.ToUInt32(length));
            _dataReaderOperation.AsTask().Wait();

            if (_dataReaderOperation.ErrorCode != null) {
                OnReceiveErrorOccured(_dataReaderOperation.ErrorCode);
                _dataReaderOperation = null;
                return false;
            }

            _dataReaderOperation = null;
            if (_dataReader.UnconsumedBufferLength <= 0)
                return true;

            receiveBuffer = new byte[_dataReader.UnconsumedBufferLength];
            _dataReader.ReadBytes(receiveBuffer);
            return true;
        }

        public void ReceiveAsync(int length, Action<bool, byte[]> callback)
        {
            if (_dataReaderOperation != null)
                return;
            
            _dataReaderOperation = _dataReader.LoadAsync(Convert.ToUInt32(length));
            _dataReaderOperation.Completed = (info, status) => {
                var isSuccess = _dataReaderOperation.ErrorCode == null;
                if (!isSuccess) {
                    OnReceiveErrorOccured(_dataReaderOperation.ErrorCode);
                }
                
                if (_dataReader.UnconsumedBufferLength <= 0)
                    return;

                var receiveBuffer = new byte[_dataReader.UnconsumedBufferLength];
                _dataReader.ReadBytes(receiveBuffer);
                
                callback(isSuccess, receiveBuffer);

                _dataReaderOperation = null;
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (_dataWriterOperation != null)
                _dataWriterOperation.Cancel();

            if (_dataReaderOperation != null)
                _dataReaderOperation.Cancel();

            // connectOperaion 要放在最後一個cancel, 否則在下面的_dataReader.DetachStream會出現Exception
            if (_connectOperation != null)
                _connectOperation.Cancel();

            DisposeDataReader();

            DisposeDataWriter();

            if (_clientSocket != null) {
                _clientSocket.Dispose();
                _clientSocket = null;
            }

            _isDisposed = true;
        }

        private void DisposeDataReader()
        {
            if (_dataReader == null)
                return;

            bool isError = false;
            try {
                _dataReader.DetachBuffer();
                _dataReader.DetachStream();
                _dataReader.Dispose();
            }
            catch {
                isError = true;
            }
            finally {
                if (!isError && _dataReader != null)
                    _dataReader.Dispose();

                _dataReader = null;
            }
        }

        private void DisposeDataWriter()
        {
            if (_dataWriter == null)
                return;

            bool isError = false;
            try {
                _dataWriter.DetachBuffer();
                _dataWriter.DetachStream();
                _dataWriter.Dispose();
            }
            catch {
                isError = true;
            }
            finally {
                if (!isError && _dataWriter != null)
                    _dataWriter.Dispose();

                _dataWriter = null;
            }
        }

        public void Disconnect()
        {
            if (_clientSocket == null)
                return;

            _clientSocket.Dispose();
        }
    }
}
