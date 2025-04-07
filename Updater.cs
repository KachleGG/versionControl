using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Diagnostics;

namespace Updater {
    class Updater {
        private readonly string _username;
        private readonly string _repository;
        private readonly string _branch;
        private readonly string _rawBaseUrl;
        public Version currentVersion;
        public Version latestVersion;
        private readonly HttpClient _httpClient;
        private readonly string _executablePath;
        private readonly string _appName;
        private readonly string _executableDirectory;

        public Updater(string username, string repository, string version, string appName, string branch = "main") {
            _username = username;
            _repository = repository;
            currentVersion = Version.Parse(version);
            _branch = branch;
            _appName = appName;
            
            // Get the path of the current executable
            _executablePath = Process.GetCurrentProcess().MainModule.FileName;
            _executableDirectory = Path.GetDirectoryName(_executablePath);
            
            // Construct the raw GitHub URL format
            _rawBaseUrl = $"https://raw.githubusercontent.com/{username}/{repository}/{branch}/";
            _httpClient = new HttpClient();
            
            try {
                // Get latest version
                string versionString = GetLatestVersion();
                latestVersion = Version.Parse(versionString);
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
                string response = _httpClient.GetStringAsync(versionUrl).GetAwaiter().GetResult();
                return response.Trim();
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
            string updatedApp = $"{_appName}-{latestVersion}.exe";

            ProcessStartInfo updatedAppRun = new ProcessStartInfo {
            FileName = updatedApp,
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
            
            // Reads the contents of the "updateInfo.txt" file
            if (File.Exists("updateInfo.txt")) { 
                    string updateInfoOldDelete = File.ReadAllText("updateInfo.txt").Trim(); 

                    // Delete the "updateInfo.txt" file
                    if (File.Exists("updateInfo.txt")) { File.Delete("updateInfo.txt"); }

                    // Make sure the file exists before attempting deletion
                    if (File.Exists(updateInfoOldDelete)) {
                        // Wait a moment to ensure the file isn't locked
                        System.Threading.Thread.Sleep(1000);
                        File.Delete(updateInfoOldDelete);
                        Console.WriteLine($"Successfully deleted {updateInfoOldDelete}");
                        System.Threading.Thread.Sleep(1000);
                    }
                }
        }

        public void Update() {
            RemoveOldVersions();
            if (IsUpdateAvailable()) {
                Console.WriteLine($"Update available: {currentVersion} → {latestVersion}");
                Console.WriteLine("Do you want to update? (y/n)");
                string input = Console.ReadLine();
                if (input == "y") {
                    Console.WriteLine("Updating...");
                    
                    try {
                        // Download the update file directly to the same directory
                        string updateFileName = $"{_appName}-{latestVersion}.exe";
                        string updateUrl = _rawBaseUrl + updateFileName;
                        string newFilePath = Path.Combine(_executableDirectory, updateFileName);
                        
                        Console.WriteLine($"Downloading update from: {updateUrl}");
                        byte[] fileData = _httpClient.GetByteArrayAsync(updateUrl).GetAwaiter().GetResult();
                        File.WriteAllBytes(newFilePath, fileData);
                        
                        // Write old app Path into a .txt file
                        using (StreamWriter writer = new StreamWriter("updateInfo.txt")) { writer.WriteLine(_executablePath); }

                        // Runs the updated app
                        RunUpdatedApp();

                        // Exits outdated(this) app to prepare it for deletion
                        Environment.ExitCode = 0;
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error during update: {ex.Message}");
                    }
                }
            } else {
                Console.WriteLine("No updates available.");
                System.Threading.Thread.Sleep(500); // Wait for half a second
                Console.Clear();
            }
        }
    }
}