using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightRiver.Net;

namespace LightRiver
{
    public sealed class TelegramSocket : ITelegramSocket
    {
        #region event

        public event EventHandler<SocketErrorEventArgs> ErrorOccured;
        private void OnConnectErrorOccured(SocketAsyncEventArgs e)
        {
            var args = CreateEventArgs(e, SocketErrorEventArgs.SocketMethod.Connect);
            SafeRaise.Raise(ErrorOccured, this, args);
        }

        private void OnSendErrorOccured(SocketAsyncEventArgs e)
        {
            var args = CreateEventArgs(e, SocketErrorEventArgs.SocketMethod.Send);
            SafeRaise.Raise(ErrorOccured, this, args);
        }

        private void OnReceiveErrorOccured(SocketAsyncEventArgs e)
        {
            var args = CreateEventArgs(e, SocketErrorEventArgs.SocketMethod.Receive);
            SafeRaise.Raise(ErrorOccured, this, args);
        }

        private SocketErrorEventArgs CreateEventArgs(SocketAsyncEventArgs e, SocketErrorEventArgs.SocketMethod method)
        {
            var endPoint = e.RemoteEndPoint as IPEndPoint;
            var errorMessage = string.Format("Method:{0}|{1}:{2}|{3}",
                method,
                ((endPoint != null) ? endPoint.Address.ToString() : string.Empty),
                ((endPoint != null) ? endPoint.Port.ToString() : string.Empty),
                e.SocketError);

            return new SocketErrorEventArgs(method, errorMessage);
        }

        #endregion

        #region field

        private bool _isDisposed = false;

        private Socket _clientSocket = null;

        private readonly ManualResetEvent _connectEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent _sendEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent _receiveEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent _disconnectEvent = new ManualResetEvent(false);

        #endregion

        public bool IsSendComplete { get; private set; }
        public bool IsReceiveComplete { get; private set; }

        public TelegramSocket()
        {
            Create();

            IsSendComplete = true;
            IsReceiveComplete = true;
        }

        public void Create()
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(string host, string port)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(host), int.Parse(port));
            var connectSocketEventArgs = new SocketAsyncEventArgs {
                RemoteEndPoint = endPoint
            };
            connectSocketEventArgs.Completed += connectSocketEventArgs_Completed;

            _connectEvent.Reset();
            bool completesAsynchronously = _clientSocket.ConnectAsync(connectSocketEventArgs);
            if (completesAsynchronously)
                _connectEvent.WaitOne();

            var isConnectSuccess = (connectSocketEventArgs.SocketError == SocketError.Success);
            if (!isConnectSuccess)
                OnConnectErrorOccured(connectSocketEventArgs);

            connectSocketEventArgs.Dispose();
            return isConnectSuccess;
        }

        private void connectSocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _connectEvent.Set();
        }

        public bool Send(byte[] buffer)
        {
            var sendSocketEventArgs = new SocketAsyncEventArgs();
            sendSocketEventArgs.SetBuffer(buffer, 0, buffer.Length);
            sendSocketEventArgs.Completed += sendSocketEventArgs_Completed;

            _sendEvent.Reset();

            bool completesAsynchronously = _clientSocket.SendAsync(sendSocketEventArgs);
            if (completesAsynchronously)
                _sendEvent.WaitOne();

            var isSendSuccess = (sendSocketEventArgs.SocketError == SocketError.Success);
            if (!isSendSuccess) {
                OnSendErrorOccured(sendSocketEventArgs);
            }

            sendSocketEventArgs.Dispose();
            return isSendSuccess;
        }

        private void sendSocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _sendEvent.Set();
        }

        public void SendAsync(byte[] buffer, Action<bool> callback)
        {
            IsSendComplete = false;

            var sendSocketEventArgs = new SocketAsyncEventArgs();
            sendSocketEventArgs.SetBuffer(buffer, 0, buffer.Length);
            sendSocketEventArgs.UserToken = callback;
            sendSocketEventArgs.Completed += sendSocketEventArgs_CompletedAsync;

            bool completesAsynchronously = _clientSocket.SendAsync(sendSocketEventArgs);
            if (completesAsynchronously)
                return;

            sendSocketEventArgs_CompletedAsync(null, sendSocketEventArgs);
        }

        private void sendSocketEventArgs_CompletedAsync(object sender, SocketAsyncEventArgs e)
        {
            var isSendSuccess = (e.SocketError == SocketError.Success);
            var callback = e.UserToken as Action<bool>;
            e.UserToken = null;

            Debug.Assert(callback != null);

            if (!isSendSuccess)
                OnSendErrorOccured(e);

            e.Dispose();
            callback(isSendSuccess);

            IsSendComplete = true;
        }


        public bool Receive(int length, out byte[] receiveBuffer)
        {
            receiveBuffer = null;
            var buffer = new byte[length];
            var receiveSocketEventArgs = new SocketAsyncEventArgs();
            receiveSocketEventArgs.SetBuffer(buffer, 0, length);
            receiveSocketEventArgs.Completed += receiveSocketEventArgs_Completed;

            _receiveEvent.Reset();

            bool completesAsynchronously = _clientSocket.ReceiveAsync(receiveSocketEventArgs);
            if (completesAsynchronously)
                _receiveEvent.WaitOne();

            var isReceiveSuccess = (receiveSocketEventArgs.SocketError == SocketError.Success);
            if (!isReceiveSuccess) {
                OnReceiveErrorOccured(receiveSocketEventArgs);
            }
            else {
                receiveBuffer = new byte[receiveSocketEventArgs.BytesTransferred];
                Buffer.BlockCopy(receiveSocketEventArgs.Buffer, 0, receiveBuffer, 0, receiveBuffer.Length);
            }

            receiveSocketEventArgs.Dispose();
            return isReceiveSuccess;
        }

        private void receiveSocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _receiveEvent.Set();
        }

        public void ReceiveAsync(int length, Action<bool, byte[]> callback)
        {
            IsReceiveComplete = false;

            var buffer = new byte[length];
            var receiveSocketEventArgs = new SocketAsyncEventArgs();
            receiveSocketEventArgs.SetBuffer(buffer, 0, length);
            receiveSocketEventArgs.UserToken = callback;
            receiveSocketEventArgs.Completed += receiveSocketEventArgs_CompletedAsync;

            bool completesAsynchronously = _clientSocket.ReceiveAsync(receiveSocketEventArgs);
            if (completesAsynchronously)
                return;

            receiveSocketEventArgs_CompletedAsync(null, receiveSocketEventArgs);
        }

        private void receiveSocketEventArgs_CompletedAsync(object sender, SocketAsyncEventArgs e)
        {
            byte[] receiveBuffer = null;

            var isReceiveSuccess = (e.SocketError == SocketError.Success);
            var callback = e.UserToken as Action<bool, byte[]>;
            e.UserToken = null;

            Debug.Assert(callback != null);

            if (!isReceiveSuccess) {
                OnReceiveErrorOccured(e);
            }
            else {
                receiveBuffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, receiveBuffer, 0, receiveBuffer.Length);
            }

            e.Dispose();
            callback(isReceiveSuccess, receiveBuffer);

            IsReceiveComplete = true;
        }

        public void Disconnect()
        {
            if (_clientSocket == null)
                return;

            _disconnectEvent.Reset();

            var disconnectSocketEventArgs = new SocketAsyncEventArgs();
            disconnectSocketEventArgs.Completed += disconnectSocketEventArgs_Completed;
            bool completesAsynchronously = _clientSocket.DisconnectAsync(disconnectSocketEventArgs);
            if (completesAsynchronously)
                _disconnectEvent.WaitOne();

            disconnectSocketEventArgs.Dispose();
        }

        private void disconnectSocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _disconnectEvent.Set();
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

            if (_clientSocket != null) {
                if (_clientSocket.Connected) {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Disconnect(false);
                }
                _clientSocket.Close();
                _clientSocket.Dispose();
                _clientSocket = null;
            }

            _isDisposed = true;
        }
    }
}
