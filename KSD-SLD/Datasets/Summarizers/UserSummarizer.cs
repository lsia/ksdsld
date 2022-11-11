using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;


namespace KSDSLD.Datasets.Summarizers
{
    class UserSummarizer : IDatasetSummarizer
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public void Summarize(Dataset dataset)
        {
            log.Info("Sessions per user...");

            var per_user = dataset.Samples.GroupBy(s => s.User.UserID);
            foreach ( var kv in per_user.OrderBy(kv => kv.Count()) )
            {
                User user = User.GetUser(kv.Key);
                log.Info("  {0,-30}   {1} sessions    {2} average keystrokes", user.Name, kv.Count(), (int) kv.Average(s => s.Features[TypingFeature.FT].Length));
            }
        }
    }
}
