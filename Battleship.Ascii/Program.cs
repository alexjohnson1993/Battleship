﻿
namespace Battleship.Ascii
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Battleship.Ascii.TelemetryClient;
	using Battleship.GameController;
	using Battleship.GameController.Contracts;

	public class Program
	{
		private static Board myBoard;
		private static Board enemyBoard;
		private static List<Ship> myFleet;

		private static List<Ship> enemyFleet;

		private static ITelemetryClient telemetryClient;
		public enum Status
		{
			Miss = 1,
			Hit = 2,
		}

		static void Main()
		{
			telemetryClient = new ApplicationInsightsTelemetryClient();
			telemetryClient.TrackEvent("ApplicationStarted", new Dictionary<string, string> { { "Technology", ".NET" } });

			try
			{
				Console.Title = "Battleship";
				Console.BackgroundColor = ConsoleColor.Black;
				Console.Clear();

				Console.WriteLine("                                     |__");
				Console.WriteLine(@"                                     |\/");
				Console.WriteLine("                                     ---");
				Console.WriteLine("                                     / | [");
				Console.WriteLine("                              !      | |||");
				Console.WriteLine("                            _/|     _/|-++'");
				Console.WriteLine("                        +  +--|    |--|--|_ |-");
				Console.WriteLine(@"                     { /|__|  |/\__|  |--- |||__/");
				Console.WriteLine(@"                    +---------------___[}-_===_.'____                 /\");
				Console.WriteLine(@"                ____`-' ||___-{]_| _[}-  |     |_[___\==--            \/   _");
				Console.WriteLine(@" __..._____--==/___]_|__|_____________________________[___\==--____,------' .7");
				Console.WriteLine(@"|                        Welcome to Battleship                         BB-61/");
				Console.WriteLine(@" \_________________________________________________________________________|");
				Console.WriteLine();

				InitializeGame();

				StartGame();
			}
			catch (Exception e)
			{
				Console.WriteLine("A serious problem occured. The application cannot continue and will be closed.");
				telemetryClient.TrackException(e);
				Console.WriteLine("");
				Console.WriteLine("Error details:");
				throw new Exception("Fatal error", e);
			}

		}

		private static void StartGame()
		{
			Console.Clear();
			Console.WriteLine("                  __");
			Console.WriteLine(@"                 /  \");
			Console.WriteLine("           .-.  |    |");
			Console.WriteLine(@"   *    _.-'  \  \__/");
			Console.WriteLine(@"    \.-'       \");
			Console.WriteLine("   /          _/");
			Console.WriteLine(@"  |      _  /""");
			Console.WriteLine(@"  |     /_\'");
			Console.WriteLine(@"   \    \_/");
			Console.WriteLine(@"    """"""""");
			GameController gc = new GameController();

			do
			{
				Console.WriteLine();
				Console.WriteLine("Player, it's your turn");
				Console.WriteLine("Enter coordinates for your shot :");
				var position = ParsePosition(Console.ReadLine());
				var enemyStatuses = GameController.CheckIsHit(enemyFleet, position);

				var isHit = enemyStatuses.Item1;
				var isSunk = enemyStatuses.Item2;
				var status = isHit ? (int)Status.Hit : (int)Status.Miss;
				gc.RecordMove(position, status);

				telemetryClient.TrackEvent("Player_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
				if (isHit)
				{
					Explode();
				}
				if (isSunk)
				{
					// TODO: add actual ship
					Console.WriteLine($"Ship was sunk");
				}

				Console.WriteLine(isHit ? "Yeah ! Nice hit !" : "Miss");

				position = GetRandomPosition();
				var myStatuses = GameController.CheckIsHit(myFleet, position);

				isHit = myStatuses.Item1;
				isSunk = myStatuses.Item2; 
				
				telemetryClient.TrackEvent("Computer_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
				Console.WriteLine();
				Console.WriteLine("Computer shot in {0}{1} and {2}", position.Column, position.Row, isHit ? "has hit your ship !" : "missed");
				if (isHit)
				{
					Explode();
				}

				if (isSunk)
				{
					// TODO: add actual ship
					Console.WriteLine($"Ship was sunk");
				}
			}
			while (true);
		}
		private static void Explode()
		{
			Console.Beep();

			Console.WriteLine(@"                \         .  ./");
			Console.WriteLine(@"              \      .:"";'.:..""   /");
			Console.WriteLine(@"                  (M^^.^~~:.'"").");
			Console.WriteLine(@"            -   (/  .    . . \ \)  -");
			Console.WriteLine(@"               ((| :. ~ ^  :. .|))");
			Console.WriteLine(@"            -   (\- |  \ /  |  /)  -");
			Console.WriteLine(@"                 -\  \     /  /-");
			Console.WriteLine(@"                   \  \   /  /");
		}
		public static Position ParsePosition(string input)
		{
			var letter = (Letters)Enum.Parse(typeof(Letters), input.ToUpper().Substring(0, 1));
			var number = int.Parse(input.Substring(1, 1));
			return new Position(letter, number);
		}

		private static Position GetRandomPosition()
		{
			int rows = 8;
			int lines = 8;
			var random = new Random();
			var letter = (Letters)random.Next(lines);
			var number = random.Next(rows);
			var position = new Position(letter, number);
			return position;
		}

		private static void InitializeGame()
		{
			InitializeBoard();

			InitializeMyFleet();

			InitializeEnemyFleet();
		}

		private static void InitializeBoard()
		{
			Console.WriteLine("Please enter a height and width for the board :");

			int height = 0;
			while (height == 0 || height > 26)
			{
				Console.WriteLine("Please enter a Height between 1-26: ");
				if (!int.TryParse(Console.ReadLine(), out height))
				{
					Console.WriteLine("Please enter an Integer");
				}
			}
			int width = 0;
			while (width == 0 || width > 26)
			{
				Console.WriteLine("Please enter a Width between 1-26: ");
				if (!int.TryParse(Console.ReadLine(), out width))
				{
					Console.WriteLine("Please enter an Integer");
				}
			}
			// minimum volume of all ships is 17
			if (height * width >= 18)
			{
				myBoard = new Board(height, width);
				enemyBoard = new Board(height, width);
			}
			else
			{
				Console.WriteLine("Board not large enough to fit all ships. Minimum size must include at least 18 spots");
				InitializeBoard();
			}
		}
		private static void InitializeMyFleet()
		{
			myFleet = GameController.InitializeShips().ToList();

			Console.WriteLine(String.Format("Please position your fleet (Game board size is from A to {0} and 1 to {1}) :", (Letters)myBoard.Height, myBoard.Width));

			foreach (var ship in myFleet)
			{
				Console.WriteLine();
				Console.WriteLine("Please enter the positions for the {0} (size: {1})", ship.Name, ship.Size);
				for (var i = 1; i <= ship.Size; i++)
				{
					Console.WriteLine("Enter position {0} of {1} (i.e A3):", i, ship.Size);
					var position = Console.ReadLine();

					// TODO: invalid position check
					ship.AddPositionAndHealth(position);
					telemetryClient.TrackEvent("Player_PlaceShipPosition", new Dictionary<string, string>() { { "Position", position }, { "Ship", ship.Name }, { "PositionInShip", i.ToString() } });
				}
			}
		}

		private static void InitializeEnemyFleet()
		{
			enemyFleet = GameController.InitializeShips().ToList();

			enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 4 });
			enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 5 });
			enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 6 });
			enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 7 });
			enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 8 });

			enemyFleet[0].Health.Add(new Position { Column = Letters.B, Row = 4 });
			enemyFleet[0].Health.Add(new Position { Column = Letters.B, Row = 5 });
			enemyFleet[0].Health.Add(new Position { Column = Letters.B, Row = 6 });
			enemyFleet[0].Health.Add(new Position { Column = Letters.B, Row = 7 });
			enemyFleet[0].Health.Add(new Position { Column = Letters.B, Row = 8 });

			enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 6 });
			enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 7 });
			enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 8 });
			enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 9 });

			enemyFleet[1].Health.Add(new Position { Column = Letters.E, Row = 6 });
			enemyFleet[1].Health.Add(new Position { Column = Letters.E, Row = 7 });
			enemyFleet[1].Health.Add(new Position { Column = Letters.E, Row = 8 });
			enemyFleet[1].Health.Add(new Position { Column = Letters.E, Row = 9 });

			enemyFleet[2].Positions.Add(new Position { Column = Letters.A, Row = 3 });
			enemyFleet[2].Positions.Add(new Position { Column = Letters.B, Row = 3 });
			enemyFleet[2].Positions.Add(new Position { Column = Letters.C, Row = 3 });

			enemyFleet[2].Health.Add(new Position { Column = Letters.A, Row = 3 });
			enemyFleet[2].Health.Add(new Position { Column = Letters.B, Row = 3 });
			enemyFleet[2].Health.Add(new Position { Column = Letters.C, Row = 3 });


			enemyFleet[3].Positions.Add(new Position { Column = Letters.F, Row = 8 });
			enemyFleet[3].Positions.Add(new Position { Column = Letters.G, Row = 8 });
			enemyFleet[3].Positions.Add(new Position { Column = Letters.H, Row = 8 });

			enemyFleet[3].Health.Add(new Position { Column = Letters.F, Row = 8 });
			enemyFleet[3].Health.Add(new Position { Column = Letters.G, Row = 8 });
			enemyFleet[3].Health.Add(new Position { Column = Letters.H, Row = 8 });

			enemyFleet[4].Positions.Add(new Position { Column = Letters.C, Row = 5 });
			enemyFleet[4].Positions.Add(new Position { Column = Letters.C, Row = 6 });

			enemyFleet[4].Health.Add(new Position { Column = Letters.C, Row = 5 });
			enemyFleet[4].Health.Add(new Position { Column = Letters.C, Row = 6 });
		}
	}
}
