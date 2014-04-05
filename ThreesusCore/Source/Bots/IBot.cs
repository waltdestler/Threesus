using Threesus.CoreGame;

namespace Threesus.Bots
{
	/// <summary>
	/// The base interface for an AI bot that can play a game of Threes.
	/// </summary>
	public interface IBot
	{
		/// <summary>
		/// Returns the next move to make based on the state of the specified game, or null to make no move.
		/// </summary>
		ShiftDirection? GetNextMove(FastBoard board, FastDeck deck, NextCardHint nextCardHint, ref long movesEvaluated);

		/// <summary>
		/// Returns the next move to make based on the state of the specified game, or null to make no move.
		/// </summary>
		ShiftDirection? GetNextMove(FastBoard board, FastDeck deck, NextCardHint nextCardHint);
	}
}