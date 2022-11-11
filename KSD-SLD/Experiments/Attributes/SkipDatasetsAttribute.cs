using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments
{
    [AttributeUsage(AttributeTargets.Class)]
    class SkipDatasetsAttribute : Attribute
    {
        public string[] Datasets { get; private set; }
        public SkipDatasetsAttribute(params string[] dataset)
        {
            Datasets = dataset;
        }
    }
}
