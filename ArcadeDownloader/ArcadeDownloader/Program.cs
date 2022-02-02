using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Common;
using Dropbox.Api.Files;
using Dropbox.Api.Team;
using System.Threading;
using System.Text;

namespace ArcadeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            EmbeddedAssembly.Load("ArcadeDownloader.Resources.Dropbox.Api.dll", "Dropbox.Api.dll");
            EmbeddedAssembly.Load("ArcadeDownloader.Resources.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");

            if (args.Length == 0)
            {
                Console.WriteLine("No args were given. Exiting.");
                Environment.Exit(1);
            }

            var remotePath = args[0];
            var destPath = args[1];
            var parallelTransfers = int.Parse(args[2]);

            Task.Run(async () => await (new GameDownloader()).Download(remotePath, destPath, parallelTransfers)).Wait();

            Console.Read();
        }
    }



    public class GameDownloader
    {
        public long totalSize = 0;
        public long downloadedSize = 0;
        public long oldDownloadedSize = 0;
        public double progress = 0;
        public int totalFiles = 0;
        public int downloadedFiles = 0;
        public List<string> currentFiles = new List<string>();
        static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        System.Timers.Timer downloadTimer = new System.Timers.Timer(200);
        System.Timers.Timer speedTimer = new System.Timers.Timer(2000);
        string accessToken = "";
        static WebRequestHandler WebRequestHandler = new WebRequestHandler { ReadWriteTimeout = 300 * 1000 * 1000, MaxConnectionsPerServer = 8, MaxResponseHeadersLength = 2048 };
        //private static HttpClient httpClient = new HttpClient(WebRequestHandler) { Timeout = TimeSpan.FromMinutes(600) };
        //CancellationTokenSource cts = new CancellationTokenSource();
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(4);
        List<Task> taskList = new List<Task>();

        public static long GetFileSize(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return file.Length;
            }
        }

        public void DownloadAsync(string remotePath, string destinationPath, int parallelTransfers)
        {
            Task.Run(async () => await Download(remotePath, destinationPath, parallelTransfers));
        }

        public async Task Download(string remotePath, string destPath, int parallelTransfers)
        {
            downloadTimer.Enabled = true;
            speedTimer.Enabled = true;
            downloadTimer.Elapsed += DownloadTimer_Tick;
            speedTimer.Elapsed += SpeedTimer_Elapsed;
            speedTimer.Start();
            downloadTimer.Start();
            stopwatch.Start();

            //MessageBox.Show(WebRequestHandler.MaxResponseHeadersLength.ToString());
            //MessageBox.Show(WebRequestHandler.MaxConnectionsPerServer.ToString());

            if (remotePath.Contains("Dropbox2"))
            {
                accessToken = "7_97qS8twEwAAAAAAAAAAacnqGuJ-9f6Ji3DWWqhKY9-84aqt0q2i-BajxOI-iT2";
                remotePath = remotePath.Replace("Dropbox2:", "/");
            }
            else if (remotePath.Contains("Dropbox"))
            {
                accessToken = "xSDBvzUA3AoAAAAAAAAAAaT5LhaX6Hm1y_Ti7bvQ4sdoMrmI6yJNDPP8sk1hba67";
                remotePath = remotePath.Replace("Dropbox:", "/");
            }

            var config = new DropboxClientConfig()
            {
                HttpClient = new HttpClient(WebRequestHandler) { Timeout = TimeSpan.FromMinutes(60) }
            };

            var client = new DropboxClient(accessToken, config);

            var list = await ListFolder(client, remotePath);

            // Get Size of Directory

            foreach (Metadata metadata in list.Entries)
            {
                if (metadata.IsFile)
                {
                    if (!File.Exists(destPath + metadata.AsFile.PathDisplay) || GetFileSize(destPath + metadata.AsFile.PathDisplay) < (long)metadata.AsFile.Size)
                        totalSize = totalSize + (long)metadata.AsFile.Size;
                    totalFiles = totalFiles + 1;
                }
                if (metadata.IsFolder)
                {
                    GetFileSizesInSubdirectory(metadata, client, destPath);
                }
            }

            //await Task.Run(() => Thread.Sleep(10000));

            // Download All Files in Directory

            semaphoreSlim = new SemaphoreSlim(parallelTransfers);

            foreach (Metadata metadata in list.Entries)
            {

                if (metadata.IsFile)
                {
                    taskList.Add(Task.Run(() => Download(remotePath, destPath, metadata.AsFile)));
                }
                if (metadata.IsFolder)
                {
                    await DownloadFilesInSubdirectory(metadata, client, destPath);
                }

            }

            // Wait for all downloads to complete
            Task.WaitAll(taskList.ToArray());

            Console.WriteLine("Download concluded");
            Environment.Exit(0);
        }

        double currentSpeed = 0;

        private void SpeedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            long downloadedInTick = downloadedSize - oldDownloadedSize;
            currentSpeed = (double)downloadedInTick / 1048576 / 2;
            oldDownloadedSize = downloadedSize;
        }

        private void DownloadTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            //(int)((double)downloadedSize / 1024 / (double)stopwatch.ElapsedMilliseconds * 1000)

            string downloadSizeShow = "";

            if ((double)downloadedSize / 1073741824 > 1)
            {
                downloadSizeShow = ((double)downloadedSize / 1073741824).ToString("0.0") + "GB";
            }
            else
            {
                downloadSizeShow = ((double)downloadedSize / 1048576).ToString("0") + "MB";
            }

            var ETA = TimeSpan.FromSeconds(((totalSize - downloadedSize) / 1048576) / currentSpeed).ToString().Remove(TimeSpan.FromSeconds(((totalSize - downloadedSize) / 1048576) / currentSpeed).ToString().IndexOf("."));

            progress = (double)downloadedSize * (double)100 / (double)totalSize;
            string elapsed = stopwatch.Elapsed.ToString().Remove(stopwatch.Elapsed.ToString().IndexOf("."));

            //Console.Clear();
            //Console.SetCursorPosition(0, 0);
            Console.WriteLine("Downloaded: " + downloadSizeShow + " of " + ((double)totalSize / 1073741824).ToString("0.0") + "GB  - " + progress.ToString("0.0") + "% -  Speed: " + currentSpeed.ToString("0.0") + "MB/s"  + " -  Files: " + downloadedFiles + "/" + totalFiles + " -  Current Files: " + currentFiles.ToList().Count +" -  Elapsed: " + elapsed + " -  ETA: " + ETA);
            //Console.WriteLine("Current Files: " + Environment.NewLine + String.Join(Environment.NewLine, currentFiles));

            if(progress == 100)
            {
                Environment.Exit(0);
            }

            /*if (mainWindow.InvokeRequired)
            {
                mainWindow.guna2ProgressBar1.Invoke(new Action(() => mainWindow.guna2ProgressBar1.Value = (int)progress));
                mainWindow.textBox1.Invoke(new Action(() => mainWindow.textBox1.Text = "Downloaded: " + downloadSizeShow + "  Total Size: " + ((double)totalSize / 1073741824).ToString("0.0") + "GB  Percentage: " + progress.ToString() + "%  Speed: " + currentSpeed.ToString("0.0") + "MB/s"));
                mainWindow.textBox2.Invoke(new Action(() => mainWindow.textBox2.Text = "Elapsed: " + elapsed + "     ETA: " + ETA));
            }*/
        }

        public async Task DownloadFilesInSubdirectory(Metadata metadata, DropboxClient client, string destPath)
        {
            var subFolderPath = metadata.AsFolder.PathDisplay;
            var subFolderList = await ListFolder(client, subFolderPath);
            foreach (Metadata metadata2 in subFolderList.Entries)
            {
                if (metadata2.IsFile)
                {
                    taskList.Add(Task.Run(() => Download(subFolderPath, destPath, metadata2.AsFile)));
                }
                if (metadata2.IsFolder)
                {
                    await DownloadFilesInSubdirectory(metadata2, client, destPath);
                }
            }
        }

        public async Task GetFileSizesInSubdirectory(Metadata metadata, DropboxClient client, string destPath)
        {
            var subFolderPath = metadata.AsFolder.PathDisplay;
            var subFolderList = await ListFolder(client, subFolderPath);
            foreach (Metadata metadata2 in subFolderList.Entries)
            {
                if (metadata2.IsFile)
                {
                    if (!File.Exists(destPath + metadata2.AsFile.PathDisplay) || GetFileSize(destPath + metadata2.AsFile.PathDisplay) < (long)metadata2.AsFile.Size)
                        totalSize = totalSize + (long)metadata2.AsFile.Size;
                    totalFiles = totalFiles + 1;
                }
                if (metadata2.IsFolder)
                {
                    GetFileSizesInSubdirectory(metadata2, client, destPath);
                }
            }
        }

        private async Task<ListFolderResult> ListFolder(DropboxClient client, string path)
        {
            var list = await client.Files.ListFolderAsync(path);
            return list;
        }

        private async Task Download(string folder, string destPath, FileMetadata file)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!File.Exists(destPath + file.PathDisplay) || GetFileSize(destPath + file.PathDisplay) < (long)file.Size)
                {
                    var request = (HttpWebRequest)WebRequest.Create("https://content.dropboxapi.com/2/files/download");
                    request.Method = "POST";
                    request.Headers.Add("Authorization", "Bearer " + accessToken);
                    request.Headers.Add("Dropbox-API-Arg", "{\"path\": \"" + folder + "/" + file.Name + "\"}");
                    request.ServicePoint.ConnectionLimit = 128;
                    request.Timeout = (int)TimeSpan.FromMinutes(600).TotalMilliseconds;
                    request.ServicePoint.ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(600).TotalMilliseconds;

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        var result = response.Headers.GetValues("Dropbox-API-Result").Last();
                        var pathDisplay = result.Substring(result.IndexOf("path_display") + 16, result.IndexOf("\", \"id") - (result.IndexOf("path_display") + 16));
                        var fileName = result.Substring(result.IndexOf("name") + 8, result.IndexOf("\", \"path_lower") - (result.IndexOf("name") + 8));

                        Directory.CreateDirectory(destPath + pathDisplay.Substring(0, pathDisplay.Length - fileName.Length));
                        const int bufferSize = 128 * 1024 * 1024;
                        long oldLenght = 0;
                        var buffer = new byte[bufferSize];

                        using (var stream = response.GetResponseStream())
                        {
                            using (var fileStream = new FileStream(destPath + pathDisplay, FileMode.OpenOrCreate))
                            {
                                var length = stream.Read(buffer, 0, bufferSize);

                                while (length > 0)
                                {
                                    fileStream.Write(buffer, 0, length);
                                    long newLenght = fileStream.Length;
                                    downloadedSize = downloadedSize - oldLenght + newLenght;
                                    oldLenght = newLenght;
                                    length = stream.Read(buffer, 0, bufferSize);
                                    if (!currentFiles.Contains(pathDisplay))
                                        currentFiles.Add(pathDisplay);
                                }
                            }
                        }
                        if (currentFiles.Contains(pathDisplay))
                            currentFiles.Remove(pathDisplay);
                    }
                }
                else
                {
                    //Console.WriteLine(file.Name + " already exists, skipping download.");
                    //totalSize = totalSize - (long)file.Size;
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {

                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                downloadedFiles = downloadedFiles + 1;
                semaphoreSlim.Release();
            }



            /*var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://content.dropboxapi.com/2/files/download"),
                Headers = {
            { HttpRequestHeader.Authorization.ToString(), "Bearer " + accessToken},
            { "Dropbox-API-Arg", "{\"path\": \"" + folder + "/" + file.Name + "\"}" }
            },
                Content = new StringContent("")
            };

            /using (HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var result = response.Headers.GetValues("Dropbox-API-Result").Last();
                var pathDisplay = result.Substring(result.IndexOf("path_display") + 16, result.IndexOf("\", \"id") - (result.IndexOf("path_display") + 16));
                var fileName = result.Substring(result.IndexOf("name") + 8, result.IndexOf("\", \"path_lower") - (result.IndexOf("name") + 8));

                Directory.CreateDirectory(Properties.Settings.Default.InstallPath + pathDisplay.Substring(0, pathDisplay.Length - fileName.Length));
                const int bufferSize = 16 * 1024 * 1024;
                long oldLenght = 0;
                var buffer = new byte[bufferSize];

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = new FileStream(Properties.Settings.Default.InstallPath + pathDisplay, FileMode.OpenOrCreate))
                    {
                        var length = stream.Read(buffer, 0, bufferSize);

                        while (length > 0)
                        {
                            fileStream.Write(buffer, 0, length);
                            long newLenght = fileStream.Length;
                            downloadedSize = downloadedSize - oldLenght + newLenght;
                            oldLenght = newLenght;
                            length = stream.Read(buffer, 0, bufferSize);
                            GC.Collect();
                        }
                    }
                }
            }*/




            /*try
            {
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                MessageBox.Show(reader.ReadToEnd());

                //Directory.CreateDirectory(destPath + response.Response.PathDisplay.Replace(response.Response.Name, ""));
                ulong fileSize = (ulong)response.ContentLength;
                const int bufferSize = 128 * 1024 * 1024;
                long oldLenght = 0;
                var buffer = new byte[bufferSize];

                using (var stream = await response.GetContentAsStreamAsync())
                {
                    using (var fileStream = new FileStream(destPath + response.Response.PathDisplay, FileMode.OpenOrCreate))
                    {
                        var length = stream.Read(buffer, 0, bufferSize);

                        while (length > 0)
                        {
                            fileStream.Write(buffer, 0, length);
                            long newLenght = fileStream.Length;
                            downloadedSize = downloadedSize - oldLenght + newLenght;
                            oldLenght = newLenght;
                            length = stream.Read(buffer, 0, bufferSize);
                        }
                    }
                }
            }


        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        /*try
        {
            var response = await client.Files.DownloadAsync(folder + "/" + file.Name);
            Directory.CreateDirectory(destPath + response.Response.PathDisplay.Replace(response.Response.Name, ""));
            ulong fileSize = response.Response.Size;
            const int bufferSize = 128 * 1024 * 1024;
            long oldLenght = 0;
            var buffer = new byte[bufferSize];

            using (var stream = await response.GetContentAsStreamAsync())
            {
                using (var fileStream = new FileStream(destPath + response.Response.PathDisplay, FileMode.OpenOrCreate))
                {
                    var length = stream.Read(buffer, 0, bufferSize);

                    while (length > 0)
                    {
                        fileStream.Write(buffer, 0, length);
                        long newLenght = fileStream.Length;
                        downloadedSize = downloadedSize - oldLenght + newLenght;
                        oldLenght = newLenght;
                        length = stream.Read(buffer, 0, bufferSize);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }*/
        }
    }
}
