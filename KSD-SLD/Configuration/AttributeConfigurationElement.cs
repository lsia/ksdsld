using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class AttributeConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("source", IsRequired = false)]
        public string Source
        {
            get
            {
                return (string)this["source"];
            }
            set
            {
                this["source"] = value;
            }
        }

        [ConfigurationProperty("parameters", IsRequired = false)]
        public string Parameters
        {
            get
            {
                return (string)this["parameters"];
            }
            set
            {
                this["parameters"] = value;
            }
        }
    }
}
