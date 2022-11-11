using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Distances
{
    class WeightedEuclidean : IWeightedDistance
    {
        public double GetDistance(double[] a, double[] b, double[] weights)
        {
            if (a.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (a.Length != b.Length)
                throw new ArgumentException("Vectors are not of equal length.");

            double sum = 0.0;
            double sum_weights = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                double tmp = a[i] - b[i];
                sum += weights[i] * tmp * tmp;
                sum_weights += weights[i];
            }

            return Math.Sqrt(sum / sum_weights);
        }
    }
}
