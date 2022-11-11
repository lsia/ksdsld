using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Distances
{
    interface IWeightedDistance
    {
        double GetDistance(double[] a, double[] b, double[] weights);
    }
}
