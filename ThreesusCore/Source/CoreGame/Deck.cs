using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Threesus.CoreGame
{
	/// <summary>
	/// Manages a randomly-shuffled deck of card values.
	/// </summary>
	public class Deck
	{
		#region Fields

		private static readonly int[] INITIAL_CARD_VALUES = {1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3};

		private readonly Rand _rand;
		private readonly List<int> _cardValues = new List<int>();

		#endregion
		#region Constructors

		/// <summary>
		/// Creates a new Deck with the initial set of cards, and shuffles them using the specified random number generator.
		/// </summary>
		public Deck(Rand rand)
		{
			if(rand == null)
				throw new ArgumentNullException("rand");

			_rand = rand;
		}

		/// <summary>
		/// Creates a new Deck with cards copied from the specified Deck.
		/// </summary>
		public Deck(Deck copyFrom)
		{
			CopyFrom(copyFrom);
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Removes and returns the next card value from the top of this Deck.
		/// </summary>
		public int DrawNextCard()
		{
			if(_cardValues.Count == 0)
				RebuildDeck();

			int ret = _cardValues[_cardValues.Count - 1];
			_cardValues.RemoveAt(_cardValues.Count - 1);
			return ret;
		}

		/// <summary>
		/// Returns a hint about what the value of the next drawn card will be.
		/// </summary>
		public int PeekNextCard()
		{
			if(_cardValues.Count == 0)
				RebuildDeck();

			return _cardValues[_cardValues.Count - 1];
		}

		/// <summary>
		/// Copies the current contents of this Deck from the specified Deck.
		/// </summary>
		public void CopyFrom(Deck deck)
		{
			if(deck == null)
				throw new ArgumentNullException("deck");

			_cardValues.Clear();
			_cardValues.AddRange(deck._cardValues);
		}

		/// <summary>
		/// Returns a dictionary with card values as keys for the number of cards of that value.
		/// </summary>
		public Dictionary<int, int> GetCountsOfCards()
		{
			if(_cardValues.Count == 0)
				RebuildDeck();

			Dictionary<int, int> ret = new Dictionary<int, int>();
			for(int i = 0; i < _cardValues.Count; i++)
			{
				int value = _cardValues[i];
				int count;
				if(ret.TryGetValue(value, out count))
					ret[value] = count + 1;
				else
					ret.Add(value, 1);
			}
			return ret;
		}

		/// <summary>
		/// Removes a card of the specified value from this deck.
		/// </summary>
		public void RemoveCard(int cardValue)
		{
			if(_cardValues.Count == 0)
				RebuildDeck();

			_cardValues.Remove(cardValue);
		}

		#endregion
		#region Non-Public Methods

		/// <summary>
		/// Rebuilds the deck using a shuffled list of initial cards.
		/// Assumes that the deck is currently empty.
		/// </summary>
		private void RebuildDeck()
		{
			Debug.Assert(_cardValues.Count == 0);

			_cardValues.AddRange(INITIAL_CARD_VALUES);
			_rand.RandomizeOrder(_cardValues);
		}

		#endregion
	}
}