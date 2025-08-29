using System;
using System.Collections.Generic;
using System.Linq;
using FFRogue.Map;
using FFRogue.Entities;
using FFRogue.Jobs;
using FFRogue.Combat;
using FFRogue.Abilities;

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
            string banner = @"Final Fantasy Roguelike v 0.0.2";
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
            Console.WriteLine("Arrow Keys or H/J/K/L to move  •  '.' rest  •  '>' down  •  'A' abilities  •  'Q' quit");
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
            GenerateFloor(startAtDown: true);

            while (Player.IsAlive)
            {
                UpdateExploration();
                Render();
                bool acted = HandleInput();
                if (!acted) continue;

                MonstersAct();

                if (Turn % 2 == 0) Player.RecoverHP();
                if (Turn % 5 == 0) Player.RecoverMP();

                // Update ability cooldowns each turn
                AbilitySystem.UpdateCooldowns();

                Turn++;
            }

            Console.Clear();
            Console.WriteLine("Game Over!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private bool IsBossFloor() => Floor % 10 == 0;

        private void GenerateFloor(bool startAtDown)
        {
            bool isBoss = IsBossFloor();

            if (isBoss)
            {
                // Generate single room boss floor
                GenerateBossFloor(startAtDown);
            }
            else
            {
                // Generate normal multi-room floor
                GenerateNormalFloor(startAtDown);
            }

            AddMessage($"Welcome to floor {Floor}!" + (isBoss ? " ** BOSS FLOOR **" : ""));
        }

        private void GenerateBossFloor(bool startAtDown)
        {
            Dungeon = new DungeonMap(Width, Height);

            // Create a single large room in the center
            int roomWidth = Math.Min(Width - 6, 20);
            int roomHeight = Math.Min(Height - 6, 15);
            int roomX = (Width - roomWidth) / 2;
            int roomY = (Height - roomHeight) / 2;

            // Fill with walls first
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Dungeon.SetTile(x, y, '#');

            // Carve out the boss room
            for (int x = roomX; x < roomX + roomWidth; x++)
                for (int y = roomY; y < roomY + roomHeight; y++)
                    Dungeon.SetTile(x, y, '.');

            // Initialize exploration map
            _explored = new bool[Width, Height];
            Monsters.Clear();
            _messages.Clear();

            // Place player at the entrance (top of room)
            Player.X = roomX + roomWidth / 2;
            Player.Y = roomY + 1;

            // Create and place the boss in the center of the room
            var boss = Monster.CreateBossForFloor(Floor);
            boss.X = roomX + roomWidth / 2;
            boss.Y = roomY + roomHeight / 2;
            Monsters.Add(boss);

            // No stairs initially - they'll appear when boss dies
            Dungeon.ClearStairs();
        }

        private void GenerateNormalFloor(bool startAtDown)
        {
            Dungeon = new DungeonMap(Width, Height);
            Dungeon.Generate();

            // Initialize exploration map
            _explored = new bool[Width, Height];

            Monsters.Clear();
            _messages.Clear();

            FFRogue.Map.Point startPoint;
            if (!startAtDown && Dungeon.DownStairs.HasValue)
                startPoint = Dungeon.DownStairs.Value;
            else
                startPoint = Dungeon.GetCenterRoom();

            startPoint.Deconstruct(out int playerStartX, out int playerStartY);
            Player.X = playerStartX;
            Player.Y = playerStartY;

            // Generate monsters based on current floor tier
            int count = 6 + Floor / 2;
            for (int i = 0; i < count; i++)
            {
                var m = Monster.RandomMonsterForFloorTier(Floor);
                var rc = Dungeon.RandomRoomCenter();
                rc.Deconstruct(out int monsterX, out int monsterY);
                m.X = monsterX;
                m.Y = monsterY;
                Monsters.Add(m);
            }

            // Make sure player doesn't start on a monster
            while (Monsters.Any(mm => mm.X == Player.X && mm.Y == Player.Y))
            {
                var rc = Dungeon.RandomRoomCenter();
                rc.Deconstruct(out int newPlayerX, out int newPlayerY);
                Player.X = newPlayerX;
                Player.Y = newPlayerY;
            }

            // Remove up stairs - only down progression
            if (Dungeon.UpStairs.HasValue)
            {
                var upStairs = Dungeon.UpStairs.Value;
                Dungeon.SetTile(upStairs.X, upStairs.Y, '.');
                Dungeon.RemoveUpStairs();
            }
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
                        else if (ch == '>') mapLine.Append('▶'); // Faded down stairs
                        else mapLine.Append('▒'); // Other faded stuff
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

            screenLines.Add("Arrows/HJKL move • '.' rest • '>' down • 'A' abilities • 'Q' quit");

            // Output everything at once
            Console.Clear();
            foreach (var line in screenLines)
            {
                Console.WriteLine(line);
            }
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
            if (_messages.Count > MaxMessages)
                _messages.RemoveAt(0);
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

            // Abilities menu
            if (char.ToLower(keyChar) == 'a')
            {
                ShowAbilitiesMenu();
                return false; // Don't consume a turn for opening menu
            }

            // Only down stairs allowed now, and on boss floors only after boss is defeated
            if (keyChar == '>' && Dungeon.HasDownStairs(Player.X, Player.Y))
            {
                if (IsBossFloor() && Monsters.Any(m => m.IsAlive && m.IsBoss))
                {
                    AddMessage("The stairs are blocked by the boss's power!");
                    return false;
                }
                Floor++;
                GenerateFloor(startAtDown: true);
                acted = true;
                return acted;
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
                        int xpGain = target.IsBoss ? 100 + 10 * Floor : 10 + 2 * Floor;
                        Player.GainXP(xpGain);
                        AddMessage($"{Player.Name} gains {xpGain} XP!");

                        // If boss dies on boss floor, create stairs
                        if (IsBossFloor() && target.IsBoss)
                        {
                            CreateBossFloorStairs();
                            AddMessage("The way forward opens!");
                        }
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

        private void CreateBossFloorStairs()
        {
            // Place down stairs near the player's position
            int stairsX = Player.X;
            int stairsY = Player.Y + 2; // Slightly south of player

            // Make sure it's in bounds and walkable
            if (Dungeon.InBounds(stairsX, stairsY) && Dungeon.GetGlyph(stairsX, stairsY) == '.')
            {
                Dungeon.SetTile(stairsX, stairsY, '>');
                Dungeon.SetDownStairs(new Point(stairsX, stairsY));
            }
            else
            {
                // Find the nearest floor tile
                for (int radius = 1; radius <= 5; radius++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int checkX = Player.X + dx;
                            int checkY = Player.Y + dy;
                            if (Dungeon.InBounds(checkX, checkY) && Dungeon.GetGlyph(checkX, checkY) == '.')
                            {
                                Dungeon.SetTile(checkX, checkY, '>');
                                Dungeon.SetDownStairs(new Point(checkX, checkY));
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void ShowAbilitiesMenu()
        {
            var abilities = AbilitySystem.GetAbilitiesForJob(Player.Job);
            if (abilities.Count == 0)
            {
                AddMessage("No abilities available for this job yet.");
                return;
            }

            while (true)
            {
                // Clear screen and show menu
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                   ABILITIES MENU                       ║");
                Console.WriteLine("╠════════════════════════════════════════════════════════╣");

                // Fixed player info line with proper padding calculation
                string playerInfo = $"{Player.Name} the {JobInfo.Get(Player.Job).DisplayName}";
                if (playerInfo.Length > 54) playerInfo = playerInfo.Substring(0, 51) + "...";
                Console.WriteLine($"║ {playerInfo.PadRight(54)} ║");

                // Fixed MP line with proper padding
                string mpInfo = $"MP: {Player.CurrentMP}/{Player.MaxMP}";
                Console.WriteLine($"║ {mpInfo.PadRight(54)} ║");

                Console.WriteLine("╠════════════════════════════════════════════════════════╣");

                for (int i = 0; i < abilities.Count; i++)
                {
                    var ability = abilities[i];
                    int cooldown = AbilitySystem.GetCooldownRemaining(Player, ability);
                    string mpText = ability.MPCost > 0 ? $"[{ability.MPCost} MP]" : "[Free]";
                    string status = "";
                    if (cooldown > 0)
                        status = $"[CD: {cooldown}]";
                    else if (Player.CurrentMP < ability.MPCost)
                        status = "[No MP]";
                    else
                        status = "[Ready]";

                    // Build ability line with proper spacing
                    string abilityName = ability.Name.Length > 12 ? ability.Name.Substring(0, 12) : ability.Name;
                    string abilityInfo = $"[{ability.Hotkey}] {abilityName} {mpText} {status}";
                    Console.WriteLine($"║ {abilityInfo.PadRight(54)} ║");

                    // Description line with truncation if needed
                    string description = ability.Description.Length > 48 ? ability.Description.Substring(0, 45) + "..." : ability.Description;
                    Console.WriteLine($"║     {description.PadRight(50)} ║");
                }

                Console.WriteLine("╠════════════════════════════════════════════════════════╣");
                Console.WriteLine("║ Select ability by hotkey, or ESC to return             ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════╝");
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                    break;

                // Find ability by hotkey
                var selectedAbility = abilities.FirstOrDefault(a => a.Hotkey == key.KeyChar);
                if (selectedAbility != null)
                {
                    var result = AbilitySystem.UseAbility(Player, this, selectedAbility);

                    switch (result)
                    {
                        case AbilityResult.Success:
                            // Ability was successful, break out and continue game
                            return;
                        case AbilityResult.InsufficientMP:
                            Console.WriteLine("\nNot enough MP! Press any key...");
                            Console.ReadKey(true);
                            break;
                        case AbilityResult.OnCooldown:
                            Console.WriteLine("\nAbility is on cooldown! Press any key...");
                            Console.ReadKey(true);
                            break;
                        case AbilityResult.InvalidTarget:
                            Console.WriteLine("\nNo valid target! Press any key...");
                            Console.ReadKey(true);
                            break;
                        case AbilityResult.AlreadyAtMax:
                            Console.WriteLine("\nAlready at maximum! Press any key...");
                            Console.ReadKey(true);
                            break;
                        default:
                            Console.WriteLine("\nAbility failed! Press any key...");
                            Console.ReadKey(true);
                            break;
                    }
                }
            }
            // Screen will be redrawn on next render
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