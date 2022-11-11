using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Features
{
    public class FeatureParameters
    {  
        public FeatureParameters
            (
                Dictionary<string, object> dictionary, 
                Sample session,

                string pattern_name,
                Model[] pattern,
                TypingFeature parameter,
                int[] parameter_values
            )
        {
            Dictionary = dictionary;
            Session = session;

            PatternName = pattern_name;
            Pattern = pattern;
            Parameter = parameter;
            ParameterValues = parameter_values;
        }

        public Dictionary<string, object> Dictionary { get; private set; }
        public Sample Session { get; private set; }

        public string PatternName { get; private set; }
        public Model[] Pattern { get; private set; }
        public TypingFeature Parameter { get; private set; }
        public int[] ParameterValues { get; private set; }
    }
}
