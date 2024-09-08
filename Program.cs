using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;

class Program
{
    static void Main()
    {
        Console.WriteLine("Checking if running as admin...");
        // 提升為管理員權限
        if (!IsRunningAsAdmin())
        {
            Console.WriteLine("Not running as admin. Restarting with admin privileges...");
            RestartAsAdmin();
            return;
        }

        Console.WriteLine("Running as admin. Proceeding with installation...");
        Install();
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void Install()
    {
        Console.WriteLine("Setting up security protocol...");
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

        Console.WriteLine("Preparing to install...");
        string installDir = @"C:\Program Files\MCBypass";
        string dllUrl = "https://raw.githubusercontent.com/rhuda21/mcbypass/main/Windows.ApplicationModel.Store.dll";
        string dllPath = Path.Combine(installDir, "Windows.ApplicationModel.Store.dll");

        Console.WriteLine("Creating directory: " + installDir);
        Directory.CreateDirectory(installDir);

        using (WebClient client = new WebClient())
        {
            try
            {
                Console.WriteLine("Downloading file from " + dllUrl + "...");
                client.DownloadFile(dllUrl, dllPath);
                Console.WriteLine("File downloaded successfully.");
            }
            catch (WebException ex)
            {
                Console.WriteLine("Failed to download file: " + ex.Message);
                // 在這裡選擇略過或退出
                return;
            }
        }

        string backupDir = Path.Combine(installDir, "backup");
        Console.WriteLine("Creating backup directory: " + backupDir);
        Directory.CreateDirectory(backupDir);
        string originalDllPath = @"C:\Windows\system32\Windows.ApplicationModel.Store.dll";
        
        try
        {
            Console.WriteLine("Backing up original DLL to " + Path.Combine(backupDir, "Windows.ApplicationModel.Store.dll") + "...");
            File.Copy(originalDllPath, Path.Combine(backupDir, "Windows.ApplicationModel.Store.dll"), overwrite: true);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Failed to backup original DLL: " + ex.Message);
        }

        Console.WriteLine("Killing processes...");
        KillProcesses();

        try
        {
            Console.WriteLine("Deleting original DLL from " + originalDllPath + "...");
            File.Delete(originalDllPath);
            Console.WriteLine("Copying new DLL from " + dllPath + " to " + originalDllPath + "...");
            File.Copy(dllPath, originalDllPath);
            Console.WriteLine("Installation complete.");
        }
        catch (IOException ex)
        {
            Console.WriteLine("Failed during installation: " + ex.Message);
        }

        Console.WriteLine("Remember that after Windows update the crack may be disabled.");
    }

    static void KillProcesses()
    {
        string[] processes = new string[]
        {
            "Gamebar.exe", "RuntimeBroker.exe", "Minecraft.Windows.exe",
            "WinStore.App.exe", "PhoneExperienceHost.exe", "NanaZip.Modern.exe",
            "StoreExperienceHost.exe"
        };

        foreach (string processName in processes)
        {
            Console.WriteLine("Attempting to kill process: " + processName);
            foreach (Process proc in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName)))
            {
                try
                {
                    Console.WriteLine("Killing process: " + processName);
                    proc.Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to kill process " + processName + ": " + ex.Message);
                }
            }
        }
    }

    static bool IsRunningAsAdmin()
    {
        try
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        catch
        {
            return false;
        }
    }

    static void RestartAsAdmin()
    {
        Console.WriteLine("Restarting process with admin privileges...");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = Environment.CommandLine,
            Verb = "runas", // 這會要求提升權限
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to restart as admin: " + ex.Message);
        }
    }
}
