using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public sealed class SocketReceiver : BaseSocketReceiver
    {
        private const int _maxBufferSize = 2048;

        protected override void ReceiveImplement(ITelegramSocket socket)
        {
            byte[] receiveBuffer;
            if (!socket.Receive(_maxBufferSize, out receiveBuffer))
                return;

            ProcessReceiveData(receiveBuffer);
        }

        protected override void ReceiveAsyncImplement(ITelegramSocket socket)
        {
            socket.ReceiveAsync(_maxBufferSize, (isSuccess, receiveBuffer) => {
                if (!isSuccess)
                    return;
                
                ProcessReceiveData(receiveBuffer);
            });
        }

        public void ProcessReceiveData(byte[] buffer)
        {
            // todo: implement your own receive logic
            throw new NotImplementedException();
        }
    }
}
