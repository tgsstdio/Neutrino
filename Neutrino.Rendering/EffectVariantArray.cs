namespace MonoGame.Graphics
{ 
	public class EffectVariantArray : IEffectPipelineCollection
	{
		private readonly EffectPipeline[] mVariants;
		public EffectVariantArray (EffectPipeline[] variants)
		{
			mVariants = variants;
		}

		#region IEffectVariantCollection implementation

		public bool TryGetValue (ushort options, out EffectPipeline result)
		{
			if (options >= mVariants.Length)
			{
				result = null;
				return false;
			}
			else
			{
				result = mVariants [options];
				return true;
			}
		}

		#endregion
	}
}

