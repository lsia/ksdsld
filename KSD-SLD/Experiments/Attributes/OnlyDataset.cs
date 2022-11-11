using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OnlyDataset : Attribute
    {
        public string Dataset { get; private set; }
        public OnlyDataset(string dataset)
        {
            Dataset = dataset;
        }
    }
}
