namespace Threesus.Bots
{
	/// <summary>
	/// A callback that evaluates the quality of a board into a single value.
	/// </summary>
	public delegate float BoardQualityEvaluator(FastBoard board);

	/// <summary>
	/// Contains modular methods that are compatible with the BoardQualityEvaluator delegate.
	/// </summary>
	public static class BoardQualityEvaluators
	{
		/// <summary>
		/// A BoardQualityEvaluator that always returns 0.
		/// </summary>
		public static float Zero(FastBoard board)
		{
			return 0;
		}

		/// <summary>
		/// A BoardQualityEvaluator that uses the Board's total score as its quality.
		/// </summary>
		public static float TotalScore(FastBoard board)
		{
			return board.GetTotalScore();
		}

		/// <summary>
		/// A BoardQualityEvaluator that uses the Board's total number of empty spaces as its quality.
		/// </summary>
		public static float EmptySpaces(FastBoard board)
		{
			int total = 0;
			for(int x = 0; x < FastBoard.Width; x++)
			{
				for(int y = 0; y < FastBoard.Height; y++)
				{
					if(board.GetCardIndex(x, y) == 0)
						total++;
				}
			}
			return total;
		}

		/// <summary>
		/// A BoardQualityEvaluator that calculates how "open" the board is.
		/// </summary>
		public static float Openness(FastBoard board)
		{
			int total = 0;
			for(int x = 0; x < FastBoard.Width; x++)
			{
				for(int y = 0; y < FastBoard.Height; y++)
				{
					ulong cardIndex = board.GetCardIndex(x, y);
					if(cardIndex == 0)
					{
						// 2 points for an empty cell.
						total += 2;
					}
					else
					{
						ulong leftCardIndex = x > 0 ? board.GetCardIndex(x - 1, y) : 0;
						ulong rightCardIndex = x < FastBoard.Width - 1 ? board.GetCardIndex(x + 1, y) : 0;
						ulong upCardIndex = y > 0 ? board.GetCardIndex(x, y - 1) : 0;
						ulong downCardIndex = y < FastBoard.Height - 1 ? board.GetCardIndex(x, y + 1) : 0;

						// 1 point for each adjacent card we can merge with.
						if(leftCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, leftCardIndex))
							total += 1;
						if(rightCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, rightCardIndex))
							total += 1;
						if(upCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, upCardIndex))
							total += 1;
						if(downCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, downCardIndex))
							total += 1;

						// -1 point if we're trapped between higher-valued cards, either horizontally or vertically.
						if((x == 0 || (leftCardIndex >= 3 && cardIndex < leftCardIndex)) &&
						   (x == FastBoard.Width - 1 || (rightCardIndex >= 3 && cardIndex < rightCardIndex)))
						{
							total -= 1;
						}
						if((y == 0 || (upCardIndex >= 3 && cardIndex < upCardIndex)) &&
						   (y == FastBoard.Height - 1 || (downCardIndex >= 3 && cardIndex < downCardIndex)))
						{
							total -= 1;
						}

						// 1 point if next to at least one card twice our value.
						if(cardIndex >= 3)
						{
							if((leftCardIndex != 0 && leftCardIndex == cardIndex + 1) ||
							   (rightCardIndex != 0 && rightCardIndex == cardIndex + 1) ||
							   (upCardIndex != 0 && upCardIndex == cardIndex + 1) ||
							   (downCardIndex != 0 && downCardIndex == cardIndex + 1))
							{
								total += 1;
							}
						}
					}
				}
			}
			return total;
		}
	}
}