using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.ServiceModel
{
    public interface IServiceParameterConveter
    {
        string Convert(object value);
    }
}
