using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public interface IQueryDispatcher : IDispatcher
    {
        void Enqueue(Telegram telegram, Action<Telegram> callback);
    }
}
