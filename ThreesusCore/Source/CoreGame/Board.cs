using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Threesus.CoreGame
{
	/// <summary>
	/// Stores a snapshot of the state of the game board.
	/// </summary>
	public class Board
	{
		#region Fields

		private static ReadOnlyCollection<ShiftDirection> _allShiftDirections; 

		private const int BOARD_WIDTH = 4;
		private const int BOARD_HEIGHT = 4;
		private readonly Card[,] _cards = new Card[BOARD_WIDTH, BOARD_HEIGHT];

		#endregion
		#region Properties

		/// <summary>
		/// Gets the number of columns in this Board.
		/// </summary>
		public int Width
		{
			get { return BOARD_WIDTH; }
		}

		/// <summary>
		/// Gets the number of rows in this Board.
		/// </summary>
		public int Height
		{
			get { return BOARD_HEIGHT; }
		}

		/// <summary>
		/// Gets or sets the card in the specified board cell.
		/// </summary>
		public Card this[int x, int y]
		{
			get { return _cards[x, y]; }
			set { _cards[x, y] = value; }
		}

		/// <summary>
		/// Gets or sets the card in the specified board cell.
		/// </summary>
		public Card this[IntVector2D cell]
		{
			get { return _cards[cell.X, cell.Y]; }
			set { _cards[cell.X, cell.Y] = value; }
		}

		#endregion
		#region Constructors

		/// <summary>
		/// Creates a new empty Board with no initial cards.
		/// </summary>
		public Board()
		{
			// Do nothing.
		}

		/// <summary>
		/// Creates a new Board copied from the specified Board.
		/// </summary>
		public Board(Board copyFrom)
		{
			CopyFrom(copyFrom);
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Copies the state of this Board from the specified Board.
		/// </summary>
		public void CopyFrom(Board board)
		{
			if(board == null)
				throw new ArgumentNullException("board");

			Array.Copy(board._cards, _cards, _cards.Length);
		}

		/// <summary>
		/// Calculates and returns the total score of all the cards on this Board.
		/// </summary>
		public int GetTotalScore()
		{
			int total = 0;
			for(int x = 0; x < BOARD_WIDTH; x++)
			{
				for(int y = 0; y < BOARD_HEIGHT; y++)
				{
					Card c = _cards[x, y];
					if(c != null)
						total += c.Score;
				}
			}
			return total;
		}

		/// <summary>
		/// Modifies this Board in-place by shifting those cards that can be shifted or merged in the specified direction.
		/// </summary>
		/// <param name="newCardCells">If not null, the possible locations for a new card will be added to this list.</param>
		/// <returns>Whether anything was able to be shifted.</returns>
		public bool Shift(ShiftDirection dir, IList<IntVector2D> newCardCells)
		{
			bool ret = false;
			IntVector2D increment = GetShiftIncrement(dir);
			int widthOrHeight = GetShiftWidthOrHeight(dir);
			foreach(IntVector2D startCell in GetShiftStartCells(dir))
			{
				bool shifted = ShiftRowOrColumn(startCell, increment, widthOrHeight);
				if(shifted && newCardCells != null)
					newCardCells.Add(startCell - increment * (widthOrHeight - 1));
				ret = ret || shifted;
			}
			return ret;
		}

		/// <summary>
		/// Returns the board cell that contains the card with the specified unique ID, or null if there is no matching card on the board.
		/// </summary>
		public IntVector2D? FindCardWithID(int cardUniqueID)
		{
			for(int x = 0; x < BOARD_WIDTH; x++)
			{
				for(int y = 0; y < BOARD_HEIGHT; y++)
				{
					Card card = this[x, y];
					if(card != null && card.UniqueID == cardUniqueID)
						return new IntVector2D(x, y);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the current highest card value on the board.
		/// </summary>
		public int GetMaxCardValue()
		{
			int ret = 0;
			for(int x = 0; x < BOARD_WIDTH; x++)
			{
				for(int y = 0; y < BOARD_HEIGHT; y++)
				{
					Card card = this[x, y];
					if(card != null)
						ret = Math.Max(ret, card.Value);
				}
			}
			return ret;
		}

		/// <summary>
		/// Returns the string representation of the current state of this board.
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					Card card = this[x, y];
					int value = card != null ? card.Value : 0;
					sb.Append(value + ",");
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}

		#endregion
		#region Non-Public Methods

		/// <summary>
		/// Shifts the row or column containing the specified startCell by the specified increment,
		/// shifting and merging cards where possible.
		/// </summary>
		/// <returns>Whether anything was able to be shifted.</returns>
		private bool ShiftRowOrColumn(IntVector2D startCell, IntVector2D increment, int widthOrHeight)
		{
			bool ret = false;

			// Impossible to shift the start cell, so just skip it.
			IntVector2D prevCell = startCell;
			IntVector2D curCell = startCell - increment;

			// Try to shift each cell one-by-one.
			for(int i = 1; i < widthOrHeight; i++)
			{
				Card curCard = this[curCell];
				if(curCard != null)
				{
					Card prevCard = this[prevCell];
					if(prevCard == null)
					{
						this[prevCell] = curCard;
						this[curCell] = null;
						ret = true;
					}
					else
					{
						// Try to merge on top of the previous card.
						Card merged = curCard.GetMergedWith(prevCard);
						if(merged != null)
						{
							this[prevCell] = merged;
							this[curCell] = null;
							ret = true;
						}
					}
				}

				prevCell = curCell;
				curCell -= increment;
			}

			return ret;
		}

		#endregion
		#region Static Properties

		/// <summary>
		/// Gets a list of all possible ShiftDirection values.
		/// </summary>
		public static ReadOnlyCollection<ShiftDirection> AllShiftDirections
		{
			get
			{
				if(_allShiftDirections == null)
				{
					List<ShiftDirection> list = new List<ShiftDirection>();
					foreach(ShiftDirection dir in Enum.GetValues(typeof(ShiftDirection)))
						list.Add(dir);
					_allShiftDirections = list.AsReadOnly();
				}
				return _allShiftDirections;
			}
		}

		#endregion
		#region Public Static Methods

		/// <summary>
		/// Returns the amount in each dimension by which each card should attempt to shift in the specified direction.
		/// </summary>
		public static IntVector2D GetShiftIncrement(ShiftDirection dir)
		{
			switch(dir)
			{
				case ShiftDirection.Left:
					return new IntVector2D(-1, 0);
				case ShiftDirection.Right:
					return new IntVector2D(1, 0);
				case ShiftDirection.Up:
					return new IntVector2D(0, -1);
				case ShiftDirection.Down:
					return new IntVector2D(0, 1);
				default:
					throw new NotSupportedException("Unknown ShiftDirection '" + dir + "'.");
			}
		}

		/// <summary>
		/// Returns the width or height of the board depending on the specified shift direction.
		/// </summary>
		public static int GetShiftWidthOrHeight(ShiftDirection dir)
		{
			switch(dir)
			{
				case ShiftDirection.Left:
				case ShiftDirection.Right:
					return BOARD_WIDTH;
				case ShiftDirection.Up:
				case ShiftDirection.Down:
					return BOARD_HEIGHT;
				default:
					throw new NotSupportedException("Unknown ShiftDirection '" + dir + "'.");
			}
		}

		/// <summary>
		/// Returns the cells along the edge toward which the board will be shifted by the specified direction.
		/// </summary>
		public static IEnumerable<IntVector2D> GetShiftStartCells(ShiftDirection dir)
		{
			switch(dir)
			{
				case ShiftDirection.Left:
				{
					for(int y = 0; y < BOARD_HEIGHT; y++)
						yield return new IntVector2D(0, y);
					break;
				}
				case ShiftDirection.Right:
				{
					for(int y = 0; y < BOARD_HEIGHT; y++)
						yield return new IntVector2D(BOARD_WIDTH - 1, y);
					break;
				}
				case ShiftDirection.Up:
				{
					for(int x = 0; x < BOARD_WIDTH; x++)
						yield return new IntVector2D(x, 0);
					break;
				}
				case ShiftDirection.Down:
				{
					for(int x = 0; x < BOARD_WIDTH; x++)
						yield return new IntVector2D(x, BOARD_HEIGHT - 1);
					break;
				}
				default:
				{
					throw new NotSupportedException("Unknown ShiftDirection '" + dir + "'.");
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Indicates the direction in which the board should be shifted.
	/// </summary>
	public enum ShiftDirection
	{
		Left,
		Right,
		Up,
		Down,
	}
}