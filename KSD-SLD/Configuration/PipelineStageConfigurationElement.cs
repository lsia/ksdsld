using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class PipelineStageConfigurationElement : ConfigurationElement
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

        [ConfigurationProperty("cfg", IsRequired = false)]
        public string Configuration
        {
            get
            {
                return (string)this["cfg"];
            }
            set
            {
                this["cfg"] = value;
            }
        }

        [ConfigurationProperty("file", IsRequired = false)]
        public string File
        {
            get
            {
                return (string)this["file"];
            }
            set
            {
                this["file"] = value;
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

        [ConfigurationProperty("action", IsRequired = false)]
        public string Action
        {
            get
            {
                return (string)this["action"];
            }
            set
            {
                this["action"] = value;
            }
        }
    }
}
