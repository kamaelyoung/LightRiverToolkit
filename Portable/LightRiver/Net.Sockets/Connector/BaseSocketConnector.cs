using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public enum TelegramSocketConnectionState
    {
        Connected,
        Disconnected,
    };

    public abstract class BaseSocketConnector
    {
        #region Property

        private readonly HostProvider _hostProvider = new HostProvider();
        public HostProvider HostProvider
        {
            get { return _hostProvider; }
        }

        public int MaxTriedConnectTimes { get; set; }

        private int RetriedConnectTimes { get; set; }

        #endregion

        protected BaseSocketConnector()
        {
            MaxTriedConnectTimes = 3;
            RetriedConnectTimes = 0;
        }

        public bool Connect(ITelegramSocket socket)
        {
            if (_hostProvider.Hosts.Count == 0) {
                return false;
            }

            if (RetriedConnectTimes >= MaxTriedConnectTimes) {
                Debug.WriteLine("SocketConnectBehave|RetriedConnectTimes exceed MaxTriedConnectTimes");
                RetriedConnectTimes = 0;
                return false;
            }

            var serverAddressPair = HostProvider.Current;
            if (serverAddressPair == null) {
                RetriedConnectTimes++;
                HostProvider.ResetIndex();
                return false;
            }

            var isSuccess = ConnectImplement(socket, serverAddressPair, HostProvider.Proxy);

            if (isSuccess) {
                RetriedConnectTimes = 0;
            }
            else {
                HostProvider.MoveNext();
            }

            return isSuccess;
        }

        protected abstract bool ConnectImplement(ITelegramSocket socket, Host serverAddressPair, Host proxy);
    }
}
