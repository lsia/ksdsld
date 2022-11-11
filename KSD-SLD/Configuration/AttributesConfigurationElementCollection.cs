using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class AttributesConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AttributeConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as AttributeConfigurationElement).Name;
        }
    }
}
