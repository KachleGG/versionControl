namespace Updater {

    class Program {
        
        static void Main(string[] args) {
            string version = "1.0.2";
            Updater updater = new Updater("https://github.com/KachleGG/versionControl/version.txt", ".zip");
            updater.CheckAndUpdate(version);
        }
    }
}