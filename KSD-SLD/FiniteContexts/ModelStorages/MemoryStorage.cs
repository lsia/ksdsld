using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.ModelStorages
{
    class MemoryStorage : ModelStorage
    {
        public MemoryStorage(User user, string name, TypingFeature feature, int max_context_order, int max_ngram_order, IModelFactory<Model> factory)
            : base(user, name, feature, max_context_order, max_ngram_order, factory)
        {
        }

        public override Model GetModel(bool create, int context_order, int ngram_order, ulong model_hash)
        {
            var current_models = models[context_order, ngram_order];

            if (current_models.ContainsKey(model_hash))
                return current_models[model_hash];
            else if (!create)
                return null;
            else
            {
                Model model = Factory.CreateModel(model_hash, (short) context_order, (int) model_hash, (short) ngram_order);
                current_models.Add(model_hash, model);
                return model;
            }
        }

        public override Model[] GetBulk(int context_order, ulong[] model_hash)
        {
            Model[] retval = new Model[model_hash.Length];
            for (int i = 0; i < model_hash.Length; i++)
                if (model_hash[i] != 0)
                {
                    var current_models = models[i % (context_order + 1), 1];
                    if (current_models.ContainsKey(model_hash[i]))
                        retval[i] = models[i % (context_order + 1), 1][model_hash[i]];
                }

            return retval;
        }
    }
}
