using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;


namespace KSDSLD.FiniteContexts.Models.Histogram
{
    class HistogramModel : Model
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public HistogramModel(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)
        {
        }

        List<int> observations = new List<int>();
        public List<int> Observations { get { return observations; } }

        public static int MAX_OBSERVATIONS = -1;

        public override void Feed(int[] parameter_values, int pos)
        {
            if (parameter_values[pos] < 0)
                throw new ArgumentException();

            Count++;
            observations.Add(parameter_values[pos]);

            if (MAX_OBSERVATIONS > 0 && observations.Count > MAX_OBSERVATIONS)
                observations.RemoveAt(0);
        }

        public double GetP(int t)
        {
            int cl = 0, cc = 0, cr = 0;
            foreach (int obs in observations)
                if (obs < t)
                    cl++;
                else if (obs == t)
                    cc++;
                else if (obs > t)
                    cr++;

            return (double)(cl + (double)cc / 2) / (double)(cl + cc + cr);
        }

        #region SERIALIZATION
        public override byte[] Serialize()
        {
            byte[] f1 = BitConverter.GetBytes(Count);

            byte[] retval = new byte[(1 + observations.Count) * sizeof(int)];
            Array.Copy(f1, retval, f1.Length);

            int pos = 4;
            foreach (var observation in observations)
            {
                byte[] tmp = BitConverter.GetBytes(observation);
                Array.Copy(tmp, 0, retval, pos, tmp.Length);
                pos += tmp.Length;
            }

            return retval;
        }

        public static HistogramModel Deserialize(ulong context, short context_order, int ngram, short ngram_order, byte[] blob)
        {
            HistogramModel retval = new HistogramModel(context, context_order, ngram, ngram_order);
            retval.Count = BitConverter.ToInt32(blob, 0);

            for (int i = 4; i < blob.Length; i += 4)
                retval.observations.Add(BitConverter.ToInt32(blob, i));

            return retval;
        }
        #endregion
    }
}
