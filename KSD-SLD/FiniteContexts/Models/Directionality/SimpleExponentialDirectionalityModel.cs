using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Models.Directionality
{
    class SimpleExponentialDirectionalityModel : SimpleDirectionalityModel
    {
        public SimpleExponentialDirectionalityModel(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)              
        {
        }

        double weight = 1.0f;
        double sum = 0.0f;

        const float K = 0.95f;

        public override void Feed(int[] parameter_values, int pos)
        {
            if (pos == 0 || ContextOrder == 0)
                return;

            int t = parameter_values[pos];
            int tp = parameter_values[pos - 1];
            if (t == int.MinValue || tp == int.MinValue)
                return;

            int directionality = 0;
            if (t > tp)
                directionality = 1;
            else if (t < tp)
                directionality = -1;

            Count++;
            sum *= K;
            sum += directionality;
            Directionality = sum / weight;

            weight *= K;
            weight += 1.0f;
        }

        #region SERIALIZATION
        public override byte[] Serialize()
        {
            byte[] f1 = BitConverter.GetBytes(Count);
            byte[] f2 = BitConverter.GetBytes(Directionality);
            byte[] f3 = BitConverter.GetBytes(sum);
            byte[] f4 = BitConverter.GetBytes(weight);

            byte[] retval = new byte[3 * sizeof(double) + sizeof(int)];
            Array.Copy(f1, retval, f1.Length);
            Array.Copy(f2, 0, retval, f1.Length, f2.Length);
            Array.Copy(f3, 0, retval, f1.Length + f2.Length, f3.Length);
            Array.Copy(f4, 0, retval, f1.Length + f2.Length + f2.Length, f4.Length);
            return retval;
        }

        public static SimpleExponentialDirectionalityModel Deserialize(ulong context, short context_order, int ngram, short ngram_order, byte[] blob)
        {
            SimpleExponentialDirectionalityModel retval = new SimpleExponentialDirectionalityModel(context, context_order, ngram, ngram_order);
            retval.Count = BitConverter.ToInt32(blob, 0);
            retval.Directionality = BitConverter.ToDouble(blob, sizeof(int));
            retval.sum = BitConverter.ToDouble(blob, sizeof(int) + sizeof(double));
            retval.weight = BitConverter.ToDouble(blob, sizeof(int) + 2 * sizeof(double));
            return retval;
        }

        #endregion
    }
}
