using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dropbox.Api;
using Dropbox.Api.Common;
using Dropbox.Api.Files;
using Dropbox.Api.Team;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Drawing;

namespace ArcadeLauncher
{
    public class GameDownloader
    {
        public long totalSize = 0;
        public long downloadedSize = 0;
        public long oldDownloadedSize = 0;
        public double progress = 0;
        static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;
        System.Timers.Timer downloadTimer = new System.Timers.Timer(200);
        System.Timers.Timer speedTimer = new System.Timers.Timer(2000);
        string accessToken = "";
        static WebRequestHandler WebRequestHandler = new WebRequestHandler { ReadWriteTimeout = 300 * 1000 * 1000, MaxConnectionsPerServer = 8, MaxResponseHeadersLength = 2048 };
        private static HttpClient httpClient = new HttpClient(WebRequestHandler) { Timeout = TimeSpan.FromMinutes(600) };
        CancellationTokenSource cts = new CancellationTokenSource();

        public void DownloadAsync(string remotePath, string destinationPath)
        {
            Task.Run(async () => await Download(remotePath, destinationPath));
        }

        public async Task Download(string remotePath, string destPath)
        {
            //Process.Start(Application.StartupPath + @"\ArcadeDownloader.exe", @"/C """ + remotePath + @""" """ + destPath + @"""");
            Process downloader = new Process();
            downloader.StartInfo.FileName = Application.StartupPath + @"\ArcadeDownloader.exe";
            downloader.StartInfo.Arguments = @"""" + remotePath + @""" """ + destPath + @""" " + Properties.Settings.Default.ParallelTransfers;
            downloader.StartInfo.UseShellExecute = false;
            downloader.StartInfo.CreateNoWindow = true;
            downloader.StartInfo.RedirectStandardOutput = true;

            downloader.OutputDataReceived += Downloader_OutputDataReceived;
            downloader.Start();
            downloader.BeginOutputReadLine();

        }

        private void Downloader_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(Process.GetProcessesByName("ArcadeDownloader").Length == 1)
            {
                try
                {
                    mainWindow.label1.Invoke(new Action(() => mainWindow.label1.Text = e.Data));
                    mainWindow.guna2ProgressBar1.Invoke(new Action(() => mainWindow.guna2ProgressBar1.Value = (int)(double.Parse(e.Data.Substring(e.Data.IndexOf("%") - 4, 4)) * 10)));
                }
                catch (Exception ex)
                {
                    // Do not handle this exception
                }
            }
            else if(Process.GetProcessesByName("ArcadeDownloader").Length > 1)
            {
                foreach (var item in Process.GetProcessesByName("ArcadeDownloader").Select((value, i) => new { i, value }))
                {
                    if(item.i > 1)
                    {
                        MessageBox.Show("Only one instance of downloader can be open");
                        item.value.Kill();
                    }
                }
            }
        }

        private void DownloadTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            /*MainWindow mainWindow = Application.OpenForms["MainWindow"] as MainWindow;

            //(int)((double)downloadedSize / 1024 / (double)stopwatch.ElapsedMilliseconds * 1000)

            string downloadSizeShow = "";

            if((double)downloadedSize / 1073741824 > 1)
            {
                downloadSizeShow = ((double)downloadedSize / 1073741824).ToString("0.0") + "GB";
            }
            else
            {
                downloadSizeShow = ((double)downloadedSize / 1048576).ToString("0") + "MB";
            }

            var ETA = TimeSpan.FromSeconds(((totalSize - downloadedSize) / 1048576) / currentSpeed).ToString().Remove(TimeSpan.FromSeconds(((totalSize - downloadedSize) / 1048576) / currentSpeed).ToString().IndexOf("."));

            progress = downloadedSize * 100 / totalSize;
            string elapsed = stopwatch.Elapsed.ToString().Remove(stopwatch.Elapsed.ToString().IndexOf("."));

            if (mainWindow.InvokeRequired)
            {
                mainWindow.guna2ProgressBar1.Invoke(new Action(() => mainWindow.guna2ProgressBar1.Value = (int)progress));
                mainWindow.textBox1.Invoke(new Action(() => mainWindow.textBox1.Text = "Downloaded: " + downloadSizeShow + "  Total Size: " + ((double)totalSize / 1073741824).ToString("0.0") + "GB  Percentage: " + progress.ToString() + "%  Speed: " + currentSpeed.ToString("0.0") + "MB/s"));
                mainWindow.textBox2.Invoke(new Action(() => mainWindow.textBox2.Text = "Elapsed: " + elapsed + "     ETA: " + ETA));
            }*/
        }
    }
}
