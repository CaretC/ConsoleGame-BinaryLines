using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace BinaryLines
{
    class Program
    {
        const ConsoleColor HERO_COLOR = ConsoleColor.Magenta;
        const ConsoleColor ENEMY_COLOR = ConsoleColor.Red;
        const ConsoleColor BACKGROUND_COLOR = ConsoleColor.Black;
        const ConsoleColor GOAL_LOCATION_COLOR = ConsoleColor.Yellow;
        const ConsoleColor BONUS_LOCATION_COLOR = ConsoleColor.DarkBlue;

        public static Coordinate Hero { get; set; }
        public static Coordinate GoalLocation { get; set; }
        public static Coordinate BonusLocation { get; set; }
        public static Coordinate Enemy1 { get; set; }
        public static Coordinate Enemy2 { get; set; }
        public static Coordinate Enemy3 { get; set; }
        public static Random goalCoordinatesGenerator = new Random();
        public static Random EnemyHeightGenerator = new Random();
        public static Random goalColorSelection = new Random();
        public static bool TrailsEnabled { get; set; }
        public static bool Enemy1Enabled { get; set; }
        public static bool Enemy2Enabled { get; set; }
        public static bool Enemy3Enabled { get; set; }
        public static bool BonusActive { get; set; }
        public static int Score { get; set; }
        public static int Bonus { get; set; }
        public static int BonusCount { get; set; }
        public static int EnemyMoveModifier { get; set; }
        public static int Enemy1StartValue { get; set; }
        public static int Enemy2StartValue { get; set; }
        public static int Enemy3StartValue { get; set; }
        public static List<Coordinate> deathAreas = new List<Coordinate>();
        public static SoundPlayer gameTune = new SoundPlayer(@"Frantic-Gameplay.wav");
        public static SoundPlayer deathSound = new SoundPlayer(@"Death.wav");
        public static SoundPlayer endScreenTune = new SoundPlayer(@"EndScreen.wav");

        public static int HiScore = Properties.Settings.Default.HighScore;


        static void Main(string[] args)
        {
            PrintWelcomeScreen();
            Console.ReadKey(true);
            Console.Clear();

            while (true)
            {

                HiScore = Properties.Settings.Default.HighScore;
                InitGame();

                ConsoleKeyInfo userInput;
                while ((userInput = Console.ReadKey(true)).Key != ConsoleKey.Escape)
                {
                    SetBackgroundColour();
                    PrintScore();

                    if (Enemy1Enabled)
                    {
                        MoveEnemy1(EnemyMoveModifier);
                    }

                    if (Enemy2Enabled)
                    {
                        MoveEnemy2(EnemyMoveModifier);
                    }

                    if (Enemy3Enabled)
                    {
                        MoveEnemy3(EnemyMoveModifier);
                    }

                    switch (userInput.Key)
                    {
                        case ConsoleKey.UpArrow:
                            MoveHero(0, -1);
                            break;

                        case ConsoleKey.DownArrow:
                            MoveHero(0, 1);
                            break;

                        case ConsoleKey.LeftArrow:
                            MoveHero(-1, 0);
                            break;

                        case ConsoleKey.RightArrow:
                            MoveHero(1, 0);
                            break;

                        case ConsoleKey.Spacebar:
                            ClearDeathArea();
                            break;

                        case ConsoleKey.E:
                            RelocateGoal();
                            break;

                        default:
                            if (Enemy1Enabled) MoveEnemy1(EnemyMoveModifier);
                            if (Enemy2Enabled) MoveEnemy2(EnemyMoveModifier);
                            break;
                    }

                    if (IsVictory())
                    {
                        Console.Beep();
                        Score++;
                        BonusCount++;
                        SetGoalLocation();
                        PrintScore();
                    }

                    if (IsBonus())
                    {
                        Console.Beep(1500, 500);
                        Bonus++;
                        PrintScore();
                        BonusActive = false;
                    }

                    if (BonusCount == 5)
                    {
                        ClearBonus();
                        BonusCount = 0;
                        SetBonusLocation();
                        BonusActive = true;
                    }

                    if (IsDead())
                    {
                        //deathSound.Play();
                        //Console.BackgroundColor = ConsoleColor.Red;
                        //Console.Clear();

                        //System.Threading.Thread.Sleep(1000);
                        break;
                    }

                    if (Score == Enemy1StartValue)
                    {
                        Enemy1Enabled = true;
                    }

                    if (Score == Enemy2StartValue)
                    {
                        Enemy2Enabled = true;
                    }

                    if (Score == Enemy3StartValue)
                    {
                        Enemy3Enabled = true;
                    }

                    if (Score > Enemy1StartValue)
                    {
                        EnemyMoveModifier++;
                    }


                }

                deathSound.Play();
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Clear();

                System.Threading.Thread.Sleep(1000);

                PrintDeathMessage();

                System.Threading.Thread.Sleep(1000);

                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                }

            }
        }

        static void SetBackgroundColour()
        {
            if (TrailsEnabled == true)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.BackgroundColor = BACKGROUND_COLOR;
                Console.ForegroundColor = HERO_COLOR;
            }

            else
            {
                Console.BackgroundColor = BACKGROUND_COLOR;
                //Console.Clear();
            }


        }

        static void InitGame()
        {
            Console.Clear();
            SetBackgroundColour();
            Console.Clear();
            deathAreas.Clear();
            

            Console.SetWindowSize(120, 30);
            Console.CursorVisible = false;
            TrailsEnabled = false;
            Enemy1Enabled = false;
            Enemy2Enabled = false;
            Enemy3Enabled = false;
            BonusActive = true;
            EnemyMoveModifier = 1;
            Enemy1StartValue = 1;
            Enemy2StartValue = 5;
            Enemy3StartValue = 10;
            Score = 0;
            Bonus = 0;

            gameTune.PlayLooping();
            SetGoalLocation();
            SetBonusLocation();
            PrintScore();

            Hero = new Coordinate()
            {
                X = 0,
                Y = 1
            };

            Enemy1 = new Coordinate()
            {
                X = 0,
                Y = EnemyHeightGenerator.Next(1,Console.WindowHeight)
            };

            Enemy2 = new Coordinate()
            {
                X = EnemyHeightGenerator.Next(0, Console.WindowWidth),
                Y = 1
            };

            Enemy3 = new Coordinate()
            {
                X = 0,
                Y = EnemyHeightGenerator.Next(1, Console.WindowHeight)
            };
            
            MoveHero(0, 0);
        }

        static bool CanMove(Coordinate c)
        {
            if (c.X < 0 || c.X >= Console.WindowWidth)
                return false;

            if (c.Y < 1 || c.Y >= Console.WindowHeight)
                return false;

            return true;
        }

        static void RemoveHero()
        {
            Console.BackgroundColor = BACKGROUND_COLOR;
            Console.SetCursorPosition(Hero.X, Hero.Y);
            Console.Write(" ");
        }

        static void RemoveEnemy()
        {
            //if (Enemy.X == 0)
            //{
            //    Console.BackgroundColor = BACKGROUND_COLOR;
            //    Console.SetCursorPosition(Enemy.X, Enemy.Y);
            //    Console.Write(" ");
            //}

        }

        static void MoveHero(int x, int y)
        {
            Coordinate newHero = new Coordinate()
            {
                X = Hero.X + x,
                Y = Hero.Y + y
            };

            if (CanMove(newHero))
            {
                RemoveHero();

                Console.BackgroundColor = HERO_COLOR;
                Console.SetCursorPosition(newHero.X, newHero.Y);
                Console.Write(" ");

                Hero = newHero;
            }
        }

        static void MoveEnemy1(int x_move_modifier)
        {
            RemoveEnemy();
            int move = EnemyHeightGenerator.Next(1, 3);

            Coordinate newEnemy = new Coordinate();

            Console.BackgroundColor = ENEMY_COLOR;

            if (Enemy1.X + move <= Console.WindowWidth -1 )
            {
                newEnemy.X = Enemy1.X + move;
                newEnemy.Y = Enemy1.Y;
            }
            
            else
            {
                newEnemy.Y = EnemyHeightGenerator.Next(1,Console.WindowHeight);
                newEnemy.X = 0;
            }

            Console.SetCursorPosition(newEnemy.X, newEnemy.Y);
            Console.Write(" ");

            deathAreas.Add(newEnemy);

            Enemy1 = newEnemy;
        }

        static void MoveEnemy2(int y_move_modifier)
        {
            RemoveEnemy();
            int move = EnemyHeightGenerator.Next(1, 3);

            Coordinate newEnemy = new Coordinate();

            Console.BackgroundColor = ENEMY_COLOR;

            if (Enemy2.Y + move <= Console.WindowHeight - 1)
            {
                newEnemy.X = Enemy2.X;
                newEnemy.Y = Enemy2.Y + move;
            }

            else
            {
                newEnemy.Y = 1;
                newEnemy.X = EnemyHeightGenerator.Next(1, Console.WindowWidth);
            }

            Console.SetCursorPosition(newEnemy.X, newEnemy.Y);
            Console.Write(" ");

            deathAreas.Add(newEnemy);

            Enemy2 = newEnemy;
        }

        static void MoveEnemy3(int x_move_modifier)
        {
            RemoveEnemy();
            int move = EnemyHeightGenerator.Next(1, 3);

            Coordinate newEnemy = new Coordinate();

            Console.BackgroundColor = ENEMY_COLOR;

            if (Enemy3.X + move <= Console.WindowWidth - 1)
            {
                newEnemy.X = Enemy3.X + move;
                newEnemy.Y = Enemy3.Y;
            }

            else
            {
                newEnemy.Y = EnemyHeightGenerator.Next(1, Console.WindowHeight);
                newEnemy.X = 0;
            }

            Console.SetCursorPosition(newEnemy.X, newEnemy.Y);
            Console.Write(" ");

            deathAreas.Add(newEnemy);

            Enemy3 = newEnemy;
        }

        static void SetGoalLocation()
        {
            bool isValidLocation = false;

            while (!isValidLocation)
            {

                int x = goalCoordinatesGenerator.Next(0, Console.WindowWidth);
                int y = goalCoordinatesGenerator.Next(1, Console.WindowHeight);

                GoalLocation = new Coordinate()
                {
                    X = x,
                    Y = y
                };

                isValidLocation = true;

                foreach (Coordinate deathArea in deathAreas)
                {
                    if (GoalLocation.X == deathArea.X && GoalLocation.Y == deathArea.Y) isValidLocation = false;
                }

                if (isValidLocation)
                {
                    Console.BackgroundColor = GOAL_LOCATION_COLOR;
                    Console.SetCursorPosition(x, y);
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("$");
                    Console.ForegroundColor = HERO_COLOR;
                }
            }




        }

        static void SetBonusLocation()
        {
            int x = goalCoordinatesGenerator.Next(0, Console.WindowWidth);
            int y = goalCoordinatesGenerator.Next(1, Console.WindowHeight);

            Console.BackgroundColor = BONUS_LOCATION_COLOR;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = GOAL_LOCATION_COLOR;
            Console.Write("B");
            Console.ForegroundColor = HERO_COLOR;

            BonusLocation = new Coordinate()
            {
                X = x,
                Y = y
            };

        }

        static bool IsVictory()
        {
            bool victory;

            if (Hero.X == GoalLocation.X && Hero.Y == GoalLocation.Y)
            {
                victory = true;
            }

            else
            {
                victory = false;
            }

            return victory;
        }

        static bool IsBonus()
        {
            bool bonus;

            if (Hero.X == BonusLocation.X && Hero.Y == BonusLocation.Y && BonusActive == true) bonus = true;
            else bonus = false;

            return bonus;

        }

        static bool IsDead()
        {
            bool dead = false;

            foreach (Coordinate deathArea in deathAreas)
            {
                if (Hero.X == deathArea.X && Hero.Y == deathArea.Y) dead = true;
            }

            return dead;
        }
        
        static void ClearDeathArea()
        {

            if (Bonus >= 2)
            {
                Console.Beep(500, 500);

                Console.BackgroundColor = ConsoleColor.Black;

                Console.Clear();
                deathAreas.Clear();
                SetGoalLocation();
                Bonus -= 2;
                PrintScore();

                MoveHero(Hero.X, Hero.Y);
            }

            else Console.Beep(400, 900);


        }

        static void ClearBonus()
        {
            Console.BackgroundColor = BACKGROUND_COLOR;
            Console.SetCursorPosition(BonusLocation.X, BonusLocation.Y);
            Console.Write(" ");
        }

        static void RelocateGoal()
        {
            if (Bonus >= 1)
            {
                Console.Beep(900, 100);

                Console.SetCursorPosition(GoalLocation.X, GoalLocation.Y);
                Console.BackgroundColor = BACKGROUND_COLOR;
                Console.Write(" ");

                SetGoalLocation();
                Bonus--;
                PrintScore();
            }

            else Console.Beep(400, 900); 
        }

        static void PrintScore()
        {
            Console.ForegroundColor = HERO_COLOR;
            Console.BackgroundColor = ConsoleColor.Green;
            string score = string.Format("SCORE: {0} BONUS: {1} HIGHSCORE: {2}", Score, Bonus, HiScore);

            Console.SetCursorPosition(Console.WindowWidth - score.Length, 0);
            Console.Write(score);
        }

        static void PrintDeathMessage()
        {
            endScreenTune.PlayLooping();

            Console.SetCursorPosition (40, 3);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(@"

                          ▄▀▀▄ ▀▀▄  ▄▀▀▀▀▄   ▄▀▀▄ ▄▀▀▄      ▄▀▀█▄▄   ▄▀▀█▀▄   ▄▀▀█▄▄▄▄  ▄▀▀█▄▄  
                         █   ▀▄ ▄▀ █      █ █   █    █     █ ▄▀   █ █   █  █ ▐  ▄▀   ▐ █ ▄▀   █ 
                         ▐     █   █      █ ▐  █    █      ▐ █    █ ▐   █  ▐   █▄▄▄▄▄  ▐ █    █ 
                               █   ▀▄    ▄▀   █    █         █    █     █      █    ▌    █    █ 
                             ▄▀      ▀▀▀▀      ▀▄▄▄▄▀       ▄▀▄▄▄▄▀  ▄▀▀▀▀▀▄  ▄▀▄▄▄▄    ▄▀▄▄▄▄▀ 
                              █                             █     ▐  █       █ █    ▐   █     ▐  
                              ▐                             ▐        ▐       ▐ ▐        ▐        
");
            Console.SetCursorPosition(40, 18);
            Console.Write(String.Format("Your score was: {0} points", Score));

            if (Score > HiScore)
            {
                Console.SetCursorPosition(40, 20);
                Console.Write(string.Format("WELL DONE!!!! YOU GOT A NEW HIGHSCORE!!!"));
                Properties.Settings.Default.HighScore = Score;
                Properties.Settings.Default.Save();
            }

            else
            {
                Console.SetCursorPosition(40, 20);
                Console.Write(string.Format("Hi-Score: {0}", HiScore));
            }

            Console.SetCursorPosition(40, 22);
            Console.Write("Press ANY key to reply or Esc to exit game...");
        }

        static void PrintWelcomeScreen()
        {
            string gameName = "Binary Lines" ;

            Console.Write(@"

                           ____  _                          _      _                 
                          |  _ \(_)                        | |    (_)                
                          | |_) |_ _ __   __ _ _ __ _   _  | |     _ _ __   ___  ___ 
                          |  _ <| | '_ \ / _` | '__| | | | | |    | | '_ \ / _ \/ __|
                          | |_) | | | | | (_| | |  | |_| | | |____| | | | |  __/\__ \
                          |____/|_|_| |_|\__,_|_|   \__, | |______|_|_| |_|\___||___/
                                                     __/ |                           
                                                    |___/       ");


            Console.Write("\n");
            Console.Write("\n");

            Console.Write("Binary Lines is a simple game, all you have to do it collect the treasure [");
            Console.BackgroundColor = GOAL_LOCATION_COLOR;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("$");
            Console.BackgroundColor = BACKGROUND_COLOR;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]. While avoiding the ");
            Console.ForegroundColor = ENEMY_COLOR;
            Console.Write("RED");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" areas, hitting");
            Console.Write("\nthese will end that game. The game can play some nasty tricks on you, so pay attention that the treasure hasn't spawnned");
            Console.Write("right on top of that peskey ");
            Console.ForegroundColor = ENEMY_COLOR;
            Console.Write("RED");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(", picking this up will result in the game ending.");

            Console.Write("\n");
            Console.Write("\n");

            Console.Write("Don't panic though, there is help out there for you. Collect the Bonus Blocs [");
            Console.BackgroundColor = BONUS_LOCATION_COLOR;
            Console.ForegroundColor = GOAL_LOCATION_COLOR;
            Console.Write("B");
            Console.BackgroundColor = BACKGROUND_COLOR;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] collecting these will give you a chance" +
                "to use special skills to help get you out of any pickles that might arise.");

            Console.Write("\n");
            Console.Write("\n");

            Console.Write("CONTROLS:\n");

            Console.Write("Up:\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("UP ARROW\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Down:\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("DOWN ARROW\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Left:\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("LEFT ARROW\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Right:\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("RIGHT ARROW\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("\n");
            Console.Write("\n");

            Console.Write("SPECIAL SKILLS:\n");

            Console.Write("Relocate Treasure:\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("E\t\t");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("(Uses 1 Bonus Point)\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Clear all ");
            Console.ForegroundColor = ENEMY_COLOR;
            Console.Write("RED");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" area:\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("SPACEBAR\t");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("(Uses 2 Bonus Points)\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("\n");
            Console.WriteLine("Press ANY key to start game....");



        }

        public class Coordinate
        {
            public int X { get; set; } //Left is 0
            public int Y { get; set; } //Top is 0
        }
    }
}
