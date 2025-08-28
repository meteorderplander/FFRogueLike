using System;
using System.Collections.Generic;
using System.Linq;
using FFRogue.Map;
using FFRogue.Entities;
using FFRogue.Jobs;
using FFRogue.Combat;

namespace FFRogue
{
    public class Game
    {
        public int Width { get; set; } = 60;
        public int Height { get; set; } = 25;
        public DungeonMap Dungeon { get; set; }
        public Player Player { get; set; }
        public List<Monster> Monsters { get; set; } = new();
        private readonly Random _rng = new();
        public int Turn { get; private set; } = 1;
        public int Floor { get; private set; } = 1;

        // Message system
        private readonly List<string> _messages = new();
        private const int MaxMessages = 4;

        // Fog of war - track explored tiles
        private bool[,] _explored;

        public Game() { }
        public Game(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void InitializeConsole()
        {
            try
            {
                // Calculate exact dimensions needed
                int requiredWidth = Width + 2;
                int requiredHeight = MaxMessages + 1 + Height + 1 + 6 + 1; // messages + gap + map + gap + stats + controls

                // Set buffer size to EXACTLY what we need (no extra)
                Console.SetBufferSize(requiredWidth, requiredHeight);

                // Set window size to match buffer (prevents scrolling)
                int windowWidth = Math.Min(requiredWidth, Console.LargestWindowWidth);
                int windowHeight = Math.Min(requiredHeight, Console.LargestWindowHeight);
                Console.SetWindowSize(windowWidth, windowHeight);

                Console.CursorVisible = false;
                Console.Title = "Final Fantasy Roguelike";
            }
            catch (Exception ex)
            {
                // If console sizing fails completely, try basic setup
                try
                {
                    Console.CursorVisible = false;
                    Console.Clear();
                }
                catch { }
            }
        }

        public void ShowTitle()
        {
            Console.Clear();
            string banner = @"Final Fantasy Roguelike v 0.0.1";
            Console.WriteLine("========================================");
            Console.WriteLine(banner);
            Console.WriteLine("Created by Meteor the Derplander");
            Console.WriteLine("========================================");
            Console.WriteLine("A concept in progress");
            Console.WriteLine("--------");
            Console.WriteLine("Hear...Feel..Think! Hydaelyn has called you to be a Warrior of Light!");
            Console.WriteLine("Descend, grow stronger, and vanquish the darkness!");
            Console.WriteLine();
            Console.WriteLine("----[IMPORTANT]----");
            Console.WriteLine("Maximize your console screen at this moment to see everything properly.");
            Console.WriteLine();
            Console.WriteLine("----[CONTROLS]----");
            Console.WriteLine("Arrow Keys or H/J/K/L to move  •  '.' rest  •  '<' up  •  '>' down  •  'Q' quit");
            Console.WriteLine();
        }

        public string AskName()
        {
            Console.Write("Enter your name: ");
            string n = Console.ReadLine() ?? "";
            return string.IsNullOrWhiteSpace(n) ? "Hero" : n.Trim();
        }

        public Job AskJob()
        {
            Console.Clear();
            Console.WriteLine("Choose your job:");
            var all = JobInfo.All().OrderBy(j => j.Role).ToList();

            for (int i = 0; i < all.Count; i++)
                Console.WriteLine($"{i + 1,2}. {all[i].DisplayName}  [{all[i].Role}]");
            while (true)
            {
                Console.Write("Number: ");
                if (int.TryParse(Console.ReadLine() ?? "", out int idx) && idx >= 1 && idx <= all.Count)
                    return all[idx - 1].Job;
                Console.WriteLine("Invalid choice.");
            }
        }

        public void InitializePlayer(string name, Job job)
        {
            Player = new Player(name, job);
            Player.InitializeStats(job);
        }

        public void Run()
        {
            InitializeConsole();
            GenerateFloor(startAtUp: true);

            while (Player.IsAlive)
            {
                UpdateExploration();
                Render();
                bool acted = HandleInput();
                if (!acted) continue;

                MonstersAct();

                if (Turn % 2 == 0) Player.RecoverHP();

                Turn++;
            }

            Console.Clear();
            Console.WriteLine("Game Over!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private void GenerateFloor(bool startAtUp)
        {
            Dungeon = new DungeonMap(Width, Height);
            Dungeon.Generate();

            // Initialize exploration map
            _explored = new bool[Width, Height];

            Monsters.Clear();
            _messages.Clear();

            FFRogue.Map.Point startPoint;
            if (startAtUp && Dungeon.UpStairs.HasValue)
                startPoint = Dungeon.UpStairs.Value;
            else if (!startAtUp && Dungeon.DownStairs.HasValue)
                startPoint = Dungeon.DownStairs.Value;
            else
                startPoint = Dungeon.GetCenterRoom();

            startPoint.Deconstruct(out int playerStartX, out int playerStartY);
            Player.X = playerStartX;
            Player.Y = playerStartY;

            int count = 6 + Floor / 2;
            for (int i = 0; i < count; i++)
            {
                var m = Monster.RandomMonsterAtLevel(Floor);
                var rc = Dungeon.RandomRoomCenter();
                rc.Deconstruct(out int monsterX, out int monsterY);
                m.X = monsterX;
                m.Y = monsterY;
                Monsters.Add(m);
            }

            if (Floor % 10 == 0)
            {
                var boss = Monster.CreateBossForFloor(Floor);
                var rc = Dungeon.RandomRoomCenter();
                rc.Deconstruct(out int bossX, out int bossY);
                boss.X = bossX;
                boss.Y = bossY;
                Monsters.Add(boss);
            }

            while (Monsters.Any(mm => mm.X == Player.X && mm.Y == Player.Y))
            {
                var rc = Dungeon.RandomRoomCenter();
                rc.Deconstruct(out int newPlayerX, out int newPlayerY);
                Player.X = newPlayerX;
                Player.Y = newPlayerY;
            }

            AddMessage($"Welcome to floor {Floor}!");
        }

        private void UpdateExploration()
        {
            int radius = 8;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int dx = x - Player.X, dy = y - Player.Y;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        _explored[x, y] = true;
                    }
                }
            }
        }

        private void Render()
        {
            // Move cursor to top-left and clear screen properly
            Console.SetCursorPosition(0, 0);

            // Build the entire screen in memory first, then output all at once
            var screenLines = new List<string>();

            // MESSAGES AT TOP (4 lines)
            for (int i = 0; i < MaxMessages; i++)
            {
                if (i < _messages.Count)
                    screenLines.Add(_messages[i].PadRight(Width));
                else
                    screenLines.Add("".PadRight(Width));
            }
            screenLines.Add(""); // Empty line after messages

            // MAP IN MIDDLE
            int radius = 8;
            for (int y = 0; y < Height; y++)
            {
                var mapLine = new System.Text.StringBuilder();
                for (int x = 0; x < Width; x++)
                {
                    int dx = x - Player.X, dy = y - Player.Y;
                    bool currentlyVisible = dx * dx + dy * dy <= radius * radius;
                    bool wasExplored = _explored[x, y];

                    if (!wasExplored)
                    {
                        mapLine.Append(' '); // Unexplored = black
                        continue;
                    }

                    char ch = Dungeon.GetGlyph(x, y);

                    if (currentlyVisible)
                    {
                        // BRIGHT - currently visible
                        var mob = Monsters.FirstOrDefault(m => m.IsAlive && m.X == x && m.Y == y);
                        if (mob != null) ch = mob.Glyph;
                        if (x == Player.X && y == Player.Y) ch = Player.Glyph;

                        mapLine.Append(ch);
                    }
                    else
                    {
                        // FADED - explored but not currently visible
                        if (ch == '#') mapLine.Append('▓'); // Faded walls
                        else if (ch == '.') mapLine.Append('·'); // Faded floors
                        else if (ch == '<') mapLine.Append('◀'); // Faded up stairs
                        else if (ch == '>') mapLine.Append('▶'); // Faded down stairs
                        else mapLine.Append('░'); // Other faded stuff
                    }
                }
                screenLines.Add(mapLine.ToString());
            }

            screenLines.Add(""); // Empty line after map

            // CHARACTER INFO AT BOTTOM
            string nameJob = $"{Player.Name} the {JobInfo.Get(Player.Job).DisplayName}";
            screenLines.Add(nameJob.PadRight(Width));

            int hpBarLength = 20;
            int hpFilled = Math.Clamp((int)Math.Round(Player.CurrentHP / (float)Player.MaxHP * hpBarLength), 0, hpBarLength);
            int mpBarLength = 20;
            int mpFilled = Player.MaxMP > 0 ? Math.Clamp((int)Math.Round(Player.CurrentMP / (float)Player.MaxMP * mpBarLength), 0, mpBarLength) : 0;
            screenLines.Add($"HP: [{new string('█', hpFilled)}{new string('-', hpBarLength - hpFilled)}] {Player.CurrentHP}/{Player.MaxHP}");
            screenLines.Add($"MP: [{new string('█', mpFilled)}{new string('-', mpBarLength - mpFilled)}] {Player.CurrentMP}/{Player.MaxMP}");

            var stats = Player.Stats;
            screenLines.Add($"ATK {Player.Attack}  DEF {Player.Defense}  STR {stats.STR}  DEX {stats.DEX}  INT {stats.INT}  MND {stats.MND}  VIT {stats.VIT}");
            screenLines.Add($"Turn {Turn}   LEVEL {Player.Level}  XP {Player.XP}   Floor {Floor}");

            screenLines.Add("Arrows/HJKL move • '.' rest • '<' up • '>' down • 'Q' quit");

            // Output everything at once
            Console.Clear();
            foreach (var line in screenLines)
            {
                Console.WriteLine(line);
            }
        }

        private void AddMessage(string message)
        {
            _messages.Insert(0, message);
            if (_messages.Count > MaxMessages)
                _messages.RemoveAt(MaxMessages);
        }

        private bool HandleInput()
        {
            var info = Console.ReadKey(true);
            var key = info.Key;
            char keyChar = info.KeyChar;
            int dx = 0, dy = 0;
            bool acted = false;

            if (key == ConsoleKey.LeftArrow || char.ToLower(keyChar) == 'h') dx = -1;
            if (key == ConsoleKey.RightArrow || char.ToLower(keyChar) == 'l') dx = 1;
            if (key == ConsoleKey.UpArrow || char.ToLower(keyChar) == 'k') dy = -1;
            if (key == ConsoleKey.DownArrow || char.ToLower(keyChar) == 'j') dy = 1;

            if (char.ToLower(keyChar) == 'q') Environment.Exit(0);
            if (keyChar == '.') { acted = true; AddMessage($"{Player.Name} rests."); }

            if (keyChar == '<' && Dungeon.HasUpStairs(Player.X, Player.Y) && Floor > 1)
            {
                Floor--; GenerateFloor(startAtUp: false); acted = true; return acted;
            }
            if (keyChar == '>' && Dungeon.HasDownStairs(Player.X, Player.Y))
            {
                Floor++; GenerateFloor(startAtUp: true); acted = true; return acted;
            }

            if (dx != 0 || dy != 0)
            {
                int nx = Player.X + dx, ny = Player.Y + dy;
                if (!Dungeon.InBounds(nx, ny)) return false;

                var target = Monsters.FirstOrDefault(m => m.IsAlive && m.X == nx && m.Y == ny);
                if (target != null)
                {
                    string result = CombatSystem.Attack(Player, target);
                    AddMessage(result);
                    if (!target.IsAlive)
                    {
                        int xpGain = target.IsBoss ? 50 + 5 * Floor : 10 + 2 * Floor;
                        Player.GainXP(xpGain);
                        AddMessage($"{Player.Name} gains {xpGain} XP!");
                    }
                    acted = true;
                }
                else if (Dungeon.IsWalkable(nx, ny))
                {
                    Player.X = nx; Player.Y = ny; acted = true;
                }
            }

            return acted;
        }

        private void MonstersAct()
        {
            foreach (var m in Monsters.ToList())
            {
                if (!m.IsAlive) continue;
                var step = m.GetNextStepToward(Player.X, Player.Y, Dungeon);
                int nx = m.X + step.dx, ny = m.Y + step.dy;
                if (nx == Player.X && ny == Player.Y)
                {
                    string res = CombatSystem.Attack(m, Player);
                    AddMessage(res);
                }
                else if (Dungeon.IsWalkable(nx, ny) && Monsters.All(om => om == m || !om.IsAlive || om.X != nx || om.Y != ny))
                {
                    m.X = nx; m.Y = ny;
                }
            }
        }
    }
}