using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public sealed class SocketConnector : BaseSocketConnector
    {
        protected override bool ConnectImplement(ITelegramSocket socket, Host serverAddressPair, Host proxy)
        {
            if (proxy == null) {
                return ConnectDirect(socket, serverAddressPair);
            }
            
            return ConnectThroughProxy(socket, serverAddressPair, proxy);
        }

        private bool ConnectDirect(ITelegramSocket socket, Host serverAddressPair)
        {
            return socket.Connect(serverAddressPair.IP, serverAddressPair.Port);
        }

        private bool ConnectThroughProxy(ITelegramSocket socket, Host serverAddressPair, Host proxy)
        {
            if (!ConnectProxy(socket, proxy))
                return false;

            if (!SendProxyConnectProtocol(socket, serverAddressPair))
                return false;

            if (!ReceiveProxyConnectResult(socket))
                return false;

            return true;
        }

        private bool ConnectProxy(ITelegramSocket socket, Host proxy)
        {
            return socket.Connect(proxy.IP, proxy.Port);
        }

        private bool SendProxyConnectProtocol(ITelegramSocket socket, Host serverAddressPair)
        {
            // SOCKS 4 Protocol
            // +----+----+----+----+----+----+----+----+----+----+....+----+
            // | VN | CD | DSTPORT |      DSTIP        | USERID       |NULL|
            // +----+----+----+----+----+----+----+----+----+----+....+----+
            //    1    1      2              4           variable       1

            byte[] buffer = new byte[14];

            buffer[0] = 0x04; // VN
            buffer[1] = 0x01; // CD, 1:CONNECT request，2: BIND request

            // DSTPORT
            UInt16 port = Convert.ToUInt16(serverAddressPair.Port);
            byte[] portBytes = BitConverter.GetBytes(port);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(portBytes);
            Array.Copy(portBytes, 0, buffer, 2, 2);

            // DSTIP
            string[] ipAddresses = serverAddressPair.IP.Split('.');
            byte[] ipBytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                ipBytes[i] = Byte.Parse(ipAddresses[i]);
            }
            Array.Copy(ipBytes, 0, buffer, 4, 4);

            // todo: replace your own socket proxy connect id
            const string userId = "guest";
            byte[] userIDBytes = Encoding.UTF8.GetBytes(userId);
            Array.Copy(userIDBytes, 0, buffer, 8, 5);

            // add c++ string terminated
            buffer[13] = 0x00;

            return socket.Send(buffer);
        }

        private bool ReceiveProxyConnectResult(ITelegramSocket socket)
        {
            // Received data from server
            // Data has now been sent and received from the server. 
            // check result from sock4 proxy
            //
            // SOCKS 4 connect receive protocol
            // +----+----+----+----+----+----+----+----+
            // | VN | CD | DSTPORT |      DSTIP        |
            // +----+----+----+----+----+----+----+----+
            //    1    1      2              4          
            /*
                Socks Server CD(return code)
                90: request granted
                91: request rejected or failed
                92: request rejected becasue SOCKS server cannot connect to
                    identd on the client
                93: request rejected because the client program and identd
                    report different user-ids
            */

            const int bufferLength = 8;
            byte[] receiveBuffer = null;

            if (!socket.Receive(bufferLength, out receiveBuffer))
                return false;

            Debug.Assert(bufferLength == receiveBuffer.Length);
            return (receiveBuffer[1] == 90);
        }
    }
}