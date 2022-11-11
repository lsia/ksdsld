using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OnlyUserAttribute : System.Attribute
    {
        public string Dataset { get; private set; }
        public string User { get; private set; }

        public OnlyUserAttribute(string user)
        {
            User = user;
        }

        public OnlyUserAttribute(string dataset, string user)
        {
            Dataset = dataset;
            User = user;
        }
    }
}
