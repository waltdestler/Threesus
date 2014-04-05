using System;
using System.Runtime.CompilerServices;
using System.Text;
using Threesus.CoreGame;

namespace Threesus.Bots
{
	/// <summary>
	/// Stores a snapshot of the state of the game board. This version is much faster than the normal Board class, but doesn't contain unique Card IDs.
	/// </summary>
	public struct FastBoard : IEquatable<FastBoard>, IEquatable<Board>
	{
		#region Public Constants

		public const int Width = 4;
		public const int Height = 4;

		#endregion
		#region Lookups

		// Masks used to access a particular cell in the _board variable.
		private const ulong MASK_0_0 = 0x000000000000000f;
		private const ulong MASK_1_0 = 0x00000000000000f0;
		private const ulong MASK_2_0 = 0x0000000000000f00;
		private const ulong MASK_3_0 = 0x000000000000f000;
		private const ulong MASK_0_1 = 0x00000000000f0000;
		private const ulong MASK_1_1 = 0x0000000000f00000;
		private const ulong MASK_2_1 = 0x000000000f000000;
		private const ulong MASK_3_1 = 0x00000000f0000000;
		private const ulong MASK_0_2 = 0x0000000f00000000;
		private const ulong MASK_1_2 = 0x000000f000000000;
		private const ulong MASK_2_2 = 0x00000f0000000000;
		private const ulong MASK_3_2 = 0x0000f00000000000;
		private const ulong MASK_0_3 = 0x000f000000000000;
		private const ulong MASK_1_3 = 0x00f0000000000000;
		private const ulong MASK_2_3 = 0x0f00000000000000;
		private const ulong MASK_3_3 = 0xf000000000000000;

		// A lookup array where the index is (x + 4*y).
		private static readonly ulong[] MASK_LOOKUPS =
		{
			MASK_0_0,
			MASK_1_0,
			MASK_2_0,
			MASK_3_0,
			MASK_0_1,
			MASK_1_1,
			MASK_2_1,
			MASK_3_1,
			MASK_0_2,
			MASK_1_2,
			MASK_2_2,
			MASK_3_2,
			MASK_0_3,
			MASK_1_3,
			MASK_2_3,
			MASK_3_3,
		};

		// Amount by which to right-shift a masked cell to move it to the lowest-order position.
		private const int SHIFT_0_0 = 0;
		private const int SHIFT_1_0 = 4;
		private const int SHIFT_2_0 = 8;
		private const int SHIFT_3_0 = 12;
		private const int SHIFT_0_1 = 16;
		private const int SHIFT_1_1 = 20;
		private const int SHIFT_2_1 = 24;
		private const int SHIFT_3_1 = 28;
		private const int SHIFT_0_2 = 32;
		private const int SHIFT_1_2 = 36;
		private const int SHIFT_2_2 = 40;
		private const int SHIFT_3_2 = 44;
		private const int SHIFT_0_3 = 48;
		private const int SHIFT_1_3 = 52;
		private const int SHIFT_2_3 = 56;
		private const int SHIFT_3_3 = 60;

		// A lookup array where the index is (x + 4*y).
		private static readonly int[] SHIFT_LOOKUPS =
		{
			SHIFT_0_0,
			SHIFT_1_0,
			SHIFT_2_0,
			SHIFT_3_0,
			SHIFT_0_1,
			SHIFT_1_1,
			SHIFT_2_1,
			SHIFT_3_1,
			SHIFT_0_2,
			SHIFT_1_2,
			SHIFT_2_2,
			SHIFT_3_2,
			SHIFT_0_3,
			SHIFT_1_3,
			SHIFT_2_3,
			SHIFT_3_3,
		};

		// A lookup array whose index is (sourceCardIndex | (destCardIndex << 4)) and whose output is the resulting index for the destination cell;
		private static readonly ulong[] DEST_SHIFT_RESULTS = new ulong[16 * 16];

		// A lookup array whose index is (sourceCardIndex | (destCardIndex << 4)) and whose output is the resulting index for the source cell.
		private static readonly ulong[] SOURCE_SHIFT_RESULTS = new ulong[16 * 16];

		/// <summary>
		/// Static constructor to initialize lookup arrays.
		/// </summary>
		static FastBoard()
		{
			for(int sourceIndex = 0; sourceIndex < FastLookups.CARD_INDEX_TO_VALUE.Length; sourceIndex++)
			{
				for(int destIndex = 0; destIndex < FastLookups.CARD_INDEX_TO_VALUE.Length; destIndex++)
				{
					int outputSourceIndex, outputDestIndex;

					if(destIndex == 0)
					{
						outputSourceIndex = 0;
						outputDestIndex = sourceIndex;
					}
					else if(sourceIndex == 0)
					{
						outputSourceIndex = 0;
						outputDestIndex = destIndex;
					}
					else if((sourceIndex == 1 && destIndex == 2) || (sourceIndex == 2 && destIndex == 1))
					{
						outputSourceIndex = 0;
						outputDestIndex = 3;
					}
					else if(sourceIndex >= 3 && sourceIndex == destIndex)
					{
						outputSourceIndex = 0;
						outputDestIndex = sourceIndex + 1; // Inceasing the index one means doubling the face value.
					}
					else
					{
						outputSourceIndex = sourceIndex;
						outputDestIndex = destIndex;
					}

					int lookupArrayIndex = sourceIndex | (destIndex << 4);
					DEST_SHIFT_RESULTS[lookupArrayIndex] = (ulong)outputDestIndex;
					SOURCE_SHIFT_RESULTS[lookupArrayIndex] = (ulong)outputSourceIndex;
				}
			}
		}

		#endregion
		#region Fields

		private ulong _board; // 16 spaces at 4 bits per space = 64 bits.

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new FastBoard from the specified Board object.
		/// </summary>
		public FastBoard(Board board)
		{
			if(board == null)
				throw new ArgumentNullException("board");

			_board = 0;
			for(int x = 0; x < Width; x++)
			{
				for(int y = 0; y < Height; y++)
				{
					Card card = board[x, y];
					SetCardIndex(x, y, FastLookups.CARD_VALUE_TO_INDEX[card != null ? card.Value : 0]);
				}
			}
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Returns whether this board is equal to the specified object.
		/// </summary>
		public override bool Equals(object obj)
		{
			if(obj is FastBoard)
				return Equals((FastBoard)obj);
			else if(obj is Board)
				return Equals((Board)obj);
			else
				return false;
		}

		/// <summary>
		/// Returns whether the state of this board is equal to the specified board.
		/// </summary>
		public bool Equals(FastBoard other)
		{
			return _board == other._board;
		}

		/// <summary>
		/// Returns whether this FastBoard is equal to the specified Board.
		/// </summary>
		public bool Equals(Board board)
		{
			if(board == null)
				return false;

			for(int x = 0; x < Width; x++)
			{
				for(int y = 0; y < Height; y++)
				{
					int ourCardValue = FastLookups.CARD_INDEX_TO_VALUE[GetCardIndex(x, y)];
					Card theirCard = board[x, y];
					int theirCardvalue = theirCard != null ? theirCard.Value : 0;
					if(ourCardValue != theirCardvalue)
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Returns the hash code for the current state of this board.
		/// </summary>
		public override int GetHashCode()
		{
			return _board.GetHashCode();
		}

		/// <summary>
		/// Returns the card index of the card at the specified cell.
		/// 0 means no card there. Use FastLookups to look up values associated with the index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong GetCardIndex(int x, int y)
		{
			int lookupIndex = x + 4 * y;
			return (_board & MASK_LOOKUPS[lookupIndex]) >> SHIFT_LOOKUPS[lookupIndex];
		}

		/// <summary>
		/// Returns the card index of the card at the specified cell.
		/// 0 means no card there. Use FastLookups to look up values associated with the index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong GetCardIndex(IntVector2D cell)
		{
			return GetCardIndex(cell.X, cell.Y);
		}

		/// <summary>
		/// Sets the card index of the card at the specified cell.
		/// 0 means no card there. Use FastLookups to look up values associated with the index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetCardIndex(int x, int y, ulong cardIndex)
		{
			int lookupIndex = x + 4 * y;
			_board = (_board & ~MASK_LOOKUPS[lookupIndex]) | (cardIndex << SHIFT_LOOKUPS[lookupIndex]);
		}

		/// <summary>
		/// Sets the card index of the card at the specified cell.
		/// 0 means no card there. Use FastLookups to look up values associated with the index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetCardIndex(IntVector2D cell, ulong cardIndex)
		{
			SetCardIndex(cell.X, cell.Y, cardIndex);
		}

		/// <summary>
		/// Returns the total point score of the current board.
		/// </summary>
		public int GetTotalScore()
		{
			int total = 0;
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_0_0) >> SHIFT_0_0];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_0_1) >> SHIFT_0_1];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_0_2) >> SHIFT_0_2];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_0_3) >> SHIFT_0_3];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_1_0) >> SHIFT_1_0];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_1_1) >> SHIFT_1_1];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_1_2) >> SHIFT_1_2];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_1_3) >> SHIFT_1_3];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_2_0) >> SHIFT_2_0];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_2_1) >> SHIFT_2_1];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_2_2) >> SHIFT_2_2];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_2_3) >> SHIFT_2_3];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_3_0) >> SHIFT_3_0];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_3_1) >> SHIFT_3_1];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_3_2) >> SHIFT_3_2];
			total += FastLookups.CARD_INDEX_TO_POINTS[(_board & MASK_3_3) >> SHIFT_3_3];
			return total;
		}

		/// <summary>
		/// Gets the card index of the highest-valued card on the board.
		/// </summary>
		public ulong GetMaxCardIndex()
		{
			ulong max = 0;
			for(int x = 0; x < Width; x++)
			{
				for(int y = 0; y < Height; y++)
					max = Math.Max(max, GetCardIndex(x, y));
			}
			return max;
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
					ulong cardIndex = GetCardIndex(x, y);
					int value = FastLookups.CARD_INDEX_TO_VALUE[cardIndex];
					sb.Append(value + ",");
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}

		/// <summary>
		/// Modifies this board in-place by shifting those cards that can be shifted or merged in the specified direction.
		/// </summary>
		/// <param name="newCardCells">The possible locations for a new card will be added to this array.</param>
		/// <returns>Whether anything was able to be shifted.</returns>
		public unsafe bool ShiftInPlace(ShiftDirection dir, IntVector2D* newCardCells)
		{
			ulong oldBoard = _board;
			switch(dir)
			{
				case ShiftDirection.Left:
					ShiftLeft(newCardCells);
					break;
				case ShiftDirection.Right:
					ShiftRight(newCardCells);
					break;
				case ShiftDirection.Up:
					ShiftUp(newCardCells);
					break;
				case ShiftDirection.Down:
					ShiftDown(newCardCells);
					break;
				default:
					throw new NotSupportedException("Unknown ShiftDirection '" + dir + "'.");
			}
			return oldBoard != _board;
		}

		#endregion
		#region Non-Public Methods

		/// <summary>
		/// Shifts this board in-place to the left.
		/// </summary>
		private unsafe void ShiftLeft(IntVector2D* newCardCells)
		{
			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_0_0) >> SHIFT_0_0;
				ulong cell2Index = (_board & MASK_1_0) >> SHIFT_1_0;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_2_0) >> SHIFT_2_0;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_3_0) >> SHIFT_3_0;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_0_0 | MASK_1_0 | MASK_2_0 | MASK_3_0)) |
				         (cell1Index << SHIFT_0_0) |
				         (cell2Index << SHIFT_1_0) |
				         (cell3Index << SHIFT_2_0) |
				         (cell4Index << SHIFT_3_0);
				if(prevBoard != _board)
				{
					newCardCells[0] = new IntVector2D(3, 0);
				}
				else
				{
					newCardCells[0] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_0_1) >> SHIFT_0_1;
				ulong cell2Index = (_board & MASK_1_1) >> SHIFT_1_1;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_2_1) >> SHIFT_2_1;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_3_1) >> SHIFT_3_1;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_0_1 | MASK_1_1 | MASK_2_1 | MASK_3_1)) |
				         (cell1Index << SHIFT_0_1) |
				         (cell2Index << SHIFT_1_1) |
				         (cell3Index << SHIFT_2_1) |
				         (cell4Index << SHIFT_3_1);
				if(prevBoard != _board)
				{
					newCardCells[1] = new IntVector2D(3, 1);
				}
				else
				{
					newCardCells[1] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_0_2) >> SHIFT_0_2;
				ulong cell2Index = (_board & MASK_1_2) >> SHIFT_1_2;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_2_2) >> SHIFT_2_2;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_3_2) >> SHIFT_3_2;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_0_2 | MASK_1_2 | MASK_2_2 | MASK_3_2)) |
				         (cell1Index << SHIFT_0_2) |
				         (cell2Index << SHIFT_1_2) |
				         (cell3Index << SHIFT_2_2) |
				         (cell4Index << SHIFT_3_2);
				if(prevBoard != _board)
				{
					newCardCells[2] = new IntVector2D(3, 2);
				}
				else
				{
					newCardCells[2] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_0_3) >> SHIFT_0_3;
				ulong cell2Index = (_board & MASK_1_3) >> SHIFT_1_3;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_2_3) >> SHIFT_2_3;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_3_3) >> SHIFT_3_3;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_0_3 | MASK_1_3 | MASK_2_3 | MASK_3_3)) |
				         (cell1Index << SHIFT_0_3) |
				         (cell2Index << SHIFT_1_3) |
				         (cell3Index << SHIFT_2_3) |
				         (cell4Index << SHIFT_3_3);
				if(prevBoard != _board)
				{
					newCardCells[3] = new IntVector2D(3, 3);
				}
				else
				{
					newCardCells[3] = new IntVector2D(-1, -1);
				}
			}
		}

		/// <summary>
		/// Shifts this board in-place to the right.
		/// </summary>
		private unsafe void ShiftRight(IntVector2D* newCardCells)
		{
			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_3_0) >> SHIFT_3_0;
				ulong cell2Index = (_board & MASK_2_0) >> SHIFT_2_0;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_1_0) >> SHIFT_1_0;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_0_0) >> SHIFT_0_0;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_3_0 | MASK_2_0 | MASK_1_0 | MASK_0_0)) |
				         (cell1Index << SHIFT_3_0) |
				         (cell2Index << SHIFT_2_0) |
				         (cell3Index << SHIFT_1_0) |
				         (cell4Index << SHIFT_0_0);
				if(prevBoard != _board)
				{
					newCardCells[0] = new IntVector2D(0, 0);
				}
				else
				{
					newCardCells[0] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_3_1) >> SHIFT_3_1;
				ulong cell2Index = (_board & MASK_2_1) >> SHIFT_2_1;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_1_1) >> SHIFT_1_1;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_0_1) >> SHIFT_0_1;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_3_1 | MASK_2_1 | MASK_1_1 | MASK_0_1)) |
				         (cell1Index << SHIFT_3_1) |
				         (cell2Index << SHIFT_2_1) |
				         (cell3Index << SHIFT_1_1) |
				         (cell4Index << SHIFT_0_1);
				if(prevBoard != _board)
				{
					newCardCells[1] = new IntVector2D(0, 1);
				}
				else
				{
					newCardCells[1] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_3_2) >> SHIFT_3_2;
				ulong cell2Index = (_board & MASK_2_2) >> SHIFT_2_2;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_1_2) >> SHIFT_1_2;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_0_2) >> SHIFT_0_2;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_3_2 | MASK_2_2 | MASK_1_2 | MASK_0_2)) |
				         (cell1Index << SHIFT_3_2) |
				         (cell2Index << SHIFT_2_2) |
				         (cell3Index << SHIFT_1_2) |
				         (cell4Index << SHIFT_0_2);
				if(prevBoard != _board)
				{
					newCardCells[2] = new IntVector2D(0, 2);
				}
				else
				{
					newCardCells[2] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_3_3) >> SHIFT_3_3;
				ulong cell2Index = (_board & MASK_2_3) >> SHIFT_2_3;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_1_3) >> SHIFT_1_3;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_0_3) >> SHIFT_0_3;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_3_3 | MASK_2_3 | MASK_1_3 | MASK_0_3)) |
				         (cell1Index << SHIFT_3_3) |
				         (cell2Index << SHIFT_2_3) |
				         (cell3Index << SHIFT_1_3) |
				         (cell4Index << SHIFT_0_3);
				if(prevBoard != _board)
				{
					newCardCells[3] = new IntVector2D(0, 3);
				}
				else
				{
					newCardCells[3] = new IntVector2D(-1, -1);
				}
			}
		}

		/// <summary>
		/// Shifts this board in-place up.
		/// </summary>
		private unsafe void ShiftUp(IntVector2D* newCardCells)
		{
			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_0_0) >> SHIFT_0_0;
				ulong cell2Index = (_board & MASK_0_1) >> SHIFT_0_1;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_0_2) >> SHIFT_0_2;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_0_3) >> SHIFT_0_3;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_0_0 | MASK_0_1 | MASK_0_2 | MASK_0_3)) |
				         (cell1Index << SHIFT_0_0) |
				         (cell2Index << SHIFT_0_1) |
				         (cell3Index << SHIFT_0_2) |
				         (cell4Index << SHIFT_0_3);
				if(prevBoard != _board)
				{
					newCardCells[0] = new IntVector2D(0, 3);
				}
				else
				{
					newCardCells[0] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_1_0) >> SHIFT_1_0;
				ulong cell2Index = (_board & MASK_1_1) >> SHIFT_1_1;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_1_2) >> SHIFT_1_2;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_1_3) >> SHIFT_1_3;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_1_0 | MASK_1_1 | MASK_1_2 | MASK_1_3)) |
				         (cell1Index << SHIFT_1_0) |
				         (cell2Index << SHIFT_1_1) |
				         (cell3Index << SHIFT_1_2) |
				         (cell4Index << SHIFT_1_3);
				if(prevBoard != _board)
				{
					newCardCells[1] = new IntVector2D(1, 3);
				}
				else
				{
					newCardCells[1] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_2_0) >> SHIFT_2_0;
				ulong cell2Index = (_board & MASK_2_1) >> SHIFT_2_1;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_2_2) >> SHIFT_2_2;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_2_3) >> SHIFT_2_3;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_2_0 | MASK_2_1 | MASK_2_2 | MASK_2_3)) |
				         (cell1Index << SHIFT_2_0) |
				         (cell2Index << SHIFT_2_1) |
				         (cell3Index << SHIFT_2_2) |
				         (cell4Index << SHIFT_2_3);
				if(prevBoard != _board)
				{
					newCardCells[2] = new IntVector2D(2, 3);
				}
				else
				{
					newCardCells[2] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_3_0) >> SHIFT_3_0;
				ulong cell2Index = (_board & MASK_3_1) >> SHIFT_3_1;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_3_2) >> SHIFT_3_2;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_3_3) >> SHIFT_3_3;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_3_0 | MASK_3_1 | MASK_3_2 | MASK_3_3)) |
				         (cell1Index << SHIFT_3_0) |
				         (cell2Index << SHIFT_3_1) |
				         (cell3Index << SHIFT_3_2) |
				         (cell4Index << SHIFT_3_3);
				if(prevBoard != _board)
				{
					newCardCells[3] = new IntVector2D(3, 3);
				}
				else
				{
					newCardCells[3] = new IntVector2D(-1, -1);
				}
			}
		}

		/// <summary>
		/// Shifts this board in-place down.
		/// </summary>
		private unsafe void ShiftDown(IntVector2D* newCardCells)
		{
			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_0_3) >> SHIFT_0_3;
				ulong cell2Index = (_board & MASK_0_2) >> SHIFT_0_2;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_0_1) >> SHIFT_0_1;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_0_0) >> SHIFT_0_0;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_0_3 | MASK_0_2 | MASK_0_1 | MASK_0_0)) |
				         (cell1Index << SHIFT_0_3) |
				         (cell2Index << SHIFT_0_2) |
				         (cell3Index << SHIFT_0_1) |
				         (cell4Index << SHIFT_0_0);
				if(prevBoard != _board)
				{
					newCardCells[0] = new IntVector2D(0, 0);
				}
				else
				{
					newCardCells[0] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_1_3) >> SHIFT_1_3;
				ulong cell2Index = (_board & MASK_1_2) >> SHIFT_1_2;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_1_1) >> SHIFT_1_1;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_1_0) >> SHIFT_1_0;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_1_3 | MASK_1_2 | MASK_1_1 | MASK_1_0)) |
				         (cell1Index << SHIFT_1_3) |
				         (cell2Index << SHIFT_1_2) |
				         (cell3Index << SHIFT_1_1) |
				         (cell4Index << SHIFT_1_0);
				if(prevBoard != _board)
				{
					newCardCells[1] = new IntVector2D(1, 0);
				}
				else
				{
					newCardCells[1] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_2_3) >> SHIFT_2_3;
				ulong cell2Index = (_board & MASK_2_2) >> SHIFT_2_2;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_2_1) >> SHIFT_2_1;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_2_0) >> SHIFT_2_0;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_2_3 | MASK_2_2 | MASK_2_1 | MASK_2_0)) |
				         (cell1Index << SHIFT_2_3) |
				         (cell2Index << SHIFT_2_2) |
				         (cell3Index << SHIFT_2_1) |
				         (cell4Index << SHIFT_2_0);
				if(prevBoard != _board)
				{
					newCardCells[2] = new IntVector2D(2, 0);
				}
				else
				{
					newCardCells[2] = new IntVector2D(-1, -1);
				}
			}

			{
				ulong prevBoard = _board;

				ulong cell1Index = (_board & MASK_3_3) >> SHIFT_3_3;
				ulong cell2Index = (_board & MASK_3_2) >> SHIFT_3_2;

				ulong arrayLookup = cell2Index | (cell1Index << 4);
				cell1Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell2Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell3Index = (_board & MASK_3_1) >> SHIFT_3_1;
				arrayLookup = cell3Index | (cell2Index << 4);
				cell2Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell3Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				ulong cell4Index = (_board & MASK_3_0) >> SHIFT_3_0;
				arrayLookup = cell4Index | (cell3Index << 4);
				cell3Index = DEST_SHIFT_RESULTS[arrayLookup];
				cell4Index = SOURCE_SHIFT_RESULTS[arrayLookup];

				_board = (_board & ~(MASK_3_3 | MASK_3_2 | MASK_3_1 | MASK_3_0)) |
				         (cell1Index << SHIFT_3_3) |
				         (cell2Index << SHIFT_3_2) |
				         (cell3Index << SHIFT_3_1) |
				         (cell4Index << SHIFT_3_0);
				if(prevBoard != _board)
				{
					newCardCells[3] = new IntVector2D(3, 0);
				}
				else
				{
					newCardCells[3] = new IntVector2D(-1, -1);
				}
			}
		}

		#endregion
		#region Public Static Methods

		/// <summary>
		/// Returns whether the specified cards can merge together.
		/// Assumes that neither card index is 0.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanCardsMerge(ulong sourceCardIndex, ulong destCardIndex)
		{
			ulong arrayLookup = sourceCardIndex | (destCardIndex << 4);
			return DEST_SHIFT_RESULTS[arrayLookup] != destCardIndex;
		}

		#endregion
	}
}