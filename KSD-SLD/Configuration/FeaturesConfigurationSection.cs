using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class FeaturesConfigurationSection : ConfigurationSection
    {
        public static FeaturesConfigurationSection GetSection()
        {
            return (FeaturesConfigurationSection) ConfigurationManager.GetSection("features");
        }

        public static FeaturesConfigurationSection GetSection(string name)
        {
            return (FeaturesConfigurationSection)ConfigurationManager.GetSection(name);
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
        [ConfigurationCollection(typeof(FeaturesConfigurationElementCollection), AddItemName = "feature")]
        public FeaturesConfigurationElementCollection Features
        {
            get
            {
                return base[""] as FeaturesConfigurationElementCollection;
            }
        }
    }
}
