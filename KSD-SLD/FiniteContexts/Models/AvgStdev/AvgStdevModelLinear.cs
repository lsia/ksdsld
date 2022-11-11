using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;


namespace KSDSLD.FiniteContexts.Models
{
    class AvgStdevModelLinear : AvgStdevModel
    {
        public static AvgStdevModelLinear Create(ulong context, short context_order, int ngram, short ngram_order, int count, double avg, double std)
        {
            AvgStdevModelLinear retval = new AvgStdevModelLinear(context, context_order, ngram, ngram_order);
            retval.Count = count;
            retval.Average = avg;

            if (count == 1)
            {
                retval.e_X = avg;
                retval.e_X2 = avg * avg;
            }
            else
            {
                retval.Variance = std * std;

                retval.e_X = avg;
                retval.e_X2 = retval.Variance + avg * avg;
            }

            return retval;
        }

        public AvgStdevModelLinear(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)
        {
        }


        double e_X = 0.0;
        double e_X2 = 0.0;

        public override void Feed(int[] parameter_values, int pos)
        {
            int t = parameter_values[pos];
            if ( t < 0 )
                throw new ArgumentException("t < 0");

            if ( Count == 0)
            {
                Count++;
                Average = t;
                e_X = t;
                e_X2 = t * t;
                return;
            }

            if (Count != 0)
            {
                e_X *= Count;
                e_X2 *= Count;
            }

            double old_e_X2 = e_X2;
            double old_e_X = e_X;

            Count++;
            e_X += t;
            e_X /= Count;
            e_X2 += t * t;
            e_X2 /= Count;
            Variance = e_X2 - e_X * e_X;

            if (e_X < 0)
                throw new ArgumentException("e_X < 0");

            if ( e_X2 < 0 )
                throw new ArgumentException("e_X2 < 0");

            if ( double.IsNaN(StandardDeviation) )
                throw new ArgumentException("StandardDeviation is NaN");

            Average = e_X;

            if (double.IsNaN(Average))
                throw new ArgumentException("Average is NaN");
        }

        #region SERIALIZATION
        public override byte[] Serialize()
        {
            byte[] f1 = BitConverter.GetBytes(Count);
            byte[] f2 = BitConverter.GetBytes(e_X);
            byte[] f3 = BitConverter.GetBytes(e_X2);

            byte[] retval = new byte[2 * sizeof(double) + sizeof(int)];
            Array.Copy(f1, retval, f1.Length);
            Array.Copy(f2, 0, retval, f1.Length, f2.Length);
            Array.Copy(f3, 0, retval, f1.Length + f2.Length, f3.Length);
            return retval;
        }

        public static AvgStdevModelLinear Deserialize(ulong context, short context_order, int ngram, short ngram_order, byte[] blob)
        {
            AvgStdevModelLinear retval = new AvgStdevModelLinear(context, context_order, ngram, ngram_order);
            retval.Count = BitConverter.ToInt32(blob, 0);
            retval.e_X = BitConverter.ToDouble(blob, sizeof(int));
            retval.e_X2 = BitConverter.ToDouble(blob, sizeof(int) + sizeof(double));
            retval.Average = retval.e_X;
            retval.Variance = retval.e_X2 - retval.e_X * retval.e_X;
            return retval;
        }
        #endregion

    }
}
