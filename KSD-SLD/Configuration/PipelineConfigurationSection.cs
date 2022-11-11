using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class PipelineConfigurationSection : ConfigurationSection
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

        [ConfigurationProperty("parallel", IsRequired = false)]
        public bool Parallel
        {
            get
            {
                return (bool)this["parallel"];
            }
            set
            {
                this["parallel"] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(PipelineStageConfigurationElementCollection), AddItemName = "stage")]
        public PipelineStageConfigurationElementCollection Stages
        {
            get
            {
                return base[""] as PipelineStageConfigurationElementCollection;
            }
        }
    }
}
