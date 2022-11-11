using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Experiments.Distances
{
    class WeightedManhattan : IWeightedDistance
    {
        public double GetDistance(double[] a, double[] b, double[] weights)
        {
            if (a.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (a.Length != b.Length)
                throw new ArgumentException("Vector are not of equal length.");

            int used_coordinates = 0;
            double sum = 0.0;
            double sum_weights = 0.0;
            for (int i = 0; i < a.Length; i++)
                if ( !double.IsNaN(a[i]) && !double.IsNaN(b[i]) 
                    && !double.IsNaN(weights[i]) && !double.IsPositiveInfinity(weights[i]) && !double.IsNegativeInfinity(weights[i])
                    && weights[i] != 0)
                {
                    sum += weights[i] * Math.Abs(a[i] - b[i]);
                    sum_weights += weights[i];
                    used_coordinates++;

                    if ( double.IsPositiveInfinity(sum))
                    {
                        // int k = 9;
                    }
                }

            return sum /= a.Length;
        }
    }
}
