using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class IterateTask : System.Attribute
    {
        public string Task { get; private set; }
        public string[] Values { get; private set; }
    
    
        public IterateTask(string task, params string[] values)
        {
            Task = task;
            Values = values;
        }
    }
}
