using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LightRiver
{
    public class DispatcherService : IDispatcherService
    {
        public void InvokeAsync(Action action)
        {
            Application.Current.Dispatcher.InvokeAsync(action);
        }
    }
}
