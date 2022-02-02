using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;

namespace ArcadeLauncher
{
    public partial class Login : Form
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

        public Login()
        {
            InitializeComponent();
            InitializeAsync();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
            guna2Panel1.MouseDown += Login_MouseDown;
            webView1.SourceChanged += WebView1_SourceChanged;
        }

        private async void WebView1_SourceChanged(object sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            if(webView1.Source.ToString().Contains("access_token"))
            {
                MainWindow mainWindow = new MainWindow();

                string source = webView1.Source.ToString();
                string access_token = source.Substring(source.IndexOf("access_token") + 13, 30);
                HttpWebRequest webRequest1 = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/@me");
                webRequest1.Method = "Get";
                webRequest1.ContentLength = 0;
                webRequest1.Headers.Add("Authorization", "Bearer " + access_token);
                webRequest1.ContentType = "application/x-www-form-urlencoded";

                string userJson = "";
                using (HttpWebResponse response1 = webRequest1.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader1 = new StreamReader(response1.GetResponseStream());
                    userJson = reader1.ReadToEnd();
                }

                UserDiscord user = JsonConvert.DeserializeObject<UserDiscord>(userJson);
                //Properties.Settings.Default.User = user;
                Properties.Settings.Default.Save();

                if(!String.IsNullOrWhiteSpace(user.Avatar))
                {
                    using(WebClient wc = new WebClient())
                    {
                        wc.DownloadFile("https://cdn.discordapp.com/avatars/" + user.Id + "/" + user.Avatar + ".png", Application.StartupPath + "\\images\\avatar.png");
                    }
                }
                else if(String.IsNullOrWhiteSpace(user.Avatar))
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile("https://cdn.discordapp.com/embed/avatars/" + (Convert.ToInt32(user.Discriminator) % 5).ToString() + ".png", Application.StartupPath + "\\images\\avatar.png");
                    }
                }
                

                mainWindow.Show();
                this.Hide();
            }
            else if(webView1.Source.ToString().Contains("error"))
            {
                string source = webView1.Source.ToString();
                string error = source.Substring(source.IndexOf("error") + 6, source.LastIndexOf("&error_description") - (source.IndexOf("error") + 6));
                string error_description = source.Substring(source.IndexOf("error_description") + 18).Replace("+", " ");
                MessageBox.Show("Error: " + error + Environment.NewLine + "Error description: " + error_description, "Discord login error");
                webView1.Source = new Uri("https://discord.com/api/oauth2/authorize?response_type=token&client_id=827127417810059286&scope=identify");
            }
        }

        private async void InitializeAsync()
        {
            GameHTML gameHTML = new GameHTML();
            var creatonProperties = new Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties();
            creatonProperties.BrowserExecutableFolder = Application.StartupPath + "\\WebView2";
            webView1.CreationProperties = creatonProperties;
            await webView1.EnsureCoreWebView2Async(null);
            webView1.CoreWebView2.SetVirtualHostNameToFolderMapping("images.arcade", Application.StartupPath + "\\images", Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
            webView1.Source = new Uri("https://discord.com/api/oauth2/authorize?response_type=token&client_id=827127417810059286&scope=identify");
            webView1.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;
        }

        private void CoreWebView2_WebResourceResponseReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            /*if(e.Request != null)
            {
                Properties.Settings.Default.User.auth_token = e.Request.Headers.GetHeader("Authorization");
                Properties.Settings.Default.Save();
                MessageBox.Show(e.Request.Headers.GetHeader("Authorization"));
            }*/
        }

        private void Login_Load(object sender, EventArgs e)
        {

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

        public Login loginWin()
        {
            return this;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            //mainWindow.Show();
            this.Hide();
            //this.WindowState = FormWindowState.Minimized;

            /*if ((guna2TextBox1.Text == "test") && (guna2TextBox2.Text == "test123"))
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show(this);
                this.Close();
            }
            else 
            {
                MessageBox.Show("Wrong credentials!");  
            };*/
        }
    }

    public class UserDiscord
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
        public string Discriminator { get; set; }
        public int PublicFlags { get; set; }
        public int Flags { get; set; }
        public string Locale { get; set; }
        public bool mfaEnabled { get; set; }
        public string auth_token { get; set; }
    }
}
