using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Util
{
    class UnrecoverableException : Exception
    {
        public UnrecoverableException(string message)
            : base(message)
        {
        }
    }
}
