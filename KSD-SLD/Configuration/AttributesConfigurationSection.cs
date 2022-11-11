using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class AttributesConfigurationSection : ConfigurationSection
    {
        public static AttributesConfigurationSection GetSection(string name)
        {
            return (AttributesConfigurationSection) ConfigurationManager.GetSection(name);
        }

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

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(AttributesConfigurationElementCollection), AddItemName = "attribute")]
        public AttributesConfigurationElementCollection Attributes
        {
            get
            {
                return base[""] as AttributesConfigurationElementCollection;
            }
        }
    }
}
