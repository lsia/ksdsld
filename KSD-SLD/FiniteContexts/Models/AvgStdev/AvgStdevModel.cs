using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;


namespace KSDSLD.FiniteContexts.Models
{
    public abstract class AvgStdevModel : Model
    {
        public AvgStdevModel(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)              
        {
            Average = double.NaN;
            Variance = double.NaN;
        }

        public double Average { get; protected set; }
        public double Variance { get; protected set; }
        public double StandardDeviation { get { return Math.Sqrt(Variance); } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("O");
            sb.Append(ContextOrder);
            sb.Append(" (");
            sb.Append(Math.Round(Average));
            sb.Append("/");
            sb.Append(Math.Round(Math.Sqrt(Variance)));
            sb.Append("/");
            sb.Append(Count);
            sb.Append(")");
            return sb.ToString();
        }
    }
}
