using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Profiles
{
    public class Authentication
    {
        public Sample Sample { get; private set; }
        public bool Legitimate { get; private set; }
        public Dictionary<string,double> MethodValues { get; private set; }

        public Dictionary<string, KeyValuePair<TypingFeature, Model[]>> PatternVectors { get; private set; }

        public double[] MethodValuesArray
        {
            get
            {
                List<double> retval = new List<double>();
                foreach (var kv in MethodValues)
                    retval.Add(kv.Value);

                return retval.ToArray();
            }
        }

        public string Summarize()
        {
            StringBuilder retval = new StringBuilder();
            retval.AppendLine(Sample.GetSessionText(true));
            foreach (var kv in MethodValues)
            {
                retval.Append(kv.Key);
                retval.Append("=");
                retval.Append(Math.Round(kv.Value, 2));
                retval.Append(", ");
            }

            return retval.ToString();
        }

        internal Authentication
            (
                Sample session, 
                bool legitimate, 
                Dictionary<string, double> method_values,
                Dictionary<string, KeyValuePair<TypingFeature, Model[]>> pattern_vectors
                )
        {
            Sample = session;
            Legitimate = legitimate;
            MethodValues = method_values;
            PatternVectors = pattern_vectors;
        }
    }
}
