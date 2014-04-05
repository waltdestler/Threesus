namespace Threesus.Bots
{
	/// <summary>
	/// Stores a stack-allocated list of up to 16 bytes.
	/// </summary>
	public unsafe struct ByteList12
	{
		#region Public Fields

		public int Count;
		public fixed byte Items[16];

		#endregion
		#region Public Methods

		/// <summary>
		/// Adds a single byte item to this list.
		/// </summary>
		public void Add(byte item)
		{
			fixed(byte* arr = Items)
				arr[Count] = item;
			Count++;
		}

		#endregion
	}
}