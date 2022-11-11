using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Datasets
{
    public class Dataset
    {
        public string Name { get; private set; }
        public string Source { get; private set; }
        public string Filename { get; private set; }
        public Sample[] Samples { get; private set; }
        public Dictionary<int,Sample[]> SessionsByUser { get; private set; }

        public Dataset(string name, string source, string filename, Sample[] sessions)
        {
            Name = name;
            Source = source;
            Filename = filename;
            SetSessions(sessions);
        }

        public void SetSessions(Sample[] sessions)
        {
            Samples = sessions;

            Dictionary<int, List<Sample>> tmp = new Dictionary<int, List<Sample>>();
            foreach (Sample session in Samples)
            {
                if (!tmp.ContainsKey(session.User.UserID))
                    tmp.Add(session.User.UserID, new List<Sample>());

                tmp[session.User.UserID].Add(session);
            }

            SessionsByUser = new Dictionary<int, Sample[]>();
            foreach (var kv in tmp)
                SessionsByUser.Add(kv.Key, kv.Value.ToArray());
        }
    }
}
