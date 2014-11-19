using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public class SocketReceivedEventArgs : EventArgs
    {
        public Telegram Telegram { get; private set; }

        public SocketReceivedEventArgs(Telegram telegram)
        {
            Telegram = telegram;
        }
    }

    public class SocketConnectionStateChangeEventArgs : EventArgs
    {
        public TelegramSocketConnectionState State { get; private set; }

        public SocketConnectionStateChangeEventArgs(TelegramSocketConnectionState state)
        {
            State = state;
        }
    }

    public class SocketErrorEventArgs : EventArgs
    {
        public enum SocketMethod
        {
            Connect,
            Send,
            Receive,
        };

        public SocketMethod Method { get; private set; }

        public string Message { get; private set; }

        //public SocketErrorEventArgs()
        //{
        //}

        public SocketErrorEventArgs(SocketMethod socketMethod, string message)
        {
            this.Method = socketMethod;
            this.Message = message;
        }
    }
}
