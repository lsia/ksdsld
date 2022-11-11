using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using NLog;

using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Store
{
    class SqlServerProfileStore : IProfileStore
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public void StoreProfile(int user_id, ModelFeeder feeder)
        {
            log.Info("Saving user profile {0}...", user_id);
            SqlConnection c = new SqlConnection();
            c.ConnectionString = "Server=.;Database=KSD;Integrated Security=SSPI;";
            c.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = c;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DELETE FROM AT_Models WHERE [User] = @userid;";
            cmd.Parameters.AddWithValue("@userid", user_id);
            cmd.ExecuteNonQuery();

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("IDModel", typeof(long)));
            dt.Columns.Add(new DataColumn("User", typeof(int)));
            dt.Columns.Add(new DataColumn("Context", typeof(long)));
            dt.Columns.Add(new DataColumn("ContextLength", typeof(short)));
            dt.Columns.Add(new DataColumn("Count", typeof(int)));
            dt.Columns.Add(new DataColumn("AVG", typeof(float)));
            dt.Columns.Add(new DataColumn("STD", typeof(float)));

            Dictionary<ulong, Model>[,] models = feeder.Storages[0].GetModels();
            for (int i = 1; i <= feeder.MaxContextOrder; i++)
                foreach (var kv in models[i, 1])
                {
                    AvgStdevModel model = (AvgStdevModel)kv.Value;

                    DataRow dr = dt.NewRow();

#if _DEBUG
                    dr[1] = user_id;
                    dr[2] = (long)model.Context;
                    dr[3] = model.ContextOrder;
                    dr[4] = model.Count;
                    dr[5] = (float)model.Average;
#else
                    Debug.Assert(false, "NO SUPPORT FOR MODEL INTERNALS");
#endif

                    if (double.IsNaN(model.StandardDeviation))
                        dr[6] = DBNull.Value;
                    else
                        dr[6] = (float)model.StandardDeviation;

                    dt.Rows.Add(dr);
                }

            SqlBulkCopy bulk = new SqlBulkCopy(c);
            bulk.DestinationTableName = "AT_Models";
            bulk.WriteToServer(dt);

            c.Close();
            log.Info("  Ready.");
        }
    }
}
