using System;
using System.Collections.Generic;
using System.Linq;
using Threesus.Bots;
using Threesus.CoreGame;

namespace Threesus
{
	/// <summary>
	/// Command-line test for testing Threes bots.
	/// </summary>
	static class ThreesusTest
	{
		private const int ITERATIONS = 100;

		private static readonly IRand _seedRand = new Rand();
		private static readonly IBot _bot = new StandardBotFramework(6, 3, BoardQualityEvaluators.OpennessMatthew);

		/// <summary>
		/// Main application entry point.
		/// </summary>
		private static void Main()
		{
			Console.WriteLine(_bot);

			List<GameResult> results = new List<GameResult>();
			DateTime startTime = DateTime.Now;
			for(int gameNum = 1; gameNum <= ITERATIONS; gameNum++)
			{
				ulong randSeed = (ulong)_seedRand.Int64();
				//Console.Write("Game #" + gameNum + " ...");
				Console.Write("Game #{0} (random seed is {1}) ...", gameNum, randSeed);
				GameResult result = RunGame(gameNum, randSeed);
				Console.WriteLine("\n\t{0} pts, {1} turns, {2:0} secs, {3:0} mv/s, high card: {4}",
					result.Score,
					result.TotalTurns,
					result.TotalTime.TotalSeconds,
					result.TotalMovesEvaluated / result.TotalTime.TotalSeconds,
					result.MaxCard);
				results.Add(result);
			}
			DateTime endTime = DateTime.Now;

			results.Sort((r1, r2) => r1.Score.CompareTo(r2.Score));
			GameResult lowScore = results[0];
			GameResult highScore = results[results.Count - 1];
			GameResult medianScore = results[results.Count / 2];

			Console.WriteLine("{0} games completed!", ITERATIONS);
			Console.WriteLine("Total time: {0}", endTime - startTime);
			Console.WriteLine("Low Score: {0}", lowScore.Score);
			Console.WriteLine("Median Score: {0}", medianScore.Score);
			Console.WriteLine("High Score: {0}", highScore.Score);

			float percentage;
			for(int cardVal = 3; (percentage = GetPercentWithCardValue(results, cardVal)) > 0; cardVal *= 2)
				Console.WriteLine("% of games with at least a {0}: {1:}%", cardVal, percentage);
		}

		/// <summary>
		/// Runs a single game of Threes with a bot and prints the results.
		/// </summary>
		private static GameResult RunGame(int gameNum, ulong randSeed)
		{
			Game game = new Game(new Rand(randSeed));

			DateTime startTime = DateTime.Now;
			ShiftDirection? nextMove;
			long totalMovesEvaluated = 0;
			while((nextMove = _bot.GetNextMove(new FastBoard(game.CurrentBoard), new FastDeck(game.CurrentDeck), game.NextCardHint, ref totalMovesEvaluated)) != null)
				game.Shift(nextMove.Value);
			DateTime endTime = DateTime.Now;

			return new GameResult(
				game.CurrentBoard.GetTotalScore(),
				game.CurrentBoard.GetMaxCardValue(),
				game.TotalTurns,
				endTime - startTime,
				totalMovesEvaluated);
		}

		/// <summary>
		/// Returns the 0-100 percentage of games that had a high card of at least the specified card value.
		/// </summary>
		private static float GetPercentWithCardValue(IList<GameResult> results, int cardValue)
		{
			int total = results.Count(r => r.MaxCard >= cardValue);
			return (float)total / results.Count * 100;
		}

		/// <summary>
		/// Stores the results of a single game run.
		/// </summary>
		private class GameResult
		{
			public int Score { get; private set; }
			public int MaxCard { get; private set; }
			public int TotalTurns { get; private set; }
			public TimeSpan TotalTime { get; private set; }
			public long TotalMovesEvaluated { get; private set; }

			public GameResult(int score, int maxCard, int totalTurns, TimeSpan totalTime, long totalMovesEvaluated)
			{
				Score = score;
				MaxCard = maxCard;
				TotalTurns = totalTurns;
				TotalTime = totalTime;
				TotalMovesEvaluated = totalMovesEvaluated;
			}
		}
	}
}
