using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics;

using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Models.Predictors
{
    class LinearRegressionModel : AvgStdevModelLinear
    {
        short initial_context_order = 0;
        public LinearRegressionModel(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)
        {
            initial_context_order = context_order;

            fs = new Func<double[], double>[context_order];
            for (int i = 0; i < fs.Length; i++)
            {
                int tmp = i;
                fs[i] = x => f(tmp, x);
            }
        }

        bool dirty = false;
        List<double[]> observations = new List<double[]>();
        List<double> t_observations = new List<double>();
        Func<double[], double>[] fs;


        public override void Feed(int[] parameter_values, int pos)
        {
            int t = parameter_values[pos];

            base.Feed(parameter_values, pos);
            dirty = true;

            bool ok = true;
            double[] tmp = new double[ContextOrder];
            for (int i = 0; i < ContextOrder; i++)
            {
                tmp[i] = parameter_values[pos - 1 - i];
                if (tmp[i] == int.MinValue)
                    ok = false;
            }

            if (ok)
            {
                observations.Add(tmp);
                t_observations.Add(t);
            }
        }

        double[] coeffs;

        void Evaluate()
        {

        }

        public void DoRegression(int[] context_timings)
        {
            if (ContextOrder == 0 || observations.Count < 5 * ContextOrder)
            {
                ContextOrder = 0;
                return;
            }

            if (dirty)
            {
                double[][] obs = observations.ToArray();
                double[] y = t_observations.ToArray();

                try
                {
                    coeffs = Fit.LinearMultiDim(obs, y, fs);
                }
                catch
                {
                    throw;
                }

                Evaluate();
            }

            double tmp = 0.0;
            for (int i = 0; i < ContextOrder; i++)
                tmp += coeffs[i] * context_timings[i];

            ContextOrder = initial_context_order;
            Average = tmp;
        }

        static double f(int i, double[] x)
        {
            return x[i];
        }
    }
}
