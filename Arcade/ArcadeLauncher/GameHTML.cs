using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeLauncher
{
    class GameHTML
    {

        public string GenerateArcadePage()
        {
            CheckInstalledGames checkInstalledGames = new CheckInstalledGames();

            string games = "";

            Game[] uninstalledGames = checkInstalledGames.GetUninstalledGames();

            foreach (var item in uninstalledGames.Select((value, i) => new { i, value }))
            {
                /*if (item.i % 4 == 0 && item.i > 0)
                {
                    string nextRow = @"
    </div>
    <br>
    <div class=""row"">
    ";
                    games = games + nextRow + GenerateGameHTML(item.value.Name, item.value.ID);
                }
                else
                {
                    games = games + GenerateGameHTML(item.value.Name, item.value.ID);
                }*/
                games = games + GenerateGameHTML(item.value.Name, item.value.ID);
            }

            string title = @"
            <script>document.getElementById(""arcade"").style.color = ""white"";</script>
            
			<div class=""row"">
";

            return getHTML("Start") + title + games + getHTML("End");
        }

        public string GenerateInstalledGamesPage()
        {
            CheckInstalledGames checkInstalledGames = new CheckInstalledGames();

            string games = "";

            Game[] installedGames = checkInstalledGames.GetInstalledGames();

            foreach (var item in installedGames.Select((value, i) => new { i, value }))
            {
                /*if (item.i % 4 == 0 && item.i > 0)
                {
                    string nextRow = @"
    </div>
    <br>
    <div class=""row"">
    ";
                    games = games + nextRow + GenerateInstalledGameHTML(item.value.Name, item.value.ID);
                }
                else
                {
                    games = games + GenerateInstalledGameHTML(item.value.Name, item.value.ID);
                }*/
                games = games + GenerateInstalledGameHTML(item.value.Name, item.value.ID);
            }

            string title = @"
            <script>document.getElementById(""installed"").style.color = ""white"";</script>
            
			<div class=""row"">
";

            return getHTML("Start") + title + games + getHTML("End");
        }

        public string GenerateSettingsPage()
        {
            return getHTML("Settings");
        }

        public string getHTML(string gameName)
        {
            if (gameName == "Start")
            {
                string HTML = Properties.Resources.start.ToString();
                return HTML;
            }
            else if (gameName == "End")
            {
                string HTML = Properties.Resources.end.ToString();
                return HTML;
            }
            else if (gameName == "Settings")
            {
                string HTML = Properties.Resources.settings.ToString();
                return HTML;
            }
            else if (string.IsNullOrEmpty(gameName))
            {
                return "";
            }
            else
            {
                return "NOT FOUND";
            }
        }

        private string GenerateGameHTML(string gameName, string gameID)
        {
            string HTML = @"
				<div class=""col-lg-3 align-items-stretch card-margin"">
                    <div class=""container"">
						<img src = ""http://images/" + gameID + @".jpg"" class=""image"">
						<div class=""desc"">
							<div class=""bottom-left"">" + gameName + @"</div>
                            <a onclick = ""window.chrome.webview.hostObjects.arcade.Install('" + gameID + @"')"" class=""btn btn-primary top-right"">Install</a>
						</div>
					</div>
				</div>
";

            return HTML;
        }

        private string GenerateInstalledGameHTML(string gameName, string gameID)
        {
            string HTML = @"
				<div class=""col-lg-3 align-items-stretch card-margin"">
                            <div class=""container"">
						<img src = ""http://images/" + gameID + @".jpg"" class=""image"">
						<div class=""desc"">
							<div class=""bottom-left"">" + gameName + @"</div>
                            <a onclick = ""window.chrome.webview.hostObjects.arcade.Uninstall('" + gameID + @"')"" class=""btn btn-danger top-right"">Uninstall</a>
							<a onclick = ""window.chrome.webview.hostObjects.arcade.Play('" + gameID + @"')"" class=""btn btn-success bottom-right"">Play</a>
						</div>
					</div>
				</div>
";

            return HTML;
        }

        public string[] GetColumn(string[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }
    }
}
