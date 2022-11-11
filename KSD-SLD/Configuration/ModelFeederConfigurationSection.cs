using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class ModelFeederConfigurationSection : ConfigurationSection
    {
        public static ModelFeederConfigurationSection GetSection(string name)
        {
            return (ModelFeederConfigurationSection) ConfigurationManager.GetSection(name);
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
        [ConfigurationCollection(typeof(ModelSetConfigurationElementCollection), AddItemName = "modelSet")]
        public ModelSetConfigurationElementCollection ModelSets
        {
            get
            {
                return base[""] as ModelSetConfigurationElementCollection;
            }
        }
    }
}
