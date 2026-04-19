using System;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        // --- UI Events (Dev Modal) ---
        private void DevInfo_Click(object sender, RoutedEventArgs e) => DevModal.Visibility = Visibility.Visible;
        private void CloseModal_Click(object sender, RoutedEventArgs e) => DevModal.Visibility = Visibility.Collapsed;

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
            e.Handled = true;
        }

        // --- Cleaning Events ---
        private async void CleanTemp_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask("Cleaning Temp Files...", () =>
            {
                long freed = 0;
                freed += CleanDirectory(Path.GetTempPath());
                freed += CleanDirectory(@"C:\Windows\Temp");
                return freed;
            });
        }

        private async void CleanPrefetch_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask("Cleaning Prefetch & Recent...", () =>
            {
                long freed = 0;
                freed += CleanDirectory(@"C:\Windows\Prefetch");
                string recentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent");
                freed += CleanDirectory(recentPath);
                return freed;
            });
        }

        private async void CleanRecycle_Click(object sender, RoutedEventArgs e)
        {
            // Note: Cannot easily calculate exact size of Recycle Bin via basic CMD, returning 0 for display
            await RunCleanupTask("Emptying Recycle Bin...", () =>
            {
                ExecuteCommand("powershell.exe", "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"");
                return 0;
            });
        }

        private async void FlushDNS_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask("Flushing DNS Cache...", () =>
            {
                ExecuteCommand("ipconfig.exe", "/flushdns");
                return 0;
            });
        }

        private async void CleanUpdate_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask("Cleaning Windows Update Cache...", () =>
            {
                long freed = 0;
                ExecuteCommand("net.exe", "stop wuauserv");
                ExecuteCommand("net.exe", "stop bits");

                freed += CleanDirectory(@"C:\Windows\SoftwareDistribution\Download");

                ExecuteCommand("net.exe", "start wuauserv");
                ExecuteCommand("net.exe", "start bits");
                return freed;
            });
        }

        private async void RunAll_Click(object sender, RoutedEventArgs e)
        {
            await RunCleanupTask("Running Full System Cleanup...", () =>
            {
                long totalFreed = 0;

                totalFreed += CleanDirectory(Path.GetTempPath());
                totalFreed += CleanDirectory(@"C:\Windows\Temp");
                totalFreed += CleanDirectory(@"C:\Windows\Prefetch");

                string recentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent");
                totalFreed += CleanDirectory(recentPath);

                ExecuteCommand("powershell.exe", "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"");
                ExecuteCommand("ipconfig.exe", "/flushdns");

                ExecuteCommand("net.exe", "stop wuauserv");
                ExecuteCommand("net.exe", "stop bits");
                totalFreed += CleanDirectory(@"C:\Windows\SoftwareDistribution\Download");
                ExecuteCommand("net.exe", "start wuauserv");
                ExecuteCommand("net.exe", "start bits");

                return totalFreed;
            });
        }

        // --- Core Logic & Helpers ---

        // Wrapper to handle UI updates, progress bar, and display freed space
        private async Task RunCleanupTask(string startMessage, Func<long> cleanupAction)
        {
            StatusText.Text = startMessage;
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB300")); // Orange warning
            CleanProgress.Visibility = Visibility.Visible; // Show progress bar

            // Run the heavy lifting in the background and get the freed bytes
            long bytesFreed = await Task.Run(cleanupAction);

            CleanProgress.Visibility = Visibility.Collapsed; // Hide progress bar

            // Format the message
            if (bytesFreed > 0)
                StatusText.Text = $"Task Completed! Freed: {FormatBytes(bytesFreed)}";
            else
                StatusText.Text = "Task Completed Successfully!";

            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB954")); // Green success

            // Handle Auto-Close
            if (ChkAutoClose.IsChecked == true)
            {
                await Task.Delay(1500); // Wait 1.5 seconds so user can see the success message
                Application.Current.Shutdown();
            }
        }

        // Safely deletes files/folders and returns the total size in bytes of deleted items
        private long CleanDirectory(string path)
        {
            if (!Directory.Exists(path)) return 0;

            long freedSpace = 0;
            DirectoryInfo di = new DirectoryInfo(path);

            // Delete Files and calculate size
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    long size = file.Length;
                    file.Delete();
                    freedSpace += size; // Add to total only if deletion succeeded
                }
                catch { /* Ignore locked files */ }
            }

            // Delete Sub-folders recursively
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    freedSpace += CleanDirectory(dir.FullName); // Add nested files size
                    dir.Delete(true);
                }
                catch { /* Ignore locked folders */ }
            }

            return freedSpace;
        }

        // Utility to format bytes into readable KB, MB, GB
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n2} {1}", number, suffixes[counter]);
        }

        // Runs hidden system commands (like CMD or PowerShell)
        private void ExecuteCommand(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process process = Process.Start(psi))
                {
                    process?.WaitForExit();
                }
            }
            catch { /* Ignore command execution errors */ }
        }
    }
}