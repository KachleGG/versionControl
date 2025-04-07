namespace Updater {

    class Program {
        
        static void Main(string[] args) {
            // Initialize updater and try to update
            Updater updater = new Updater("KachleGG", "versionControl", "1.0.7", "Updatr");

            // Updates to a new version
            updater.Update();

            // Checks if there are any older versions of this code and removes them
            updater.RemoveOldVersions();
        }
    }
}