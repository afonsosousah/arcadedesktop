using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using ArcadeLauncher.Properties;
using System.Reflection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Security;
using System.Net.Http;

namespace ArcadeLauncher
{
    public partial class MainWindow : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        private Size oldSize = new Rectangle(0, 0, 1280, 720).Size;
        GameHTML gameHTML = new GameHTML();
        CheckInstalledGames checkInstalledGames = new CheckInstalledGames();
        List<Game> allGames;
        public CancellationTokenSource cancellationTokenSource;
        private KeyHandler ghk;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();

            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
            //guna2Panel1.MouseDown += Login_MouseDown;
            //label4.MouseDown += Login_MouseDown;
            allGames = checkInstalledGames.GetAllGames();

            ghk = new KeyHandler(Constants.ALT, Keys.X, this);
            ghk.Register();

            label1.Parent = guna2ProgressBar1;
            guna2Button1.Parent = guna2ProgressBar1;

            Loading();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += Application_ApplicationExit;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Process.GetProcessesByName("ArcadeDownloader").Length > 0 && e.IsTerminating)
            {
                foreach (Process p in Process.GetProcessesByName("ArcadeDownloader"))
                {
                    p.Kill();
                }
            }
            MessageBox.Show("Exception thrown: " + e.ExceptionObject.ToString());
            if(e.IsTerminating)
                Environment.Exit(1);
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            if(Process.GetProcessesByName("ArcadeDownloader").Length > 0)
            {
                foreach(Process p in Process.GetProcessesByName("ArcadeDownloader"))
                {
                    p.Kill();
                }
            }
        }

        Overlay overlay = new Overlay();

        private void HandleHotkey()
        {
            if(overlay.Visible)
            {
                overlay.Hide();
            }
            else if(!overlay.Visible)
            {
                overlay.Show();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY_MSG_ID)
                HandleHotkey();
            base.WndProc(ref m);
        }

        private async void InitializeAsync()
        {
            GameHTML gameHTML = new GameHTML();
            var creatonProperties = new Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties();
            creatonProperties.BrowserExecutableFolder = Application.StartupPath + "\\WebView2";
            webView1.CreationProperties = creatonProperties;
            await webView1.EnsureCoreWebView2Async(null);
            webView1.CoreWebView2.SetVirtualHostNameToFolderMapping("images", Application.StartupPath + "\\images", Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            webView1.NavigationCompleted += async (sender, e) =>
            {
                if(!File.Exists(Application.StartupPath + "/images/avatar.png"))
                    (new WebClient()).DownloadFile("https://benevis.com/wp-content/uploads/2019/09/default-avatar-1.jpg", Application.StartupPath + "/images/avatar.png");
                
                if (Properties.Settings.Default.User == null)
                    Properties.Settings.Default.User = new User { Username = "Guest" };

                await webView1.ExecuteScriptAsync($"document.getElementById('username').innerHTML = '{ Properties.Settings.Default.User.Username }';");
                await webView1.ExecuteScriptAsync($"document.getElementById('username').value = '{ Properties.Settings.Default.User.Username }';");
                await webView1.ExecuteScriptAsync($"document.getElementById('userimage').src = '{ "http://images/avatar.png" }';");
                await webView1.ExecuteScriptAsync($"document.getElementById('parallelTransfers').value = '{ Properties.Settings.Default.ParallelTransfers }';");
            };

            webView1.CoreWebView2.NavigateToString(gameHTML.GenerateArcadePage());
            webView1.CoreWebView2.AddHostObjectToScript("arcade", new Arcade());
            webView1.CoreWebView2.AddHostObjectToScript("eventForwarder", new EventForwarder(this.Handle));
            webView1.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView1.CoreWebView2.Settings.AreDevToolsEnabled = false;
        }

        public async void Loading()
        {
            PictureBox loading = new PictureBox();
            loading.Image = Properties.Resources.loading;
            loading.SizeMode = PictureBoxSizeMode.CenterImage;
            loading.Location = new Point(1, 1);
            loading.Dock = DockStyle.Fill;
            Controls.Add(loading);
            loading.BringToFront();
            //await Task.Run(() => Task.Delay(3000));
            Controls.Remove(loading);
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(base.Handle, 161, 2, 0);
            }
        }

        public void UninstallGame(string installPath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(installPath);

            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                dir.Delete(true);
            }

            Directory.Delete(installPath);

            webView1.CoreWebView2.NavigateToString(gameHTML.GenerateInstalledGamesPage());
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            webView1.CoreWebView2.NavigateToString(gameHTML.GenerateArcadePage());

            //guna2Button4.ForeColor = Color.White;
            //guna2Button5.ForeColor = Color.FromArgb(154, 157, 160);
            //guna2Button7.ForeColor = Color.FromArgb(154, 157, 160);
            webView1.Show();
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            webView1.CoreWebView2.NavigateToString(gameHTML.GenerateInstalledGamesPage());

            //guna2Button5.ForeColor = Color.White;
            //guna2Button4.ForeColor = Color.FromArgb(154, 157, 160);
            //guna2Button7.ForeColor = Color.FromArgb(154, 157, 160);
            webView1.Show();
        }

        private async void guna2Button6_Click(object sender, EventArgs e)
        {
            //await Task.Run(() => Thread.Sleep(2000));
            //guna2Panel2.Hide();
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            //guna2Button7.ForeColor = Color.White;
            //guna2Button4.ForeColor = Color.FromArgb(154, 157, 160);
            //guna2Button5.ForeColor = Color.FromArgb(154, 157, 160);
            webView1.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            /*guna2Panel2.Show();
            var instance = new DrpBox();
            try
            {
                var task = Task.Run((Func<Task<int>>)instance.Run);

                task.Wait();

                MessageBox.Show(task.Result.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }*/
        }

        private void guna2ProgressBar1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void webView1_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click_2(object sender, EventArgs e)
        {

        }

        private async void guna2Button1_Click_1(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("ArcadeDownloader").Length > 0)
            {
                foreach (Process p in Process.GetProcessesByName("ArcadeDownloader"))
                {
                    p.Kill();
                }
            }
            else
            {
                MessageBox.Show("Couldn't find downloader process");
            }
            await Task.Run(() => Thread.Sleep(2000));
            guna2Panel2.Hide();
        }

        void TransparentBackground(Control C)
        {
            C.Visible = false;

            C.Refresh();
            Application.DoEvents();

            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;
            int Right = screenRectangle.Left - this.Left;

            Bitmap bmp = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
            Bitmap bmpImage = new Bitmap(bmp);
            bmp = bmpImage.Clone(new Rectangle(C.Location.X + Right, C.Location.Y + titleHeight, C.Width, C.Height), bmpImage.PixelFormat);
            C.BackgroundImage = bmp;

            C.Visible = true;
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Arcade
    {
        static GameDownloader gameDownloader = new GameDownloader();
        GameHTML gameHTML = new GameHTML();
        User user = new User();

        public async void Install(string gameName)
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            mainWindow.guna2Panel2.Invoke(new Action(() => mainWindow.guna2Panel2.Show()));
            CheckInstalledGames checkInstalledGames = new CheckInstalledGames();

            foreach(Game game in checkInstalledGames.GetAllGames())
            {
                if(game.ID == gameName)
                {
                    gameDownloader.Download(game.RemotePath, Properties.Settings.Default.InstallPath);

                    //MessageBox.Show("Installation concluded");
                    //mainWindow.guna2Panel2.Invoke(new Action(() => mainWindow.guna2Panel2.Hide()));

                    //mainWindow.InstallGame(game.RemotePath, Properties.Settings.Default.InstallPath);

                    //gameDownloader.DownloadAsync(game.RemotePath, Properties.Settings.Default.InstallPath, mainWindow.cancellationTokenSource);
                }
            }
        }

        public async void Uninstall(string gameName)
        {


            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            CheckInstalledGames checkInstalledGames = new CheckInstalledGames();

            foreach (Game game in checkInstalledGames.GetAllGames())
            {
                if (game.ID == gameName)
                {
                    if (MessageBox.Show("Are you sure you want to uninstall " + game.Name + " ?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        mainWindow.UninstallGame(Properties.Settings.Default.InstallPath + "\\" + game.Name);
                    }
                    else { }
                }
            }
        }

        public async void Play(string gameName)
        {
            CheckInstalledGames checkInstalledGames = new CheckInstalledGames();

            foreach (Game game in checkInstalledGames.GetAllGames())
            {
                if (game.ID == gameName)
                {
                    Process.Start(Properties.Settings.Default.InstallPath + game.EXEPath);

                    /*Discord.Discord discord = new Discord.Discord(828327510344859678, (ulong)Discord.CreateFlags.Default);

                    discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
                    {
                        MessageBox.Show($"Log[{level}] {message}");
                    });

                    var applicationManager = discord.GetApplicationManager();
                    // Get the current locale. This can be used to determine what text or audio the user wants.
                    MessageBox.Show($"Current Locale: {applicationManager.GetCurrentLocale()}");
                    // Get the current branch. For example alpha or beta.
                    MessageBox.Show($"Current Branch: {applicationManager.GetCurrentBranch()}");

                    var userManager = discord.GetUserManager();
                    // The auth manager fires events as information about the current user changes.
                    // This event will fire once on init.
                    //
                    // GetCurrentUser will error until this fires once.
                    userManager.OnCurrentUserUpdate += () =>
                    {
                        var currentUser = userManager.GetCurrentUser();
                        MessageBox.Show(currentUser.Username);
                        MessageBox.Show(currentUser.Id.ToString());
                    };

                    // If you store Discord user ids in a central place like a leaderboard and want to render them.
                    // The users manager can be used to fetch arbitrary Discord users. This only provides basic
                    // information and does not automatically update like relationships.
                    userManager.GetUser(4444444444444, (Discord.Result result, ref Discord.User user) =>
                    {
                        if (result == Discord.Result.Ok)
                        {
                            MessageBox.Show($"user fetched: {user.Username}");
                        }
                        else
                        {
                            MessageBox.Show($"user fetch error: {result}");
                        }
                    });

                    var activityManager = discord.GetActivityManager();

                    var activity = new Discord.Activity
                    {
                        State = "olleh",
                        Details = "foo details",
                        Timestamps =
            {
                Start = 5,
                End = 6,
            },
                        Assets =
            {
                LargeImage = "foo largeImageKey",
                LargeText = "foo largeImageText",
                SmallImage = "foo smallImageKey",
                SmallText = "foo smallImageText",
            },
                        Party = {
               Id = "1234",
               Size = {
                    CurrentSize = 4,
                    MaxSize = 4,
                },
            },
                        Secrets = {
                Join = "1221213122321",
            },
                        Instance = true,
                    };

                    activityManager.UpdateActivity(activity, result =>
                    {
                        MessageBox.Show($"Update Activity {result}");

                        // Send an invite to another user for this activity.
                        // Receiver should see an invite in their DM.
                        // Use a relationship user's ID for this.
                        // activityManager
                        //   .SendInvite(
                        //       364843917537050624,
                        //       Discord.ActivityActionType.Join,
                        //       "",
                        //       inviteResult =>
                        //       {
                        //           Console.WriteLine("Invite {0}", inviteResult);
                        //       }
                        //   );
                    });*/
                }
            }
        }

        public async void Close()
        {
            Environment.Exit(0);
        }

        public async void Minimize()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            mainWindow.WindowState = FormWindowState.Minimized;
        }

        public async void Maximize()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            if (mainWindow.WindowState == FormWindowState.Maximized)
            {
                mainWindow.WindowState = FormWindowState.Normal;
            }
            else if (mainWindow.WindowState == FormWindowState.Normal)
            {
                mainWindow.WindowState = FormWindowState.Maximized;
            }
        }

        public async void ArcadePage()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            mainWindow.webView1.CoreWebView2.NavigateToString(gameHTML.GenerateArcadePage());
        }

        public async void InstalledPage()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            mainWindow.webView1.CoreWebView2.NavigateToString(gameHTML.GenerateInstalledGamesPage());
        }

        public async void SettingsPage()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            mainWindow.webView1.CoreWebView2.NavigateToString(gameHTML.GenerateSettingsPage());
            await Task.Run(() => Task.Delay(200));
            string path = Properties.Settings.Default.InstallPath.Replace(@"\", @"\\");
            await mainWindow.webView1.ExecuteScriptAsync($"document.getElementById('path').value = '{ path }';");
        }

        public async void BrowseDownloadFolder()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            if (mainWindow.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var path = mainWindow.folderBrowserDialog1.SelectedPath;
                path = path.Replace(@"\", @"\\");
                await mainWindow.webView1.ExecuteScriptAsync($"document.getElementById('path').value = '{ path }';");
            }
        }

        public async void SaveSettings(string path, string username, int parallelTransfers)
        {
            Properties.Settings.Default.InstallPath = path;
            Properties.Settings.Default.User.Username = username;
            Properties.Settings.Default.ParallelTransfers = parallelTransfers;
            Properties.Settings.Default.Save();
            MessageBox.Show($"Saved '{ path }' as download location" + Environment.NewLine + $"Saved '{parallelTransfers}' as number of parallel transfers" + Environment.NewLine + $"Saved '{username}' as username");
        }

        public async void PickAvatar()
        {
            MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.PNG;*.JPG;*.GIF)|*.PNG;*.JPG;*.GIF|All files (*.*)|*.*";
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var path = openFileDialog.FileName;
                //File.Copy(path, Application.StartupPath + "/images/avatar.png", true);
                user.Avatar = (Bitmap)Bitmap.FromFile(path);
                user.Avatar.Save(Application.StartupPath + "/images/avatar.png");
                await mainWindow.webView1.ExecuteScriptAsync($"document.getElementById('avatar').src = '{ "http://images/avatar.png?t=" + DateTime.Now }';");
            }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class EventForwarder
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        readonly IntPtr target;

        public EventForwarder(IntPtr target)
        {
            this.target = target;
        }

        public void MouseDownDrag()
        {
            ReleaseCapture();
            SendMessage(target, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }

    public static class Constants
    {
        //modifiers
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;


        //windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }

    public class KeyHandler
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private int key;
        private IntPtr hWnd;
        private int id;
        private int modifier;

        public KeyHandler(int modifier, Keys key, Form form)
        {
            this.modifier = modifier;
            this.key = (int)key;
            this.hWnd = form.Handle;
            id = this.GetHashCode();
        }

        public override int GetHashCode()
        {
            return modifier ^ key ^ hWnd.ToInt32();
        }

        public bool Register()
        {
            return RegisterHotKey(hWnd, id, modifier, key);
        }

        public bool Unregister()
        {
            return UnregisterHotKey(hWnd, id);
        }
    }

    public class User
    {
        public string Username { get; set; }
        public Bitmap Avatar { get; set; }
    }
}
