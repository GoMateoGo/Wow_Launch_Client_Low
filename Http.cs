using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using HandyControl.Controls;

namespace WowLaunchApp
{
    class Http
    {
        // 声明一个公共静态异步方法，用于下载指定URL的文件并保存到指定路径中
        public static async Task<bool> DownloadResource(string url, string downloadPath, Action<double> progress = default, Action<bool> startBtnStatus = default, CancellationToken cancelationToken = default)
        {
            try
            {
                // 在刚开始时时禁止开始按钮
                startBtnStatus?.Invoke(false);
                var httpClient = new HttpClient(new HttpClientHandler() { CookieContainer = new CookieContainer(), UseProxy = false });

                var response = await httpClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                long contentLength = response.Content.Headers.ContentLength ?? 0;

                // 在开始下载时触发显示进度条的逻辑
                progress?.Invoke(0);

                var fs = System.IO.File.Open(downloadPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                byte[] buffer = new byte[65536];
                var httpStream = await response.Content.ReadAsStreamAsync();
                int readLength = 0;

                while ((readLength = await httpStream.ReadAsync(buffer, 0, buffer.Length, cancelationToken)) > 0)
                {
                    if (cancelationToken.IsCancellationRequested)
                    {
                        fs.Close();
                        File.Delete(downloadPath);
                        return false;
                    }

                    await fs.WriteAsync(buffer, 0, readLength, cancelationToken);
                    progress?.Invoke(Math.Round((double)fs.Length / contentLength * 100, 2));
                }

                fs.Close(); // 关闭文件流
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static async Task<bool> DownloadAndExtractResource(string url, Action<double> progress = null, Action<bool> startBtnStatus = default, CancellationToken cancelationToken = default)
        {
            try
            {
                string downloadPath = "downloaded.zip"; // 下载文件保存的位置
                string extractPath = AppDomain.CurrentDomain.BaseDirectory; // 客户端解压缩位置为当前程序执行的当前目录

                // 1. 下载文件
                var downloadSuccess = await DownloadResource(url, downloadPath, progress, startBtnStatus, cancelationToken);
                if (!downloadSuccess)
                {
                    return false;
                }

                // 2. 解压缩文件
                using (ZipArchive archive = ZipFile.OpenRead(downloadPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = System.IO.Path.Combine(extractPath, entry.FullName);

                        // 处理文件名冲突，如果文件已存在，先删除
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }

                        // 确保目标目录存在
                        string destinationDir = System.IO.Path.GetDirectoryName(destinationPath);
                        if (!Directory.Exists(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }

                        entry.ExtractToFile(destinationPath);
                        File.SetLastWriteTime(destinationPath, DateTime.Now);
                    }
                }

                // 3. 计算md5值并更新md5.txt文件
                string md5Value = Utils.CalculateMD5(downloadPath);
                string md5FilePath = System.IO.Path.Combine(extractPath, "md5.txt");

                if (File.Exists(md5FilePath))
                {
                    File.Delete(md5FilePath);
                }

                using (StreamWriter writer = File.CreateText(md5FilePath))
                {
                    await writer.WriteAsync(md5Value);
                }

                // 4. 删除下载的ZIP文件
                //File.Delete(downloadPath);
                startBtnStatus?.Invoke(true);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        //post请求计算md5是否相同
        public static async Task<bool> PostRequest(string uri, string md5Value)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent($"{md5Value}", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
