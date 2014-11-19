using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace LightRiver.ServiceModel
{
    public class HttpServiceParameter
    {
        [ServiceParameterIgnore]
        public HttpMethod Method { get; set;}

        public HttpServiceParameter()
        {
            Method = HttpMethod.Get;
        }
    }
}
