using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public interface ITelegramSocket : IDisposable
    {
        event EventHandler<SocketErrorEventArgs> ErrorOccured;

        bool IsSendComplete { get; }

        bool IsReceiveComplete { get; }

        void Create();

        bool Connect(string host, string port);

        bool Send(byte[] buffer);

        void SendAsync(byte[] buffer, Action<bool> callback);

        bool Receive(int length, out byte[] receiveBuffer);

        void ReceiveAsync(int length, Action<bool, byte[]> callback);

        void Disconnect();
    }
}
