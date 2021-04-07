using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeLauncher
{
    class CheckInstalledGames
    {

        GameHTML gameHTML = new GameHTML();

        public List<Game> GetAllGames()
        {
            List<Game> allGames = new List<Game>();

            allGames.Add(new Game() 
            { 
                ID= "GTA5",
                Name = "Grand Theft Auto V",
                RemotePath = "Dropbox:Grand Theft Auto V",
                EXEPath = "\\Grand Theft Auto V\\GTA5.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "RDR2",
                Name = "Red Dead Redemption 2",
                RemotePath = "Dropbox:Red Dead Redemption 2",
                EXEPath = "\\Red Dead Redemption 2\\Launcher.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "Cyberpunk",
                Name = "Cyberpunk 2077",
                RemotePath = "Dropbox:Cyberpunk 2077",
                EXEPath = "\\Cyberpunk 2077\\bin\\x64\\Cyberpunk2077.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "MGS5",
                Name = "Metal Gear Solid V",
                RemotePath = "Dropbox2:Metal Gear Solid V",
                EXEPath = "\\Metal Gear Solid V\\mgsvtpp.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "JForce",
                Name = "JUMP FORCE",
                RemotePath = "Dropbox2:JUMP FORCE",
                EXEPath = "\\JUMP FORCE\\JUMP_FORCE\\Binaries\\JUMP_FORCE-Win64-Shipping.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "NBA2K21",
                Name = "NBA 2K21",
                RemotePath = "Dropbox2:NBA 2K21",
                EXEPath = "\\NBA 2K21\\NBA2K21.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "NFSHeat",
                Name = "Need for Speed Heat",
                RemotePath = "Dropbox2:Need for Speed Heat",
                EXEPath = "\\Need for Speed Heat\\NeedForSpeedHeat.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "PC3",
                Name = "Project CARS 3",
                RemotePath = "Dropbox2:Project CARS 3",
                EXEPath = "\\Project CARS 3\\pCARS3.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "SWJFO",
                Name = "Star Wars Jedi Fallen Order",
                RemotePath = "Dropbox2:Star Wars Jedi Fallen Order",
                EXEPath = "\\Star Wars Jedi Fallen Order\\SwGame\\Binaries\\Win64\\starwarsjedifallenorder.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "Borderlands3",
                Name = "Borderlands 3",
                RemotePath = "Dropbox2:Borderlands 3",
                EXEPath = "\\Borderlands 3\\OakGame\\Binaries\\Win64\\Borderlands3.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "CrysisR",
                Name = "Crysis Remastered",
                RemotePath = "Dropbox2:Crysis Remastered",
                EXEPath = "\\Crysis Remastered\\Bin64\\CrysisRemastered.exe"
            });

            allGames.Add(new Game() 
            {
                ID = "FarCryND",
                Name = "Far Cry New Dawn",
                RemotePath = "Dropbox2:Far Cry New Dawn",
                EXEPath = "\\Far Cry New Dawn\\bin\\FarCryNewDawn.exe"
            });

            allGames.Add(new Game()
            {
                ID = "MetroExodus",
                Name = "Metro Exodus Gold Edition",
                RemotePath = "Dropbox2:Metro Exodus Gold Edition",
                EXEPath = "\\Metro Exodus\\MetroExodus.exe"
            });

            allGames.Add(new Game()
            {
                ID = "HZD",
                Name = "Horizon Zero Dawn",
                RemotePath = "Dropbox2:Horizon Zero Dawn",
                EXEPath = "\\Horizon Zero Dawn\\HorizonZeroDawn.exe"
            });

            allGames.Add(new Game()
            {
                ID = "FH4",
                Name = "Forza Horizon 4",
                RemotePath = "Dropbox2:Forza Horizon 4",
                EXEPath = "\\Forza Horizon 4\\ForzaHorizon4.exe"
            });

            allGames.Add(new Game()
            {
                ID = "MKXL",
                Name = "Mortal Kombat XL",
                RemotePath = "Dropbox2:Mortal Kombat XL",
                EXEPath = "\\Mortal Kombat XL\\Binaries\\Retail\\MK10.exe"
            });

            allGames.Add(new Game()
            {
                ID = "ACV",
                Name = "Assassins Creed Valhalla",
                RemotePath = "Dropbox2:Assassins Creed Valhalla",
                EXEPath = "\\Assassins Creed Valhalla\\ACValhalla.exe"
            });

            allGames.Add(new Game()
            {
                ID = "RE3",
                Name = "Resident Evil 3",
                RemotePath = "Dropbox2:Resident Evil 3",
                EXEPath = "\\Resident Evil 3\\re3.exe"
            });

            allGames.Add(new Game()
            {
                ID = "SOTTR",
                Name = "Shadow Of The Tomb Raider",
                RemotePath = "Dropbox2:Shadow Of The Tomb Raider",
                EXEPath = "\\Shadow Of The Tomb Raider\\SOTTR.exe"
            });

            allGames.Add(new Game()
            {
                ID = "FIFA19",
                Name = "FIFA 19",
                RemotePath = "Dropbox2:FIFA 19",
                EXEPath = "\\FIFA 19\\FIFA19.exe"
            });

            return allGames;
        }

        public Game[] GetInstalledGames()
        {
            List<Game> result = new List<Game>();

            foreach (Game game in GetAllGames())
            {
                if (File.Exists(Properties.Settings.Default.InstallPath + game.EXEPath))
                {
                    result.Add(game);
                }
            }

            return result.ToArray();
        }

        public Game[] GetUninstalledGames()
        {
            List<Game> result = new List<Game>();

            IList<Game> listA = GetInstalledGames();
            IList<Game> listB = GetAllGames();
            var keysA = new HashSet<Tuple<string, string, string, string>>(
                listA.Select(site => Tuple.Create(site.ID, site.Name, site.RemotePath, site.EXEPath))
            );
            foreach (var site in listB)
            {
                var keyB = Tuple.Create(site.ID, site.Name, site.RemotePath, site.EXEPath);
                if (keysA.Contains(keyB))
                {
                    // site is in both lists
                }
                else
                {
                    // site is in listB but not listA
                    result.Add(site);
                }
            }

            return result.ToArray();
        }
    }

    public class Game
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string RemotePath { get; set; }
        public string EXEPath { get; set; }
    }
}
