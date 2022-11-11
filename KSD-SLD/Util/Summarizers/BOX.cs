using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Util.Summarizers
{
    class Box
    {
        public Box(IEnumerable<double> values)
        {
            double[] tmp = values.ToArray();
            Array.Sort(tmp);

            Minimum = tmp[0];
            Maximum = tmp[tmp.Length - 1];
            Average = tmp.Average();
            Median = tmp[tmp.Length / 2];

            FirstQuartileStart = tmp[tmp.Length / 4];
            ThirdQuartileEnd = tmp[3 * tmp.Length / 4];
        }

        public static Box Create(IEnumerable<double> values)
        {
            return new Box(values);
        }

        public double Minimum { get; private set; }
        public double Maximum { get; private set; }
        public double Median { get; private set; }
        public double Average { get; private set; }
        public double FirstQuartileStart { get; private set; }
        public double ThirdQuartileEnd { get; private set; }

        public override string ToString()
        {
            return "MIN " + Minimum + "    MAX " + Maximum + "    MED " + Median + "    AVG " + Average + "    1ST " + FirstQuartileStart + "    3RD " + ThirdQuartileEnd;
        }

        public string Latex
        {
            get
            {
                return "\\boxplot{0}{" + Median + "}{" + FirstQuartileStart + "}{" + ThirdQuartileEnd + "}{" + Minimum + "}{" + Maximum + "}";
            }
        }
    }
}
