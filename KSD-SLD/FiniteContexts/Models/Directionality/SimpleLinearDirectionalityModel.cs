using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Models.Directionality
{
    class SimpleLinearDirectionalityModel : SimpleDirectionalityModel
    {
        public SimpleLinearDirectionalityModel(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)              
        {
        }

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

            Directionality *= Count;

            Count++;
            Directionality += directionality;
            Directionality /= Count;
        }

        #region SERIALIZATION
        public override byte[] Serialize()
        {
            byte[] f1 = BitConverter.GetBytes(Count);
            byte[] f2 = BitConverter.GetBytes(Directionality);

            byte[] retval = new byte[sizeof(double) + sizeof(int)];
            Array.Copy(f1, retval, f1.Length);
            Array.Copy(f2, 0, retval, f1.Length, f2.Length);
            return retval;
        }

        public static SimpleLinearDirectionalityModel Deserialize(ulong context, short context_order, int ngram, short ngram_order, byte[] blob)
        {
            SimpleLinearDirectionalityModel retval = new SimpleLinearDirectionalityModel(context, context_order, ngram, ngram_order);
            retval.Count = BitConverter.ToInt32(blob, 0);
            retval.Directionality = BitConverter.ToDouble(blob, sizeof(int));
            return retval;
        }
        #endregion
    }
}
