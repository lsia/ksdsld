using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.Util;


namespace KSDSLD
{
    static class ListExtensions
    {
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        public static double StandardDeviation(this IEnumerable<int> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        public static double StandardDeviation<T> (this IEnumerable<T> values, Func<T, int> selector)
        {
            double avg = values.Average(v => selector(v));
            return Math.Sqrt(values.Average(v => Math.Pow(selector(v) - avg, 2)));
        }

        public static void Feed(this ModelFeeder[] feeders, Sample session, bool initial_training)
        {
            for (int i = 0; i < feeders.Length; i++)
                feeders[i].Feed(session, initial_training);
        }

        public static Dictionary<string, KeyValuePair<TypingFeature, Model[]>> Rebuild(this Builder[] builders, Sample session)
        {
            Dictionary<string, KeyValuePair<TypingFeature, Model[]>> retval = new Dictionary<string, KeyValuePair<TypingFeature,Model[]>>();
            for (int i = 0; i < builders.Length; i++)
            {
                Model[] pattern = builders[i].Rebuild(session);
                retval.Add(builders[i].Storage.Feature.ToString() + "_" + builders[i].Storage.Name, 
                    new KeyValuePair<TypingFeature, Model[]>(builders[i].Storage.Feature, pattern));
            }

            return retval;
        }


        static RNG rng = new RNG();
        public static Sample ChooseImpostor(this Sample session, List<Sample> candidates)
        {
            Sample[] arr = candidates.Where(s => s != session).ToArray();
            if (arr.Length == 0)
                throw new ArgumentException("No suitable candidates for an impostor.");

            return arr[rng.Next() % arr.Length];
        }

        public static T ChooseOne<T>(this IEnumerable<T> candidates)
        {
            T[] arr = candidates.ToArray();
            return RNG.Instance.Choose(arr);
        }


        public static T ChooseOne<T>(this IEnumerable<T> candidates, out T[] remaining) where T : class
        {
            T[] arr = candidates.ToArray();
            T retval = RNG.Instance.Choose(arr);
            remaining = candidates.Where(c => c != retval).ToArray();
            return retval;
        }

        public static T[] ChooseN<T>(this IEnumerable<T> candidates, int n, out T[] remaining) where T : class
        {
            T[] arr = candidates.ToArray();
            Debug.Assert(n < arr.Length);

            HashSet<int> pos = new HashSet<int>();
            while (pos.Count < n)
            {
                int tmp = RNG.Instance.Next(arr.Length);
                if (!pos.Contains(tmp))
                    pos.Add(tmp);
            }

            T[] retval = new T[n];

            int[] posarr = pos.ToArray();
            for (int i = 0; i < n; i++)
            {
                retval[i] = arr[i];
                arr[i] = null;
            }

            throw new Exception("LA IMPLEMENTACI'ON ESTA MAL");

            remaining = arr.Where(c => c != null).ToArray();
            return retval;
        }

        public static T[] ChooseN<T>(this IEnumerable<T> candidates, int n) where T : class
        {
            T[] arr = candidates.ToArray();
            Debug.Assert(n < arr.Length);

            HashSet<int> pos = new HashSet<int>();
            while (pos.Count < n)
            {
                int tmp = RNG.Instance.Next(arr.Length);
                if (!pos.Contains(tmp))
                    pos.Add(tmp);
            }

            T[] retval = new T[n];

            int[] posarr = pos.ToArray();
            for (int i = 0; i < n; i++)
                retval[i] = arr[posarr[i]];

            return retval;
        }

        public static T[] AllBut<T>(this IEnumerable<T> candidates, T exclude) where T : class
        {
            return candidates.Where(c => c != exclude).ToArray();
        }

        public static string SummarizeProbabilitySeries(this List<double> values, int sample_scale = 1)
        {
            double avg = values.Average();
            double interval = 1.96 *Math.Sqrt(avg * (1 - avg) / (values.Count * sample_scale));
            double std = values.StandardDeviation();
            
            return Math.Round(100.0 * avg, 2) + "% +/- " + Math.Round(100.0 * interval, 2) + 
                " (" + Math.Round(100.0 * std, 2) + ")"; 
        }

        public static void SplitTrainTest(this Sample[] sessions, int training_sessions, out Sample[] training, out Sample[] testing)
        {
            training = sessions.Take(training_sessions).ToArray();
            testing = sessions.Skip(training_sessions).ToArray();
        }
    }
}
