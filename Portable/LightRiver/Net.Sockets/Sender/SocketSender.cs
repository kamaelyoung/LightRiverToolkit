using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public sealed class SocketSender : BaseSocketSender
    {
        protected override bool SendImplement(ITelegramSocket socket, Telegram telegram)
        {
            return socket.Send(telegram.Buffer);
        }

        protected override void SendAsyncImplement(ITelegramSocket socket, Telegram telegram, Action<bool> callback)
        {
            socket.SendAsync(telegram.Buffer, callback);
        }
    }
}
