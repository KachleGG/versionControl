namespace Updater {

    class Program {
        
        static void Main(string[] args) {
            // Initialize updater and try to update
            Updater updater = new Updater("KachleGG", "versionControl", "1.0.3", "Updatr");
            updater.Update();
        }
    }
}