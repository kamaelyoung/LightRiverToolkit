using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public abstract class BaseSocketReceiver
    {
        public event EventHandler<SocketReceivedEventArgs> ReceiveFinished;
        protected void OnReceiveFinished(SocketReceivedEventArgs e)
        {
            SafeRaise.Raise(ReceiveFinished, this, e);
        }

        public void Receive(ITelegramSocket socket)
        {
            ReceiveImplement(socket);
        }

        public void ReceiveAsync(ITelegramSocket socket)
        {
            if (!socket.IsReceiveComplete)
                return;

            ReceiveAsyncImplement(socket);
        }

        protected abstract void ReceiveImplement(ITelegramSocket socket);

        protected abstract void ReceiveAsyncImplement(ITelegramSocket socket);
    }
}
