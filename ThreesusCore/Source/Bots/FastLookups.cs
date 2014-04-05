using System.Collections.Generic;

namespace Threesus.Bots
{
	/// <summary>
	/// Contains fast lookup arrays by card number.
	/// </summary>
	public static class FastLookups
	{
		/// <summary>
		/// Looks up the face value of a card by its 4-bit index.
		/// </summary>
		public static readonly int[] CARD_INDEX_TO_VALUE =
		{
			0, // No card.
			1,
			2,
			3,
			6,
			12,
			24,
			48,
			96,
			192,
			384,
			768,
			1536,
			3072,
			6144,
			12288
		};

		/// <summary>
		/// Looks up the total point value of a card by its 4-bit index.
		/// </summary>
		public static readonly int[] CARD_INDEX_TO_POINTS =
		{
			0,
			0,
			0,
			3,
			9,
			27,
			81,
			243,
			729,
			2187,
			6561,
			19683,
			59049,
			177147,
			531441,
			1594323,
		};

		/// <summary>
		/// Looks up the card index by its face value.
		/// </summary>
		public static readonly Dictionary<int, ulong> CARD_VALUE_TO_INDEX = new Dictionary<int, ulong>();

		/// <summary>
		/// Initializes lookups.
		/// </summary>
		static FastLookups()
		{
			for(ulong cardIndex = 0; cardIndex < (ulong)CARD_INDEX_TO_VALUE.Length; cardIndex++)
			{
				CARD_VALUE_TO_INDEX.Add(CARD_INDEX_TO_VALUE[cardIndex], cardIndex);
			}
		}
	}
}