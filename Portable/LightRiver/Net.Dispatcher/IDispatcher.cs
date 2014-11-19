using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public interface IDispatcher
    {
        event EventHandler<SocketConnectionStateChangeEventArgs> ConnectionStateChanged;

        event EventHandler<SocketErrorEventArgs> ErrorOccured;

        TelegramSocketConnectionState ConnectionState { get; }

        HostProvider HostProvider { get; }

        bool IsStarted { get; }

        void Enqueue(Telegram telegram);
    }
}
