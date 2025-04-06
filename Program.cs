namespace Updater {

    class Program {
        
        static void Main(string[] args) {
            Version currentVersion = Version.Parse(File.ReadAllText("version.txt"));
            Version latestVersion = Version.Parse(File.ReadAllText("https://github.com/KachleGG/versionControl/version.txt"));
            Console.WriteLine(currentVersion);
            Console.WriteLine(latestVersion);
        }
    }
}