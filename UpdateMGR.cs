using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UpdateMGR;
class Updater {
    // Repository info
    private readonly string _username;
    private readonly string _repository;
    private readonly string _branch;
    private readonly string _rawBaseUrl;

    // Version info
    public Version currentVersion;
    public Version latestVersion;

    // HTTP client
    private readonly HttpClient _httpClient;

    // Executable info
    private readonly string _executablePath;
    private readonly string _executableDirectory;
    private readonly string _appName;
    private readonly string _platform;
    private readonly string _latestAppName;
    
    public Updater(string username, string repository, string version, string appName, string branch = "main") {
        _username = username;
        _repository = repository;
        _branch = branch;
        _appName = appName;

        _platform = GetPlatform();
        currentVersion = Version.Parse(version);
        
        // Get the path of the current executable
        _executablePath = Process.GetCurrentProcess().MainModule.FileName;
        _executableDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        
        // Construct the raw GitHub URL format
        _rawBaseUrl = $"https://raw.githubusercontent.com/{username}/{repository}/{branch}/";
        _httpClient = new HttpClient();
        
        try {
            // Get latest version
            string versionString = GetLatestVersion();
            latestVersion = Version.Parse(versionString);
            
            // Now set the _latestAppName after latestVersion is assigned
            _latestAppName = $"{_appName}_{_platform}-{latestVersion}{(_platform == "win-x64" ? ".exe" : "")}";
        }
        catch (FormatException) {
            // Handle error in version format
            Console.WriteLine("Error parsing version string");
            latestVersion = new Version(0, 0, 0);
        }
    }

    public string GetLatestVersion() {
        try {
            // Access file directly from the raw GitHub URL
            string versionUrl = _rawBaseUrl + "version.txt";
            string response = _httpClient.GetStringAsync(versionUrl).GetAwaiter().GetResult().Trim();
            return response;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error retrieving latest version: {ex.Message}");
            return "0.0.0"; // Return a default version that can be parsed
        }
    }

    public bool IsUpdateAvailable() {
        return latestVersion > currentVersion;
    }

    public void RunUpdatedApp() {
        ProcessStartInfo updatedAppRun = new ProcessStartInfo {
            FileName = _latestAppName,
            CreateNoWindow = false,
            UseShellExecute = true
        };

        try {
            Process.Start(updatedAppRun);
        } catch (Exception ex) {
            Console.WriteLine($"Error running updated app: {ex.Message}");
            Console.WriteLine("Run the updated app manually");
        }
    }

    public void RemoveOldVersions() {
        // Check if the "updateInfo.txt" file exists
        if (!File.Exists("updateInfo.txt")) {
            return; // If it doesn't exist, exit the method
        }

        // Read the contents of the "updateInfo.txt" file
        string updateInfoOldDelete = File.ReadAllText("updateInfo.txt").Trim();

        // Attempt to delete the old version if it exists
        if (File.Exists(updateInfoOldDelete)) {
            // Wait a moment to ensure the file isn't locked
            System.Threading.Thread.Sleep(1000);
            File.Delete(updateInfoOldDelete);
            Console.WriteLine($"Successfully deleted {updateInfoOldDelete}");
            System.Threading.Thread.Sleep(1000);
        }

        // Delete the "updateInfo.txt" file
        File.Delete("updateInfo.txt");
    }

    public string GetPlatform() {
        string platform = string.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            platform = Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86"; // 64-bit or 32-bit Windows
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm) {
                platform = "win-arm"; // ARM Windows
            } else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) {
                platform = "win-arm64"; // ARM64 Windows
            }
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            platform = Environment.Is64BitOperatingSystem ? "linux-x64" : "linux-arm"; // 64-bit or ARM Linux
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) {
                platform = "linux-arm64"; // ARM64 Linux
            }
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            platform = Environment.Is64BitOperatingSystem ? "osx-x64" : "osx-arm64"; // 64-bit or ARM macOS
        }

        if (string.IsNullOrEmpty(platform)) {
            Console.WriteLine("Your platform is not supported. Automatic updating isn't permitted.");
        }

        return platform;
    }

    public void LinuxExecCheck() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return; // Exit if not on Linux
        }

        // Construct the full path to the executable
        string filePath = Path.Combine(_executableDirectory, _latestAppName);

        // Check if the file exists
        if (!File.Exists(filePath)) {
            Console.WriteLine($"File {filePath} does not exist.");
            return;
        }

        // Set the file to executable using ProcessStartInfo
        var processInfo = new ProcessStartInfo {
            FileName = "chmod",
            Arguments = $"+x \"{filePath}\"", // Use quotes to handle spaces in file paths
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo)) {
            process.WaitForExit();
            if (process.ExitCode == 0) {
                Console.WriteLine($"File {filePath} has been made executable.");
            } else {
                Console.WriteLine($"Failed to make {filePath} executable. Error: {process.StandardError.ReadToEnd()}");
            }
        }
    }

    public void Update() {
        RemoveOldVersions();

        if (!IsUpdateAvailable()) {
            Console.WriteLine("No updates available.");
            System.Threading.Thread.Sleep(500); // Wait for half a second
            Console.Clear();
            return; // Early return if no update is available
        }

        Console.WriteLine($"Update available: {currentVersion} → {latestVersion}");
        Console.WriteLine("Do you want to update? (y/n)");
        string input = Console.ReadLine();

        if (input != "y") {
            return; // Early return if the user does not want to update
        }

        Console.WriteLine("Updating...");

        try {
            string updateUrl = _rawBaseUrl + _latestAppName;
            string newFilePath = Path.Combine(_executableDirectory, _latestAppName);

            Console.WriteLine($"Downloading update from: {updateUrl}");
            byte[] fileData = _httpClient.GetByteArrayAsync(updateUrl).GetAwaiter().GetResult();
            File.WriteAllBytes(newFilePath, fileData);

            // Write old app Path into a .txt file
            using (StreamWriter writer = new StreamWriter("updateInfo.txt")) { 
                writer.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
            }

            // Checks linux file executability
            LinuxExecCheck();

            // Runs the updated app
            RunUpdatedApp();

            // Exits outdated(this) app to prepare it for deletion
            Environment.Exit(0);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error during update: {ex.Message}");
        }
    }
}