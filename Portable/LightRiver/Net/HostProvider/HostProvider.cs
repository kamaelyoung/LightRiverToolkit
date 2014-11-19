using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public class HostProvider
    {
        /// <summary>
        /// Current Server Index
        /// </summary>
        private int _currentIndex = 0;

        private readonly List<Host> _hostList = new List<Host>();
        public IReadOnlyList<Host> Hosts
        {
            get { return _hostList; }
        }

        public Host Current
        {
            get
            {
                return this[_currentIndex];
            }
        }

        public Host this[int index]
        {
            get
            {
                if (_hostList.Count == 0)
                    return null;

                if (index < 0 || index > _hostList.Count - 1)
                    return null;

                return _hostList[index];
            }
        }

        public Host Proxy { get; set; }

        public HostProvider()
        {
        }

        public HostProvider(IEnumerable<string> ipPortList, char split = ' ')
        {
            AddRagne(ipPortList, split);
        }

        public void AddRagne(IEnumerable<string> ipPortList, char split = ' ')
        {
            foreach (var ipPort in ipPortList) {
                _hostList.Add(new Host(ipPort, split));
            }
        }

        public void Clear()
        {
            _hostList.Clear();
            _currentIndex = 0;
        }

        public void MoveNext()
        {
            _currentIndex++;
        }

        public void ResetIndex()
        {
            _currentIndex = 0;
        }
    }
}
