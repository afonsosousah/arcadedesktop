using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeLauncher
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
		GetWebView2LoaderDLL();
			EmbeddedAssembly.Load("ArcadeLauncher.Resources.Microsoft.Web.WebView2.WinForms.dll", "Microsoft.Web.WebView2.WinForms.dll");
			EmbeddedAssembly.Load("ArcadeLauncher.Resources.Guna.UI2.dll", "Guna.UI2.dll");
			EmbeddedAssembly.Load("ArcadeLauncher.Resources.Microsoft.Web.WebView2.Core.dll", "Microsoft.Web.WebView2.Core.dll");
			EmbeddedAssembly.Load("ArcadeLauncher.Resources.Microsoft.Web.WebView2.Wpf.dll", "Microsoft.Web.WebView2.Wpf.dll");
			EmbeddedAssembly.Load("ArcadeLauncher.Resources.Dropbox.Api.dll", "Dropbox.Api.dll");
			EmbeddedAssembly.Load("ArcadeLauncher.Resources.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");

			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			MainWindow mainWindow = new MainWindow();
			SaveImages();
			Application.Run(mainWindow);
		}

		static void SaveImages()
		{
			CheckInstalledGames checkInstalledGames = new CheckInstalledGames();
			foreach (Game game in checkInstalledGames.GetAllGames())
			{
				if(!String.IsNullOrEmpty(game.ID))
                {
					Directory.CreateDirectory(Application.StartupPath + "\\images");
					Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArcadeLauncher.Resources." + game.ID + ".jpg");
					using (var fileStream = File.Create(Application.StartupPath + "\\images\\" + game.ID + ".jpg"))
					{
						stream.Seek(0, SeekOrigin.Begin);
						stream.CopyTo(fileStream);
					}
                }
			}
			Stream stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArcadeLauncher.Resources.login.png");
			using (var fileStream = File.Create(Application.StartupPath + "\\images\\login.png"))
			{
				stream2.Seek(0, SeekOrigin.Begin);
				stream2.CopyTo(fileStream);
			}
		}

        static void GetWebView2LoaderDLL()
        {
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArcadeLauncher.Resources.WebView2Loader.dll");
			using (var fileStream = File.Create(Application.StartupPath + "\\WebView2Loader.dll"))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(fileStream);
			}
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return EmbeddedAssembly.Get(args.Name);
		}
	}
}
