using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Datasets
{
    public class User
    {
        public int UserID { get; private set; }
        public string Name { get; private set; }
        public DateTime BirthDate { get; private set; }
        public Gender Gender { get; private set; }

        private User(int user_id, string name, DateTime birth_date, Gender gender)
        {
            UserID = user_id;
            Name = name;
            BirthDate = birth_date;
            Gender = gender;
        }

        static object giant_lock = new object();
        static Dictionary<int, User> users = new Dictionary<int, User>();
        public static User GetUser(int user_id, string name, DateTime birth_date, Gender gender)
        {
            lock (giant_lock)
            {
                if (users.ContainsKey(user_id))
                    return users[user_id];
                else
                {
                    User retval = new User(user_id, name, birth_date, gender);
                    users.Add(user_id, retval);
                    return retval;
                }
            }
        }

        public static User GetUser(int user_id)
        {
            lock (giant_lock)
            {
                if (users.ContainsKey(user_id))
                    return users[user_id];
                else
                {
                    if (user_id != int.MinValue)
                        throw new ArgumentException("User " + user_id + " does not exist.");

                    User min = new User(int.MinValue, "", DateTime.Now, Gender.Unknown);
                    users.Add(int.MinValue, min);
                    return min;
                }
            }
        }

        public Dictionary<string, object> Properties = new Dictionary<string, object>();
    }
}
