using System.Collections.Generic;
using System.Linq;
using Threesus.CoreGame;

namespace Threesus.Bots
{
	/// <summary>
	/// A simple implementation of IBot that peforms moves a random.
	/// </summary>
	public class RandomBot : IBot
	{
		#region Fields

		private readonly IRand _rand = new Rand();

		#endregion
		#region Public Methods

		/// <summary>
		/// Returns the next move to make based on the state of the specified game, or null to make no move.
		/// </summary>
		public ShiftDirection? GetNextMove(FastBoard board, FastDeck deck, NextCardHint nextCardHint, ref long movesEvaluated)
		{
			movesEvaluated = 0;
			List<ShiftDirection> validDirs = new List<ShiftDirection>(Board.AllShiftDirections.Where(d => TestShiftDirection(board, d)));
			if(validDirs.Count > 0)
				return validDirs[_rand.Int32(0, validDirs.Count - 1)];
			else
				return null;
		}

		/// <summary>
		/// Returns the next move to make based on the state of the specified game, or null to make no move.
		/// </summary>
		public ShiftDirection? GetNextMove(FastBoard board, FastDeck deck, NextCardHint nextCardHint)
		{
			long movesEvaluated = 0;
			return GetNextMove(board, deck, nextCardHint, ref movesEvaluated);
		}

		#endregion
		#region Non-Public Methods

		/// <summary>
		/// Returns whether the specified ShiftDirection is valid for the specified game.
		/// </summary>
		private unsafe static bool TestShiftDirection(FastBoard board, ShiftDirection dir)
		{
			IntVector2D* newCardCells = stackalloc IntVector2D[4];
			return board.ShiftInPlace(dir, newCardCells);
		}

		#endregion
	}
}