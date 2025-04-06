namespace Updater {

    class Program {
        
        static void Main(string[] args) {
            // Initialize updater
            Updater updater = new Updater("KachleGG", "versionControl");

            Console.WriteLine(updater.currentVersion);
            Console.WriteLine(updater.latestVersion);
        }
    }
}