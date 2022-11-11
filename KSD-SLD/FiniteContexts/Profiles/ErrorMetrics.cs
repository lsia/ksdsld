using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


namespace KSDSLD.FiniteContexts.Profiles
{
    public class ErrorMetrics
    {
        public string Name { get; private set; }
        public double EER { get; private set; }
        public double Threshold { get; private set; }

        public double FAR { get; private set; }
        public double FRR { get; private set; }

        public double[] LegitimateValues { get; private set; }
        public double[] ImpostorValues { get; private set; }

        public ErrorMetrics(string name, double eer)
        {
            Name = name;
            EER = eer;

            FAR = FRR = Threshold = double.NaN;
            LegitimateValues = ImpostorValues = null;
        }

        public ErrorMetrics(string name, double eer, double far, double frr)
        {
            Name = name;
            EER = eer;
            FAR = far;
            FRR = frr;

            Threshold = double.NaN;
            LegitimateValues = ImpostorValues = null;
        }

        public ErrorMetrics(string name, double[] legitimate_values, double[] impostor_values)
        {
            Name = name;
            LegitimateValues = legitimate_values;
            ImpostorValues = impostor_values;

            CalculateMetrics();
        }

        void CalculateMetrics()
        {
            Array.Sort(LegitimateValues);
            Array.Sort(ImpostorValues);

            if (LegitimateValues[LegitimateValues.Length - 1] < ImpostorValues[0])
            {
                EER = 0;
                Threshold = (LegitimateValues[LegitimateValues.Length - 1] + ImpostorValues[0]) / 2.0;
                return;
            }

            int ipos = 0;
            for (int i = 0; i < LegitimateValues.Length; i++)
            {
                while (ipos < ImpostorValues.Length && ImpostorValues[ipos] <= LegitimateValues[i])
                    ipos++;

                FRR = (double)(LegitimateValues.Length - i) / (double)LegitimateValues.Length;
                FAR = (double)ipos / (double)ImpostorValues.Length;

                if (FAR >= FRR)
                {
                    EER = (FAR + FRR) / 2.0;

                    if (ipos >= ImpostorValues.Length) ipos = ImpostorValues.Length - 1;
                    Threshold = (LegitimateValues[i] + ImpostorValues[ipos]) / 2.0;
                    return;
                }
            }

            throw new ArgumentException("Unable to calculate EER");
        }

        public override string ToString()
        {
            return Math.Round(100.0 * EER, 2) + "%/" + Math.Round(Threshold, 2);
        }
    }
}
