using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Game
{
    static class Program
    {
        //player info
        static string playerName;
        static Entity player;
        static string playerClass;
        static int rounds;

        //a list of all basic class types. used for player selection and enemy creation
        static Entity[] baseClasses = {
            new Entity("Fighter", "Increased attack and stamina, medium health. Begin with 1 healing item", 5, 3, 1, 2),
            new Entity("Barbarian", "Increased attack and health, low stamina. Begin with 1 healing item", 8, 4, 1, 1),
            new Entity("Healer", "Increased health and healing, low attack. Begin with 3 healing items", 6, 1, 3, 2),
            new Entity("Rogue", "Increased attack and stamina, low health. Begin with no healing items", 4, 3, 0, 4)
        };

        //enemy info
        static Entity enemy;
        static int enemyUpgradePoints = 0;

        static void Main(string[] args)
        {
            rounds = 0;

            bool gameLoop = true;

            string[] names = File.ReadAllLines(@"resources/Names.txt");

            //menu returns 0 or 1
                //0 = start
                //1 = exit
            int selection = Menu();
            if (selection == 1)
                gameLoop = false;
            else
            {
                PickClass();
                Console.Clear();
                while (gameLoop) //main game loop
                {
                    SetupRound();
                    Turn();

                    if (enemy.currentHealth <= 0)
                    {
                        Renderer.drawMenu($"&2{enemy.name}&4 is dead. (&3A&4 to continue)");
                        Utility.getAnswerFromList(new string[] { "a" });
                        enemyUpgradePoints += 2;
                        rounds++;
                        Upgrade();
                    }
                    else
                    {
                        Renderer.drawMenu($"&4You are dead. (&3A&4 to continue)");
                        Utility.getAnswerFromList(new string[] { "a" });
                        gameLoop = false;
                    }
                }

                End();
            }
        }

        //function for drawing menu
        static int Menu()
        { 
            //this was made before I had the menu system set up, so this code is not optimal

            Renderer.drawMenuScreen();
            while (true)
            {
                string input = Console.ReadLine();
                switch (input.ToLower())
                {
                    case "start":
                        return 0;
                    case "exit":
                        return 1;
                    default:
                        Console.WriteLine("Invalid Option selected.");
                        break;
                }
            }
        }

        //pick class prompts user for a name and starting kit
        static void PickClass()
        {
            //choose name
            Console.Clear();
            Renderer.drawPictureFromFile(@"resources/Name.txt");
            Renderer.drawStringWithFormat("&4Welcome Challenger! &2THE GAUNTLET&4 is about to begin. \nPlease give your name.&1\n");
            bool correct = false;
            while(!correct)
            {
                string input = Console.ReadLine();
                string name = input;
                Renderer.drawStringWithFormat($"\n&4Your name is now \"&2{input}&4\". Is this correct?\n");
                Renderer.drawMenu(new string[] { "&3Yes", "&3No" });
                input = Utility.getAnswerFromList(new string[] { "yes", "no" });
                if (input == "yes")
                {
                    correct = true;
                    playerName = name;
                    Renderer.drawStringWithFormat($"\n&4You are now &2{name}&4.");
                    Renderer.drawMenu("&4Type &3A&4 to continue.");
                    Utility.getAnswerFromList(new string[] { "a" });
                }
                else
                {
                    Console.Clear();
                    Renderer.drawPictureFromFile(@"resources/Name.txt");
                    Renderer.drawStringWithFormat("&4I see, you misspoke. What name did you mean?\n&1");
                }
            }

            //choose class
            Console.Clear();
            Renderer.drawPictureFromFile(@"resources/Class.txt");
            Renderer.drawStringWithFormat("&4Next you must choose a class. The selectable classes are: ");
            correct = false;
            while (!correct)
            {
                Renderer.drawMenu(new string[] {
                    "&3" + baseClasses[0].name + "&1:&4 " + baseClasses[0].description,
                    "&3" + baseClasses[1].name + "&1:&4 " + baseClasses[1].description,
                    "&3" + baseClasses[2].name + "&1:&4 " + baseClasses[2].description,
                    "&3" + baseClasses[3].name + "&1:&4 " + baseClasses[3].description + "&1"
                });

                string input = Utility.getAnswerFromList(new string[] {
                    baseClasses[0].name.ToLower(),
                    baseClasses[1].name.ToLower(),
                    baseClasses[2].name.ToLower(),
                    baseClasses[3].name.ToLower()
                });
                string selectedClass = input;
                Renderer.drawStringWithFormat($"\n&4You selected &2{selectedClass}&4. Is this correct?");
                Renderer.drawMenu(new string[] { "&3Yes", "&3No" });
                input = Utility.getAnswerFromList(new string[] { "yes", "no" });
                if (input == "yes")
                {
                    correct = true;
                    for (int i = 0; i < baseClasses.Length; i++)
                    {
                        if (baseClasses[i].name.ToLower() == selectedClass.ToLower())
                        {
                            player = baseClasses[i].cloneClass();
                            playerClass = baseClasses[i].name;
                        }
                    }
                    Renderer.drawStringWithFormat($"\n&4You are now a &2{player.name}&4.");
                    Renderer.drawMenu("&4Type &3A&4 to continue.");
                    Utility.getAnswerFromList(new string[] { "a" });
                }
                else
                {
                    Console.Clear();
                    Renderer.drawPictureFromFile(@"resources/Class.txt");
                    Renderer.drawStringWithFormat("&4Very well. Select a different class\n&1");
                }
            }
            player.name = playerName;
        }

        //create a new enemy to fight
        static void SetupRound()
        {
            Random rand = new Random();
            int randClass = rand.Next(baseClasses.Length);
            enemy = baseClasses[randClass].cloneClass();
            string[] names = File.ReadAllLines(@"resources/Names.txt");
            enemy.name = names[rand.Next(names.Length)] + " " + names[rand.Next(names.Length)];
            for (int i = 0; i < enemyUpgradePoints; i++)
            {
                if(rand.Next(2) == 0)
                    enemy.attack++;
                else if(rand.Next(2) == 0)
                    enemy.max_health++;
                else if(rand.Next(2) == 0)
                    enemy.healing++;
                else
                    enemy.stamina++;
            }

            //battle intro text
            Console.Clear();
            Renderer.drawPictureFromFile(@"resources/New Attacker.txt");
            Renderer.drawStringWithFormat($"\n&2{enemy.name}\n&3Health&1:&4 {enemy.max_health}\n&3Attack&1:&4 {enemy.attack}\n&3Healing&1:&4 {enemy.healing}\n&3Stamina&1:&4 {enemy.stamina}\n");
            Renderer.drawMenu("&4Type &3A&4 to Start the Round.");
            Utility.getAnswerFromList(new string[] { "a" });
        }

        //ingame loop. handles player and enemy turns, loops until one dies
        static void Turn()
        {
            //reset variable
            string message = "";
            int turn = 0;
            int playerBlock = 0;
            int enemyBlock = 0;

            player.currentHealth = player.max_health;
            player.currentStamina = player.stamina;
            player.currentHealing = player.healing;

            enemy.currentHealth = enemy.max_health;
            enemy.currentStamina = enemy.stamina;
            enemy.currentHealing = enemy.healing;

            //loop
            while (player.currentHealth > 0 && enemy.currentHealth > 0)
            {
                //draw info
                Console.Clear();
                Renderer.drawStringWithFormat(player.name);
                Renderer.drawHealth(player.max_health, player.currentHealth);
                Renderer.drawStringWithFormat("\n\n" + enemy.name);
                Renderer.drawHealth(enemy.max_health, enemy.currentHealth);

                //player turn
                if(turn == 0)
                {
                    playerBlock = 0;
                    
                    Renderer.drawMenu(new string[] { $"&3Attack &4(&2{player.currentStamina}&4)", "&3Block", $"&3Heal &4(&2{player.currentHealing}&4)" });
                    List<string> options = new List<string> { "block" };
                    if (player.currentStamina > 0)
                        options.Add("attack");
                    if (player.currentHealing > 0)
                        options.Add("heal");
                    switch (Utility.getAnswerFromList(options.ToArray()))
                    {
                        case "attack":
                            int damage = Utility.getRandomNum(player.attack, 2) - enemyBlock;
                            if (damage < 0)
                                damage = 0;
                            enemy.currentHealth -= damage;
                            player.currentStamina--;
                            message = $"&4Your attack did &2{damage}&4 damage.";
                            break;
                        case "block":
                            playerBlock = player.attack;
                            player.currentStamina = player.stamina;
                            message = "&4You blocked.";
                            break;
                        case "heal":
                            int heal = Math.Abs(Utility.getRandomNum(0, 3));
                            player.currentHealth += heal;
                            player.currentStamina = player.stamina;
                            player.currentHealing--;
                            if (player.currentHealth > player.max_health)
                                player.currentHealth = player.max_health;
                            message = $"&4You healed for &2{heal}&4 health.";
                            break;
                    }
                }
                else
                {
                    //enemy turn
                    if(turn == 2)
                    {
                        enemyBlock = 0;
                        if(enemy.currentHealth < 4 && enemy.currentHealing > 0)
                        {
                            int heal = Math.Abs(Utility.getRandomNum(0, 3));
                            enemy.currentHealth += heal;
                            enemy.currentStamina = enemy.stamina;
                            enemy.currentHealing--;
                            if (enemy.currentHealth > enemy.max_health)
                                enemy.currentHealth = enemy.max_health;
                            message = $"&2{enemy.name}&4 healed for &2{heal}&4 health.";
                        }
                        else if(enemy.currentStamina > 0)
                        {
                            int damage = Utility.getRandomNum(enemy.attack, 2) - playerBlock;
                            if (damage < 0)
                                damage = 0;
                            player.currentHealth -= damage;
                            enemy.currentStamina--;
                            message = $"&2{enemy.name}&4 attacked for &2{damage}&4 damage.";
                        }
                        else
                        {
                            enemyBlock = enemy.attack;
                            enemy.currentStamina = enemy.stamina;
                            message = $"&2{enemy.name}&4 blocked.";
                        }
                    }

                    //end of turn readout
                    Renderer.drawMenu("&4" + message + " (&3A&4 to continue)");
                    Utility.getAnswerFromList(new string[] { "a" });
                }
                //loop turn back to start
                turn++;
                if (turn == 3)
                    turn = 0;
            }
        }

        //dilogue for upgrading player
        static void Upgrade()
        {
            Console.Clear();
            Renderer.drawPictureFromFile(@"resources/Upgrade.txt");
            Renderer.drawStringWithFormat("&4Select an upgrade.");
            Renderer.drawMenu(new string[] { "&3Health +1", "&3Damage +1", "&3Stamina +1", "&3Healing +1" });
            string input = Utility.getAnswerFromList(new string[] { "health", "damage", "stamina", "healing"});
            switch(input)
            {
                case "health":
                    player.max_health++;
                    break;
                case "damage":
                    player.attack++;
                    break;
                case "stamina":
                    player.stamina++;
                    break;
                case "healing":
                    player.healing++;
                    break;
            }
            Renderer.drawStringWithFormat($"\n&2{input} &4upgraded.");
            Renderer.drawMenu("&3A&4 to start the next round.");
            Utility.getAnswerFromList(new string[] { "a" });
        }

        //death screen
        public static void End()
        {
            //put player into list
            string name = player.name + " &4(&2" + playerClass + "&4)";
            name = name.Replace(" ", "_"); //replace spaces with underscore, otherwise system errors
            File.AppendAllText(@"resources/scores.txt", name + " " + rounds + Environment.NewLine);
            string[] top = Utility.getTopTen(@"resources/scores.txt");

            //game over text
            Console.Clear();
            Renderer.drawPictureFromFile(@"resources/Game Over.txt", 2);
            Renderer.drawStringWithFormat($"\n&4 You survived &2{rounds}&4 rounds.\n\n");
            Renderer.drawStringWithFormat("&4Leaderboard:\n\n");

            foreach(string str in top)
            {
                if (str != null)
                {
                    Renderer.drawStringWithFormat(str.Replace("_", " ") + "\n");
                }
            }

            Renderer.drawMenu("&3A&4 to Exit.");
            Utility.getAnswerFromList(new string[] { "a" });
        }
    }

    /*
     Each Class uses a similar setup
     Name & description are aesthetic

        variables include:
        health
        attack str
        healing amount
        stamina
         */
     
    //information for a single object.
    class Entity
    {
        public string name { get; set; }
        public string description { get; private set; }

        public int max_health { get; set; }
        public int attack { get; set; }
        public int healing { get; set; }
        public int stamina { get; set; }

        public int currentHealth { get; set; }
        public int currentStamina { get; set; }
        public int currentHealing { get; set; }

        public Entity(string name)
           : this(name, "", 0, 0, 0, 0)
        {

        }

        public Entity(string name, string desc, int health, int attack, int healing, int stamina)
        {
            this.name = name;
            this.description = desc;
            this.currentHealth = health;
            this.max_health = health;
            this.attack = attack;
            this.healing = healing;
            this.currentHealing = healing;
            this.stamina = stamina;
            this.currentStamina = stamina;

        }

        public Entity cloneClass()
        {
            return (Entity) this.MemberwiseClone();
        }
    }

    //operations used to draw text to the screen
    static class Renderer
    {
        //colors used in game
        static ConsoleColor[] colors = {
            ConsoleColor.Black, //not used
            ConsoleColor.White,
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Gray
        };

        //draw healthbar with slots empty based on health
        public static void drawHealth(int max, int health, bool newline = true)
        {
            if (newline)
            {
                Console.WriteLine();
            }
            drawStringWithFormat("[");
            for(int i = 0; i < health; i++)
            {
                drawStringWithFormat("&3X");
            }
            for (int i = 0; i < max - health; i++)
            {
                drawStringWithFormat("&2-");
            }
            drawStringWithFormat("]");
        }

        //draw a single line options menu w/ border
        public static void drawMenu(string text, bool newline = true)
        {
            string[] stext = { text };
            drawMenu(stext, newline);
        }

        //draw a multi-line options menu w/ border
        public static void drawMenu(string[] text, bool newline = true)
        {
            if (newline)
            {
                Console.WriteLine();
            }

            string[] unformatted_text = new string[text.Length];
            for(int i = 0; i < text.Length; i++)
            {
                unformatted_text[i] = Utility.getStringWithoutFormat(text[i]);
            }

            int longest = Utility.getLongestStringInList(unformatted_text);

            drawStringWithFormat("*=");
            for (int i = 0; i < unformatted_text[longest].Length; i++)
            {
                Console.Write("=");
            }
            drawStringWithFormat("=*");
            for(int i = 0; i < text.Length; i++)
            {
                string temp_text = text[i];
                while (Utility.getStringWithoutFormat(temp_text).Length < unformatted_text[longest].Length)
                {
                    temp_text += " ";
                }
                drawStringWithFormat("\n| " + temp_text + " &1|\n");
            }
            drawStringWithFormat("*=");
            for (int i = 0; i < unformatted_text[longest].Length; i++)
            {
                drawStringWithFormat("=");
            }
            drawStringWithFormat("=*\n");
        }

        //many lines ingame have text formatting to make coloring easier. This is the
        //render that draws them to hide formatting tips
        public static void drawStringWithFormat(string rawString)
        {
            Console.ForegroundColor = ConsoleColor.White;
            for(int i = 0;  i < rawString.Length; i++)
            {
                if(rawString[i] == '&')
                {
                    string letter = Convert.ToString(rawString[i+1]);
                    int location = Convert.ToInt32(letter);
                    Console.ForegroundColor = colors[location];
                    i++;
                }
                else
                {
                    Console.Write(rawString[i]);
                }
            }
        }

        //draw main menu
        public static void drawMenuScreen()
        {
            drawPictureFromFile(@"resources/Logo.txt", 2);
            string[] options = { "&3Start", "&3Exit" };
            drawMenu(options, false);
        }

        //displays a list as a picture
        public static void drawPictureFromFile(string path, int color = 1)
        {
            Console.ForegroundColor = colors[color];
            string[] img = File.ReadAllLines(path);
            foreach (string line in img)
            {
                Console.WriteLine(line);
            }
        }
    }

    //helpful functions for operation
    static class Utility
    {
        //used to draw menus
        public static int getLongestStringInList(string[] list)
        {
            int index = 0;
            for(int i = 0; i < list.Length; i++)
            {
                if(getStringWithoutFormat(list[i]).Length > getStringWithoutFormat(list[index]).Length)
                {
                    index = i;
                }
            }
            return index;
        }

        //remove all color formatting tips
        public static string getStringWithoutFormat(string s)
        {
            string unformatted_s = "";
            for(int i = 0; i < s.Length; i++)
            {
                if(s[i] == '&')
                {
                    i++;
                }
                else
                {
                    unformatted_s += s[i];
                }
            }
            return unformatted_s;
        }

        //get a random number given a start point and a range
        public static int getRandomNum(int start, int range)
        {
            Random rand = new Random();
            return start + (rand.Next(0 - range, range + 1));
        }

        //pause the game until player answers with one of the given responses
        public static string getAnswerFromList(string[] answers, string failResponse = "Invalid Answer", bool clearAfterIncorrect = false)
        {
            while(true)
            {
                string input = Console.ReadLine();
                if (answers.Contains(input.ToLower()))
                    return input.ToLower();
                else
                {
                    if (clearAfterIncorrect)
                        Console.Clear();
                    Console.WriteLine(failResponse);
                }
            }
        }

        //convert list into code-readable list and return top 10 scores
        public static string[] getTopTen(string listFile)
        {
            List<string> names = new List<string>();
            List<int> scores = new List<int>();

            string[] top = new string[10];

            List<string> players = File.ReadAllLines(listFile).ToList();
            if (players.Count > 0)
            {
                foreach (string str in players)
                {
                    string[] split = str.Split(' ');
                    names.Add(split[0]);
                    scores.Add(Convert.ToInt32(split[1]));
                }

                for (int i = 0; i < 10; i++)
                {
                    if (i < players.Count)
                    {
                        int max = scores.IndexOf(scores.Max());
                        top[i] = "&3" + names[max] + "&1: " + "&4" + scores[max];
                        scores.RemoveAt(max);
                        names.RemoveAt(max);
                    }
                }
            }

            return top;
        }
    }
}