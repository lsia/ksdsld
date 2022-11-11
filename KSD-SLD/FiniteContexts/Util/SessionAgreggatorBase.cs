using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Util
{
    class SessionAgreggatorBase
    {
        public string Name { get; private set; }
        public string[] Classes { get; private set; }
        public SessionAgreggatorBase(string name, params string[] classes)
        {
            Name = name;
            Classes = classes;
        }

        public void Start()
        {
        }

        ARFF arff;
        Dictionary<int, ARFF> user_arff = new Dictionary<int, ARFF>();
        Dictionary<int, ARFF> len_arff = new Dictionary<int, ARFF>();

        void InitializeARFF(ref ARFF arff, string name, Authentication auth)
        {
            if (arff == null)
            {
                arff = new ARFF(name);
                arff.StartHeaders();
                foreach (var kv in auth.MethodValues)
                    arff.AddNumericAttribute(kv.Key);

                arff.AddCategory("class", Classes);
                arff.StartData();
            }
        }

        void UpdateARFF(ARFF arff, Authentication auth, string auth_class)
        {
            arff.AppendData(auth.MethodValuesArray);
            arff.AppendCategory(auth_class);
            arff.Flush();
        }

        object giant_lock = new object();
        public void UpdateARFFs(Authentication auth, string auth_class)
        {
            lock (giant_lock)
            {
                InitializeARFF(ref arff, "OUTPUT/" + Name, auth);

                int len = auth.Sample.VKs.Length;
                if (!len_arff.ContainsKey(len))
                {
                    ARFF arff_len = null;
                    InitializeARFF(ref arff_len, "OUTPUT/" + Name + "-LEN-" + len, auth);
                    len_arff.Add(len, arff_len);
                }

                int userid = auth.Sample.User.UserID;
                if (!user_arff.ContainsKey(userid))
                {
                    ARFF arff_user = null;
                    InitializeARFF(ref arff_user, "OUTPUT/" + Name + "-USER-" + userid, auth);
                    user_arff.Add(userid, arff_user);
                }

                UpdateARFF(arff, auth, auth_class);
                UpdateARFF(len_arff[len], auth, auth_class);
                UpdateARFF(user_arff[userid], auth, auth_class);
            }
        }

        public void Stop()
        {
            if (arff != null)
                arff.Close();

            foreach (var kv in len_arff)
                kv.Value.Close();

            foreach (var kv in user_arff)
                kv.Value.Close();
        }
    }
}
