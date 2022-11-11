using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.ModelStorages
{
    public abstract class ModelStorage
    {
        public User User { get; private set; }
        public string Name { get; private set; }
        public TypingFeature Feature { get; private set; }
        public int MaxContextOrder { get; private set; }
        public int MaxNGramOrder { get; private set; }
        public IModelFactory<Model> Factory { get; private set; }


        public ModelStorage(User user, string name, TypingFeature feature, int max_context_order, int max_ngram_order, IModelFactory<Model> factory)
        {
            if (max_context_order + max_ngram_order > 8)
                throw new ArgumentException("max_context_length + max_ngram_length > 8");

            User = user;
            Name = name;
            Feature = feature;
            MaxContextOrder = max_context_order;
            MaxNGramOrder = max_ngram_order;
            Factory = factory;

            models = new Dictionary<ulong, Model>[MaxContextOrder + 1, MaxNGramOrder + 1];
            for (int i = 0; i <= MaxContextOrder; i++)
                for (int j = 0; j <= MaxNGramOrder; j++)
                    models[i, j] = new Dictionary<ulong, Model>();
        }

        protected Dictionary<ulong, Model>[,] models;
        public Dictionary<ulong, Model>[,] GetModels()
        {
            return models;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Save()
        {
        }
        public virtual bool IsPersistent { get { return false; } }

        public virtual bool MustFeedManually { get { return true; } }
        public virtual void FeedSession(Sample session)
        {
        }

        public virtual void FeedModel(int context_order, ulong model_hash, int[] parameter_values, int pos)
        {
            Model model = GetModel(true, context_order, 1, model_hash);
            model.Feed(parameter_values, pos);
        }

        public abstract Model GetModel(bool create, int context_order, int ngram_order, ulong model_hash);

        public abstract Model[] GetBulk(int max_context_order, ulong[] model_hash);
    }
}
