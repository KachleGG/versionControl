using System.Runtime.InteropServices;
namespace UpdateMGR {

    class Program {
        
        static void Main(string[] args) {
            // Initialize updater and try to update
            Updater updater = new Updater("KachleGG", "versionControl", "1.2.2", "Updatr");
            updater.Update();
            Console.WriteLine("Runing version: " + updater.currentVersion);
        }
    }
}