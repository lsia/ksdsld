using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class ReorderDatasetsAttribute : System.Attribute
    {
        public string[] Datasets { get; private set; }
        public ReorderDatasetsAttribute(params string[] dataset_names)
        {
            Datasets = dataset_names;
        }
    }
}
