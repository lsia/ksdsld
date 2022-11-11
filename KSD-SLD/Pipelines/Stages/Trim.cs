using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;


namespace KSDSLD.Pipelines.Stages
{
    public class Trim : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public Trim(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        void TrimSessionKeystrokesMin(Results results)
        {
            log.Info("Trimming sessions with less than {0} keystrokes...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];

                List<Sample> sessions = new List<Sample>();
                foreach (Sample session in dataset.Samples)
                    if (session.VKs.Length >= threshold)
                        sessions.Add(session);

                log.Info("  {0} trimmed, {1} remain ({2}%)",
                    dataset.Samples.Length - sessions.Count,
                    sessions.Count,
                    Math.Round(100.0 * sessions.Count / dataset.Samples.Length, 2));

                dataset.SetSessions(sessions.ToArray());                    
            }
        }

        void TrimSessionKeystrokesMax(Results results)
        {
            log.Info("Trimming sessions with more than {0} keystrokes...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];

                List<Sample> sessions = new List<Sample>();
                foreach (Sample session in dataset.Samples)
                    if (session.VKs.Length < threshold)
                        sessions.Add(session);

                log.Info("  {0} trimmed, {1} remain ({2}%)",
                    dataset.Samples.Length - sessions.Count,
                    sessions.Count,
                    Math.Round(100.0 * sessions.Count / dataset.Samples.Length, 2));

                dataset.SetSessions(sessions.ToArray());
            }
        }

        void TrimUserSessionsMin(Results results)
        {
            log.Info("Trimming users with less than {0} sessions...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];

                var sessions_by_user = dataset.Samples.GroupBy(s => s.User.UserID);
                var survivors = sessions_by_user.Where(kv => kv.Count() >= threshold);

                List<Sample> sessions = new List<Sample>();
                foreach ( var survivor in survivors )
                    sessions.AddRange(survivor);

                int original_users = dataset.Samples.Select(s => s.User.UserID).Distinct().Count();
                int filtered_users = sessions.Select(s => s.User.UserID).Distinct().Count();
                log.Info("  {0} trimmed, {1} remain ({2}%)",
                    original_users - filtered_users,
                    filtered_users,
                    Math.Round(100.0 * filtered_users / original_users, 2));

                dataset.SetSessions(sessions.ToArray());
            }
        }

        void TrimUserSessionsMax(Results results)
        {
            log.Info("Trimming users with more than {0} sessions...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];

                var sessions_by_user = dataset.Samples.GroupBy(s => s.User.UserID);
                var survivors = sessions_by_user.Where(kv => kv.Count() < threshold);

                List<Sample> sessions = new List<Sample>();
                foreach (var survivor in survivors)
                    sessions.AddRange(survivor);

                int original_users = dataset.Samples.Select(s => s.User.UserID).Distinct().Count();
                int filtered_users = sessions.Select(s => s.User.UserID).Distinct().Count();
                log.Info("  {0} trimmed, {1} remain ({2}%)",
                    original_users - filtered_users,
                    filtered_users,
                    Math.Round(100.0 * filtered_users / original_users, 2));

                dataset.SetSessions(sessions.ToArray());
            }
        }

        void TrimUserSessionsSet(Results results)
        {
            log.Info("Trimming sessions above {0}...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];
                List<Sample> sessions = new List<Sample>();

                var sessions_by_user = dataset.Samples.GroupBy(s => s.User.UserID);
                foreach (var kv in sessions_by_user)
                {
                    int count = 0;
                    foreach (var survivor in kv)
                    {
                        sessions.Add(survivor);

                        count++;
                        if (count >= threshold)
                            break;
                    }
                }

                dataset.SetSessions(sessions.ToArray());
            }
        }

        void TrimNotEnoughAlphaNumeric(Results results)
        {
            log.Info("Trimming sessions without enough alphanumeric keystrokes (<{0}%)...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];
                List<Sample> sessions = new List<Sample>();

                foreach (Sample session in dataset.Samples)
                    if (session.AlphanumericRate >= threshold)
                        sessions.Add(session);

                log.Info("  {0} trimmed, {1} remain ({2}%)",
                    dataset.Samples.Length - sessions.Count,
                    sessions.Count,
                    Math.Round(100.0 * sessions.Count / dataset.Samples.Length , 2));
                
                dataset.SetSessions(sessions.ToArray());
            }
        }

        void TrimAlphaNumericVariety(Results results)
        {
            log.Info("Trimming sessions without enough alphanumeric variety (<{0})...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];
                List<Sample> sessions = new List<Sample>();

                foreach (Sample session in dataset.Samples)
                    if (session.AlphanumericVariety >= threshold)
                        sessions.Add(session);

                log.Info("  {0} trimmed, {1} remain ({2}%)",
                    dataset.Samples.Length - sessions.Count,
                    sessions.Count,
                    Math.Round(100.0 * sessions.Count / dataset.Samples.Length, 2));

                dataset.SetSessions(sessions.ToArray());
            }
        }

        void VerifyVKs(Results results)
        {
            Dictionary<int, int> code_counts = new Dictionary<int, int>();
            Dictionary<int, int> location_counts = new Dictionary<int, int>();

            log.Info("Trimming sessions above {0}...", threshold);
            for (int i = 0; i < results.Datasets.Length; i++)
            {
                Dataset dataset = results.Datasets[i];
                foreach ( var session in dataset.Samples )
                    for (int j = 0; j < session.VKs.Length; j++)
                    {
                        int code = session.VKs[j] & 0xFF;
                        int location = session.VKs[j] >> 8;

                        if (!code_counts.ContainsKey(code))
                            code_counts.Add(code, 0);

                        if (!location_counts.ContainsKey(location))
                            location_counts.Add(location, 0);

                        code_counts[code]++;
                        location_counts[location]++;
                    }
            }

            log.Info("CODES");
            foreach (int code in code_counts.Keys.OrderBy(k => k))
                log.Info("  " + code.ToString("X").PadLeft(2, '0') + "  " + code_counts[code]);

            log.Info("LOCATION");
            foreach (int location in location_counts.Keys.OrderBy(k => k))
                log.Info("  " + location.ToString("X").PadLeft(2, '0') + "  " + location_counts[location]);
        }

        int threshold = 0;
        Dictionary<string, Pipeline.PipelineStageDelegate> actions = new Dictionary<string, Pipeline.PipelineStageDelegate>();
        public override void Initialize()
        {
            actions.Add("session_keystrokes_min", new Pipeline.PipelineStageDelegate(TrimSessionKeystrokesMin));
            actions.Add("session_keystrokes_max", new Pipeline.PipelineStageDelegate(TrimSessionKeystrokesMax));
            actions.Add("user_sessions_min", new Pipeline.PipelineStageDelegate(TrimUserSessionsMin));
            actions.Add("user_sessions_max", new Pipeline.PipelineStageDelegate(TrimUserSessionsMax));
            actions.Add("user_sessions_set", new Pipeline.PipelineStageDelegate(TrimUserSessionsSet));
            actions.Add("verify_vks", new Pipeline.PipelineStageDelegate(VerifyVKs));
            actions.Add("alphanumeric_rate", new Pipeline.PipelineStageDelegate(TrimNotEnoughAlphaNumeric));
            actions.Add("alphanumeric_variety", new Pipeline.PipelineStageDelegate(TrimAlphaNumericVariety));

            if (!actions.ContainsKey(Configuration.Action))
                throw new ArgumentException("Invalid action " + Configuration.Action + ".");

            if ( !int.TryParse(Configuration.Parameters, out threshold))
                throw new ArgumentException("Invalid parameters (must be an integer)");
        }

        protected override void DoRun(Results results)
        {
            actions[Configuration.Action](results);
        }
    }
}
