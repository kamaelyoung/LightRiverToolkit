using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public class Host
    {
        /// <summary>
        /// server address (IP address)
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// connection port (between 0 ~ 65535)
        /// </summary>
        public string Port { get; set; }

        public Host(string ipPort, char split = ' ')
        {
            var sections = ipPort.Split(split);
            if (sections.Length < 2)
                throw new ArgumentException(string.Format("ipPort argemnt split less than two part by {0}", split));

            IP = sections[0];
            Port = sections[1];
        }

        public Host(string ip, string port)
        {
            IP = ip;
            Port = port;
        }
    }
}
