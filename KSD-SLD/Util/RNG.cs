using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;


namespace KSDSLD.Util
{
    public class RNG
    {
        private static readonly Random _global = new Random(1234);

        [ThreadStatic]
        private static Random _local;

        public RNG()
        {
            Initialize();
        }


        public bool Choice(double p)
        {
            double val = NextDouble();
            return val <= p;
        }

        public void Initialize()
        {
            if (_local == null)
            {
                lock (_global)
                {
                    if (_local == null)
                    {
                        int seed = _global.Next();
                        _local = new Random(seed);
                    }
                }
            }
        }

        public T[] GenerateSampleArray<T>(int count, T[] candidates, double[] probabilities)
        {
            if (probabilities.Sum() != 1.0)
                throw new ArgumentException("The probabilities do not sum to one.");

            List<T> tmp = new List<T>();
            for (int i = 0; i < (probabilities.Length - 1); i++)
            {
                int this_class_count = (int) (count * probabilities[i]);
                for (int j = 0; j < this_class_count; j++)
                    tmp.Add(candidates[j]);
            }

            for (int i = tmp.Count; i < count; i++)
                tmp.Add(candidates[candidates.Length - 1]);

            T[] retval = tmp.ToArray();
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < count; j++)
                {
                    int pos = Next(count);

                    T swap = retval[pos];
                    retval[pos] = retval[j];
                    retval[j] = swap;
                }

            return retval;
        }

        public int Next()
        {
            Initialize();
            return _local.Next();
        }

        public int Next(int max)
        {
            Initialize();
            return _local.Next(max);
        }

        public double NextDouble()
        {
            Initialize();
            return _local.NextDouble();
        }

        public T Choose<T>(T[] ts, Func<T, bool> choice_function)
        {
            T retval = ts[Next(ts.Length)];
            while (!choice_function(retval))
                retval = ts[Next(ts.Length)];

            return retval;
        }

        public T Choose<T>(T[] ts)
        {
            return ts[Next(ts.Length)];
        }

        static RNG instance = null;
        public static RNG Instance
        {
            get
            {
                if (instance == null)
                    instance = new RNG();

                return instance;
            }
        }

        public T[] SampleWithoutReposition<T>(T[] instances, int count)
        {
            Debug.Assert(instances.Length != 0);

            if (instances.Length <= count)
                return instances;

            HashSet<T> retval = new HashSet<T>();
            while (retval.Count < count)
            {
                int pos = Next(instances.Length);
                T candidate = instances[pos];
                if (!retval.Contains(candidate))
                    retval.Add(candidate);
            }

            return retval.ToArray();
        }

        public T[] SampleWithReposition<T>(T[] instances, int count)
        {
            Debug.Assert(instances.Length != 0);

            List<T> retval = new List<T>();
            while (retval.Count < count)
            {
                int pos = Next(instances.Length);
                T candidate = instances[pos];
                retval.Add(candidate);
            }

            return retval.ToArray();
        }

        public T[] Amplify<T>(T[] instances, int count)
        {
            Debug.Assert(instances.Length <= count);

            List<T> retval = new List<T>();
            retval.AddRange(instances);

            while (retval.Count < count)
            {
                int pos = Next(instances.Length);
                T candidate = instances[pos];
                retval.Add(candidate);
            }

            return retval.ToArray();
        }

        public T[] AmplifyOrSample<T>(T[] instances, int count)
        {
            if (instances.Length == count)
                return instances;
            else if (instances.Length > count)
                return SampleWithoutReposition(instances, count);
            else
                return Amplify(instances, count);
        }

        public double SampleFromNormalDistribution(double mean, double stdev)
        {
            const int SAMPLES = 20;

            double sum = 0.0;
            for (int i = 0; i < SAMPLES; i++)
                sum += NextDouble();

            sum /= SAMPLES;
            sum -= 0.5;
            sum *= Math.Sqrt(12) * Math.Sqrt(SAMPLES);

            sum *= stdev;
            sum += mean;

            return sum;
        }

        public double SampleFromLogNormalDistribution(double mean, double stdev)
        {
            return Math.Exp(SampleFromNormalDistribution(Math.Log(mean), Math.Log(stdev)));
        }
    }
}
