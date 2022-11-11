using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using NLog;
using NLog.LayoutRenderers;


namespace KSDSLD
{
    [LayoutRenderer("indenter")]
    public class IndentLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (LogThreadID)
            {
                builder.Append("[");
                builder.Append(Thread.CurrentThread.ManagedThreadId);
                builder.Append("] ");
            }

            int thread_id = AddThreadData();
            string tmp = current[thread_id];
            if (tmp.Length != 0)
                builder.Append(tmp);
        }

        static int AddThreadData()
        {
            int thread_id = Thread.CurrentThread.ManagedThreadId;
            if (!current.ContainsKey(thread_id))
                current.Add(thread_id, "");

            return thread_id;
        }

        static Dictionary<int, string> current = new Dictionary<int, string>();
        public static string Add()
        {
            int thread_id = AddThreadData();

            string retval = current[thread_id];
            current[thread_id] += "  ";
            return retval;
        }

        public static void Set(string previous_indent)
        {
            int thread_id = AddThreadData();
            current[thread_id] = previous_indent;
        }

        public static void Remove()
        {
            int thread_id = AddThreadData();

            string tmp = current[thread_id];
            if (tmp.Length >= 2)
                current[thread_id] = tmp.Substring(2);
        }

        public static bool LogThreadID = false;
    }
}
