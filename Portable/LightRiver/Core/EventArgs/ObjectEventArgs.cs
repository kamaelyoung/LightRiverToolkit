using System;

namespace LightRiver
{
    public class ObjectEventArgs : EventArgs
    {
        public Object Tag { get; private set; }

        public ObjectEventArgs(object obj)
        {
            Tag = obj;
        }
    }
}
