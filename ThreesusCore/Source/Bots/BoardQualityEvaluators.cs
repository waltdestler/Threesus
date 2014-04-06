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

		/// <summary>
		/// A BoardQualityEvaluator that calculates how "open" the board is.
		/// </summary>
		public static float OpennessMatthew(FastBoard board)
		{
			var maxIndex = board.GetMaxCardIndex();

			int total = 0;
			for(int x = 0; x < FastBoard.Width; x++)
			{
				for(int y = 0; y < FastBoard.Height; y++)
				{
					ulong cardIndex = board.GetCardIndex(x, y);
					if(cardIndex == 0)
					{
						// 2 points for an empty cell.
						total += 3;
					}
					else
					{
						ulong leftCardIndex = x > 0 ? board.GetCardIndex(x - 1, y) : 0;
						ulong rightCardIndex = x < FastBoard.Width - 1 ? board.GetCardIndex(x + 1, y) : 0;
						ulong upCardIndex = y > 0 ? board.GetCardIndex(x, y - 1) : 0;
						ulong downCardIndex = y < FastBoard.Height - 1 ? board.GetCardIndex(x, y + 1) : 0;

						// for each adjacent card we can merge with.
						if(leftCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, leftCardIndex))
							total += 2;
						if(rightCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, rightCardIndex))
							total += 2;
						if(upCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, upCardIndex))
							total += 2;
						if(downCardIndex != 0 && FastBoard.CanCardsMerge(cardIndex, downCardIndex))
							total += 2;

						// negative if we're trapped between higher-valued cards, either horizontally or vertically.
						if((x == 0 || (leftCardIndex >= 3 && cardIndex < leftCardIndex)) &&
						   (x == FastBoard.Width - 1 || (rightCardIndex >= 3 && cardIndex < rightCardIndex)))
						{
							total -= 5;
						}
						if((y == 0 || (upCardIndex >= 3 && cardIndex < upCardIndex)) &&
						   (y == FastBoard.Height - 1 || (downCardIndex >= 3 && cardIndex < downCardIndex)))
						{
							total -= 5;
						}

						// point if next to at least one card twice our value.
						if(cardIndex >= 3)
						{
							if((leftCardIndex != 0 && leftCardIndex == cardIndex + 1) ||
							   (rightCardIndex != 0 && rightCardIndex == cardIndex + 1) ||
							   (upCardIndex != 0 && upCardIndex == cardIndex + 1) ||
							   (downCardIndex != 0 && downCardIndex == cardIndex + 1))
							{
								total += 2;
							}
						}

						if(maxIndex > 4)
						{

							// for each wall we're touching if we're the biggest card
							if(cardIndex == maxIndex)
							{
								if(x == 0 || x == 3)
								{
									total += 3;
								}

								if(y == 0 || y == 3)
								{
									total += 3;
								}
							}

							// for sticking next to the biggest piece
							if(cardIndex == maxIndex - 1)
							{
								int testX, testY;
								if(NeighborsWith(board, x, y, maxIndex, out testX, out testY))
								{
									total += 1;

									// and a bonus if we're also along a wall
									if(x == 0 || x == 3)
									{
										total += 1;
									}

									if(y == 0 || y == 3)
									{
										total += 1;
									}
								}
							}

							// if we're two below
							if(cardIndex == maxIndex - 2)
							{
								// and we're neighbors with a 1-below
								int testX, testY;
								if(NeighborsWith(board, x, y, maxIndex - 1, out testX, out testY))
								{
									// who is also neighbors with the max
									if(NeighborsWith(board, testX, testY, maxIndex, out testX, out testY))
									{
										total += 1;
									}
								}
							}
						}
					}
				}
			}
			return total;
		}

		/// <summary>
		/// Is a coordinate neighbors with a specific index?
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool NeighborsWith(FastBoard board, int x, int y, ulong index, out int testX, out int testY)
		{
			ulong leftCardIndex = x > 0 ? board.GetCardIndex(x - 1, y) : 0;
			ulong rightCardIndex = x < FastBoard.Width - 1 ? board.GetCardIndex(x + 1, y) : 0;
			ulong upCardIndex = y > 0 ? board.GetCardIndex(x, y - 1) : 0;
			ulong downCardIndex = y < FastBoard.Height - 1 ? board.GetCardIndex(x, y + 1) : 0;

			if(leftCardIndex == index)
			{
				testX = x - 1;
				testY = y;
				return true;
			}

			if(rightCardIndex == index)
			{
				testX = x + 1;
				testY = y;
				return true;
			}

			if(upCardIndex == index)
			{
				testX = x;
				testY = y - 1;
				return true;
			}

			if(downCardIndex == index)
			{
				testX = x;
				testY = y + 1;
				return true;
			}

			testX = -1;
			testY = -1;

			return false;

		}
	}
}