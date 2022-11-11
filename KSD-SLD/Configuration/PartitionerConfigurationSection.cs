using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class PartitionerConfigurationSection : ConfigurationSection
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

        [ConfigurationProperty("threshold", IsRequired = false)]
        public string Threshold
        {
            get
            {
                return (string)this["threshold"];
            }
            set
            {
                this["threshold"] = value;
            }
        }
    }
}
