using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.ServiceModel
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ServiceParameterIgnoreAttribute : Attribute
    {
    }
}
