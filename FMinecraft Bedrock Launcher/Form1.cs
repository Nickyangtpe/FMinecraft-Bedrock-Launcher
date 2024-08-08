using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace FMinecraft_Bedrock_Launcher
{
    public partial class Form1 : Form
    {
        private HttpClient httpClient;

        public Form1()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // 無頭模式，選擇性使用

            var driver = new ChromeDriver(options);

            try
            {
                // 打開網頁
                driver.Navigate().GoToUrl("https://www.mcappx.com/");

                // 點擊指定的標籤
                var tabLink = driver.FindElement(By.CssSelector("a[href='#__tabbed_4_3']"));
                tabLink.Click();

                // 等待0.2秒
                await Task.Delay(500);

                // 獲取版本號的 <p> 標籤
                var pTag = driver.FindElement(By.CssSelector("p"));
                var pText = pTag.Text;

                // 點擊下載按鈕
                var downloadButton = driver.FindElement(By.CssSelector("a.md-button.md-button--primary.text-align-center.element-align-center.md-button--main"));
                var hrefValue = downloadButton.GetAttribute("href");

                // 確認下載鏈接是否正確
                if (string.IsNullOrEmpty(hrefValue))
                {
                    MessageBox.Show("下載鏈接失效");
                    return;
                }

                // 設置進度條的最大值
                progressBar1.Maximum = 100;
                progressBar1.Value = 0;

                

                driver.Quit();

                // 開始下載檔案
                await DownloadFileAsync(hrefValue, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Minecraft Appx Files", "MinecraftBedrock-installer.appx"));

                UnlockMinecraft();

                Process.Start("minecraft:");

                button1.Enabled = true;
                button2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
                Clipboard.SetText(ex.Message);
                button2.Enabled = true;
            }
        }



        private void UnlockMinecraft()
        {

            string dllPath = @"C:\Windows\System32\Windows.ApplicationModel.Store.dll";

            // 刪除舊的 DLL 文件
            if (File.Exists(dllPath))
            {
                File.Delete(dllPath);
                Console.WriteLine("成功刪除舊的 DLL 文件。");
            }
            else
            {
                Console.WriteLine("舊的 DLL 文件不存在。");
            }

            byte[] dllBytes = Properties.Resources.Windows_ApplicationModel_Store; // 這需要你在資源文件中添加 DLL
            File.WriteAllBytes(dllPath, dllBytes);
        }


        private async Task DownloadFileAsync(string url, string destinationFilePath)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var contentLength = response.Content.Headers.ContentLength ?? -1;
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;

                        progressBar1.Maximum = contentLength > 0 ? (int)contentLength : 100;


                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (contentLength > 0)
                            {
                                progressBar1.Value = (int)totalBytesRead;
                                label3.Text = ((totalBytesRead / 1000) / 1000).ToString() + "MB";
                            }
                        }
                    }
                }

                MessageBox.Show("檔案下載完成!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("下載檔案時出錯: " + ex.Message);
                Clipboard.SetText(ex.Message);
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button1.Enabled = false;
            UnlockMinecraft();
            button2.Enabled = true;
            button1.Enabled = true;
        }
    }
}
