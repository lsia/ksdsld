using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class BiometricParameterConfigurationElement : ConfigurationElement
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
    }
}
