using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class ModelSetConfigurationElement : ConfigurationElement
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

        [ConfigurationProperty("parameter", IsRequired = true)]
        public string Parameter
        {
            get
            {
                return (string)this["parameter"];
            }
            set
            {
                this["parameter"] = value;
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

        [ConfigurationProperty("storage", IsRequired = true)]
        public string Storage
        {
            get
            {
                return (string) this["storage"];
            }
            set
            {
                this["storage"] = value;
            }
        }

        [ConfigurationProperty("maxContextSize", IsRequired = true)]
        public int MaxContextSize
        {
            get
            {
                return (int) this["maxContextSize"];
            }
            set
            {
                this["maxContextSize"] = value;
            }
        }

        [ConfigurationProperty("maxNGramSize", IsRequired = true)]
        public int MaxNGramSize
        {
            get
            {
                return (int) this["maxNGramSize"];
            }
            set
            {
                this["maxNGramSize"] = value;
            }
        }
    }
}
