using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.ModelStorages;
using KSDSLD.FiniteContexts.PatternVector;


namespace KSDSLD.FiniteContexts.Models
{
    public class ModelFeeder
    {
        public int MaxContextOrder { get; private set; }
        public ModelStorage[] Storages { get; private set; }
        public ModelFeeder(int max_context_order, ModelStorage[] storages)
        {
            MaxContextOrder = max_context_order;
            Storages = storages;

            foreach (ModelStorage storage in storages)
                if (storage.MaxContextOrder != max_context_order)
                    throw new ArgumentException("All model storages must have consistent max_context_order.");
        }

        void InitializeContext(ref int context_order, ref ulong context, int[] context_values)
        {
            for (int i = 0; i < 8; i++)
                context_values[i] = int.MinValue;

            context_order = 1;
            context = 0xFF;
        }

        public void Feed(Sample session, bool initial_training)
        {
            bool must_feed_manually = false;
            foreach (ModelStorage storage in Storages)
                if (!initial_training || !storage.IsPersistent)
                {
                    storage.FeedSession(session);
                    must_feed_manually |= storage.MustFeedManually;
                }

            if (!must_feed_manually)
                return;

            int context_order = 1;
            ulong context = 0xFF;
            int partition_pos = 0;
            int[] context_values = new int[8];

            InitializeContext(ref context_order, ref context, context_values);
            for (int i = 0; i < session.VKs.Length; i++)
            {
                if (partition_pos < session.PartitionOffsets.Length && i == session.PartitionOffsets[partition_pos])
                {
                    InitializeContext(ref context_order, ref context, context_values);
                    partition_pos++;
                }

                ulong current_context_mask = 0;
                for (int cco = 0; cco <= context_order && cco <= MaxContextOrder; cco++)
                {
                    ulong current_context = context & current_context_mask;
                    ulong model_hash = current_context | ((ulong)session.VKs[i] << 56);
                    foreach (ModelStorage storage in Storages)
                        if (!initial_training || !storage.IsPersistent)
                        {
                            int[] parameter_values = session.Features[storage.Feature];
                            if (parameter_values[i] != int.MinValue)
                                storage.FeedModel(cco, model_hash, parameter_values, i);
                        }

                    current_context_mask <<= 8;
                    current_context_mask |= 0xFF;
                }

                context <<= 8;
                context |= session.VKs[i];
                context_order++;
            }

            foreach (ModelStorage storage in Storages)
                storage.Save();
        }

        public static ulong GetModelHash(byte vk, params byte[] context)
        {
            ulong retval = 0;
            for (int i = 0; i < context.Length; i++)
            {
                retval <<= 8;
                retval |= context[i];
            }

            retval |= (ulong) vk << 56;
            return retval;
        }

        public static byte GetVKFromModelHash(ulong model_hash)
        {
            return (byte)(model_hash >> 56);
        }

        public Builder[] GetBuilders()
        {
            List<Builder> tmp_builders = new List<Builder>();
            foreach (var storage in Storages)
                tmp_builders.Add(new Builder(storage));

            return tmp_builders.ToArray();
        }
    }
}
