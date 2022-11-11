using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class PipelineStageConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PipelineStageConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as PipelineStageConfigurationElement).Name;
        }
    }
}
