Threesus
========

Threesus is an A.I. program that knows how to play [Threes](http://asherv.com/threes/)! The core A.I. source code was originally created by [Walt Destler](https://twitter.com/waltdestler). [Matthew Wegner](https://twitter.com/mwegner) used it to create an actual Threes-playing robot and [streamed it live](http://www.twitch.tv/teamcolorblind/b/517126107) on Twitch.

This source code project on Github contains only the core A.I. source code.

If you want a high-level overview of how the Threesus A.I. works, then go read [Walt's blog post](http://blog.waltdestler.com/2014/04/threesus.html).

## Getting Started ##
1. If you don't already have it, download Microsoft Visual Studio 2013 (the free [Express for Windows Desktop](http://www.visualstudio.com/downloads/download-visual-studio-vs) version should work fine.) Unfortunately, Visual Studio only runs on Windows, but if you're on Mac, then it probably wouldn't be too hard to get the code working using [Xamarin Studio](https://xamarin.com/).
2. Download the source code using the "Download ZIP" button on the right of this page, or clone the Git repository.
3. Open the Threesus.sln file in Visual Studio.
4. In the Solution Explorer in Visual Studio, right-click on the ThreesusAssistant project and select "Set as StartUp Project".
5. Click the green play button on the toolbar to start ThreesusAssistant.
6. Launch a game of Threes on your phone or tablet.
7. Follow the instructions from ThreesusAssistant to play through Threes with the A.I.

## Source Code Overview ##
The Threesus.sln solution contains three sub-projects:

- **ThreesusCore** contains most of the code for Threesus, including the game simulation code and the A.I. thinking code.
- **ThreesusAssistant** is a command-line program that will help you play through an actual game of Threes on a phone or tablet.
- **ThreesusTest** is a program that plays through a hundred game simulations of Threes as fast as possible and reports statistical results.

The ThreesusCore project contains two sub-folders containing source code:

- **CoreGame** contains source code that re-implements the game of Threes in C#. As far as I know it is an exact replica of the game of Threes.
- **Bots** contains the source code for the A.I. thinking logic.

Some important files in the Bots folder are:

- **BoardQualityEvaluators.cs** contains various functions to evaluate the quality of possible Threes boards. If you want to experiment with what the A.I. looks for in a good board, then look at this file.
- **StandardBotFramework.cs** contains the code that knows how to "look ahead" into the future and examine the effectiveness of different moves. It works in conjunction with one of the BoardQualityEvaluator functions.
- **IBot.cs** is the base interface that all Threes A.I. bots must implement. If you want to write your own bot that isn't based on StandardBotFramework, then you should implement this interface.

Near the top of the ThreesusAssistant.cs and ThreesusTest.cs files are a line of code that looks like this:
    
		private static readonly IBot _bot = new StandardBotFramework(6, 3, BoardQualityEvaluators.Openness);

This line of code creates the actual A.I. bot. In this case, the A.I. bot being created is a StandardBotFramework-type bot that looks 6 moves ahead into the future, card-counts the deck for the first 3 of those moves, and uses the "Openness" board quality evaluator. You may modify this line to configure your own Threes bot however you want. Note that increasing either the 6 or the 3 will exponentially increase the time it takes the A.I. to consider a move.

## A note about the license ##
Threesus is intended to be a community-driven project in which we all work together to create the best Threes A.I. possible. To that end, you are welcome to download and modify the source code, but if you make a modification and distribute it, then you must also open-source your own modifications under a GPLv2-compatible license. 

