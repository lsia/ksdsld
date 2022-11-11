using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    class ConfigurationFileAttribute : Attribute
    {
        public string Name { get; private set; }
        public ConfigurationFileAttribute(string name)
        {
            Name = name;
        }

        public bool IncludeConfigurationAsFolder { get; private set; }
        public ConfigurationFileAttribute(string name, bool include_configuration_as_folder)
        {
            Name = name;
            IncludeConfigurationAsFolder = include_configuration_as_folder;
        }
    }
}
