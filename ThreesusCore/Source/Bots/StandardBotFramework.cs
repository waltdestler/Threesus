using System;
using System.Threading.Tasks;
using Threesus.CoreGame;

namespace Threesus.Bots
{
	/// <summary>
	/// Provides a generic framework that uses callbacks to customize bot thinking logic.
	/// </summary>
	public class StandardBotFramework : IBot
	{
		#region Fields

		private readonly int _moveSearchDepth;
		private readonly int _cardCountDepth;
		private readonly BoardQualityEvaluator _evaluator;

		#endregion
		#region Constructors

		/// <summary>
		/// Creates a new StandardBotFramework that evaluates moves using the specified logic callbacks.
		/// </summary>
		/// <param name="moveSearchDepth">The number of moves into the future to examine. Should be at least 1.</param>
		/// <param name="cardCountDepth">The number of moves into the future in which the deck should be "card counted"</param>
		/// <param name="evaluator">A callback that evaluates the quality of a board into a single value.</param>
		public StandardBotFramework(int moveSearchDepth, int cardCountDepth, BoardQualityEvaluator evaluator)
		{
			if(moveSearchDepth < 1)
				throw new ArgumentOutOfRangeException("moveSearchDepth");
			if(cardCountDepth < 1 || cardCountDepth > moveSearchDepth)
				throw new ArgumentOutOfRangeException("cardCountDepth");
			if(evaluator == null)
				throw new ArgumentNullException("evaluator");

			_moveSearchDepth = moveSearchDepth;
			_cardCountDepth = cardCountDepth;
			_evaluator = evaluator;
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Returns the next move to make based on the state of the specified game, or null to make no move.
		/// </summary>
		public ShiftDirection? GetNextMove(FastBoard board, FastDeck deck, NextCardHint nextCardHint, ref long movesEvaluated)
		{
			ulong knownNextCardIndex;
			switch(nextCardHint)
			{
				case NextCardHint.One:
					knownNextCardIndex = 1;
					break;
				case NextCardHint.Two:
					knownNextCardIndex = 2;
					break;
				case NextCardHint.Three:
					knownNextCardIndex = 3;
					break;
				case NextCardHint.Bonus:
					knownNextCardIndex = ulong.MaxValue; // MaxValue means "bonus" and is calculated specially.
					break;
				default:
					throw new NotSupportedException("Unknown NextCardHint '" + nextCardHint + "'.");
			}

			float quality;
			return GetBestMoveForBoard(board, deck, knownNextCardIndex, _moveSearchDepth - 1, out quality, ref movesEvaluated);
		}

		/// <summary>
		/// Returns the next move to make based on the state of the specified game, or null to make no move.
		/// </summary>
		public ShiftDirection? GetNextMove(FastBoard board, FastDeck deck, NextCardHint nextCardHint)
		{
			long movesEvaluated = 0;
			return GetNextMove(board, deck, nextCardHint, ref movesEvaluated);
		}

		/// <summary>
		/// Returns the string representation of this StandardBotFramework.
		/// </summary>
		public override string ToString()
		{
			return string.Format("Standard Bot Framework\nMove Search Depth: {0}\nCard Count Depth: {1}\nEvaluator: {2}",
				_moveSearchDepth,
				_cardCountDepth,
				_evaluator.Method.Name);
		}

		#endregion
		#region Non-Public Methods

		/// <summary>
		/// Returns the best move to make for the specified board, or null if there are no moves to make.
		/// Outputs the quality of the returned move.
		/// </summary>
		private ShiftDirection? GetBestMoveForBoard(FastBoard board, FastDeck deck, ulong knownNextCardIndex, int recursionsLeft, out float moveQuality, ref long movesEvaluated)
		{
			float? leftQuality = null;
			float? rightQuality = null;
			float? upQuality = null;
			float? downQuality = null;
			long moves1 = 0, moves2 = 0;
			Parallel.Invoke(
				() =>
				{
					leftQuality = EvaluateMoveForBoard(board, deck, knownNextCardIndex, ShiftDirection.Left, recursionsLeft, ref moves1);
					rightQuality = EvaluateMoveForBoard(board, deck, knownNextCardIndex, ShiftDirection.Right, recursionsLeft, ref moves1);
				},
				() =>
				{
					upQuality = EvaluateMoveForBoard(board, deck, knownNextCardIndex, ShiftDirection.Up, recursionsLeft, ref moves2);
					downQuality = EvaluateMoveForBoard(board, deck, knownNextCardIndex, ShiftDirection.Down, recursionsLeft, ref moves2);
				});
			movesEvaluated += moves1 + moves2;

			float? bestQuality = leftQuality;
			ShiftDirection? bestDir = leftQuality != null ? ShiftDirection.Left : (ShiftDirection?)null;
			if(rightQuality != null && (bestQuality == null || rightQuality.Value > bestQuality.Value))
			{
				bestQuality = rightQuality;
				bestDir = ShiftDirection.Right;
			}
			if(upQuality != null && (bestQuality == null || upQuality.Value > bestQuality.Value))
			{
				bestQuality = upQuality;
				bestDir = ShiftDirection.Up;
			}
			if(downQuality != null && (bestQuality == null || downQuality.Value > bestQuality.Value))
			{
				bestQuality = downQuality;
				bestDir = ShiftDirection.Down;
			}
			moveQuality = bestQuality ?? float.MinValue;
			return bestDir;
		}

		/// <summary>
		/// Returns the quality value for shifting the specified board in the specified direction.
		/// Returns null if shifting in that direction is not possible.
		/// </summary>
		private unsafe float? EvaluateMoveForBoard(FastBoard board, FastDeck deck, ulong knownNextCardIndex, ShiftDirection dir, int recursionsLeft, ref long movesEvaluated)
		{
			FastBoard shiftedBoard = board;
			IntVector2D* newCardCells = stackalloc IntVector2D[4];
			if(shiftedBoard.ShiftInPlace(dir, newCardCells))
			{
				float totalQuality = 0;
				float totalWeight = 0;

				if(knownNextCardIndex == ulong.MaxValue) // Special value for bonus card.
				{
					ByteList12 indexes = new ByteList12();
					Game.GetPossibleBonusCardIndexes(board.GetMaxCardIndex(), ref indexes);
					for(int i = 0; i < indexes.Count; i++)
					{
						ulong cardIndex = indexes.Items[i];
						for(int j = 0; j < 4; j++)
						{
							IntVector2D cell = newCardCells[j];
							if(cell.X < 0)
								continue;

							FastBoard newBoard = shiftedBoard;
							newBoard.SetCardIndex(cell, cardIndex);

							float quality;
							if(recursionsLeft == 0 || GetBestMoveForBoard(newBoard, deck, 0, recursionsLeft - 1, out quality, ref movesEvaluated) == null)
							{
								quality = _evaluator(newBoard);
								movesEvaluated++;
							}

							totalQuality += quality;
							totalWeight += 1;
						}
					}
				}
				else if(knownNextCardIndex > 0)
				{
					FastDeck newDeck = deck;
					newDeck.Remove(knownNextCardIndex);
					for(int i = 0; i < 4; i++)
					{
						IntVector2D cell = newCardCells[i];
						if(cell.X < 0)
							continue;

						FastBoard newBoard = shiftedBoard;
						newBoard.SetCardIndex(cell, knownNextCardIndex);

						float quality;
						if(recursionsLeft == 0 || GetBestMoveForBoard(newBoard, newDeck, 0, recursionsLeft - 1, out quality, ref movesEvaluated) == null)
						{
							quality = _evaluator(newBoard);
							movesEvaluated++;
						}

						totalQuality += quality;
						totalWeight += 1;
					}
				}
				else if(_moveSearchDepth - recursionsLeft - 1 < _cardCountDepth)
				{
					if(deck.Ones > 0)
					{
						FastDeck newDeck = deck;
						newDeck.RemoveOne();
						for(int i = 0; i < 4; i++)
						{
							IntVector2D cell = newCardCells[i];
							if(cell.X < 0)
								continue;

							FastBoard newBoard = shiftedBoard;
							newBoard.SetCardIndex(cell, 1);

							float quality;
							if(recursionsLeft == 0 || GetBestMoveForBoard(newBoard, newDeck, 0, recursionsLeft - 1, out quality, ref movesEvaluated) == null)
							{
								quality = _evaluator(newBoard);
								movesEvaluated++;
							}

							totalQuality += quality;
							totalWeight += deck.Ones;
						}
					}

					if(deck.Twos > 0)
					{
						FastDeck newDeck = deck;
						newDeck.RemoveTwo();
						for(int i = 0; i < 4; i++)
						{
							IntVector2D cell = newCardCells[i];
							if(cell.X < 0)
								continue;

							FastBoard newBoard = shiftedBoard;
							newBoard.SetCardIndex(cell, 2);

							float quality;
							if(recursionsLeft == 0 || GetBestMoveForBoard(newBoard, newDeck, 0, recursionsLeft - 1, out quality, ref movesEvaluated) == null)
							{
								quality = _evaluator(newBoard);
								movesEvaluated++;
							}

							totalQuality += quality;
							totalWeight += deck.Twos;
						}
					}

					if(deck.Threes > 0)
					{
						FastDeck newDeck = deck;
						newDeck.RemoveThree();
						for(int i = 0; i < 4; i++)
						{
							IntVector2D cell = newCardCells[i];
							if(cell.X < 0)
								continue;

							FastBoard newBoard = shiftedBoard;
							newBoard.SetCardIndex(cell, 3);

							float quality;
							if(recursionsLeft == 0 || GetBestMoveForBoard(newBoard, newDeck, 0, recursionsLeft - 1, out quality, ref movesEvaluated) == null)
							{
								quality = _evaluator(newBoard);
								movesEvaluated++;
							}

							totalQuality += quality;
							totalWeight += deck.Threes;
						}
					}

					// Note that we're not taking the chance of getting a bonus card into consideration. That would be way too expensive at not much benefit.
				}
				else
				{
					float quality;
					if(recursionsLeft == 0 || GetBestMoveForBoard(shiftedBoard, deck, 0, recursionsLeft - 1, out quality, ref movesEvaluated) == null)
					{
						quality = _evaluator(shiftedBoard);
						movesEvaluated++;
					}

					totalQuality += quality;
					totalWeight += 1;
				}

				return totalQuality / totalWeight;
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}