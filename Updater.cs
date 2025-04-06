using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace Updater {

    public class Updater {
        private readonly string _versionUrl;
        private readonly string _applicationUrl;
        private readonly string _backupSuffix;
        private readonly string _repositoryPath;
        

        public Updater(string versionUrl, string applicationUrl, string repositoryPath = null, string backupSuffix = "_old") {
            _versionUrl = versionUrl;
            _applicationUrl = applicationUrl;
            _backupSuffix = backupSuffix;
            _repositoryPath = repositoryPath ?? AppDomain.CurrentDomain.BaseDirectory;
        }
        
        public bool IsUpdateAvailable(string currentVersion) {
            try {
                // First check for internet connectivity
                if (!IsInternetAvailable()) {
                    return false;
                }
                
                // Check version.txt for latest version
                var latestVersion = new WebClient().DownloadString(_versionUrl).Trim();
                return new Version(latestVersion) > new Version(currentVersion);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
                return false;
            }
        }
        
        private bool UpdateUsingGit() {
            try {
                // Check if .git directory exists
                if (!Directory.Exists(Path.Combine(_repositoryPath, ".git"))) {
                    Console.WriteLine("Not a Git repository. Git update failed.");
                    return false;
                }
                
                // Store the current directory to return to it later
                string currentDirectory = Directory.GetCurrentDirectory();
                
                try {
                    // Change to repository directory
                    Directory.SetCurrentDirectory(_repositoryPath);
                    
                    // Run git fetch to get latest changes
                    Console.WriteLine("Running git fetch...");
                    ExecuteGitCommand("fetch", "--all");
                    
                    // Get current branch
                    string currentBranch = GetCurrentGitBranch();
                    if (string.IsNullOrEmpty(currentBranch)) {
                        currentBranch = "main"; // Default to main if we can't determine the branch
                    }
                    
                    // Reset to latest commit on the branch
                    Console.WriteLine($"Resetting to latest commit on {currentBranch}...");
                    ExecuteGitCommand("reset", "--hard", $"origin/{currentBranch}");
                    
                    // Clean any untracked files
                    Console.WriteLine("Cleaning untracked files...");
                    ExecuteGitCommand("clean", "-fd");
                    
                    return true;
                }
                finally {
                    // Return to original directory
                    Directory.SetCurrentDirectory(currentDirectory);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Git update failed: {ex.Message}");
                return false;
            }
        }
        

        private string GetCurrentGitBranch() {
            try {
                ProcessStartInfo gitInfo = new ProcessStartInfo {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (Process process = Process.Start(gitInfo)) {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return output;
                }
            }
            catch {
                return string.Empty;
            }
        }
        
        private void ExecuteGitCommand(string command, params string[] args) {
            string arguments = $"{command} {string.Join(" ", args)}";
            
            ProcessStartInfo gitInfo = new ProcessStartInfo {
                FileName = "git",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(gitInfo)) {
                process.WaitForExit();
                
                if (process.ExitCode != 0) {
                    throw new Exception($"Git command failed with exit code {process.ExitCode}: git {arguments}");
                }
            }
        }
        
        public void CheckAndUpdate(string currentVersion) {
            // Skip update check if no internet
            if (!IsInternetAvailable()) {
                return;
            }
            
            if (IsUpdateAvailable(currentVersion)) {
                UpdateUsingGit();
            }
        }
        
        /// <summary>
        /// Checks if internet connection is available
        /// </summary>
        /// <returns>True if internet is available</returns>
        private bool IsInternetAvailable() {
            try {
                // First try to ping a reliable host
                using (var ping = new Ping()) {
                    var reply = ping.Send("8.8.8.8", 3000); // Google DNS with 3 second timeout
                    if (reply?.Status == IPStatus.Success) {
                        return true;
                    }
                }
                
                // If ping fails, try a HTTP request as fallback
                using (var client = new WebClient()) {
                    using (client.OpenRead("http://www.gstatic.com/generate_204")) {
                        return true;
                    }
                }
            }
            catch {
                // Any exception means no internet
                return false;
            }
            
            return false;
        }
    }
}