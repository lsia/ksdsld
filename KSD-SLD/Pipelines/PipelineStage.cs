using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Threading.Tasks;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Experiments;
using KSDSLD.Experiments.Attributes;
using KSDSLD.Util;


namespace KSDSLD.Pipelines
{
    public abstract class PipelineStage
    {
        public Pipeline Pipeline { get; private set; }
        public PipelineStageConfigurationElement Configuration { get; private set; }

        public PipelineStage(Pipeline pipeline, PipelineStageConfigurationElement configuration)
        {
            Pipeline = pipeline;
            Configuration = configuration;

            if (Pipeline != null && Pipeline.Configuration != null)
            {
                RunInParallel = Pipeline.Configuration.Parallel;
                if (AlwaysRunInParallel)
                    RunInParallel = true;
            }

            var attr = this.GetType().GetCustomAttribute<SkipDatasetsAttribute>();
            if (attr != null)
                SkipDatasets = attr.Datasets;

            OnlyDatasets = this.GetType().GetCustomAttributes<OnlyDataset>().ToArray();

            var reorder = this.GetType().GetCustomAttribute<ReorderDatasetsAttribute>();
            if (reorder != null)
                ReorderDatasets = reorder.Datasets;

            SkipUsers = this.GetType().GetMethod("ForEachUser").GetCustomAttributes<SkipUserAttribute>().ToArray();
            OnlyUsers = this.GetType().GetMethod("ForEachUser").GetCustomAttributes<OnlyUserAttribute>().ToArray();
        }

        public string[] ReorderDatasets { get; private set; }

        public bool AlwaysRunInParallel
        {
            get
            {
                return this.GetType().GetCustomAttribute<AlwaysRunInParallelAttribute>() != null;
            }
        }

        public Logger Log
        {
            get
            {
                return LogManager.GetCurrentClassLogger();
            }
        }

        public virtual void Initialize() { }

        public virtual void OnStageStart()
        {
        }

        public virtual void OnStageEnd()
        {
        }

        public string[] SkipDatasets { get; private set; }
        public OnlyDataset[] OnlyDatasets { get; private set; }

        public SkipUserAttribute[] SkipUsers { get; private set; }
        public OnlyUserAttribute[] OnlyUsers { get; private set; }


        public virtual void OnDatasetStart(Dataset dataset) { }
        public virtual void OnDatasetEnd(Dataset dataset) { }

        public virtual void OnUserStart(Dataset dataset, User user) { }
        public virtual void OnUserEnd(Dataset dataset, User user) { }

        public virtual void ForEachDataset(Dataset dataset)
        {
        }

        public virtual void ForEachUser(Dataset dataset, User user, Sample[] sessions)
        {
        }
        public virtual void ForEachSession(Dataset dataset, Sample session)
        {
        }

        protected virtual void DoRun(Results results)
        {
        }

        public delegate void ForEachFeatureDelegate(TypingFeature feature);
        public void ForEachFeature(ForEachFeatureDelegate f)
        {
            f(TypingFeature.HT);
            f(TypingFeature.FT);
        }

        void DoForEachUser(Dataset dataset, User user, Sample[] sessions)
        {
            CurrentUser = user;
            OnUserStart(dataset, user);
            ForEachUser(dataset, user, sessions);

            foreach (var session in sessions)
            {
                CurrentSession = session;
                ForEachSession(dataset, session);
            }

            OnUserEnd(dataset, user);
        }

        public Dataset CurrentDataset { get; private set; }
        public User CurrentUser { get; private set; }
        public Sample CurrentSession { get; private set; }

        public CsvWriter DatasetCSV { get; private set; }
        public CsvWriter GlobalCSV { get; private set; }

        string TaskTreeWithSuffix(string suffix)
        {
            if (TaskTree == null)
                return "";
            else
                return TaskTree + suffix;
        }

        public bool RunInParallel { get; private set; }
        void RunTask(Results results)
        {
            if (typeof(Experiment).IsAssignableFrom(this.GetType()))
                GlobalCSV = CsvWriter.Create(ExperimentUtil.OutputFolder + "/" + TaskTreeWithSuffix("-") + "GLOBAL.csv");

            List<string> fields = new List<string>();
            if (ReorderDatasets != null)
                results.ReorderDatasets(ReorderDatasets);

            OnStageStart();
            if (results.Datasets != null)
                foreach (var dataset in results.Datasets)
                    if ((SkipDatasets == null || !SkipDatasets.Contains(dataset.Name))
                            && (OnlyDatasets.Length == 0 || OnlyDatasets.Any(d => d.Dataset == dataset.Name))
                        )
                    {
                        CurrentDataset = dataset;
                        if (typeof(Experiment).IsAssignableFrom(this.GetType()))
                            Log.Info("---------------------------------------- DATASET {0}", dataset.Name);

                        Pipeline.CurrentDataset = dataset;
                        if (typeof(Experiment).IsAssignableFrom(this.GetType()))
                            DatasetCSV = CsvWriter.Create(ExperimentUtil.OutputFolder + "/" + TaskTreeWithSuffix("-") + dataset.Name + ".csv");

                        OnDatasetStart(dataset);
                        ForEachDataset(dataset);

                        if (RunInParallel)
                            Parallel.ForEach(dataset.SessionsByUser, kv => RunUser(dataset, fields, kv.Key, kv.Value));
                        else
                        {
                            foreach (var kv in dataset.SessionsByUser)
                                RunUser(dataset, fields, kv.Key, kv.Value);
                        }

                        OnDatasetEnd(dataset);
                        if (typeof(Experiment).IsAssignableFrom(this.GetType()))
                            DatasetCSV.Dump();
                    }

            DoRun(results);
            OnStageEnd();

            if (typeof(Experiment).IsAssignableFrom(this.GetType()))
                GlobalCSV.Dump();
        }

        void RunUser(Dataset dataset, List<string> fields, int user_id, Sample[] sessions)
        {
            User user = User.GetUser(user_id);
            
            if (SkipUsers.Length != 0 && SkipUsers.Any(u => u.User == user.Name))
                return;

            if (OnlyUsers.Length != 0 && !OnlyUsers.Any(u => u.User == user.Name))
                return;

            DoForEachUser(dataset, user, sessions);
        }

        public Stack<string> CurrentTaskStack { get; private set; } = new Stack<string>();
        public string TaskTree
        {
            get
            {
                if (CurrentTaskStack.Count == 0)
                    return null;

                StringBuilder sb = new StringBuilder();
                foreach (var task in CurrentTaskStack)
                {
                    if (sb.Length != 0)
                        sb.Append("-");

                    sb.Append(task);
                }

                return sb.ToString();
            }
        }

        void RunTaskLevels(Results results, IterateTask[] task_levels)
        {
            if (task_levels.Length > 1)
            {
                IterateTask[] next_level = new IterateTask[task_levels.Length - 1];
                Array.Copy(task_levels, 1, next_level, 0, task_levels.Length - 1);

                for (int i = 0; i < task_levels[0].Values.Length; i++)
                {
                    CurrentTaskStack.Push(task_levels[0].Values[i]);
                    RunTaskLevels(results, next_level);
                    CurrentTaskStack.Pop();
                }
            }
            else
            {
                for (int i = 0; i < task_levels[0].Values.Length; i++)
                {
                    CurrentTaskStack.Push(task_levels[0].Values[i]);
                    RunTask(results);
                    CurrentTaskStack.Pop();
                }
            }
        }

        public virtual void Run(Results results)
        {
            var type = this.GetType();
            var task_levels = this.GetType().GetCustomAttributes<IterateTask>().ToArray();
            Array.Reverse(task_levels);
            
            if (task_levels.Length == 0)
                RunTask(results);
            else
                RunTaskLevels(results, task_levels);
        }
    }
}
