using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Distances
{
    class Euclidean : IDistance
    {
        public double GetDistance(double[] a, double[] b)
        {
            if (a.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (a.Length != b.Length)
                throw new ArgumentException("Vector are not of equal length.");

            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                double tmp = a[i] - b[i];
                sum += tmp * tmp;
            }

            return Math.Sqrt(sum / a.Length);
        }
    }
}
