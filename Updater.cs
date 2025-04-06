using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace Updater {
    class Updater {
        private readonly string _username;
        private readonly string _repository;
        private readonly string _branch;
        private readonly string _rawBaseUrl;
        public Version currentVersion;
        public Version latestVersion;
        private readonly HttpClient _httpClient;

        public Updater(string username, string repository, string branch = "main") {
            _username = username;
            _repository = repository;
            _branch = branch;
            
            // Construct the raw GitHub URL format
            _rawBaseUrl = $"https://raw.githubusercontent.com/{username}/{repository}/{branch}/";
            
            _httpClient = new HttpClient();
            
            // Read current version
            if (File.Exists("version.txt")) {
                currentVersion = Version.Parse(File.ReadAllText("version.txt"));
            } else {
                currentVersion = new Version(0, 0, 0);
            }
            
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
                Console.WriteLine($"Fetching version from: {versionUrl}");
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

        public void Update() {
            if (IsUpdateAvailable()) {
                Console.WriteLine($"Update available: {currentVersion} → {latestVersion}");
                // Update functionality can be implemented later when needed
            } else {
                Console.WriteLine("No updates available.");
            }
        }
    }
}