using System;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using Terraria;
using TShockAPI;
using Hooks;
using System.Net;

namespace SmarterFingers
{
    [APIVersion(1, 11)]
    public class Smarterfingers : TerrariaPlugin
    {
        Random rnd = new Random();
        public static List<Player> Players = new List<Player>();
        public static WordList readWordsList;
        public static string save = "";
        public static string perma = "";
        public static int update = 0;
        public static string downloadFromUpdate;
        public static string versionFromUpdate;
        public static DateTime lastupdate = DateTime.Now;

        public override Version Version
        {
            get { return new Version(0, 9, 1); }
        }

        public override string Name
        {
            get { return "SmarterFingers"; }
        }

        public override string Author
        {
            get { return "Spectrewiz"; }
        }

        public override string Description
        {
            get { return "Makes players smarter based on their sentences"; }
        }

        public Smarterfingers(Main game)
            : base(game)
        {
            Order = -1;
        }

        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            NetHooks.GreetPlayer += OnGreetPlayer;
            ServerHooks.Chat += OnChat;
            GameHooks.Update += OnUpdate;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Initialize -= OnInitialize;
                NetHooks.GreetPlayer -= OnGreetPlayer;
                ServerHooks.Chat -= OnChat;
                GameHooks.Update -= OnUpdate;
            }

            base.Dispose(disposing);
        }

        public static string randomString(params string[] args)
        {
            Random rnd = new Random();
            return args[rnd.Next(args.Length + 1)];
        }

        public void OnUpdate()
        {
            if (update == 0)
            {
                if (UpdateChecker())
                    update++;
                else
                    update--;
            }
            else if (update < 0)
            {
                if ((DateTime.Now - lastupdate).TotalHours >= 3)
                {
                    if (UpdateChecker())
                        update = 1;
                    else
                        lastupdate = DateTime.Now;
                }
            }
        }

        public static void OnInitialize()
        {
            Commands.ChatCommands.Add(new Command("smartize", Smartize, "smartize"));
            save = Path.Combine(TShock.SavePath, @"SmarterFingers\WordsList.json");
            perma = Path.Combine(TShock.SavePath, @"SmarterFingers\People.txt");

            WordReader reader = new WordReader();
            if (File.Exists(save))
            {
                try
                {
                    readWordsList = reader.readFile(save);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (readWordsList.SmarterFingers.Count != 0)
                        Console.WriteLine(readWordsList.SmarterFingers.Count + " SmarterFingers changes have been loaded.");
                    else
                    {
                        readWordsList = reader.writeFile(save);
                        Console.WriteLine("Original SmarterFingers word list created.");
                    }
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error in SmarterFingers \"WordsList.json\" file! Check log for more details.");
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                    Log.Error("------- Config Exception in SmarterFingers Config file (WordsList.json) -------");
                    Log.Error(e.Message);
                    Log.Error("---------------------------------- Error End ----------------------------------");
                }
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(TShock.SavePath, "SmarterFingers"));
                readWordsList = reader.writeFile(save);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Original SmarterFingers word list created.");
                Console.ResetColor();
                Log.Info("Original SmarterFingers word list created.");
            }
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            lock (Players)
                Players.Add(new Player(who));
            string name = TShock.Players[who].Name.ToLower();
            string line;
            if (File.Exists(perma))
            {
                using (StreamReader reader = new StreamReader(perma))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (String.Compare(line, name) == 0)
                        {
                            Players[GetPlayerIndex(who)].Smartized = true;
                        }
                    }
                }
            }
            if (TShock.Players[who].Group.Name.ToLower() == "superadmin")
                if (update > 0)
                {
                    TShock.Players[who].SendMessage("Update for Ticket System available! Check log for download link.", Color.Yellow);
                    Log.Info(string.Format("NEW VERSION: {0}  |  Download here: {1}", versionFromUpdate, downloadFromUpdate));
                }
        }

        public class Player
        {
            public int Index { get; set; }
            public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
            public bool Smartized { get; set; }
            public Player(int index)
            {
                Index = index;
                Smartized = false;
            }
        }

        private static int GetPlayerIndex(int ply)
        {
            lock (Players)
            {
                int index = -1;
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Index == ply)
                        index = i;
                }
                return index;
            }
        }

        public static TSPlayer GetTSPlayerByIndex(int index)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Index == index)
                    return player;
            }
            return null;
        }

        public static void Smartize(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Invalid syntax! Syntax: /smartize <player>", Color.Red);
            }
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (foundplr.Count == 0)
            {
                args.Player.SendMessage("Invalid player!", Color.Red);
            }
            else if (foundplr.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) player matched!", foundplr.Count), Color.Red);
            }
            var plr = foundplr[0];
            if (Players[GetPlayerIndex(plr.Index)].Smartized)
            {
                Players[GetPlayerIndex(plr.Index)].Smartized = false;
                args.Player.SendMessage("Player is no longer smart (uh oh).", 30, 144, 255);
                try
                {
                    string line = null;
                    string line_to_delete = args.Parameters[0];
                    int i = 0;

                    StreamReader reader = new StreamReader(perma);
                    StreamWriter writer = new StreamWriter("temp.txt", true);
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (String.Compare(line, line_to_delete, true) != 0)
                        {
                            writer.WriteLine(line);
                        }
                    }
                    reader.Close();
                    writer.Close();
                    File.Delete(perma);
                    File.Move("temp.txt", perma);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
            else
            {
                Players[GetPlayerIndex(plr.Index)].Smartized = true;
                args.Player.SendMessage("Smartized " + plr.Name + "!", 30, 144, 255);
                using (StreamWriter writer = new StreamWriter(perma))
                {
                    writer.WriteLine(plr.Name.ToLower());
                }
            }
        }

        public string finder(string originalText, string replacementText)
        {
            string text2 = replacementText;
            try
            {
                foreach (char var in originalText.ToLower())
                {
                    if (var == ',')
                        text2 = replacementText + ',';
                    else if (var == '.')
                        text2 = replacementText + '.';
                    else if (var == '?')
                        text2 = replacementText + '?';
                    else if (var == '!')
                        text2 = replacementText + '!';
                }
            }
            catch (Exception e) { Log.Error(e.Message); return originalText; }
            return text2;
        }

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
            char[] text2 = text.ToCharArray();
            if (text2[0] == '/')
            {
                return;
            }
            TSPlayer player = GetTSPlayerByIndex(ply);

            if (Players[GetPlayerIndex(ply)].Smartized)
            {
                e.Handled = true;
                string[] words = text.ToLower().Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (finder(words[i], "ur") == words[i] || finder(words[i], "yur") == words[i] || finder(words[i], "yer") == words[i] || finder(words[i], "er") == words[i] || finder(words[i], "u") == words[i] || finder(words[i], "you'r") == words[i])
                    {
                        if (words.Length - 1 > i && (words[i + 1] == "are" || words[i + 1] == "r"))
                        {
                            words[i] = finder(words[i], "you're");
                        }
                        else
                        {
                            words[i] = finder(words[i], "you");
                        }
                    }

                    if (finder(words[i], "hw") == words[i])
                    {
                        if (words.Length - 1 > i && (words[i + 1] == "?" || words[i + 1] == "are" || words[i + 1] == "can" || words[i + 1] == "r"))
                            words[i] = finder(words[i], "how");
                        else
                            words[i] = finder(words[i], "homework");
                    }

                    if (finder(words[i], "do") == words[i])
                        if (words.Length - 1 > i && words[i + 1] == "not")
                        {
                            words[i + 1] = "";
                            words[i] = finder(words[i], "don't");
                        }

                    for (int i2 = 0; i2 < readWordsList.SmarterFingers.Count; i2++)
                    {
                        try
                        {
                            List<string> stupid = readWordsList.SmarterFingers[i2].getOriginal();
                            List<string> smart = readWordsList.SmarterFingers[i2].getReplacement();
                            int index = smart.Count;
                            for (int i4 = 0; i4 < stupid.Count; i4++)
                            {
                                if (finder(words[i], stupid[i4].ToLower()) == words[i])
                                    words[i] = finder(words[i], smart[rnd.Next(index)]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            continue;
                        }
                    }
                }
                string textR = string.Join(" ", words);
                e.Handled = true;
                TShock.Utils.Broadcast(
                    string.Format(TShock.Config.ChatFormat, player.Group.Name, player.Group.Prefix, player.Name, player.Group.Suffix, textR),
                    player.Group.R, player.Group.G, player.Group.B);
                return;
            }
        }

        public bool UpdateChecker()
        {
            string raw;
            try
            {
                raw = new WebClient().DownloadString("https://raw.github.com/Spectrewiz/SmarterFingers/master/Desktop/Smartize/README.txt");//fix l8r
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
            string[] readme = raw.Split('\n');
            string[] download = readme[readme.Length - 1].Split('-');
            Version version;
            if (!Version.TryParse(readme[0], out version)) return false;
            if (Version.CompareTo(version) >= 0) return false;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("New Ticket System version: " + readme[0].Trim());
            Console.WriteLine("Download here: " + download[1].Trim());
            Console.ResetColor();
            Log.Info(string.Format("NEW VERSION: {0}  |  Download here: {1}", readme[0].Trim(), download[1].Trim()));
            downloadFromUpdate = download[1].Trim();
            versionFromUpdate = readme[0].Trim();
            return true;
        }
    }
}