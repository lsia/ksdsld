using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class BiometricParametersConfigurationSection : ConfigurationSection
    {
        public static BiometricParametersConfigurationSection GetSection(string name)
        {
            return (BiometricParametersConfigurationSection) ConfigurationManager.GetSection(name);
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
        [ConfigurationCollection(typeof(BiometricParameterConfigurationElementCollection), AddItemName = "biometricParameter")]
        public BiometricParameterConfigurationElementCollection Parameters
        {
            get
            {
                return base[""] as BiometricParameterConfigurationElementCollection;
            }
        }
    }
}
