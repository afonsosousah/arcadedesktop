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

namespace ArcadeLauncher
{
    public partial class OverlayBrowser : Form
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

        public OverlayBrowser()
        {
            InitializeComponent();
            InitializeAsync();
            webView1.MouseDown += WebView1_MouseDown;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void WebView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(base.Handle, 161, 2, 0);
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
            webView1.Source = new Uri("https://discord.com/login");
        }

        private void OverlayBrowser_Load(object sender, EventArgs e)
        {
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }

        private void webView1_Click(object sender, EventArgs e)
        {

        }
    }
}
