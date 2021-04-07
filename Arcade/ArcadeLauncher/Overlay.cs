using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeLauncher
{
    public partial class Overlay : Form
    {
        OverlayBrowser overlayBrowser = new OverlayBrowser();

        public Overlay()
        {
            InitializeComponent();
            //this.BackColor = Color.FromArgb(25, 255, 255, 255);
            this.Opacity = 0.5;
            this.WindowState = FormWindowState.Maximized;

            this.VisibleChanged += Overlay_VisibleChanged;

            overlayBrowser.Size = new Size((int)(Screen.FromControl(this).Bounds.Width * 0.75), (int)(Screen.FromControl(this).Bounds.Height * 0.75));
            overlayBrowser.StartPosition = FormStartPosition.CenterScreen;
            this.AddOwnedForm(overlayBrowser);
        }


        private void Overlay_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible)
            {
                overlayBrowser.Show();
            }
            else if (!this.Visible)
            {
                overlayBrowser.Hide();
            }
        }

        private void Overlay_Load(object sender, EventArgs e)
        {

        }
    }
}
