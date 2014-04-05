using System;
using System.Collections.Generic;
using Threesus.CoreGame;

namespace Threesus.Bots
{
	/// <summary>
	/// Stores count totals of the current deck for the purpose of counting cards. 
	/// </summary>
	public struct FastDeck
	{
		#region Public Fields

		public int Ones;
		public int Twos;
		public int Threes;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes the counts in a new FastDeck to the card counts in the specified Deck.
		/// </summary>
		public FastDeck(Deck deck)
		{
			if(deck == null)
				throw new ArgumentNullException("deck");

			Dictionary<int, int> cardCounts = deck.GetCountsOfCards();
			cardCounts.TryGetValue(1, out Ones);
			cardCounts.TryGetValue(2, out Twos);
			cardCounts.TryGetValue(3, out Threes);
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Initializes the card counts to their full-deck values.
		/// </summary>
		public void Initialize()
		{
			Ones = 4;
			Twos = 4;
			Threes = 4;
		}

		/// <summary>
		/// Removes a single 1 card from the deck.
		/// </summary>
		public void RemoveOne()
		{
			Ones--;
			if(Ones + Twos + Threes == 0)
				Initialize();
		}

		/// <summary>
		/// Removes a single 2 card from the deck.
		/// </summary>
		public void RemoveTwo()
		{
			Twos--;
			if(Ones + Twos + Threes == 0)
				Initialize();
		}

		/// <summary>
		/// Removes a single 3 card from the deck.
		/// </summary>
		public void RemoveThree()
		{
			Threes--;
			if(Ones + Twos + Threes == 0)
				Initialize();
		}

		/// <summary>
		/// Removes a single card of the specified value from the deck.
		/// </summary>
		public void Remove(ulong cardIndex)
		{
			switch(cardIndex)
			{
				case 1:
					Ones--;
					break;
				case 2:
					Twos--;
					break;
				case 3:
					Threes--;
					break;
			}

			if(Ones + Twos + Threes == 0)
				Initialize();
		}

		#endregion
	}
}