using System;
using System.Collections.Generic;
using Threesus.Bots;

namespace Threesus.CoreGame
{
	/// <summary>
	/// Manages the current state and rules of the game.
	/// </summary>
	public class Game
	{
		#region Fields

		private const int NUM_INITIAL_CARDS = 9;
		private const float BONUS_CARD_CHANCE = 1f / 21f;

		private readonly Rand _rand;
		private readonly Deck _deck;
		private readonly Board _board;
		private readonly Board _prevBoard;
		private readonly Board _tempBoard;
		private int _nextCardID = 0;
		private int? _nextBonusCard;

		#endregion
		#region Properties

		/// <summary>
		/// Gets the current state of the game board.
		/// </summary>
		public Board CurrentBoard
		{
			get { return _board; }
		}

		/// <summary>
		/// Gets the previous state of the game board before the last shift.
		/// </summary>
		public Board PreviousBoard
		{
			get { return _prevBoard; }
		}

		/// <summary>
		/// Gets the current state of the game card deck.
		/// </summary>
		public Deck CurrentDeck
		{
			get { return _deck; }
		}

		/// <summary>
		/// Returns a hint that indicates the value of the next card to be added to the board.
		/// </summary>
		public NextCardHint NextCardHint
		{
			get
			{
				int nextCardValue = _nextBonusCard ?? _deck.PeekNextCard();
				switch(nextCardValue)
				{
					case 1:
						return NextCardHint.One;
					case 2:
						return NextCardHint.Two;
					case 3:
						return NextCardHint.Three;
					default:
						return NextCardHint.Bonus;
				}
			}
		}

		/// <summary>
		/// Gets the time at which the board was last shifted.
		/// </summary>
		public DateTime LastShiftTime { get; private set; }

		/// <summary>
		/// Gets the direction in which the board was last shifted.
		/// </summary>
		public ShiftDirection LastShiftDirection { get; private set; }

		/// <summary>
		/// Gets the total number of times that the board has been shifted.
		/// </summary>
		public int TotalTurns { get; private set; }

		#endregion
		#region Constructors

		/// <summary>
		/// Creates a new Game that uses the specified random number generator.
		/// </summary>
		public Game(Rand rand)
		{
			if(rand == null)
				throw new ArgumentNullException("rand");

			_rand = rand;
			_deck = new Deck(_rand);
			_board = new Board();

			InitializeBoard();

			_prevBoard = new Board(_board);
			_tempBoard = new Board();
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Shifts the game board in the specified direction, merging cards where possible.
		/// </summary>
		/// <returns>Whether any cards were actually shifted.</returns>
		public bool Shift(ShiftDirection dir)
		{
			_tempBoard.CopyFrom(_board);
			List<IntVector2D> newCardCells = new List<IntVector2D>();
			bool shifted = _board.Shift(dir, newCardCells);
			if(shifted)
			{
				IntVector2D newCardCell = newCardCells[_rand.Int32(0, newCardCells.Count - 1)];
				_board[newCardCell] = DrawNextCard();

				_prevBoard.CopyFrom(_tempBoard);
				LastShiftTime = DateTime.Now;
				LastShiftDirection = dir;
				TotalTurns++;
			}
			return shifted;
		}

		#endregion
		#region Non-Public Methods

		/// <summary>
		/// Initializes the game's Board with its starting cards.
		/// </summary>
		private void InitializeBoard()
		{
			for(int i = 0; i < NUM_INITIAL_CARDS; i++)
			{
				IntVector2D cell = GetRandomEmptyCell();
				_board[cell] = DrawNextCard();
			}
		}

		/// <summary>
		/// Returns a random empty cell on the game board.
		/// </summary>
		private IntVector2D GetRandomEmptyCell()
		{
			IntVector2D ret;
			do
			{
				ret = new IntVector2D(
					_rand.Int32(0, _board.Width - 1),
					_rand.Int32(0, _board.Height - 1));
			}
			while(_board[ret] != null);
			return ret;
		}

		/// <summary>
		/// Draws the next card to add to the board.
		/// </summary>
		private Card DrawNextCard()
		{
			int cardValue = _nextBonusCard ?? _deck.DrawNextCard();

			// Should the next card be a bonus card?
			int maxCardValue = _board.GetMaxCardValue();
			if(maxCardValue >= 48 && _rand.Single() < BONUS_CARD_CHANCE)
			{
				List<int> possibleBonusCards = new List<int>(GetPossibleBonusCards(maxCardValue));
				_nextBonusCard = possibleBonusCards[_rand.Int32(0, possibleBonusCards.Count - 1)];
			}
			else
			{
				_nextBonusCard = null;
			}

			return new Card(cardValue, _nextCardID++);
		}

		#endregion
		#region Public Static Methods

		/// <summary>
		/// Returns the possible bonus cards for the specified board.
		/// </summary>
		public static IEnumerable<int> GetPossibleBonusCards(int maxCardValue)
		{
			int maxBonusCard = maxCardValue / 8;
			for(int val = 6; val <= maxBonusCard; val *= 2)
				yield return val;
		}

		/// <summary>
		/// Returns the possible bonus cards for the specified board.
		/// </summary>
		public static void GetPossibleBonusCardIndexes(ulong maxCardIndex, ref ByteList12 cardIndexes)
		{
			byte maxBonusCardIndex = (byte)(maxCardIndex - 3);
			for(byte cardIndex = 4; cardIndex <= maxBonusCardIndex; cardIndex++)
				cardIndexes.Add(cardIndex);
		}

		#endregion
	}

	/// <summary>
	/// A hint that indicates which next card will be drawn.
	/// </summary>
	public enum NextCardHint
	{
		One,
		Two,
		Three,
		Bonus,
	}
}