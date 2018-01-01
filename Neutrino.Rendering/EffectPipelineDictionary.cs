using Magnesium;
using System.Collections.Generic;

namespace Neutrino
{
	public class EffectPipelineDictionary
	{
		private readonly Dictionary<EffectVariantKey, EffectVariant> mVariants;
		public EffectPipelineDictionary ()
		{
			mVariants = new Dictionary<EffectVariantKey, EffectVariant> ();
		}

		public void Add(EffectVariantKey key, EffectVariant item)
		{
			mVariants.Add (key, item);
		}

		public bool TryGetValue (EffectVariantKey key, out EffectVariant result)
		{
			return mVariants.TryGetValue (key, out result);
		}

        public void Clear(IMgDevice device)
        {
            foreach(var entry in mVariants.Values)
            {
                entry.Pipeline.DestroyPipeline(device, null);
            }
            mVariants.Clear();
        }
	}
}

