using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace TempCleaner
{
    public partial class MainWindow : Window
    {
        private enum AppLanguage { English, Arabic }
        private AppLanguage _currentLanguage = AppLanguage.English;

        private readonly Dictionary<string, string> _englishStrings = new();
        private readonly Dictionary<string, string> _arabicStrings = new();

        // List of UI elements to disable during cleanup
        private List<UIElement> _cleanupControls;

        // Dynamic Windows directory path
        private readonly string _windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        public MainWindow()
        {
            InitializeComponent();
            InitializeLocalizationDictionaries();
            SetLanguage(AppLanguage.English);

            _cleanupControls = new List<UIElement>
            {
                CleanTempButton,
                CleanPrefetchButton,
                CleanRecycleButton,
                FlushDNSButton,
                CleanUpdateButton,
                RunAllButton,
                LangToggleBtn,
                DevInfoButton,
                AutoCloseCheckBox
            };

            LoadLastRunInfo();
        }

        private void InitializeLocalizationDictionaries()
        {
            // English
            _englishStrings["WindowTitle"] = "TempCleaner";
            _englishStrings["MainTitle"] = "TEMP CLEANER";
            _englishStrings["Subtitle"] = "System Optimization Tool";
            _englishStrings["CleanTemp"] = "Clean Temp Files";
            _englishStrings["CleanPrefetch"] = "Clear Prefetch & Recent";
            _englishStrings["CleanRecycle"] = "Empty Recycle Bin";
            _englishStrings["FlushDNS"] = "Flush DNS Cache";
            _englishStrings["CleanUpdate"] = "Clean Win Update Cache";
            _englishStrings["RunAll"] = "RUN ALL CLEANUP";
            _englishStrings["AutoClose"] = "Close app after cleanup";
            _englishStrings["StatusReady"] = "Ready";
            _englishStrings["CleaningTempFiles"] = "Cleaning Temp Files...";
            _englishStrings["CleaningPrefetch"] = "Cleaning Prefetch & Recent...";
            _englishStrings["EmptyingRecycle"] = "Emptying Recycle Bin...";
            _englishStrings["FlushingDNS"] = "Flushing DNS Cache...";
            _englishStrings["CleaningUpdate"] = "Cleaning Windows Update Cache...";
            _englishStrings["RunningFullCleanup"] = "Running Full System Cleanup...";
            _englishStrings["TaskFreedFormat"] = "Task Completed! Freed: {0}";
            _englishStrings["TaskCompletedSuccess"] = "Task Completed Successfully!";
            _englishStrings["DevInfoTitle"] = "Developer Info";
            _englishStrings["DevName"] = "Nasser Al-Nimr";
            _englishStrings["DevNickname"] = "(Mr.Ghost)";
            _englishStrings["DevEmail"] = "alialojeely@gmail.com";
            _englishStrings["GitHubLink"] = "GitHub Profile";
            _englishStrings["Version"] = "version 2.7";
            _englishStrings["CloseButton"] = "CLOSE";
            _englishStrings["LangToggleEN"] = "EN";
            _englishStrings["LangToggleAR"] = "AR";

            // Arabic
            _arabicStrings["WindowTitle"] = "منظف الملفات المؤقتة";
            _arabicStrings["MainTitle"] = "تنظيف الملفات المؤقتة";
            _arabicStrings["Subtitle"] = "أداة تحسين النظام";
            _arabicStrings["CleanTemp"] = "تنظيف الملفات المؤقتة";
            _arabicStrings["CleanPrefetch"] = "مسح Prefetch والمستندات الحديثة";
            _arabicStrings["CleanRecycle"] = "تفريغ سلة المحذوفات";
            _arabicStrings["FlushDNS"] = "مسح ذاكرة DNS المؤقتة";
            _arabicStrings["CleanUpdate"] = "تنظيف ذاكرة تحديث ويندوز";
            _arabicStrings["RunAll"] = "تشغيل التنظيف الشامل";
            _arabicStrings["AutoClose"] = "إغلاق التطبيق بعد التنظيف";
            _arabicStrings["StatusReady"] = "جاهز";
            _arabicStrings["CleaningTempFiles"] = "جاري تنظيف الملفات المؤقتة...";
            _arabicStrings["CleaningPrefetch"] = "جاري مسح Prefetch والمستندات الحديثة...";
            _arabicStrings["EmptyingRecycle"] = "جاري تفريغ سلة المحذوفات...";
            _arabicStrings["FlushingDNS"] = "جاري مسح ذاكرة DNS...";
            _arabicStrings["CleaningUpdate"] = "جاري تنظيف ذاكرة تحديث ويندوز...";
            _arabicStrings["RunningFullCleanup"] = "جاري التنظيف الشامل للنظام...";
            _arabicStrings["TaskFreedFormat"] = "تمت المهمة! تم تحرير: {0}";
            _arabicStrings["TaskCompletedSuccess"] = "تمت المهمة بنجاح!";
            _arabicStrings["DevInfoTitle"] = "معلومات المطور";
            _arabicStrings["DevName"] = "ناصر النمر";
            _arabicStrings["DevNickname"] = "(السيد غوست)";
            _arabicStrings["DevEmail"] = "alialojeely@gmail.com";
            _arabicStrings["GitHubLink"] = "GitHub Profile";
            _arabicStrings["Version"] = "الإصدار 2.7";
            _arabicStrings["CloseButton"] = "إغلاق";
            _arabicStrings["LangToggleEN"] = "EN";
            _arabicStrings["LangToggleAR"] = "عربي";
        }

        private string GetString(string key)
        {
            var dict = _currentLanguage == AppLanguage.English ? _englishStrings : _arabicStrings;
            return dict.TryGetValue(key, out var value) ? value : key;
        }

        private void SetLanguage(AppLanguage language)
        {
            _currentLanguage = language;

            Title = GetString("WindowTitle");
            MainTitleText.Text = GetString("MainTitle");
            SubtitleText.Text = GetString("Subtitle");
            TempCleanText.Text = GetString("CleanTemp");
            PrefetchCleanText.Text = GetString("CleanPrefetch");
            RecycleCleanText.Text = GetString("CleanRecycle");
            FlushDNSText.Text = GetString("FlushDNS");
            UpdateCleanText.Text = GetString("CleanUpdate");
            RunAllButton.Content = GetString("RunAll");
            AutoCloseCheckBox.Content = GetString("AutoClose");
            StatusText.Text = GetString("StatusReady");

            DevModalTitleText.Text = GetString("DevInfoTitle");
            DevNameText.Text = GetString("DevName");
            DevNicknameText.Text = GetString("DevNickname");
            DevEmailText.Text = GetString("DevEmail");
            GitHubLinkText.Text = GetString("GitHubLink");
            VersionText.Text = GetString("Version");
            CloseModalButton.Content = GetString("CloseButton");

            LangToggleBtn.Content = GetString("LangToggleEN");

            if (language == AppLanguage.Arabic)
            {
                LayoutRoot.FlowDirection = FlowDirection.RightToLeft;
                LangToggleBtn.Content = GetString("LangToggleAR");
            }
            else
            {
                LayoutRoot.FlowDirection = FlowDirection.LeftToRight;
            }
        }

        private void LangToggle_Click(object sender, RoutedEventArgs e)
        {
            SetLanguage(_currentLanguage == AppLanguage.English ? AppLanguage.Arabic : AppLanguage.English);
        }

        private void DevInfo_Click(object sender, RoutedEventArgs e) => DevModal.Visibility = Visibility.Visible;
        private void CloseModal_Click(object sender, RoutedEventArgs e) => DevModal.Visibility = Visibility.Collapsed;

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
            e.Handled = true;
        }

        // --------------------------------------------------------
        //  Button Click Handlers
        // --------------------------------------------------------
        private async void CleanTemp_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask(GetString("CleaningTempFiles"), () =>
            {
                long freed = 0;
                freed += CleanDirectory(Path.GetTempPath());
                freed += CleanDirectory(Path.Combine(_windowsDir, "Temp"));
                return freed;
            });
        }

        private async void CleanPrefetch_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask(GetString("CleaningPrefetch"), () =>
            {
                long freed = 0;
                freed += CleanDirectory(Path.Combine(_windowsDir, "Prefetch"));
                string recentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent");
                freed += CleanDirectory(recentPath);
                return freed;
            });
        }

        private async void CleanRecycle_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask(GetString("EmptyingRecycle"), () =>
            {
                ExecuteCommand("powershell.exe", "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"");
                return 0;
            });
        }

        private async void FlushDNS_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask(GetString("FlushingDNS"), () =>
            {
                ExecuteCommand("ipconfig.exe", "/flushdns");
                return 0;
            });
        }

        private async void CleanUpdate_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask(GetString("CleaningUpdate"), () =>
            {
                long freed = 0;
                ExecuteCommand("net.exe", "stop wuauserv");
                ExecuteCommand("net.exe", "stop bits");
                freed += CleanDirectory(Path.Combine(_windowsDir, @"SoftwareDistribution\Download"));
                ExecuteCommand("net.exe", "start wuauserv");
                ExecuteCommand("net.exe", "start bits");
                return freed;
            });
        }

        private async void RunAll_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask(GetString("RunningFullCleanup"), () =>
            {
                long totalFreed = 0;
                totalFreed += CleanDirectory(Path.GetTempPath());
                totalFreed += CleanDirectory(Path.Combine(_windowsDir, "Temp"));
                totalFreed += CleanDirectory(Path.Combine(_windowsDir, "Prefetch"));

                string recentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent");
                totalFreed += CleanDirectory(recentPath);

                ExecuteCommand("powershell.exe", "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"");
                ExecuteCommand("ipconfig.exe", "/flushdns");

                ExecuteCommand("net.exe", "stop wuauserv");
                ExecuteCommand("net.exe", "stop bits");
                totalFreed += CleanDirectory(Path.Combine(_windowsDir, @"SoftwareDistribution\Download"));
                ExecuteCommand("net.exe", "start wuauserv");
                ExecuteCommand("net.exe", "start bits");

                return totalFreed;
            });
        }

        // --------------------------------------------------------
        //  Core cleanup runner with UI disable/enable and LOGGING
        // --------------------------------------------------------
        private async Task RunCleanupTask(string startMessage, Func<long> cleanupAction)
        {
            SetControlsEnabled(false);

            try
            {
                StatusText.Text = startMessage;
                StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB300"));
                CleanProgress.Visibility = Visibility.Visible;

                long bytesFreed = await Task.Run(cleanupAction);

                CleanProgress.Visibility = Visibility.Collapsed;

                // Create a clean action name for the log (removing trailing dots)
                string actionNameForLog = startMessage.Replace("...", "").Trim();

                // Write to our local log file
                WriteToLog(actionNameForLog, bytesFreed);

                if (bytesFreed > 0)
                    StatusText.Text = string.Format(GetString("TaskFreedFormat"), FormatBytes(bytesFreed));
                else
                    StatusText.Text = GetString("TaskCompletedSuccess");

                StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB954"));

                if (AutoCloseCheckBox.IsChecked == true)
                {
                    await Task.Delay(1500);
                    Application.Current.Shutdown();
                }
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            foreach (var control in _cleanupControls)
            {
                control.IsEnabled = enabled;
            }
        }

        // --------------------------------------------------------
        //  Helper methods
        // --------------------------------------------------------

        /// <summary>
        /// Writes a record of the cleanup task to a local log file.
        /// </summary>
        private void WriteToLog(string actionName, long bytesFreed)
        {
            try
            {
                // File will be created in the same folder as the .exe
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempCleaner_History.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string formattedSize = FormatBytes(bytesFreed);

                // Format: [2026-06-30 15:30:00] Action: Cleaning Temp Files | Space Freed: 250.50 MB
                string logEntry = $"[{timestamp}] Action: {actionName} | Space Freed: {formattedSize}{Environment.NewLine}";

                File.AppendAllText(logFilePath, logEntry);
                Dispatcher.Invoke(() => LoadLastRunInfo());
            }
            catch
            {
                // Silently ignore if file is locked so the app doesn't crash
            }
        }

        private long CleanDirectory(string path)
        {
            if (!Directory.Exists(path)) return 0;

            long freedSpace = 0;
            var di = new DirectoryInfo(path);

            foreach (var file in di.GetFiles())
            {
                try
                {
                    long size = file.Length;
                    file.Delete();
                    freedSpace += size;
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            foreach (var dir in di.GetDirectories())
            {
                try
                {
                    freedSpace += CleanDirectory(dir.FullName);
                    dir.Delete(true);
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            return freedSpace;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n2} {suffixes[counter]}";
        }

        private void ExecuteCommand(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }

        private void LoadLastRunInfo()
        {
            try
            {
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempCleaner_History.log");
                if (File.Exists(logFilePath))
                {
                    string[] lines = File.ReadAllLines(logFilePath);
                    for (int i = lines.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrWhiteSpace(lines[i]))
                        {
                            string lastEntry = lines[i].Replace(Environment.NewLine, "");
                            LastRunText.Text = lastEntry;
                            return;
                        }
                    }
                }

                LastRunText.Text = _currentLanguage == AppLanguage.Arabic ? "لا توجد عمليات تنظيف سابقة" : "No previous cleanups found";
            }
            catch
            {
                LastRunText.Text = "";
            }
        }
    }
}