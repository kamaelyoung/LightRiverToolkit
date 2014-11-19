using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LightRiver
{
    public class DispatcherService : IDispatcherService
    {
        public void InvokeAsync(Action action)
        {
            var task = Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                action();
            }).AsTask();
        }
    }
}
