using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;


namespace KSDSLD.Datasets.Summarizers
{
    class LengthSummarizer : IDatasetSummarizer
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        const int BUCKETS = 20;
        const int BUCKET_SIZE = 50;

        public void Summarize(Dataset dataset)
        {
            log.Info("Dataset bucket count...");
            int min = 0;
            int max = BUCKET_SIZE;
            for (int i = 0; i < BUCKETS; i++)
            {
                int count = dataset.Samples.Where(s => s.Features[TypingFeature.FT].Length >= min && s.Features[TypingFeature.FT].Length < max).Count();
                log.Info("  < {0,4} {1,6} {2,6:0.00}%", max, count, Math.Round(100.0 * count / dataset.Samples.Length, 2));

                min += BUCKET_SIZE;
                max += BUCKET_SIZE;
            }

            int above = dataset.Samples.Where(s => s.Features[TypingFeature.FT].Length >= max).Count();
            log.Info("  + {0,4} {1,6} {2,6:0.00}%", max, above, Math.Round(100.0 * above / dataset.Samples.Length, 2));
        }
    }
}
