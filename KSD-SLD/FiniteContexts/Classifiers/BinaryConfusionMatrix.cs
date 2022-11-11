using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Classifiers
{
    public class BinaryConfusionMatrix
    {
        public static BinaryConfusionMatrix FromCrossValidationDetails(ClassifierWEKA.CrossValidationInstanceDetail[] details)
        {
            int tp = details.Where(d => d.Actual == "legitimate" && d.Predicted == "legitimate").Count();
            int fp = details.Where(d => d.Actual == "impostor" && d.Predicted == "legitimate").Count();
            int tn = details.Where(d => d.Actual == "impostor" && d.Predicted == "impostor").Count();
            int fn = details.Where(d => d.Actual == "legitimate" && d.Predicted == "impostor").Count();

            return new BinaryConfusionMatrix(tp, fn, tn, fp);
        }

        public int TruePositives { get; private set; }
        public int FalsePositives { get; private set; }
        public int TrueNegatives { get; private set; }
        public int FalseNegatives { get; private set; }

        public BinaryConfusionMatrix(int tp, int fn, int tn, int fp)
        {
            TruePositives = tp;
            FalsePositives = fp;
            TrueNegatives = tn;
            FalseNegatives = fn;
        }

        public double Accuracy
        {
            get
            {
                return 
                    Math.Round(
                    100.0 * (double)(TruePositives + TrueNegatives) / (double) (TruePositives + TrueNegatives + FalsePositives + FalseNegatives)
                    ,2);
            }
        }

        public double ErrorRate
        {
            get
            {
                return
                    Math.Round(
                    100.0 * (double)(FalsePositives + FalseNegatives) / (double)(TruePositives + TrueNegatives + FalsePositives + FalseNegatives)
                    ,2);
            }
        }

        public double FAR
        {
            get
            {
                return
                    Math.Round(
                    100.0 * (double)(FalsePositives) / (double)(FalsePositives + TrueNegatives)
                    , 2);
            }
        }

        public double FRR
        {
            get
            {
                return
                    Math.Round(
                    100.0 * (double)(FalseNegatives) / (double)(FalseNegatives + TruePositives)
                    , 2);
            }
        }

        public double Precision
        {
            get
            {
                return
                    Math.Round(
                    100.0 * (double)TruePositives / (double)(TruePositives + FalsePositives)
                    , 2);
            }
        }

        public double Recall
        {
            get
            {
                return
                    Math.Round(
                    100.0 * (double)TruePositives / (double)(TruePositives + FalseNegatives)
                    , 2);
            }
        }
    }
}
