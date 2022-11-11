using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

using NLog;
using StackExchange.Redis;

using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Store
{
    class RedisProfileStore : IProfileStore
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public void StoreProfile(int user_id, ModelFeeder feeder)
        {
            log.Info("Saving user profile {0}...", user_id);

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();

            IBatch batch = db.CreateBatch();

            int count = 0;
            Dictionary<ulong, Model>[,] models = feeder.Storages[0].GetModels();
            for (int i = 1; i <= feeder.MaxContextOrder; i++)
                foreach (var kv in models[i, 1])
                {
                    count++;
                    AvgStdevModel model = (AvgStdevModel)kv.Value;

                    byte[] key = new byte[13];
                    key[0] = (byte)i;
                    key[1] = (byte)((user_id >> 0) & 0xFF);
                    key[2] = (byte)((user_id >> 8) & 0xFF);
                    key[3] = (byte)((user_id >> 16) & 0xFF);
                    key[4] = (byte)((user_id >> 24) & 0xFF);
                    key[5] = (byte)((model.Hash >> 0) & 0xFF);
                    key[6] = (byte)((model.Hash >> 8) & 0xFF);
                    key[7] = (byte)((model.Hash >> 16) & 0xFF);
                    key[8] = (byte)((model.Hash >> 24) & 0xFF);
                    key[9] = (byte)((model.Hash >> 32) & 0xFF);
                    key[10] = (byte)((model.Hash >> 40) & 0xFF);
                    key[11] = (byte)((model.Hash >> 48) & 0xFF);
                    key[12] = (byte)((model.Hash >> 56) & 0xFF);

                    byte[] val = new byte[20];
                    byte[] cnt = BitConverter.GetBytes(model.Count);
                    Array.Copy(cnt, 0, val, 0, 4);
                    byte[] avg = BitConverter.GetBytes(model.Average);
                    Array.Copy(avg, 0, val, 4, 8);
                    if (!double.IsNaN(model.StandardDeviation))
                    {
                        byte[] std = BitConverter.GetBytes(model.StandardDeviation);
                        Array.Copy(std, 0, val, 12, 8);
                    }

                    batch.StringSetAsync(key, val, flags: CommandFlags.FireAndForget);
                }

            batch.Execute();
            log.Info("  {0} operations.", count);
            log.Info("  Ready.");
        }
    }
}
