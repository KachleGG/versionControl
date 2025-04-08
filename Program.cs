namespace Updater {

    class Program {
        
        static void Main(string[] args) {
            // Initialize updater and try to update
            Updater updater = new Updater("KachleGG", "versionControl", "1.1.2", "Updatr");
            updater.Update();
        }
    }
}