using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightRiver.ServiceModel;

namespace LightRiver.ServiceModel
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ServicePrameterPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public Type ConveterType { get; private set; }

        public ServicePrameterPropertyAttribute(string name, Type converterType = null)
        {
            Name = name;
            ConveterType = converterType;
        }
    }
}
