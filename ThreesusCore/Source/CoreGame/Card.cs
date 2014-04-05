using System;

namespace Threesus.CoreGame
{
	/// <summary>
	/// Represents a single card in the game.
	/// </summary>
	public class Card : IEquatable<Card>
	{
		#region Properties

		/// <summary>
		/// Gets or sets the face-value of this card.
		/// 1, 2, 3, 6, 12, 24, etc...
		/// </summary>
		public int Value { get; private set; }

		/// <summary>
		/// Gets or sets the ID of this card that is unique within a single game of Threes.
		/// This number can be used to track how a card was moved between consecutive turns.
		/// </summary>
		public int UniqueID { get; private set; }

		/// <summary>
		/// Gets the score points that this card is worth at the end of the game.
		/// </summary>
		public int Score
		{
			get
			{
				int valOver3 = Value / 3;
				int exp = (int)Math.Log(valOver3, 2) + 1;
				return (int)Math.Pow(3, exp);
			}
		}

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new Card.
		/// </summary>
		public Card(int value, int uniqueID)
		{
			Value = value;
			UniqueID = uniqueID;
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Returns whether this Card is equal to the specified object, including having the same UniqueID.
		/// </summary>
		public override bool Equals(object obj)
		{
			Card card = obj as Card;
			if(card != null)
				return Equals(card);
			else
				return false;
		}

		/// <summary>
		/// Returns whether this Card is equal to the specified Card, including having the same UniqueID.
		/// </summary>
		public bool Equals(Card card)
		{
			return card != null && Value == card.Value && UniqueID == card.UniqueID;
		}

		/// <summary>
		/// Returns the hash code for this Card.
		/// </summary>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Value.GetHashCode();
				hash = hash * 23 + UniqueID.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Returns the string representation of this Card.
		/// </summary>
		public override string ToString()
		{
			return "{Value=" + Value + ",UID=" + UniqueID + "}";
		}

		/// <summary>
		/// Returns whether this Card can be merged with the specified other card.
		/// The merge relationship between two cards is always symmetrical.
		/// </summary>
		public bool CanMergeWith(Card other)
		{
			if(Value == 1)
				return other.Value == 2;
			else if(Value == 2)
				return other.Value == 1;
			else
				return Value == other.Value;
		}

		/// <summary>
		/// Gets the result of merging this Card with the specified Card.
		/// Returns null if the merge cannot happen.
		/// The UniqueID of the new card will be the UniqueID of this Card.
		/// </summary>
		public Card GetMergedWith(Card other)
		{
			if(Value == 1)
				return other.Value == 2 ? new Card(3, UniqueID) : null;
			else if(Value == 2)
				return other.Value == 1 ? new Card(3, UniqueID) : null;
			else
				return Value == other.Value ? new Card(Value * 2, UniqueID) : null;
		}

		#endregion
		#region Operators

		/// <summary>
		/// Returns whether the specified Cards are equal.
		/// </summary>
		public static bool operator ==(Card card1, Card card2)
		{
			return Equals(card1, card2);
		}

		/// <summary>
		/// Returns whether the specified Cards are not equal.
		/// </summary>
		public static bool operator !=(Card card1, Card card2)
		{
			return !Equals(card1, card2);
		}

		#endregion
	}
}