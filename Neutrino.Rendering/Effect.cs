namespace MonoGame.Graphics
{
	public class Effect
	{
		/// <summary>
		/// Data type should match 
		/// </summary>
		/// <value>The index of the techinque.</value>
		public byte TechinqueIndex {get;set;}

		public EffectPass[] Passes {get;set;}
	}
}

