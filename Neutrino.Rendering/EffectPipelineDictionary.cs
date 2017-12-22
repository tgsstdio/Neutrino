using System.Collections.Generic;

namespace MonoGame.Graphics
{
	public class EffectPipelineDictionary : IEffectPipelineCollection
	{
		private readonly Dictionary<ushort, EffectPipeline> mEntries;
		public EffectPipelineDictionary ()
		{
			mEntries = new Dictionary<ushort, EffectPipeline> ();
		}

		public void Add(ushort key, EffectPipeline item)
		{
			mEntries.Add (key, item);
		}

		#region IEffectVariantCollection implementation

		public bool TryGetValue (ushort options, out EffectPipeline result)
		{
			return mEntries.TryGetValue (options, out result);
		}

		#endregion
	}
}

