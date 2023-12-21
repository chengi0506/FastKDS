using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using log4net;
<<<<<<< HEAD
using System.Threading;
=======
>>>>>>> 728cc880c8cfc9ac557b6e69bcc28b4a82b20cfb

namespace FastKDSPrint
{
    public partial class Service1 : ServiceBase
    {
        private static ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FileSystemWatcher watcher;

        public Service1()
        {
            InitializeComponent();

            // 檢查processPath目錄是否存在
            if (!Directory.Exists(Properties.Settings.Default.processPath))
            {
                Directory.CreateDirectory(Properties.Settings.Default.processPath);
            }

            // 檢查finishPath目錄是否存在
            if (!Directory.Exists(Properties.Settings.Default.finishPath))
            {
                Directory.CreateDirectory(Properties.Settings.Default.finishPath);
            }
        }

        protected override void OnStart(string[] args)
        {
            // 初始化 FileSystemWatcher
            watcher = new FileSystemWatcher();
            watcher.Path = Properties.Settings.Default.processPath; // 監控的目錄
            watcher.Filter = "*.*"; // 監控所有檔案
            watcher.EnableRaisingEvents = true;

            Log.Info("開始監控");

            // 設定事件處理程序
            watcher.Created += OnFileCreated;
        }

        protected override void OnStop()
        {
            Log.Info("停止監控");
            // 停止監控
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                Log.Info($"監控到檔案:{e.Name} {e.FullPath}");

                // 檢查檔案名稱是否以 "KDS_" 開頭
                if (e.Name.StartsWith("KDS_", StringComparison.OrdinalIgnoreCase))
                {
                    // 在這裡處理檔案列印及搬移的邏輯
                    string fileName = e.Name;
                    string sourcePath = e.FullPath;
                    string destinationPath = Properties.Settings.Default.finishPath + fileName;

                    // 假設你有一個列印函式 PrintFile，用於列印指定的檔案
<<<<<<< HEAD
                    Thread.Sleep(1000);
=======
>>>>>>> 728cc880c8cfc9ac557b6e69bcc28b4a82b20cfb
                    PrintFile(sourcePath, Properties.Settings.Default.printerName);

                    // 搬移檔案到完成目錄
                    File.Move(sourcePath, destinationPath);

                    Log.Info($"檔案 {fileName} 已列印並移至完成目錄");
                }
                else
                {
                    Log.Info($"檔案 {e.Name} 不符合列印條件，未處理");
                }
            }
            catch (Exception ex)
            {
                // 處理例外狀況
                Log.Error($"在處理檔案 {e.Name} 時發生錯誤: {ex.Message}");
            }
        }

        private void PrintFile(string filePath, string printerName)
        {
            try
            {
                // 創建 PrintDocument 實例
                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = printerName;


                // 設定 PrintPage 事件處理程序
                pd.PrintPage += (sender, e) =>
                {
                    // 讀取文字檔內容
                    string[] lines = File.ReadAllLines(filePath);
                    // 取得字型和字型大小
                    string fontName = Properties.Settings.Default.fontName;
                    float fontSize = Properties.Settings.Default.fontSize;
                    // 設定字型和格式
                    using (Font font = new Font(fontName, fontSize))
                    {
                        // 設定列印位置
                        float yPos = 0;

                        // 插入LOGO圖片
                        string logoFilePath = Properties.Settings.Default.logoPath; // 替換成您的LOGO圖片路徑
                        if (File.Exists(logoFilePath))
                        {
<<<<<<< HEAD
=======
                            Log.Info($"插入LOGO圖片:{logoFilePath}");
>>>>>>> 728cc880c8cfc9ac557b6e69bcc28b4a82b20cfb
                            Image logoImage = Image.FromFile(logoFilePath);
                            e.Graphics.DrawImage(logoImage, new PointF(10, yPos));
                            yPos += logoImage.Height + 10; // 設定LOGO和文字之間的間距
                        }

                        // 列印文字
                        foreach (string line in lines)
                        {
                            e.Graphics.DrawString(line, font, Brushes.Black, 10, yPos);
                            yPos += font.GetHeight();
                        }
                    }

                    // 設定 HasMorePages 為 false，表示沒有更多頁要列印
                    e.HasMorePages = false;
                };

                Log.Info($"執行列印:{filePath}");

                // 執行列印
<<<<<<< HEAD
                for (int i = 0; i < Properties.Settings.Default.PrintNum; i++)
                {
                    pd.Print();
                }
=======
                pd.Print();
>>>>>>> 728cc880c8cfc9ac557b6e69bcc28b4a82b20cfb

                // 手動呼叫 Dispose
                pd.Dispose();
            }
            catch (Exception ex)
            {
                // 處理列印錯誤
                Console.WriteLine($"列印檔案時發生錯誤: {ex.Message}");
            }
        }
    }
}
